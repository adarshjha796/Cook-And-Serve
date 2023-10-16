using UnityEngine;
using System.Collections;

public class TextMeshController : MonoBehaviour {
	
	//***************************************************************************//
	// This simple class will manage and animate 3dtexts ($Money) which are used when a 
	// customer is happy and is paying the price. 
	//***************************************************************************//

	internal Vector3 startingSize;

	//Do not modify this.
	//this public var is used for other controller to set the amount of money the customer is willing to pay.
	public string myText;


	//***************************************************************************//
	// Simple Init
	//***************************************************************************//
	void Start (){
		//start at the default scale.
		startingSize = transform.localScale;
		
		//update my text with the provided one from other classes.
		//GetComponent<TextMesh>().text = myText;

		//animate the 3d text.
		StartCoroutine(scaleUp());
	}


	//***************************************************************************//
	// Simple animation routine based on scale.
	//***************************************************************************//
	IEnumerator scaleUp (){

		GetComponent<TextMesh>().text = myText;
		while(transform.localScale.x < 2) {
			transform.localScale = new Vector3(transform.localScale.x + 0.045f,
			                                   transform.localScale.y + 0.045f,
			                                   transform.localScale.z);
			transform.position = new Vector3(transform.position.x,
			                                 transform.position.y + 0.025f,
			                                 transform.position.z);
			yield return 0;
		}

		float t = 2;
		while(t > 0) {
			t -= Time.deltaTime;	
			transform.position = new Vector3(transform.position.x,
			                                 transform.position.y + 0.01f,
			                                 transform.position.z);
			/*renderer.material.color = new Color(renderer.material.color.r,
			                                    renderer.material.color.g,
			                                    renderer.material.color.b,
			                                    renderer.material.color.a - 0.02f);*/
			if(t <= 0)
				Destroy(gameObject);
			
			yield return 0;
		}
	}

}