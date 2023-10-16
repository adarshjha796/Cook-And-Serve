using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class PlateController : MonoBehaviour
{

    //***************************************************************************//
    // This calss will manages all things related to the deliveryPlate, including
    // handing it over to the customers, trashing it, drag and release events, etc...
    //***************************************************************************//

    //Static variable to let customers know they can receive their orders
    internal bool canDeliverOrder;
    internal bool isReadyToServe;   //if we are done and have picked up the final product to deliver it to customers

    //Private flags
    private Vector3 initialPosition;
    private GameObject trashbin;

    //AudioClip
    public AudioClip trashSfx;

    //money fx
    public GameObject money3dText;  //3d text mesh

    //delivery arrays
    internal bool deliveryQueueIsFull;                                      //delivery queue can accept 6 ingredients. more is not acceptable.
    internal int deliveryQueueItems;                                        //number of items in delivery queue
    internal List<int> deliveryQueueItemsContent = new List<int>();         //conents of delivery queue

    //component cache
    private Renderer r;


    //***************************************************************************//
    // Simple Init
    //***************************************************************************//
    void Awake()
    {
        canDeliverOrder = true;
        isReadyToServe = false;

        initialPosition = transform.position;
        trashbin = GameObject.FindGameObjectWithTag("trashbin");

        //clear & reset the delivery arrays
        deliveryQueueIsFull = false;
        deliveryQueueItems = 0;
        deliveryQueueItemsContent.Clear();

        r = GetComponent<Renderer>();
    }


    //***************************************************************************//
    // FSM
    //***************************************************************************//
    void Update()
    {

        //no more ingredient can be picked
        if (deliveryQueueItems >= MainGameController.maxSlotState)
            deliveryQueueIsFull = true;
        else
            deliveryQueueIsFull = false;

        manageDeliveryDrag();
    }


    //***************************************************************************//
    // If we are starting our drag on deliveryPlate, move the plate with our touch/mouse...
    //***************************************************************************//
    private RaycastHit hitInfo;
    private Ray ray;
    void manageDeliveryDrag()
    {
        //Mouse of touch?
        if (Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Moved)
            ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
        else if (Input.GetMouseButtonDown(0))
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        else
            return;

        if (Physics.Raycast(ray, out hitInfo))
        {
            GameObject objectHit = hitInfo.transform.gameObject;
            if (objectHit.tag == "serverPlate" && objectHit.name == gameObject.name && !IngredientsController.itemIsInHand)
            {
                StartCoroutine(createDeliveryPackage());
            }
        }
    }


    //***************************************************************************//
    // Move the plate
    //***************************************************************************//
    private Vector3 _Pos;
    IEnumerator createDeliveryPackage()
    {
        while (canDeliverOrder && deliveryQueueItems > 0)
        {
            //follow mouse or touch
            _Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
            //follow player's finger
            transform.position = _Pos + new Vector3(0, 0, 0);

            //while we are dragging the product, this var will be true. but as soon as we released the drag, it will be false.
            isReadyToServe = true;

            //better to be transparent, when dragged
            r.material.color = new Color(r.material.color.r,
                                        r.material.color.g,
                                        r.material.color.b,
                                        0.5f);

            //deliver (dragging the plate) is not possible when user is not touching screen
            //so we must decide what we are going to do after dragging and releasing the plate
            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {

                //we no longer have control over the product
                isReadyToServe = false;

                //if we are giving the order to a customer (plate is close enough)
                GameObject[] availableCustomers = GameObject.FindGameObjectsWithTag("customer");

                //if there is no customer in shop, take the plate back.
                if (availableCustomers.Length < 1)
                {
                    //take the plate back to it's initial position
                    resetPosition();
                    yield break;
                }

                bool delivered = false;
                GameObject theCustomer = null;
                for (int cnt = 0; cnt < availableCustomers.Length; cnt++)
                {
                    if (availableCustomers[cnt].GetComponent<CustomerController>().isCloseEnoughToDelivery)
                    {
                        //we know that just 1 customer is always nearest to the delivery. so "theCustomer" is unique.
                        theCustomer = availableCustomers[cnt];
                        delivered = true;
                    }
                }

                //if customer got the delivery
                if (delivered)
                {
                    //deliver the order
                    //deliveredProduct= new Array();
                    List<int> deliveredProduct = new List<int>();

                    //contents of the delivery which customer got from us.
                    deliveredProduct = deliveryQueueItemsContent;

                    //debug delivery
                    for (int i = 0; i < deliveryQueueItemsContent.Count; i++)
                    {
                        print("Delivery Items ID " + i.ToString() + " = " + deliveryQueueItemsContent[i]);
                    }

                    //let the customers know what he got.
                    theCustomer.GetComponent<CustomerController>().receiveOrder(deliveredProduct);

                    //reset main queue
                    deliveryQueueItems = 0;
                    deliveryQueueIsFull = false;
                    deliveryQueueItemsContent.Clear();


                    //destroy the contents of the serving plate.
                    GameObject[] DeliveryQueueItems = GameObject.FindGameObjectsWithTag("deliveryQueueItem");
                    foreach (GameObject item in DeliveryQueueItems)
                    {
                        if (item.transform.parent.gameObject == gameObject)
                            Destroy(item);
                    }

                    //take the plate back to it's initial position
                    resetPosition();

                }
                else
                {
                    resetPosition();
                }
            }

            yield return 0;
        }
    }


    //***************************************************************************//
    // Move the plate to it's initial position.
    // we also check if user wants to trash his delivery, before any other process.
    // this way we can be sure that nothing will interfere with deleting the delivery. (prevents many bugs)
    //***************************************************************************//
    void resetPosition()
    {
        //just incase user wants to move this to trashbin, check it here first
        if (trashbin.GetComponent<TrashBinController>().isCloseEnoughToTrashbin)
        {

            //empty plate contents
            playSfx(trashSfx);

            //New v1.7.2 - trash loss
            MainGameController.totalMoneyMade -= MainGameController.globalTrashLoss;
            GameObject money3d = Instantiate(money3dText,
                                            trashbin.transform.position + new Vector3(0, 0, -0.8f),
                                            Quaternion.Euler(0, 0, 0)) as GameObject;
            money3d.GetComponent<TextMeshController>().myText = "- $" + MainGameController.globalTrashLoss.ToString();


            deliveryQueueItems = 0;
            deliveryQueueIsFull = false;
            deliveryQueueItemsContent.Clear();
            GameObject[] DeliveryQueueItems = GameObject.FindGameObjectsWithTag("deliveryQueueItem");
            foreach (GameObject item in DeliveryQueueItems)
                if (item.transform.parent.gameObject == gameObject)
                    Destroy(item);
        }

        //take the plate back to it's initial position
        print("Back to where we belong");
        transform.position = initialPosition;
        r.material.color = new Color(r.material.color.r,
                                    r.material.color.g,
                                    r.material.color.b,
                                    1);
        canDeliverOrder = false;
        StartCoroutine(reactivate());
    }


    //***************************************************************************//
    // make the deliveryPlate draggable again.
    //***************************************************************************//
    IEnumerator reactivate()
    {
        yield return new WaitForSeconds(0.25f);
        canDeliverOrder = true;
    }


    //***************************************************************************//
    // Play AudioClips
    //***************************************************************************//
    void playSfx(AudioClip _sfx)
    {
        GetComponent<AudioSource>().clip = _sfx;
        if (!GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Play();
    }

}