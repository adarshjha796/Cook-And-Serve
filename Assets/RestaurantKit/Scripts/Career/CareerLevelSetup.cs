using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CareerLevelSetup : MonoBehaviour {
	
	///*************************************************************************///
	/// Use this class to set different missions for each level.
	/// when you click/tap on any level button, these values automatically get saved 
	/// inside playerPrefs and then get read when the game starts.
	///*************************************************************************///

	public GameObject label;				//reference to child gameObject

    [Space(20)]
    public int levelID;						//unique level identifier. Starts from 1.    
    public string levelName;				//optional string for level name (can be shown on button instead of the number)
    public bool showLevelNameOnButton;      //Indicates if we need to show level name or level number
	public int levelPrize = 150;			//prize (money) given to player if level is finished successfully
	public int careerGoalBallance = 1500;	//mission goal
	public int careerAvailableTime = 300;	//mission time
	public bool canUseCandy = true;         //are we allowed to use candy
    //Level location (main BG image)
    public enum environments
    {
        Environment_1 = 0,
        Environment_2 = 1,
        Environment_3 = 2,
        Environment_4 = 3,
        Environment_5 = 4,
    }
    public environments levelLocation = environments.Environment_1;

    [Space(20)]
    public int[] availableProducts;			//array of indexes of available products. starts from 1.
	//star rating for levels
	//start rating checks player saved (passed) time with the target time, and based on a fixed difference,
	//grants 3, 2 or 1 star. Unlocked levels will always show 0-star image.
	public GameObject levelStarsGo;	//reference to child game object
	public Material[] starMats;		//avilable start materials
	private float levelSavedTime;	//time record for this level
	private float timeDifference;	//difference between player saved time & target time

    //Cache components
    private TextMesh tMesh;
    private BoxCollider bCollider;
    private Renderer ren;
    private Renderer lsg;

    private void Awake()
    {
        tMesh = label.GetComponent<TextMesh>();
        bCollider = GetComponent<BoxCollider>();
        ren = GetComponent<Renderer>();
        lsg = levelStarsGo.GetComponent<Renderer>();
    }


    void Start (){
		
		if(CareerMapManager.userLevelAdvance >= levelID - 1) {
            //this level is open
            bCollider.enabled = true;

            if(showLevelNameOnButton)
                tMesh.text = levelName;
            else
                tMesh.text = levelID.ToString();

            ren.material.color = new Color(1,1,1,1);

			//grant a few stars
			levelSavedTime = PlayerPrefs.GetFloat("Level-" + levelID.ToString() , careerAvailableTime);
			timeDifference = careerAvailableTime - levelSavedTime;
			if (timeDifference > 60) {
                //3-star
                lsg.material = starMats[3];

			} else if (timeDifference <= 60 && timeDifference > 30) {
                //2-star
                lsg.material = starMats[2];

			} else if (timeDifference <= 30 && timeDifference > 0) {
                //1-star
                lsg.material = starMats[1];

			} else if (timeDifference <= 0) {   //onlu occures if this is the first time we want to play this level
                //0-star
                lsg.material = starMats[0];
			}

			//set heartbeat animation to active, if this is the newest opened level.
			if(CareerMapManager.userLevelAdvance == levelID - 1)
				GetComponent<HeartBeatAnimationEffect>().enabled = true;

		} else {
            //level is locked
            bCollider.enabled = false;
            tMesh.text = "Locked";
            ren.material.color = new Color(1,1,1,0.5f);

            //set 0-star image
            lsg.material = starMats[0];
            lsg.material.color = new Color(1,1,1,0.5f);

			//set heartbeat animation to inactive
			GetComponent<HeartBeatAnimationEffect>().enabled = false;
		}
	}
}