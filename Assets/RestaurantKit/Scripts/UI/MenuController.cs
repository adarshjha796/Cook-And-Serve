using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
	
	///*************************************************************************///
	/// Main Menu Controller.
	/// This class handles all touch events on buttons, and also updates the 
	/// player status (available-money) on screen.
	///*************************************************************************///

	private float buttonAnimationSpeed = 9;	//speed on animation effect when tapped on button
	private bool  canTap = true;			//flag to prevent double tap
	//public AudioClip tapSfx;				//tap sound for buttons click

	//Reference to GameObjects
	public GameObject playerMoney;          //UI 3d text object

	public GameObject settingMenu;

	private SpriteRenderer _spriteRenderer;

	private GameObject objectHit;

	//*****************************************************************************
	// Init. Updates the 3d texts with saved values fetched from playerprefs.
	//*****************************************************************************
	void Awake (){
		//PlayerPrefs.DeleteAll();
		Time.timeScale = 1.0f;
		playerMoney.GetComponent<TextMesh>().text = "Coins: $" + PlayerPrefs.GetInt("PlayerMoney");
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

    //*****************************************************************************
    // FSM
    //*****************************************************************************
    void Update (){	
		if(canTap) {
			StartCoroutine(tapManager());
		}

		if (settingMenu.activeInHierarchy == false)
		{
			GameObject.Find("Button-01").GetComponent<BoxCollider>().enabled = true;
			GameObject.Find("Button-02").GetComponent<BoxCollider>().enabled = true;
			GameObject.Find("Button-03").GetComponent<BoxCollider>().enabled = true;
			GameObject.Find("Button-04").GetComponent<BoxCollider>().enabled = true;
		}
	}


	//*****************************************************************************
	// This function monitors player touches on menu buttons.
	// detects both touch and clicks and can be used with editor, handheld device and 
	// every other platforms at once.
	//*****************************************************************************
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
			objectHit = hitInfo.transform.gameObject;
			switch(objectHit.name) {
			
				//Game Modes
				case "Button-01":							//Play Button
					playSfx();								//play touch sound
					PlayerPrefs.SetString("gameMode", "FREEPLAY");	//set game mode to fetch later in "Game" scene
					StartCoroutine(animateButton(objectHit));       //touch animation effect
					yield return new WaitForSeconds(1.0f);          //Wait for the animation to end
					SceneManager.LoadScene("Game-c#");				//Load the next scene
					break;
				//case "Button-02":									//Career Button
				//	playSfx();
				//	PlayerPrefs.SetString("gameMode", "CAREER");
				//	StartCoroutine(animateButton(objectHit));
				//	yield return new WaitForSeconds(1.0f);
				//	SceneManager.LoadScene("LevelSelection-c#");
				//	break;
				//case "Button-03":									//Shop Button
				//	playSfx(tapSfx);
				//	StartCoroutine(animateButton(objectHit));
				//	yield return new WaitForSeconds(1.0f);
				//	SceneManager.LoadScene("Shop-c#");
				//	break;
				//case "Button-04":									
				//	playSfx(tapSfx);
				//	StartCoroutine(animateButton(objectHit));
				//	yield return new WaitForSeconds(1.0f);
				//	SceneManager.LoadScene("BuyCoinPack-c#");
				//	break;

				case "Button-04":									//Setting Button
					playSfx();
                    StartCoroutine(animateButton(objectHit));
                    //yield return new WaitForSeconds(1.0f);
					if(settingMenu.activeInHierarchy == false)
                    {
						settingMenu.SetActive(true);
						GameObject.Find("Button-01" ).GetComponent<BoxCollider>().enabled = false;
						GameObject.Find("Button-02").GetComponent<BoxCollider>().enabled = false;
						GameObject.Find("Button-03").GetComponent<BoxCollider>().enabled = false;
						GameObject.Find("Button-04").GetComponent<BoxCollider>().enabled = false;
					}
                    else 
                    {
						settingMenu.SetActive(false);
					}
					break;

				case "Button-05":									//Cross Button/Setting Panel Close Button
					playSfx();
					StartCoroutine(animateButton(objectHit));
					yield return new WaitForSeconds(0.2f);
					settingMenu.SetActive(false);
					break;
			}
		}
	}


	//*****************************************************************************
	// This function animates a button by modifying it's scales on x-y plane.
	// can be used on any element to simulate the tap effect.
	//*****************************************************************************
	IEnumerator animateButton ( GameObject _btn  ){
		canTap = false;
		Vector3 startingScale = _btn.transform.localScale;	//initial scale	
		Vector3 destinationScale = startingScale * 0.85f;		//target scale
		
		//Scale up
		float t = 0.0f; 
		while (t <= 1.0f) {
			t += Time.deltaTime * buttonAnimationSpeed;
			_btn.transform.localScale = new Vector3( Mathf.SmoothStep(startingScale.x, destinationScale.x, t),
			                                      	 Mathf.SmoothStep(startingScale.y, destinationScale.y, t),
			                                        _btn.transform.localScale.z);
			yield return 0;
		}
		
		//Scale down
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
		
		if(r >= 1)
			canTap = true;
	}


	//*****************************************************************************
	// Play sound clips
	//*****************************************************************************
	public void playSfx ()
	{
		//if(objectHit.GetComponent<AudioSource>() != null)
		//{
		//	objectHit.GetComponent<AudioSource>().clip = _clip; 
		//}
		
		if(!objectHit.GetComponent<AudioSource>().isPlaying) {
			objectHit.GetComponent<AudioSource>().Play();
		}
	}

	/// <summary>
	/// Hovering on Button will change color.
	/// </summary>
    public void OnMouseOver()
	{
		_spriteRenderer.color = new Color(.9f, .9f, .9f, 1);
	}

	/// <summary>
	/// Hovering end on Button will change back to original color.
	/// </summary>
	public void OnMouseExit()
    {
		_spriteRenderer.color = new Color(255/255,255/255,255/255,1);
	}
}