using UnityEngine;
using System.Collections;

public class CoffeeMakerController : MonoBehaviour {

	/// <summary>
	/// CoffeeMaker machine controller. It provide variables for other controllers to control it's
	/// status. You can change the timer to simulate an upgrade.
	/// </summary>

	public static float processTimer = 5.0f;		//Time it takes to process the given item
	public bool isOn = false;						//is this machine turned on? (is processing?)
	public bool isEmpty = true;						//is there any item mounted into the machine?
	public AudioClip processSfx;

	//child game objects
	public GameObject lightGO;
    private Renderer lgr;
	public GameObject pourGO;

	public Material[] statusMat;                    //material used to show the stuatus of this machine
                                                    //index[0] = off - not working
                                                    //index[1] = on

    private void Awake()
    {
        lgr = lightGO.GetComponent<Renderer>();
    }

    void Start () {
        lgr.material = statusMat[0];
		pourGO.SetActive(false);
	}


	void Update() {
		if(isOn) {
            lgr.material = statusMat[1];
			pourGO.SetActive(true);
		} else {
            lgr.material = statusMat[0];
			pourGO.SetActive(false);
		}
	}

	
	public void playSfx() {
		GetComponent<AudioSource>().clip = processSfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}
}
