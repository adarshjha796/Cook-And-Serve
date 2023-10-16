using UnityEngine;
using System.Collections;

public class TextureScroller : MonoBehaviour {

	///***********************************************************************
	/// This script will scroll a texture on it's gameObject.
	/// this is used to simulate pouring coffee into customers cup (side-request)
	///***********************************************************************

	private float offset;
	private float damper = 5.0f;	//scroll speed modifier

	void LateUpdate (){
		offset +=  damper * Time.deltaTime;
		GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", new Vector2(0,offset));
	}
}