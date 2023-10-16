using UnityEngine;
using System.Collections;

public class CoinpackProperties : MonoBehaviour {
	
	///*************************************************************************///
	/// A very simple value holder for different coinpack items.
	// You can easily add/edit properties via this controller.
	///*************************************************************************///

	public int itemIndex;
	public float itemPrice;
	public int itemValue;

	//GameObjects
	public GameObject nameGo;
	public GameObject priceGo;

	void Start (){
		nameGo.GetComponent<TextMesh>().text = itemValue + " Coins";
		priceGo.GetComponent<TextMesh>().text = "Buy for $" + itemPrice;
	}

}