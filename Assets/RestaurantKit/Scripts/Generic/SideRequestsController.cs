using UnityEngine;
using System.Collections;

public class SideRequestsController : MonoBehaviour {

//***************************************************************************//
// Main class for Handling all things related to Side-Requests
//***************************************************************************//

	//Static var
	public static bool canDeliverSideRequest;

	//Only if this side-request item needs processing before handing over to customer
	//************************************
	public bool needsProcess = false;
	public string processorTag = "";
	public Material[] beforeAfterMat;	//index[0] = raw    index[1] = processed
	//************************************

	//public list of all available sideRequests.
	public GameObject[] sideRequestsArray;
	//Public ID of this Side-Request.
	public int sideReqID;

	//Private flags
	private float delayTime;			//after this delay, we let player to be able to choose another one again
	private bool canCreate = true;		//cutome flag to prevent double picking

	public AudioClip itemPick;


	//***************************************************************************//
	// Init
	//***************************************************************************//
	void Awake ()
	{
		delayTime = 1.0f;
		canDeliverSideRequest = false;
		if(needsProcess)
			GetComponent<Renderer>().material = beforeAfterMat[0];
	}


	//***************************************************************************//
	// FSM
	//***************************************************************************//
	void Update ()
	{
		StartCoroutine(managePlayerDrag());
	}


	//***************************************************************************//
	// If player has dragged on of the side-requests, make an instance of it, then
	// follow players touch/mouse position.
	//***************************************************************************//
	private RaycastHit hitInfo;
	private Ray ray;
	IEnumerator managePlayerDrag ()
	{
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
			
		if(Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			if(objectHit.tag == "sideRequest" && objectHit.name == gameObject.name && !IngredientsController.itemIsInHand) {

				if(!needsProcess)
					createSideRequest();
				else
					createRawSideRequest();	//raw side request needs to be processed before use

			}
		}
	}


	//***************************************************************************//
	// Create an instance of this sideReq...
	//***************************************************************************//
	void createSideRequest ()
	{
		if(canCreate && !MainGameController.gameIsFinished) 
		{
			GameObject sideReq = Instantiate(sideRequestsArray[sideReqID - 1], transform.position + new Vector3(0,0, -1), Quaternion.Euler(0, 180, 0)) as GameObject;
			sideReq.name = sideRequestsArray[sideReqID - 1].name;
			sideReq.tag = "deliverySideRequest";
			canDeliverSideRequest = true;
			sideReq.GetComponent<BoxCollider>().enabled = false;
			sideReq.GetComponent<SideRequestMover>().sideReqID = sideReqID;
			//sideReq.transform.localScale = Vector3(0.25f, 0.01f, 0.20f);
			playSfx(itemPick);
			canCreate = false;
			IngredientsController.itemIsInHand = true;
			StartCoroutine(reactivate());
		}
	}


	//***************************************************************************//
	// Create an instance of this RAW sideReq...
	//***************************************************************************//
	void createRawSideRequest ()
	{
		if(canCreate && !MainGameController.gameIsFinished) 
		{
			GameObject sideReq = Instantiate(sideRequestsArray[sideReqID - 1], transform.position + new Vector3(0,0, -1), Quaternion.Euler(0, 180, 0)) as GameObject;
			sideReq.name = sideRequestsArray[sideReqID - 1].name + "-RAW";
			sideReq.tag = "rawSideRequest";

            SideRequestMover srm = sideReq.GetComponent<SideRequestMover>();
            srm.sideReqID = sideReqID;
            srm.needsProcess = true;
            srm.processorTag = processorTag;

			//sideReq.transform.localScale = Vector3(0.25f, 0.01f, 0.20f);
			playSfx(itemPick);
			canCreate = false;
			IngredientsController.itemIsInHand = true;
			StartCoroutine(reactivate());
		}
	}


	//***************************************************************************//
	// make this ingredient draggable again
	//***************************************************************************//	
	IEnumerator reactivate ()
	{
		yield return new WaitForSeconds(delayTime);
		canCreate = true;
	}


	//***************************************************************************//
	// Play AudioClips
	//***************************************************************************//
	void playSfx ( AudioClip _sfx  )
	{
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}

}