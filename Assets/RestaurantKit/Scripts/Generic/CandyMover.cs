using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CandyMover : MonoBehaviour {

	//***************************************************************************//
	// This class manages user inputs (drags and touches) on Candy items.
	//***************************************************************************//

	//Private flags.
	private bool canGetDragged;


	//***************************************************************************//
	// Simple Init
	//***************************************************************************//
	void Awake (){
		canGetDragged = true;
	}


	//***************************************************************************//
	// FSM
	//***************************************************************************//
	void Update (){
		//If dragged
		if(Input.GetMouseButton(0) && canGetDragged) {
			followInputPosition();
		}
		
		//if released
		if(!Input.GetMouseButton(0) && Input.touches.Length < 1) {
			canGetDragged = false;
			CandyController.canDeliverCandy = false;
			checkCorrectPlacement();
		}
	}


	//***************************************************************************//
	// Check if this candy is delivered to a customer.
	//***************************************************************************//
	void checkCorrectPlacement (){

		bool delivered = false;
		//if we are giving this candy to a customer (candy is close enough)
		GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("customer");
		//if there is no customer in shop, delete the candy.
		if(availableCustomers.Length < 1) {
			Destroy(gameObject);
			return;
		}
		GameObject theCustomer = null;
		for(int cnt = 0; cnt < availableCustomers.Length; cnt++) {
			if(availableCustomers[cnt].GetComponent<CustomerController>().isCloseEnoughToCandy) {
				//we know that just 1 customer is always nearest to the candy. so "theCustomer" is unique.
				theCustomer = availableCustomers[cnt];
				delivered = true;
			}
		}

		//if customer got the candy..
		if(delivered) {
			//deliver the candy and let the customers know he got a candy.
			theCustomer.GetComponent<CustomerController>().receiveCandy();	
			CandyController.availableCandy--;
		} 
		//destroy the candy gameObject
		Destroy(gameObject);
	}


	//***************************************************************************//
	// Follow players mouse or finger position on screen.
	//***************************************************************************//
	private Vector3 _Pos;
	void followInputPosition (){
		_Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//Custom offset. these objects should be in front of every other GUI instances.
		_Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
		//follow player's finger
		transform.position = _Pos + new Vector3(0, 0, 0);
	}

}