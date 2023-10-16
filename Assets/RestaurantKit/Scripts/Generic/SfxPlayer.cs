using UnityEngine;
using System.Collections;

public class SfxPlayer : MonoBehaviour {


public AudioClip[] Sfx;

	//***************************************************************************//
	// Play AudioClips
	//***************************************************************************//
	void playSfx ( int index  ){
		GetComponent<AudioSource>().clip = Sfx[index];
		if(!GetComponent<AudioSource>().isPlaying)
			GetComponent<AudioSource>().Play();
	}
}