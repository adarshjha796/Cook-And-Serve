using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SideRequestMover : MonoBehaviour {

    //***************************************************************************//
    // This class manages user inputs (drags and touches) on side-request panel.
    //***************************************************************************//

    //Side-request's price
    //Amount of money that customer pays after receiving this item
    public int sideReqPrice = 15;

    //public variable.
    //do not edit this.
    //we get this vaue from other classes.
    public int sideReqID;

	/// only required for side-reqs that needs process!
	/// Do not edit these variables. They should be set from other controllers.
	/// ************************************************
	public bool needsProcess;			//does this side-req should be processed before delivering to customer?
	private bool processFlag;			//will be set to true the first time this side-req enters a processor
	private bool isProcessed;			//process has been finished
	private bool isProcessing;			//process is being done
	private bool isFinished;			//are we done with positioning and processing the side-req on the processor?
	public string processorTag;			//tag of the processor game object
	public Material[] beforeAfterMat;	//index[0] = raw    index[1] = processed

	private GameObject processorMachine;//reference to coffeeMaker machine
	private GameObject target;			//target object for this side-req (a processor machine, etc...)
										//can also be null in special events
	/// *************************************************

	//Private flags.
	private bool canGetDragged;
	private float minDeliveryDistance;


	//***************************************************************************//
	// Simple Init
	//***************************************************************************//
	void Awake (){
		canGetDragged = true;
		isFinished = false;
		minDeliveryDistance = 1.0f;
		processorMachine = GameObject.FindGameObjectWithTag("coffeeMaker");
		target = processorMachine;
	}


	//***************************************************************************//
	// FSM
	//***************************************************************************//
	void Update (){
		//If dragged
		if(Input.GetMouseButton(0) && canGetDragged) {
			followInputPosition();
		}
		
		//if released and doesn't need process
		if(!Input.GetMouseButton(0) && Input.touches.Length < 1 && !needsProcess && !isFinished) {
			canGetDragged = false;
			SideRequestsController.canDeliverSideRequest = false;
			checkCorrectPlacement();
		}

		//if released and needs process
		if( (!Input.GetMouseButton(0) && Input.touches.Length < 1) && needsProcess && !isFinished && !processFlag) {
			canGetDragged = false;
			checkCorrectPlacementOnProcessor();
		}

		//if needs process and process is finished successfully
		if(needsProcess && isProcessed && !IngredientsController.itemIsInHand) {
			manageSecondDrag();
		}

		//you can make some special effects like particle, smoke or other visuals when your side-req in being processed
		if(isProcessing) {
			//Special FX here! - make sure to stop, disable ro destroy your FX object after processing is finished.
			
		}

		//Optional - change target's color when this side-req is near enough
		if( needsProcess && target != null && (!processFlag || isProcessed) )
			changeTargetsColor(target);
	}

	
	//***************************************************************************//
	// Let the player move the processed side-req
	//***************************************************************************//
	private RaycastHit hitInfo;
	private Ray ray;
	void manageSecondDrag() {
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;
		
		if(Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			if(objectHit.tag == "sideRequest" && objectHit.name == gameObject.name) {
				
				IngredientsController.itemIsInHand = true;	//we have a side-req in hand. no room for other ingredients!
				target = null;								//we can just deliver this side-req to a customer!
				
				StartCoroutine(followInputTimeIndependent());
			}
		}
	}


	//***************************************************************************//
	// Follow players mouse or finger position on screen.
	// This is an IEnumerator and run independent of game main cycle
	//***************************************************************************//
	private Vector3 tmpPos;
	IEnumerator followInputTimeIndependent() {
		while(IngredientsController.itemIsInHand || target == null) {

			gameObject.tag = "deliverySideRequest";	//because we want customers to be able to receive this side-req
			SideRequestsController.canDeliverSideRequest = true;

			//follow player input
			tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
			transform.position = tmpPos + new Vector3(0, 0, 0);
			
			//if user release the input, check if we delivered the processed ingredient to the plate or we just release it nowhere!
			if(Input.touches.Length < 1 && !Input.GetMouseButton(0)) {

				bool delivered = false;
				//if we are giving this side-request to a customer (this is close enough)
				GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("customer");
				//if there is no customer in shop, delete the side-request object.
				if(availableCustomers.Length < 1) {
					Destroy(gameObject);
					yield break;
				}
				GameObject theCustomer = null;
				for(int cnt = 0; cnt < availableCustomers.Length; cnt++) {
					if(availableCustomers[cnt].GetComponent<CustomerController>().isCloseEnoughToSideRequest) {
						//we know that just 1 customer is always nearest to the item. so "theCustomer" is unique.
						theCustomer = availableCustomers[cnt];
						delivered = true;
					}
				}
				
				//if customer got the side request..
				if(delivered) {
					//deliver the side-req and let the customers know they got something.
					theCustomer.GetComponent<CustomerController>().receiveSideRequest(sideReqID);
					processorMachine.GetComponent<CoffeeMakerController>().isEmpty = true;
					processorMachine.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
					SideRequestsController.canDeliverSideRequest = false;
					//destroy the candy gameObject
					Destroy(gameObject, 0.05f);
					yield break;

				}  else {
					//if we released it nowhere
					//print ("Reset Position");
					gameObject.tag = "sideRequest";
					SideRequestsController.canDeliverSideRequest = false;

					target = processorMachine;	
					transform.parent = target.transform;
					transform.position = new Vector3(target.transform.position.x + 0.35f,
					                                 target.transform.position.y - 0.35f,
					                                 target.transform.position.z - 0.1f);
				}
			}
			
			yield return 0;
		}
	}


	//***************************************************************************//
	// change plate's color when dragged ingredients are near enough
	//***************************************************************************//
	private float myDistance;
	void changeTargetsColor(GameObject _target) {
		if(!IngredientsController.itemIsInHand)	//if nothing is being dragged
			return;
		else {
			myDistance = Vector3.Distance(_target.transform.position, gameObject.transform.position);
			//print("myDistance: " + myDistance);
			if(myDistance < minDeliveryDistance) {
				//change target's color to let the player know this is the correct place to release the items.
				_target.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
			} else {
				//change target's color back to normal
				_target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
			}
		}
	}


	//***************************************************************************//
	// Check if the side-req is dragged into the processor machine. Otherwise delete it.
	//***************************************************************************//
	void checkCorrectPlacementOnProcessor() {
		
		//if there is already an item on the processor, destroy this new ingredient
		if(!target.GetComponent<CoffeeMakerController>().isEmpty)	{
			Destroy(this.gameObject);
			target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
			return;
		}
		
		//if this side-req is close enough to it's processor machine, leave it there. otherwise drop and delete it.
		float distanceToProcessor = Vector3.Distance(target.transform.position, gameObject.transform.position);
		//print("distanceToProcessor: " + distanceToProcessor);
		
		if(distanceToProcessor < minDeliveryDistance) {
			//close enough to land on processor machine
			transform.parent = target.transform;
			transform.position = new Vector3(target.transform.position.x + 0.15f,
			                                 target.transform.position.y - 0.45f,
			                                 target.transform.position.z - 0.1f);
			
			//change processor machine's color back to normal
			target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);

			//change the scale if neccesary
			transform.localScale = new Vector3(transform.localScale.x / 1.5f,
			                                   transform.localScale.y / 1.5f,
			                                   transform.localScale.z / 1.5f);
			
			//start processing the raw ingredient
			StartCoroutine(processRawSideReq());
			
		} else {
			Destroy(gameObject);
		}

		//Not draggable anymore.
		isFinished = true;
	}


	//***************************************************************************//
	// Process raw side-req and transform it to a usable side-req item
	//***************************************************************************//
	IEnumerator processRawSideReq() {

		processFlag = true; //should always remain true!
		isProcessing = true;
		isProcessed = false;
		float processTime = CoffeeMakerController.processTimer;

        CoffeeMakerController cmc = target.GetComponent<CoffeeMakerController>();
        cmc.isOn = true;
        cmc.isEmpty = false;
        cmc.playSfx();

		float t = 0.0f;
		while(t < 1) {
			t += Time.deltaTime * (1/processTime);
			//print (gameObject.name + " is getting processed! - Time to wait: " + t);
			yield return 0;
		}
		
		if(t >= 1) {
			isProcessing = false;
			isProcessed = true;
			gameObject.tag = "sideRequest";
			gameObject.name = gameObject.name.Substring(0, gameObject.name.Length);
            cmc.isOn = false;
			GetComponent<Renderer>().material = beforeAfterMat[1];

			target = null;
		}
	}


	//***************************************************************************//
	// Check if this side-request is delivered to a customer.
	//***************************************************************************//
	void checkCorrectPlacement() {

		bool delivered = false;

		//if we are giving this side-request to a customer (this is close enough)
		GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("customer");
		//if there is no customer in shop, delete the side-request object.
		if(availableCustomers.Length < 1) {
			Destroy(gameObject);
			return;
		}
		GameObject theCustomer = null;
		for(int cnt = 0; cnt < availableCustomers.Length; cnt++) {
			if(availableCustomers[cnt].GetComponent<CustomerController>().isCloseEnoughToSideRequest) {
				//we know that just 1 customer is always nearest to the item. so "theCustomer" is unique.
				theCustomer = availableCustomers[cnt];
				delivered = true;
			}
		}
		
		//if customer got the side request..
		if(delivered) {
			//deliver the side-req and let the customers know he got something.
			theCustomer.GetComponent<CustomerController>().receiveSideRequest(sideReqID);	
		} 
		
		//destroy the candy gameObject
		Destroy(gameObject);
	}


	//***************************************************************************//
	// Follow players mouse or finger position on screen.
	//***************************************************************************//
	private Vector3 _Pos;
	void followInputPosition (){
		_Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		_Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
		//follow player's finger
		transform.position = _Pos + new Vector3(0, 0, 0);
	}

	//***************************************************************************//
	// Set correct material
	//***************************************************************************//
	public void setMaterial (Material _mat){
		GetComponent<Renderer>().material = _mat;
	}

}