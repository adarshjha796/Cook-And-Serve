using UnityEngine;
using System.Collections;

public class IngredientsController : MonoBehaviour {

	//***************************************************************************//
	// Main class for Handling all things related to ingredients
	//***************************************************************************//

	//public list of all available ingredients.
	public GameObject[] ingredientsArray;
	//Public ID of this ingredient. (used to build up the delivery queue based on customers orders)
	public int factoryID;
	public bool needsProcess = false;		//items that needs process, should first be moved to a machine
											//to become ready to serve. normal items can be served directly and
											//do not need to be processed.
	public string processorTag = "";		//actual tag of the processor machine.
											//only required if this ingredient needs to be processed.

	//Private flags
	private float delayTime;				//after this delay, we let player to be able to choose another ingredient again
	private bool  canCreate = true;			//cutome flag to prevent double picking

	//Static flag
	public static bool  itemIsInHand;       //do we already picked something? we can only pick and drag one ingredient eachtime.
                                            //Important. We also use this static flag to prevent picking the deliveryPlate, when we are
                                            //dragging some ingredients into it.

    public float Zoffset = -1f;             //ingredient will keep a certain (offset) distance from mouse/finger when being dragged.
    
	public AudioClip itemPick;
	//***************************************************************************//
	// Init
	//***************************************************************************//
	void Awake (){
		delayTime = 1.0f;
		itemIsInHand = false;
	}


	//***************************************************************************//
	// FSM
	//***************************************************************************//
	void Update (){
		managePlayerDrag();
		
		if(Input.touches.Length < 1 && !Input.GetMouseButton(0)) {
			itemIsInHand = false;
		}

		//debug
		//print ("itemIsInHand: " + itemIsInHand);
	}


	//***************************************************************************//
	// If player has dragged on of the ingredients, make an instance of it, then
	// follow players touch/mouse position.
	//***************************************************************************//
	private RaycastHit hitInfo;
	private Ray ray;
	void managePlayerDrag (){
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;
			
		if(Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			if(objectHit.tag == "ingredient" && objectHit.name == gameObject.name && !itemIsInHand) {

				if(!needsProcess)
					createIngredient();
				else
					createRawIngredient();	//raw ingredient needs to be processed (in a machine) before use
			}
		}
	}


	//***************************************************************************//
	// Create an instance of this ingredient and make it a child of deliveryPlate.
	//***************************************************************************//
	void createIngredient (){
		if(canCreate && !MainGameController.gameIsFinished && !MainGameController.deliveryQueueIsFull) {
			canCreate = false;
			itemIsInHand = true;
			GameObject prod = Instantiate(ingredientsArray[factoryID - 1], transform.position + new Vector3(0,0, Zoffset), Quaternion.Euler(90, 180, 0)) as GameObject;
			prod.name = ingredientsArray[factoryID - 1].name;
			prod.tag = "deliveryQueueItem";
			prod.GetComponent<MeshCollider>().enabled = false;
			prod.GetComponent<ProductMover>().factoryID = factoryID;
			prod.GetComponent<ProductMover>().needsProcess = false;
			prod.transform.localScale = new Vector3(0.13f, 0.01f, 0.135f); //hardcoded scale value. change with caution!
			playSfx(itemPick);
			StartCoroutine(reactivate());
		}
	}


	//***************************************************************************//
	// Create an instance of this ingredient and make it a child of deliveryPlate.
	//***************************************************************************//
	void createRawIngredient (){
		if(canCreate && !MainGameController.gameIsFinished) {
			canCreate = false;
			itemIsInHand = true;
			GameObject prod = Instantiate(ingredientsArray[factoryID - 1], transform.position + new Vector3(0,0, Zoffset), Quaternion.Euler(90, 180, 0)) as GameObject;
			prod.name = ingredientsArray[factoryID - 1].name + "-RAW";
			prod.tag = "rawIngredient";
			//prod.GetComponent<MeshCollider>().enabled = false;
			prod.GetComponent<ProductMover>().factoryID = factoryID;
			prod.GetComponent<ProductMover>().needsProcess = true;
			prod.GetComponent<ProductMover>().processorTag = processorTag;
			prod.transform.localScale = new Vector3(0.12f, 0.03f, 0.1f); //hardcoded scale value. change with caution!
			playSfx(itemPick);
			StartCoroutine(reactivate());
		}
	}


	//***************************************************************************//
	// make this ingredient draggable again
	//***************************************************************************//	
	IEnumerator reactivate (){
		yield return new WaitForSeconds(delayTime);
		canCreate = true;
	}


	//***************************************************************************//
	// Play AudioClips
	//***************************************************************************//
	void playSfx ( AudioClip _sfx  ){
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}

}