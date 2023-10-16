using UnityEngine;
using System.Collections;

public class GrillController : MonoBehaviour {

	/// <summary>
	/// grill machine controller. It provide variables for other controllers to control it's
	/// status. You can change the timer to simulate an upgrade.
	/// </summary>

	public static float grillTimer = 3.0f;				//Time it takes to process the given item
	public static float grillKeepWarmTimer = 5.0f;		//after burger has been processed, the grill can keep it warm for a while 
														//before overburning it.

	public bool isOn = false;					//is this machine turned on? (is processing?)
	public bool isEmpty = true;					//is there any item mounted into the machine?
	public bool isWarm = false;					//is processing the burger is done and it's being kept warm?
	public bool isOverburned = false;			//is this burger should be discarded, because it's overburned?

	public AudioClip frySfx;					//sfx for starting to fry the burger
	public AudioClip readySfx;					//sfx when burger has been fried and is ready to deliver
	public AudioClip overburnSfx;				//sfx when burger stayed too long on the grill and is now useless
												//and should be discarded

	public GameObject lightGO;					//child game object
    private Renderer lgr;                       //Renderer component of the lightGO object

	public Material[] statusMat;                //material used to show the stuatus of the grill machine
                                                //index[0] = off - not working
                                                //index[1] = on - normal
                                                //index[2] = off - overburn

    private void Awake()
    {
        lgr = lightGO.GetComponent<Renderer>();
    }

    void Start () {
        lgr.material = statusMat[0];
	}


	void Update() {

		if(isWarm) {
            lgr.material = statusMat[1];
		} else if(isOverburned) {
            lgr.material = statusMat[2];
		} else {
            lgr.material = statusMat[0];
		}

	}


	public void playSfx(AudioClip _sfx) {
		GetComponent<AudioSource>().clip = _sfx;
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}
}
