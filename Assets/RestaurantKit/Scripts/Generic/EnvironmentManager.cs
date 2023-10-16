using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour {

    /// <summary>
    /// This class sets a new texture (material) to background object everytime player loads a new level. 
    /// The actual texture index is set via "LevelSelection" scene.
    /// </summary>

    public string envPrefsKey = "levelLocation";
	public Material[] availableEnvironments;
	private int envID; 

	void Awake () {

		envID = PlayerPrefs.GetInt (envPrefsKey, 0);
		GetComponent<Renderer>().material = availableEnvironments[envID];

	}

}
