using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {

	//***************************************************************************//
	// This class manages pause and unpause states.
	//***************************************************************************//
	public static bool  soundEnabled;
	public static bool  isPaused;
	private float savedTimeScale;
	public GameObject pausePlane;
	//public AudioClip tapSfx;

	public SpriteRenderer spriteRenderer;
	public Sprite newSprite;
	public Sprite newSprite1;

	private float delayBeforeLoading = 3f;
	private float timeElapsed;

	private GameObject objectHit;
	private RaycastHit hitInfo;
	private Ray ray;

	enum Page {
		PLAY, PAUSE
	}
	private Page currentPage = Page.PLAY;


	void Awake (){		
		soundEnabled = true;
		isPaused = false;
		
		Time.timeScale = 1.0f;
		
		if(pausePlane)
	    	pausePlane.SetActive(false); 
	}


	void Update (){
		timeElapsed += Time.deltaTime;

		//touch control
		//touchManager();
		StartCoroutine(touchManager());
		//optional pause
		//if(Input.GetKeyDown(KeyCode.P) || Input.GetKeyUp(KeyCode.Escape)) {
		//	//PAUSE THE GAME
		//	switch (currentPage) {
		//           case Page.PLAY: 
		//			PauseGame(); 
		//			break;
		//           case Page.PAUSE: 
		//			UnPauseGame(); 
		//			break;
		//           default: 
		//			currentPage = Page.PLAY;
		//			break;
		//       }
		//}

		////debug restart
		//if(Input.GetKeyDown(KeyCode.R)) {
		//	SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		//}
	}


	IEnumerator touchManager (){
		//if (Input.GetMouseButtonUp(0))
		//{
		//	//RaycastHit hitInfo;
		//	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		//}
		//Mouse of touch?
		if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Ended)
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if (Input.GetMouseButtonUp(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;

		if (Physics.Raycast(ray, out hitInfo)) {
				objectHit = hitInfo.transform.gameObject;
				string objectHitName = hitInfo.transform.gameObject.name;
				switch(objectHitName) {
					case "PauseBtn":
						//pause is not allowed when game is finished
						if (MainGameController.gameIsFinished)
							 yield return null;

						switch (currentPage) {
				            case Page.PLAY:
								ChangeSprite();
								PauseGame(); 
								break;
				            case Page.PAUSE:
								playSfx1();
								UnPauseGame();
								ResetSprite();
								break;
				            default: 
								currentPage = Page.PLAY;
								break;
				        }

						break;
					
					case "Btn-Resume":
                    switch (currentPage)
                    {
                        case Page.PLAY:
                            PauseGame();
                            break;

                        case Page.PAUSE:
                            //playSfx1();
                            UnPauseGame();
							playSfx1();
							ResetSprite();
                            break;

                        default:
                            currentPage = Page.PLAY;
                            break;
                    }
                    break;

                case "Btn-Menu":
						playSfx1();
						UnPauseGame();
						yield return new WaitForSeconds(.07f);
						SceneManager.LoadScene("Menu-c#");
						break;
						
					case "Btn-Restart":
						playSfx1();
						UnPauseGame();
						yield return new WaitForSeconds(.1f);
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
						
					case "End-Menu":
						AudioListener.volume = 1;
						playSfx1();
						yield return new WaitForSeconds(1.0f);
						SceneManager.LoadScene("Menu-c#");
						break;

					case "End-Next":
						playSfx1();
						yield return new WaitForSeconds(1.0f);
						SceneManager.LoadScene("LevelSelection-c#");
						break;
						
					case "End-Restart":
						playSfx1();
						yield return new WaitForSeconds(1.0f);
						SceneManager.LoadScene(SceneManager.GetActiveScene().name);
						break;
				}
		}
	}


	void PauseGame (){
		//print("Game in Paused...");
		isPaused = true;
		savedTimeScale = Time.timeScale;
		Time.timeScale = 0;
		//AudioListener.volume = 0;
		GetComponent<AudioSource>().Pause();
		Camera.main.GetComponent<AudioSource>().Stop();
		if (pausePlane)
	    	pausePlane.SetActive(true);
	    currentPage = Page.PAUSE;
	}


	void UnPauseGame (){
		//print("Unpause");
	    isPaused = false;
	    Time.timeScale = savedTimeScale;
		//AudioListener.volume = 1.0f;
		GetComponent<AudioSource>().Play();
		Camera.main.GetComponent<AudioSource>().Play();
		if (pausePlane)
	    	pausePlane.SetActive(false);   
	    currentPage = Page.PLAY;
	}

	public void playSfx1()
	{
		//if (GetComponent<AudioSource>() != null)
		//{
		//	GetComponent<AudioSource>().clip = _clip;
		//}

		if (!objectHit.GetComponent<AudioSource>().isPlaying)
		{
			objectHit.GetComponent<AudioSource>().Play();
		}
	}

	void ChangeSprite()
	{
		spriteRenderer.sprite = newSprite;
		transform.localScale = new Vector2(.6f, .6f);
		GameObject.Find("Dash").SetActive(false);
		GameObject.Find("Dash1").SetActive(false);
	}
	
	void ResetSprite()
    {
		spriteRenderer.sprite = newSprite1;
		transform.localScale = new Vector2(.4f, .4f);
		for (int a = 0; a < transform.childCount; a++)
		{
			transform.GetChild(a).gameObject.SetActive(true);
		}
	}

	public void OnMouseOver()
	{
		spriteRenderer.color = new Color(.9f, .9f, .9f, 1);
	}

	public void OnMouseExit()
	{
		spriteRenderer.color = new Color(255 / 255, 255 / 255, 255 / 255, 1);
	}

}