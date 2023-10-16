using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerController : MonoBehaviour
{

    //***************************************************************************//
    // This class manages all thing related to a customer, including
    // wishlist, wishlist ingredients, patience and animations.
    //***************************************************************************//
    //Modifiable variables (Only Through Inspector - Do not hardcode!)
    public float customerPatience = 30.0f;              //seconds (default = 30 or whatever you wish)
    public int customerNeeds;                           //Product ID which the customer wants. if left to 0, it randomly chooses a product.
                                                        //set anything but 0 to override it.
                                                        //max limit is the length of the availableProducts array.
    public float customerMoveSpeed = 3f;
    public float askingSideRequestChance = 0.35f;
    public float sideReqWithNoMainOrderChance = 0.75f;

    public GameObject positionDummy;                    //dummy to indicate where items should be displayed over customer's head
    public bool showProductIngredientHelpers = true;    //whether to show customer wish's ingredeints as a helper or not (player has to guess)

    // Audio Clips
    public AudioClip orderIsOkSfx;
    public AudioClip orderIsNotOkSfx;
    public AudioClip receivedSomethingGoodSfx;

    //*** Customer Moods ***//
    //We use different materials for each mood
    /* Currently we have 4 moods: 
	 [0]Defalut
	 [1]Bored 
	 [2]Satisfied
	 [3]Angry
	 we change to appropriate material whenever needed. 
	*/
    public Material[] customerMoods;
    private int moodIndex;

    //**** Special Variables ******//
    // Warning!!! - Do not Modify //
    public int mySeat;                          //Do not modify this.
    public Vector3 destination;
    private GameObject gameController;          //Do not modify this.
                                                //private GameObject deliveryPlate; 			//Do not modify this.
    public bool isCloseEnoughToDelivery;        //Do not modify this.
    public bool isCloseEnoughToCandy;           //Do not modify this. 
    public bool isCloseEnoughToSideRequest;     //Do not modify this.
                                                //****************************//

    //Protected variables. do not edit.
    public GameObject[] availableProducts;      //list of all available product to choose from
    public GameObject[] availableIngredients;   //List of all available ingredients to cook above products
    public GameObject[] helperIngredientDummies;//Position dummies for positioning the helper images for ingredients
    public GameObject[] availableSideReqs;      //List of all available side-request items

    //Private customer variables
    private string customerName;                //random name
    private int productIngredients;             //ingredients of the choosen product
    private int[] productIngredientsIDs;        //IDs of the above ingredients
    private float currentCustomerPatience;      //current patience of the customer
    private bool isOnSeat;                      //is customer on his seat?
    private int customerSideReq;                //-1 means no side-request. anything but -1 points to a side request.
    private bool sideRequestIsFulfilled;        //flag to let us know if we have delivered the side-request
    private bool mainOrderIsFulfilled;          //flag to let us know if we have delivered the main order
    private bool isOnlyAskingForDrink;          //flag to let us know if this customer only wants a drink (side-req) and no main food

    //Patience bar GUI items and vars
    public GameObject patienceBarFG;
    public GameObject patienceBarBG;
    private bool patienceBarSliderFlag;
    internal float leaveTime;
    private float creationTime;
    private bool isLeaving;
    public GameObject requestBubble;            //reference to (gameObject) bubble over customers head
    public GameObject money3dText;              //3d text mesh over customers head after successful delivery

    //Transforms
    private Vector3 startingPosition;           //reference

    //all about more than 1 plate goes here:
    private GameObject[] serverPlates;          //all available plates inside the game
    private float[] distanceToPlates;           //distance to all available plates
    private GameObject target;                  //which plate is nearest? that will be the main target


    void Awake()
    {

        target = null;
        serverPlates = GameObject.FindGameObjectsWithTag("serverPlate");
        distanceToPlates = new float[serverPlates.Length];

        requestBubble.SetActive(false);
        patienceBarFG.SetActive(false);
        patienceBarBG.SetActive(false);

        customerSideReq = -1;
        isCloseEnoughToDelivery = false;
        isCloseEnoughToCandy = false;
        isCloseEnoughToSideRequest = false;

        sideRequestIsFulfilled = false;
        mainOrderIsFulfilled = false;
        isOnlyAskingForDrink = false;

        isOnSeat = false;
        currentCustomerPatience = customerPatience;
        moodIndex = 0;
        leaveTime = 0;
        isLeaving = false;
        creationTime = Time.time;
        startingPosition = transform.position;
        gameController = GameObject.FindGameObjectWithTag("GameController");
        //deliveryPlate = GameObject.FindGameObjectWithTag("serverPlate");

        Init();
        StartCoroutine(goToSeat());
    }


    //***************************************************************************//
    // Here we will initialize all customer related variables.
    //***************************************************************************//
    private GameObject productImage;
    private GameObject[] helperIngredients;
    private GameObject sideReq;
    void Init()
    {
        //we give this customer a nice name
        customerName = "Customer_" + Random.Range(100, 10000);
        gameObject.name = customerName;

        //choose a product for this customer
        if (customerNeeds == 0)
        {
            //for freeplay mode, customers can choose any product. But in career mode,
            //they have to choose from products we allowed in CareerLevelSetup class.
            //if (PlayerPrefs.GetString("gameMode") == "CAREER")
            //{
            //    int totalAvailableProducts = PlayerPrefs.GetInt("availableProducts");
            //    customerNeeds = PlayerPrefs.GetInt("careerProduct_" + Random.Range(0, totalAvailableProducts).ToString());
            //    customerNeeds -= 1; //Important. We count the indexes from zero, while selecting the products from 1.
            //                        //so we subtract a unit from customerNeeds to be equal to main AvailableProducts array.
            //}

            //It is on the basis or ingredients/layer of Ingredients order complexicity
            //else
            //{
                if(MainGameController.gameTime < 135)
                {
                    customerNeeds = Random.Range(0, availableProducts.Length - 12);
                }
                else if (MainGameController.gameTime > 135 && MainGameController.gameTime < 255)
                {
                    customerNeeds = Random.Range(2, availableProducts.Length - 6);
                }
                else if (MainGameController.gameTime > 255 && MainGameController.gameTime < 495)
                {
                    customerNeeds = Random.Range(8, availableProducts.Length);
                }
                else
                {
                    customerNeeds = Random.Range(0, availableProducts.Length);
                }
           //}
        }

        //also let's give this customer a chance to wish a side-request.
        if (Random.value > askingSideRequestChance)
            customerSideReq = Random.Range(0, availableSideReqs.Length);
        else
            customerSideReq = -1;

        //Update 1.7.4
        //give the customers a chance to ask for no main food, and just ask for a drink (side-req)
        //with 25% chance, we create a customer that only wants a drink.
        if (Random.value > sideReqWithNoMainOrderChance) //sideReqWithNoMainOrderChance
        {
            isOnlyAskingForDrink = true;    //only wants a drink
            mainOrderIsFulfilled = true;    //we set this flag to true instantly, as this customer does not have any main order.
            customerSideReq = Random.Range(0, availableSideReqs.Length);    //we force him to order a side-req!
            print(customerName + " only wants a drink! (no main order)");
        }

        //debug customer's wish	
        //if (!isOnlyAskingForDrink)
        //{
        //    print(customerName + " would like a " + availableProducts[customerNeeds].name + " and can wait for " + customerPatience + " seconds");
        //    if (customerSideReq != -1)
        //        print(customerName + " also likes a " + availableSideReqs[customerSideReq].name);
        //}

        //get and show product's image
        productImage = Instantiate(availableProducts[customerNeeds], positionDummy.transform.position, Quaternion.Euler(90, 180, 0)) as GameObject;
        productImage.name = "customerNeeds";
        productImage.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        productImage.transform.parent = requestBubble.transform;

        //if customer wants a side-request, show it
        if (customerSideReq != -1)
        {
            sideReq = Instantiate(availableSideReqs[customerSideReq], positionDummy.transform.position + new Vector3(0, -0.65f, 0), Quaternion.Euler(0, 180, 0)) as GameObject;
            //move the main order a little up! to make some space for the side-request
            productImage.transform.position = new Vector3(productImage.transform.position.x,
                                                          productImage.transform.position.y + 0.4f,
                                                          productImage.transform.position.z);
            sideReq.transform.localScale = new Vector3(1.3f, 1.3f, 0.005f);
            sideReq.transform.parent = requestBubble.transform;

            //if this side-req item needs to be processed, show it (ask it) with processed image :)
            if (sideReq.GetComponent<SideRequestMover>().beforeAfterMat.Length > 1)
                sideReq.GetComponent<SideRequestMover>().setMaterial(sideReq.GetComponent<SideRequestMover>().beforeAfterMat[1]);

            sideReq.GetComponent<SideRequestMover>().enabled = false;
        }

        //we need to hide the image of main order, if this customer only wants a side-req (drink)
        if (isOnlyAskingForDrink)
        {
            sideReq.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f); // It was .6f all earlier.
            sideReq.transform.position = positionDummy.transform.position;
            productImage.SetActive(false);
        }

        //get product ingredients
        productIngredients = availableProducts[customerNeeds].GetComponent<ProductManager>().totalIngredients;
        //print(availableProducts[customerNeeds].name + " has " + productIngredients + " ingredients.");
        productIngredientsIDs = new int[productIngredients];
        for (int i = 0; i < productIngredients; i++)
        {
            productIngredientsIDs[i] = availableProducts[customerNeeds].GetComponent<ProductManager>().ingredientsIDs[i];
            //print(availableProducts[customerNeeds].name + " ingredients ID[" + i + "] is: " + productIngredientsIDs[i]);
        }

        //show ingredients in the bubble if needed by developer (can make the game easy or hard)
        helperIngredients = new GameObject[productIngredients];
        for (int j = 0; j < productIngredients; j++)
        {
            int ingredientID = productIngredientsIDs[j] - 1; // Array always starts from 0, while we named our ingredients from 1.
            helperIngredients[j] = Instantiate(availableIngredients[ingredientID], helperIngredientDummies[j].transform.position, Quaternion.Euler(90, 180, 0)) as GameObject;
            helperIngredients[j].tag = "ingIcon";
            helperIngredients[j].name = "helperIngredient #" + j;
            helperIngredients[j].GetComponent<ProductMover>().enabled = false;
            helperIngredients[j].transform.parent = requestBubble.transform;

            if (!showProductIngredientHelpers)
            {
                helperIngredients[j].GetComponent<Renderer>().enabled = false;
            }
        }
    }


    //***************************************************************************//
    // After this customer has been instantiated by MainGameController,
    // it starts somewhere outside game scene and then go to it's position (seat)
    // with a nice animation. Then asks for it's order.
    //***************************************************************************//
    private float timeVariance;
    IEnumerator goToSeat()
    {
        timeVariance = Random.value;
        while (!isOnSeat)
        {
            transform.position = new Vector3(transform.position.x + (Time.deltaTime * customerMoveSpeed),
                                             startingPosition.y - 0.25f + (Mathf.Sin((Time.time + timeVariance) * 10) / 8),
                                             transform.position.z);

            if (transform.position.x >= destination.x)
            {
                isOnSeat = true;
                patienceBarSliderFlag = true; //start the patience bar
                requestBubble.SetActive(true);
                patienceBarFG.SetActive(true);
                patienceBarBG.SetActive(true);
                yield break;
            }
            yield return 0;
        }
    }


    ///**********************************************************
    /// Takes an array and return the lowest number in it, 
    /// with the optional index of this lowest number (position of it in the array)
    ///**********************************************************
    Vector2 findMinInArray(float[] _array)
    {
        int lowestIndex = -1;
        float minval = 1000000.0f;
        for (int i = 0; i < _array.Length; i++)
        {
            if (_array[i] < minval)
            {
                minval = _array[i];
                lowestIndex = i;
            }
        }
        //return the Vector2(minimum population, index of this minVal in the argument Array)
        return (new Vector2(minval, lowestIndex));
    }


    //***************************************************************************//
    // FSM
    //***************************************************************************//
    void Update()
    {
        for (int i = 0; i < serverPlates.Length; i++)
        {
            distanceToPlates[i] = Vector3.Distance(serverPlates[i].transform.position, gameObject.transform.position);
            //find the correct (nearest plate) target
            target = serverPlates[(int)findMinInArray(distanceToPlates).y];
            //print (gameObject.name + " target plate is: " + target.name);
        }

        if (patienceBarSliderFlag)
            StartCoroutine(patienceBar());

        //Manage customer's mood by changing it's material
        updateCustomerMood();
    }


    //***************************************************************************//
    // We need these events to be checked at the eend of every frame.
    //***************************************************************************//
    void LateUpdate()
    {

        //check if this customer is close enough to delivery, in order to receive it.
        if (target)
        {
            if (target.GetComponent<PlateController>().isReadyToServe)
                checkDistanceToDelivery();
        }

        //check if this customer is close enough to delivery, in order to receive it.
        //if(PlateController.canDeliverOrder && gameObject.tag == "customer") {
        //	checkDistanceToDelivery();
        //}

        //check if this customer is close enough to a candy, in order to receive it.
        if (CandyController.canDeliverCandy && gameObject.tag == "customer")
        {
            checkDistanceToCandy();
        }

        //check if this customer is close enough to a side-request delivery, in order to receive it.
        //we must also check if this customer wants a side-request or not!
        if (SideRequestsController.canDeliverSideRequest && customerSideReq != -1 && gameObject.tag == "customer")
        {
            checkDistanceToSideRequest();
        }

        //check the status of customers with both main order and side-request, and look if they already received both their orders.
        if (sideRequestIsFulfilled && mainOrderIsFulfilled && !isLeaving)
        {
            settle();
        }
    }


    //***************************************************************************//
    // make the customer react to events by changing it's material (and texture)
    //***************************************************************************//
    void updateCustomerMood()
    {
        //if customer has waited for half of his/her patience, make him/her bored.
        if (!isLeaving)
        {
            if (currentCustomerPatience <= customerPatience / 2)
                moodIndex = 1;
            else
                moodIndex = 0;
        }

        GetComponent<Renderer>().material = customerMoods[moodIndex];
    }


    //***************************************************************************//
    // fill customer's patience bar and make it full again.
    //***************************************************************************//
    void fillCustomerPatience()
    {
        currentCustomerPatience = customerPatience;
        patienceBarFG.transform.localScale = new Vector3(1,
                                                         patienceBarFG.transform.localScale.y,
                                                         patienceBarFG.transform.localScale.z);
        patienceBarFG.transform.localPosition = new Vector3(0,
                                                            patienceBarFG.transform.localPosition.y,
                                                            patienceBarFG.transform.localPosition.z);
    }


    //***************************************************************************//
    // check distance to delivery
    // we check if player is dragging the order towards the customer or towards it's
    // requestBubble. the if the order was close enough to any of these objects,
    // we se the deliver flag to true.
    //***************************************************************************//
    private float distanceToDelivery;
    private float frameDistanceToDelivery;
    void checkDistanceToDelivery()
    {
        distanceToDelivery = Vector3.Distance(transform.position, target.transform.position);
        frameDistanceToDelivery = Vector3.Distance(requestBubble.transform.position, target.transform.position);
        //print(gameObject.name + " distance to candy is: " + distanceToDelivery + ".");

        //Hardcoded integer for distance.
        if (distanceToDelivery < 1.0f || frameDistanceToDelivery < 1.0f)
        {
            isCloseEnoughToDelivery = true;
            //tint color
            GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            isCloseEnoughToDelivery = false;
            //reset tint color
            GetComponent<Renderer>().material.color = new Color(1, 1, 1);
        }
    }


    //***************************************************************************//
    // check distance to Candy
    //***************************************************************************//
    private float distanceToCandy;
    private GameObject deliveryCandy;
    void checkDistanceToCandy()
    {
        //first find the deliveryCandy GameObject
        if (!deliveryCandy)
        { //do we already found a candy?
            deliveryCandy = GameObject.FindGameObjectWithTag("deliveryCandy");
        }

        distanceToCandy = Vector3.Distance(transform.position, deliveryCandy.transform.position);
        //print(gameObject.name + " distance to candy is: " + distanceToCandy + ".");

        //Hardcoded integer for distance.
        if (distanceToCandy < 2.0f)
        {
            isCloseEnoughToCandy = true;
            //tint color
            GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            isCloseEnoughToCandy = false;
            //reset tint color
            GetComponent<Renderer>().material.color = new Color(1, 1, 1);
        }
    }


    //***************************************************************************//
    // check distance to side-request
    //***************************************************************************//
    private float distanceToSideRequest;
    private float frameDistanceToSideRequest;
    private GameObject deliverySideRequest;
    void checkDistanceToSideRequest()
    {
        //first find the delivery SideRequest GameObject
        if (!deliverySideRequest)
        { //do we already found a candy?
            deliverySideRequest = GameObject.FindGameObjectWithTag("deliverySideRequest");
        }

        distanceToSideRequest = Vector3.Distance(transform.position, deliverySideRequest.transform.position);
        frameDistanceToSideRequest = Vector3.Distance(requestBubble.transform.position, deliverySideRequest.transform.position);
        //print(gameObject.name + " distance to SideRequest is: " + distanceToSideRequest + ".");

        //Hardcoded integer for distance.
        if (distanceToSideRequest < 2.0f || frameDistanceToSideRequest < 2.0f)
        {
            isCloseEnoughToSideRequest = true;
            //tint color
            GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
        }
        else
        {
            isCloseEnoughToSideRequest = false;
            //reset tint color
            GetComponent<Renderer>().material.color = new Color(1, 1, 1);
        }
    }


    //***************************************************************************//
    // show and animate progress bar based on customer's patience
    //***************************************************************************//
    IEnumerator patienceBar()
    {
        patienceBarSliderFlag = false;
        while (currentCustomerPatience > 0)
        {
            currentCustomerPatience -= Time.deltaTime * Application.targetFrameRate * 0.02f;
            patienceBarFG.transform.localScale = new Vector3(patienceBarFG.transform.localScale.x - ((0.02f / customerPatience) * Time.deltaTime * Application.targetFrameRate),
                                                             patienceBarFG.transform.localScale.y,
                                                             patienceBarFG.transform.localScale.z);
            patienceBarFG.transform.position = new Vector3(patienceBarFG.transform.position.x,
                                                           patienceBarFG.transform.position.y - ((0.021f / customerPatience) * Time.deltaTime * Application.targetFrameRate),
                                                           patienceBarFG.transform.position.z);
            yield return 0;
        }
        if (currentCustomerPatience <= 0)
        {
            patienceBarFG.GetComponent<Renderer>().enabled = false;
            //customer is angry and will leave with no food received.
            StartCoroutine(leave());
        }

    }


    //***************************************************************************//
    // receive and check order contents
    //***************************************************************************//
    public void receiveOrder(List<int> _getOrder)
    {
        //print("Order received. contents are: " + _getOrder);

        //check the received order with the original one (customer's wish).
        int[] myOriginalOrder = productIngredientsIDs;
        List<int> myReceivedOrder = _getOrder;

        //check if the two array are the same, meaning that we received what we were looking for.
        //print(myOriginalOrder + " - " + myReceivedOrder);

        //1.check the length of two arrays
        if (myOriginalOrder.Length == myReceivedOrder.Count)
        {
            //2.compare two arrays
            bool detectInequality = false;
            for (int i = 0; i < myOriginalOrder.Length; i++)
            {
                if (myOriginalOrder[i] != myReceivedOrder[i])
                {
                    detectInequality = true;
                }
            }

            if (!detectInequality)
                orderIsCorrect();
            else
                OrderIsIncorrect(); //different array items

        }
        else    //different array length
            OrderIsIncorrect();
    }


    //***************************************************************************//
    // if order is delivered correctly
    //***************************************************************************//
    void orderIsCorrect()
    {
        print("Order is correct.");

        //if the customer just asked for an order with NO sideRequest, we make him settle and leave instantly.
        if (customerSideReq == -1)
        {
            settle();
        }
        else
        { //But if it also wants a side-request, we enter another loop to check if the second wish is fulfilled or not.
          //first hide the main order
            productImage.GetComponent<Renderer>().enabled = false;
            foreach (var helper in helperIngredients)
                helper.GetComponent<Renderer>().enabled = false;
            //set the flag
            mainOrderIsFulfilled = true;
            playSfx(receivedSomethingGoodSfx);
        }
    }


    //***************************************************************************//
    // if order is NOT delivered correctly
    //***************************************************************************//
    void OrderIsIncorrect()
    {
        print("Order is not correct.");
        moodIndex = 3;
        playSfx(orderIsNotOkSfx);
        StartCoroutine(leave());
    }


    //***************************************************************************//
    // receive a candy to refill the patience bar :)
    //***************************************************************************//
    public void receiveCandy()
    {
        //Candy Received!
        print(gameObject.name + " received a candy!");
        playSfx(receivedSomethingGoodSfx);
        //fill customer's patient
        fillCustomerPatience();
    }


    //***************************************************************************//
    // receive a side-request
    //***************************************************************************//
    public void receiveSideRequest(int _ID)
    {
        //side-request Received!
        print(gameObject.name + " received a side-request, ID: " + (_ID - 1) + " - Original request ID was: " + customerSideReq);
        //check if this is what we want
        if (customerSideReq == _ID - 1)
        { //we subtract 1 from _ID, because we start side-request items from 0, while Ids starts from 1.
          //we received the right side-request, so the side-req is fulfilled. But we should wait for the main order status.
          //first hide the side-request
            sideReq.GetComponent<Renderer>().enabled = false;
            playSfx(receivedSomethingGoodSfx);
            sideRequestIsFulfilled = true;
        }
        else
        {
            //we received the wrong side-request
            OrderIsIncorrect();
        }
    }


    //***************************************************************************//
    // Customer should pay and leave the restaurant.
    //***************************************************************************//
    void settle()
    {
        moodIndex = 2;  //make him/her happy :)

        //give cash, money, bonus, etc, here.
        float leaveTime = Time.time;
        int remainedPatienceBonus = (int)Mathf.Round(customerPatience - (leaveTime - creationTime));

        //if we have purchased additional items for our restaurant, we should receive more tips
        int tips = 0;
        if (PlayerPrefs.GetInt("shopItem-1") == 1) tips += 2;   //if we have seats
        if (PlayerPrefs.GetInt("shopItem-2") == 1) tips += 3;   //if we have music player
        if (PlayerPrefs.GetInt("shopItem-3") == 1) tips += 5;   //if we have flowers

        //update 1.8.3 (2020)
        //to calculate the final money, first check if this order contains a sideRequest or maybe only consists of a sideRequest.
        //as we need to calculate the price of the sideReq items as well.
        int sideReqPrice = 0;
        int mainOrderPrice = 0;

        //if customer is only asking for a drink with no main order
        if (isOnlyAskingForDrink)
        {
            mainOrderPrice = 0;
            sideReqPrice = availableSideReqs[customerSideReq].GetComponent<SideRequestMover>().sideReqPrice;

        } else
        {
            //calculate the price of main order
            mainOrderPrice = availableProducts[customerNeeds].GetComponent<ProductManager>().price;

            //if customer also wants a sideRed item, calculate it as well
            if (customerSideReq != -1)
                sideReqPrice = availableSideReqs[customerSideReq].GetComponent<SideRequestMover>().sideReqPrice;
        }

        //debug
        print("mainOrderPrice: " + mainOrderPrice + " // sideReqPrice: " + sideReqPrice);
            
        //Prior to update 1.8.3
        /*int finalMoney = availableProducts[customerNeeds].GetComponent<ProductManager>().price +
                            remainedPatienceBonus +
                            tips;*/

        int finalMoney =    mainOrderPrice +
                            sideReqPrice +
                            remainedPatienceBonus +
                            tips;

        MainGameController.totalMoneyMade += finalMoney;
        GameObject money3d = Instantiate(money3dText,
                                            transform.position + new Vector3(0, 0, -0.8f),
                                            Quaternion.Euler(0, 0, 0)) as GameObject;
        print("FinalMoney: " + finalMoney);
        money3d.GetComponent<TextMeshController>().myText = "$" + finalMoney.ToString();

        playSfx(orderIsOkSfx);
        StartCoroutine(leave());
    }


    //***************************************************************************//
    // Leave routine with get/set ers and animations.
    //***************************************************************************//
    public IEnumerator leave()
    {

        //prevent double animation
        if (isLeaving)
            yield break;

        //set the leave flag to prevent multiple calls to this function
        isLeaving = true;

        //we need to change the tag of this customer, in order to not be able to receive new deliveries.
        gameObject.tag = "Untagged";

        //animate (close) patienceBar
        StartCoroutine(animate(Time.time, patienceBarBG, 0.7f, 0.8f));
        yield return new WaitForSeconds(0.3f);

        //animate (close) request bubble
        StartCoroutine(animate(Time.time, requestBubble, 0.75f, 0.95f));
        yield return new WaitForSeconds(0.4f);

        //animate mainObject (hide it, then destroy it)
        while (transform.position.x < 10)
        {
            transform.position = new Vector3(transform.position.x + (Time.deltaTime * customerMoveSpeed),
                                             startingPosition.y - 0.25f + (Mathf.Sin(Time.time * 10) / 8),
                                             transform.position.z);

            if (transform.position.x >= 10)
            {
                gameController.GetComponent<MainGameController>().availableSeatForCustomers[mySeat] = true;
                Destroy(gameObject);
                yield break;
            }
            yield return 0;
        }
    }


    //***************************************************************************//
    // animate customer.
    //***************************************************************************//
    IEnumerator animate(float _time, GameObject _go, float _in, float _out)
    {
        float t = 0.0f;
        while (t <= 1.0f)
        {
            t += Time.deltaTime * 10;
            _go.transform.localScale = new Vector3(Mathf.SmoothStep(_in, _out, t),
                                                   _go.transform.localScale.y,
                                                   _go.transform.localScale.z);
            yield return 0;
        }
        float r = 0.0f;
        if (_go.transform.localScale.x >= _out)
        {
            while (r <= 1.0f)
            {
                r += Time.deltaTime * 2;
                _go.transform.localScale = new Vector3(Mathf.SmoothStep(_out, 0.01f, r),
                                                       _go.transform.localScale.y,
                                                       _go.transform.localScale.z);
                if (_go.transform.localScale.x <= 0.01f)
                    _go.SetActive(false);
                yield return 0;
            }
        }
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