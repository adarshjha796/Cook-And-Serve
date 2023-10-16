using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class CareerMapManager : MonoBehaviour {

	///*************************************************************************///
	/// CareerMapManager will load the game scene with parameters set by you
	/// for the selected level. It will saves those values inside playerPrefs and
	/// tehy will be fetched and applied in the game scene.
	///*************************************************************************///

	private int currentHolderIndex = 1;			//we start the game while using holder 1, 
												//and we then can switch to other button holders
	private int totalButtonHolders = 3; 		//we are using two button holders inside the master button holder (each containing 8 level buttons)
	public GameObject masterHolder;				//master button holder object which we use to move
	private float masterHolderX = 0;			//current x position of master holder
	public GameObject arrowPrev;				//arrow objects used to navigate between multiple button holders
	public GameObject arrowNext;				//...
	private float transitionAmount = 20.0f;		//how much transition is needed?

	static public int userLevelAdvance;
	private int totalLevels;
	private GameObject[] levels;

	public AudioClip menuTap;
	private bool canTap;
	private float buttonAnimationSpeed = 9;

	void Awake (){
		canTap = true; //player can tap on buttons
		
		if(PlayerPrefs.HasKey("userLevelAdvance"))
			userLevelAdvance = PlayerPrefs.GetInt("userLevelAdvance");
		else
			userLevelAdvance = 0; //default. only level 1 in open.

		//cheat - debug
		//userLevelAdvance = 18;
	}


	void Start (){
		
		//prevent screenDim in handheld devices
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		//go to correct level holder, if we have advanced in the game (levels)
		//you need to continue this process if you need/want to add more button holders
		if (userLevelAdvance <= 7) {
			//stay on the first page
			//...

		} if (userLevelAdvance > 7 && userLevelAdvance <= 15) {
			//go to second page
			currentHolderIndex += 1;
			StartCoroutine(startHolderTransition (1, 10, transitionAmount));

		} if (userLevelAdvance > 15 && userLevelAdvance <= 23) {
			//go to third page
			currentHolderIndex += 2;
			StartCoroutine(startHolderTransition (1, 10, transitionAmount * 2));
		} 

	}


	void Update (){
		if(canTap)	
			StartCoroutine(tapManager());
	}


	///***********************************************************************
	/// Process user inputs
	///***********************************************************************
	private RaycastHit hitInfo;
	private Ray ray;
	IEnumerator tapManager (){

		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
			
		if (Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			//print(objectHit.name);

			if(objectHit.tag == "levelSelectionItem") {
				canTap = false;
				playSfx(menuTap);
				StartCoroutine(animateButton(objectHit));
				
				//save the game mode
				PlayerPrefs.SetString("gameMode", "CAREER");
				PlayerPrefs.SetInt("careerLevelID", objectHit.GetComponent<CareerLevelSetup>().levelID);
				
				//save level prize
				PlayerPrefs.SetInt("careerPrize", objectHit.GetComponent<CareerLevelSetup>().levelPrize);
				
				//save mission variables
				PlayerPrefs.SetInt("careerGoalBallance", objectHit.GetComponent<CareerLevelSetup>().careerGoalBallance);
				PlayerPrefs.SetInt("careerAvailableTime", objectHit.GetComponent<CareerLevelSetup>().careerAvailableTime);
				
				int availableProducts = objectHit.GetComponent<CareerLevelSetup>().availableProducts.Length;
				PlayerPrefs.SetInt("availableProducts", availableProducts); //save the length of availableProducts
				for(int j = 0; j < availableProducts; j++) {
					PlayerPrefs.SetInt(	"careerProduct_" + j.ToString(), 
										objectHit.GetComponent<CareerLevelSetup>().availableProducts[j]);
				}
				
				PlayerPrefs.SetInt( "canUseCandy", 
									Convert.ToInt32(objectHit.GetComponent<CareerLevelSetup>().canUseCandy) );

                //set level location
                PlayerPrefs.SetInt("levelLocation", (int)objectHit.GetComponent<CareerLevelSetup>().levelLocation);

                yield return new WaitForSeconds(0.25f);
				SceneManager.LoadScene("Game-c#");
			}

			if(objectHit.name == "ArrowNext") {

				if (currentHolderIndex < totalButtonHolders) {
					playSfx(menuTap);
					StartCoroutine(animateButton(objectHit));
					canTap = false;
					currentHolderIndex++;
					StartCoroutine(startHolderTransition (1, 2.5f, transitionAmount));
				}

				yield break;
			}

			if(objectHit.name == "ArrowPrev") {

				if (currentHolderIndex > 1) {
					playSfx(menuTap);
					StartCoroutine(animateButton(objectHit));
					canTap = false;
					currentHolderIndex--;
					StartCoroutine(startHolderTransition (-1, 2.5f, transitionAmount));
				}

				yield break;
			}


			if(objectHit.name == "BackButton") {
				playSfx(menuTap);
				StartCoroutine(animateButton(objectHit));
				yield return new WaitForSeconds(1.0f);
				SceneManager.LoadScene("Menu-c#");
				yield break;
			}
		}
	}


	/// <summary>
	/// Starts the holder transition.
	/// </summary>
	/// <returns>The holder transition.</returns>
	/// <param name="side">Side.</param>
	IEnumerator startHolderTransition(int dir, float speed, float amount) {

		float t = 0;
		while (t < 1) {
			t += Time.deltaTime * speed;
			masterHolder.transform.position = new Vector3 (Mathf.SmoothStep(masterHolderX, (masterHolderX + (amount * dir * -1)), t),
															masterHolder.transform.position.y,
															masterHolder.transform.position.z);
			yield return 0;
		}

		if (t >= 1) {
			//set the new position
			masterHolderX = masterHolderX + (amount * dir * -1);
			//restore tapping
			canTap = true;
		}
	}


	///***********************************************************************
	/// Animate button by modifying it's scale
	///***********************************************************************
	IEnumerator animateButton ( GameObject _btn  ){
		Vector3 startingScale = _btn.transform.localScale;
		Vector3 destinationScale = startingScale * 0.85f;
		//yield return new WaitForSeconds(0.1f);
		float t = 0.0f; 
		while (t <= 1.0f) {
			t += Time.deltaTime * buttonAnimationSpeed;
			_btn.transform.localScale = new Vector3( Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
			                                        Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
			                                        _btn.transform.localScale.z);
			yield return 0;
		}
		
		float r = 0.0f; 
		if(_btn.transform.localScale.x >= destinationScale.x) {
			while (r <= 1.0f) {
				r += Time.deltaTime * buttonAnimationSpeed;
				_btn.transform.localScale = new Vector3( Mathf.SmoothStep(destinationScale.x, startingScale.x, r),
				                                        Mathf.SmoothStep(destinationScale.y, startingScale.y, r),
				                                        _btn.transform.localScale.z);
				yield return 0;
			}
		}
		
		//if(r >= 1)
			//canTap = true;
	}


	///***********************************************************************
	/// play audio clip
	///***********************************************************************
	void playSfx ( AudioClip _sfx  ){
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}


}