using UnityEngine;
using System.Collections;

public class ShopItemProperties : MonoBehaviour {
	
	///*************************************************************************///
	/// A very simple value holder for different shop items.
	// You can easily add/edit item properties via this controller.
	///*************************************************************************///

	public int itemIndex;
	public int itemPrice;
	public GameObject priceTag;

	void Awake (){
		priceTag.GetComponent<TextMesh>().text = "$" + itemPrice;
	}
}