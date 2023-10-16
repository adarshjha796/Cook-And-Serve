using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour {
	
	//***************************************************************************//
	// Very compact class for a button to navigate users to different scenes
	//***************************************************************************//

	//destination scene's name
	public string backTo = "";

	//tap sfx
	public AudioClip menuClickSfx;

	void Update () {
		StartCoroutine(tapManager());
		if(Input.GetKeyDown(KeyCode.Escape)) //back button in android / escape in windows
			SceneManager.LoadScene(backTo);
	}


	private RaycastHit hitInfo;
	private Ray ray;
	IEnumerator tapManager (){
		//Mouse of touch?
		if(	Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began)  
			ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
		else if(Input.GetMouseButtonDown(0))
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		else
			yield break;
			
		if(Physics.Raycast(ray, out hitInfo)) {
			GameObject objectHit = hitInfo.transform.gameObject;
			switch(objectHit.name) {
				case "BackButton":
					playSFX(menuClickSfx);
					makeButtonBigger(hitInfo.transform.gameObject);
					yield return new WaitForSeconds(0.15f);
					SceneManager.LoadScene(backTo);
					break;
			}
		}
	}


	void playSFX ( AudioClip _sfx  ){
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}


	void makeButtonBigger ( GameObject _objectHit  ){
		_objectHit.transform.localScale += new Vector3(0.25f,
		                                               0,
		                                               0.25f * Mathf.Sign(_objectHit.transform.localScale.z));
	}

}