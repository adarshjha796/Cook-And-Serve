using UnityEngine;
using System.Collections;

public class CandyController : MonoBehaviour {

	/// <summary>
	/// This class handles dragging candies to customers, monitoring available candies, and
	/// an option to buy more candies for the shop.
	/// </summary>
	
	public static bool canDeliverCandy;		//can we give the candy to a customer?
	public static int availableCandy;		//how many candy we have

	public int candyPrice = 20;				//price of candy, if we want to buy more
	public GameObject candyGO;				//reference to candy prefab
	public GameObject ingameCandyIcon;		//child candy object
	public GameObject availableCandyText;	//child text
	public GameObject buyMoreCandyBtn;		//more button child object
	public GameObject moneyText;			//money child object

	//Private flags
	private float delayTime = 0.25f;		//after this delay, we let player to be able to choose another ingredient again
	private bool canCreate;					//cutome flag to prevent double picking
	private bool canBuyCandy;				//are we allowed to buy more candy?

	public AudioClip itemPick;
	public AudioClip checkout;

    //component cache
    private Renderer icir;

	void Awake (){
		canDeliverCandy = false;
		canCreate = true;
		canBuyCandy = false;
		availableCandy = 5;

        icir = ingameCandyIcon.GetComponent<Renderer>();
    }


	void Start() {
		
		//check if we are allowed to use candy in this level
		if(!MainGameController.canUseCandy)
			gameObject.SetActive(false);
	}


	void Update (){

		//let player give candy to customers
		managePlayerDrag();
		
		//always count available candies
		updateAvailableCandies();	
		
		if(availableCandy > 0) {
            //looks like active
            icir.material.color = new Color(icir.material.color.r,
                                            icir.material.color.g,
                                            icir.material.color.b,
			                                1);
			canBuyCandy = false; //hide BuyCandy button

		} else {
            //make it looks like inactive
            icir.material.color = new Color(icir.material.color.r,
                                            icir.material.color.g,
                                            icir.material.color.b,
			                                0.5f);
			canBuyCandy = true; //Show BuyCandy button
		}
	}


	//***************************************************************************//
	// Always count available candies and show it in-game.
	//***************************************************************************//
	void updateAvailableCandies (){
		availableCandyText.GetComponent<TextMesh>().text = "x" + availableCandy.ToString();
		//print("availableCandy: " + availableCandy);
		
		if(canBuyCandy)
			buyMoreCandyBtn.SetActive(true);
		else
			buyMoreCandyBtn.SetActive(false);
	}


	//***************************************************************************//
	// If player has dragged a candy, make an instance of it, then
	// follow players touch/mouse position.
	//***************************************************************************//
	private RaycastHit hitInfo;
	private Ray ray;
	void managePlayerDrag() {
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			return;
			
		if(Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			//print(objectHit.name);
			if(objectHit.tag == "candy" && !IngredientsController.itemIsInHand && availableCandy > 0) {
				createCandy();
			}
			
			if(objectHit.tag == "buyMoreCandy" && MainGameController.totalMoneyMade >= (5 * candyPrice)) {
				print("Bought 5 candy!");
				availableCandy += 5;
				MainGameController.totalMoneyMade -= 5 * candyPrice;
				playSfx(checkout);
				GameObject money3d = Instantiate(moneyText, transform.position + new Vector3(3,0,0), Quaternion.Euler(0, 0, 0)) as GameObject;
				money3d.GetComponent<TextMeshController>().myText = "-$" + (5 * candyPrice).ToString();
				return;
			}
		}
	}


	//***************************************************************************//
	// Create an instance of Candy
	//***************************************************************************//
	void createCandy (){
		if(canCreate && !MainGameController.gameIsFinished) {
			GameObject candy = Instantiate(candyGO, transform.position + new Vector3(0,0, -1), Quaternion.Euler(180, 180, 0)) as GameObject;
			candy.name = "DeliveryCandy";
			candy.tag = "deliveryCandy";
			canDeliverCandy = true;
			candy.GetComponent<BoxCollider>().enabled = false;
			candy.transform.localScale = new Vector3(1.3f, 1.3f, 0.001f);
			playSfx(itemPick);
			canCreate = false;
			IngredientsController.itemIsInHand = true; //prevent user from picking an ingredient when trying to deliver a cany!
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