using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

public class ProductMover : MonoBehaviour
{

    //***************************************************************************//
    // This class manages user inputs (drags and touches) on ingredients panel,
    // and update main queue array accordingly.
    //***************************************************************************//

    //public variable.
    //do not edit these vars.
    //we get their values from other classes.
    public int factoryID;               //actual ingredient ID to be served to customer
    public bool needsProcess;           //does this ingredient should be processed before delivering to customer?

    ///only required for ingredients that needs process!
    /// ************************************************
    private bool processFlag;           //will be set to true the first time this ingredient enters a processor
    private bool isProcessed;           //process has been finished
    private bool isOverburned;          //burger stayed for too long on the grill
    private bool isProcessing;          //process is being done
    public string processorTag;         //tag of the processor game object
    public Material[] beforeAfterMat;   //index[0] = raw    
                                        //index[1] = processed
                                        //index[2] = overburned
                                        /// *************************************************

    //Private flags.
    [ReadOnly]
    public GameObject target;               //target object for this ingredient 
                                            //(deliveryPlate, a processor machine, etc...)
    private bool canGetDragged;             //are we allowed to drag this plate to customer?
    private GameObject[] serverPlates;      //server plate game objects (there can be more than 1 serverplate)
    private float[] distanceToPlates;       //distance to all available serverPlates
    private GameObject grill;               //grill machine to process burgers
    private GameObject trashBin;            //reference to trashBin object
    private bool isFinished;                //are we done with positioning and processing the ingredient on the plate?
    private float minDeliveryDistance = 1f; //Minimum distance to deliveryPlate required to land this ingredint on plate.
    private Vector3 tmpPos;                 //temp variable for storing player input position on screen
    private float itemsDistanceOnPlate = 0.1f;     //the distance between ingredients on the plate

    //player input variables
    private RaycastHit hitInfo;
    private Ray ray;

    //money fx
    public GameObject money3dText;  //3d text mesh

    //***************************************************************************//
    // Simple Init
    //***************************************************************************//
    void Start()
    {
        canGetDragged = true;
        isFinished = false;         //!Important : we use this flag to prevent ingredients to be draggable after placed on the plate.
        isProcessed = false;
        isProcessing = false;
        isOverburned = false;
        processFlag = false;

        //find possible targets
        serverPlates = GameObject.FindGameObjectsWithTag("serverPlate");
        grill = GameObject.FindGameObjectWithTag("grill");
        trashBin = GameObject.FindGameObjectWithTag("trashbin");

        distanceToPlates = new float[serverPlates.Length];

        if (needsProcess)
            target = grill;
        else
            target = serverPlates[0];   //Temporary - we need to edit this in update loop to set correct serverplate for each ingredient!

        //print (gameObject.name + " - " + target.name);
        //print ("serverPlates.length: " + serverPlates.Length);
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

        if (needsProcess)
            target = grill;

        else if(!needsProcess && isOverburned)
        {
            target = trashBin;

        } else
        {
            for (int i = 0; i < serverPlates.Length; i++)
            {
                distanceToPlates[i] = Vector3.Distance(serverPlates[i].transform.position, gameObject.transform.position);
                //find the correct (nearest plate) target
                target = serverPlates[(int)findMinInArray(distanceToPlates).y];
                //debug
                //print ("Distance to serverPlate-" + i.ToString() + " : " + distanceToPlates[i]);
                //print ("Target is: " + target.name);
            }
        }

        //if dragged
        if (Input.GetMouseButton(0) && canGetDragged)
        {
            followInputPosition();
        }

        //if released and doesn't need process
        if ((!Input.GetMouseButton(0) && Input.touches.Length < 1) && !isFinished && !needsProcess)
        {
            canGetDragged = false;
            checkCorrectPlacement();
        }

        //if released and needs process
        if ((!Input.GetMouseButton(0) && Input.touches.Length < 1) && !isFinished && needsProcess)
        {
            canGetDragged = false;
            checkCorrectPlacementOnProcessor();
        }

        //if needs process and process is finished successfully
        if (/*needsProcess &&*/ isProcessed && !isOverburned && !target.GetComponent<PlateController>().deliveryQueueIsFull && !IngredientsController.itemIsInHand)
        {
            manageSecondDrag();
        }

        //if needs process and process took too long and burger is overburned and must be discarded
        if (/*needsProcess &&*/ isProcessed && isOverburned && /*!target.GetComponent<PlateController>().deliveryQueueIsFull &&*/ !IngredientsController.itemIsInHand)
        {
            manageDiscard();
        }

        //you can make some special effects like particle, smoke or other visuals when your ingredient in being processed
        if (isProcessing)
        {
            //Special FX here! - make sure to stop, disable ro destroy your FX object after processing is finished.

        }

        //Optional - change target's color when this ingredient is near enough
        if (!processFlag || (isProcessed && target.tag == "serverPlate"))
            changeTargetsColor(target);

    }


    //***************************************************************************//
    // Let the player move the processed ingredient
    //***************************************************************************//
    void manageDiscard()
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
            if (objectHit.tag == "overburnedIngredient" && objectHit.name == gameObject.name)
            {

                IngredientsController.itemIsInHand = true;  //we have an ingredient in hand. no room for other ingredients!
                target = trashBin;                          //we can just deliver this ingredient to trashbin.

                StartCoroutine(discardIngredient());
            }
        }
    }


    //***************************************************************************//
    // Let the player move the processed ingredient to the delivery plate
    //***************************************************************************//
    void manageSecondDrag()
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
            if (objectHit.tag == "ingredient" && objectHit.name == gameObject.name)
            {

                IngredientsController.itemIsInHand = true;  //we have an ingredient in hand. no room for other ingredients!
                                                            //target = serverPlate;						//we can just deliver this ingredient to main plate. there is no other
                                                            //destination for this processed ingredient
                StartCoroutine(followInputTimeIndependent());
            }
        }
    }


    //***************************************************************************//
    // change plate's color when dragged ingredients are near enough
    //***************************************************************************//
    private float myDistance;
    void changeTargetsColor(GameObject _target)
    {
        if (!IngredientsController.itemIsInHand)    //if nothing is being dragged
            return;
        else if (_target.tag == "serverPlate" && _target.GetComponent<PlateController>().deliveryQueueIsFull)
            return;
        else
        {
            myDistance = Vector3.Distance(_target.transform.position, gameObject.transform.position);
            //print("myDistance: " + myDistance);
            if (myDistance < minDeliveryDistance)
            {
                //change target's color to let the player know this is the correct place to release the items.
                _target.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
            }
            else
            {
                //change target's color back to normal
                _target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            }
        }
    }


    //***************************************************************************//
    // Check if the ingredients are dragged into the deliveryPlate. Otherwise delete them.
    //***************************************************************************//
    void checkCorrectPlacementOnProcessor()
    {

        //if there is already an item on the processor, destroy this new ingredient
        if (!target.GetComponent<GrillController>().isEmpty)
        {
            Destroy(this.gameObject);
            target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
            return;
        }

        //if this ingredient is close enough to it's processor machine, leave it there. otherwise drop and delete it.
        float distanceToProcessor = Vector3.Distance(target.transform.position, gameObject.transform.position);
        //print("distanceToProcessor: " + distanceToProcessor);

        if (distanceToProcessor < minDeliveryDistance)
        {
            //close enough to land on processor
            transform.parent = target.transform;
            transform.position = new Vector3(target.transform.position.x,
                                             target.transform.position.y + 0.4f,
                                             target.transform.position.z - 0.1f);

            //change deliveryPlate's color back to normal
            target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);

            //start processing the raw ingredient
            StartCoroutine(processRawIngredient());

            //we no longer need this ingredient's script (ProductMover class)
            //GetComponent<ProductMover>().enabled = false;

        }
        else
        {
            Destroy(gameObject);
        }

        //Not draggable anymore.
        isFinished = true;
    }


    //***************************************************************************//
    // Process raw ingredinet and transform it to a usable ingredient
    //***************************************************************************//
    IEnumerator processRawIngredient()
    {

        processFlag = true; //should always remain true!
        isProcessing = true;
        isProcessed = false;
        isOverburned = false;

        float processTime = GrillController.grillTimer;
        float keepWarmTime = GrillController.grillKeepWarmTimer;

        GrillController gc = target.GetComponent<GrillController>();
        gc.isOn = true;
        gc.isEmpty = false;
        gc.playSfx(gc.frySfx);

        float t = 0.0f;
        while (t < 1)
        {
            t += Time.deltaTime * (1 / processTime);
            //print (gameObject.name + " is getting processed! - Time to wait: " + t);
            yield return 0;
        }

        if (t >= 1)
        {
            isProcessing = false;
            isProcessed = true;
            isOverburned = false;

            needsProcess = false;

            gc.isWarm = true;   //grill has entered the state to keep the burger warm.
            gc.playSfx(gc.readySfx);

            gameObject.tag = "ingredient";
            gameObject.name = gameObject.name.Substring(0, gameObject.name.Length - 4);
            //target.GetComponent<GrillController>().isOn = false;
            GetComponent<Renderer>().material = beforeAfterMat[1];

            //change the target 
            //target = serverPlate;

            //Now check if we pick the fried burger on time, otherwise this burger will get overburned and should be discarded.
            float v = 0.0f;
            while (v < 1)
            {
                if (!IngredientsController.itemIsInHand)
                    v += Time.deltaTime * (1 / keepWarmTime);
                print("Time to OverBurn: " + (1 - v));
                yield return 0;
            }
            //This burger is overburned! so it should be discarded in trashbin.
            //Important: as we are checking this condition independently from main game cycle, we need to double check
            //if this ingredient has been moved to delivery plate or is still waiting on the grill
            //if it was not on the plate, then we are sure that it is burned. otherwise we do not continue the procedure.
            if (v >= 1 && gameObject.tag != "deliveryQueueItem" && !IngredientsController.itemIsInHand)
            {
                isProcessing = false;
                isProcessed = true;
                isOverburned = true;

                GetComponent<Renderer>().material = beforeAfterMat[2];  //change the material to overburned burger

                GrillController ngc = grill.GetComponent<GrillController>();
                ngc.playSfx(ngc.overburnSfx);   //play fail sfx
                ngc.isEmpty = false;
                ngc.isWarm = false;
                ngc.isOn = true;
                ngc.isOverburned = true;

                gameObject.tag = "overburnedIngredient";
                target = trashBin;
            }

        }

    }


    //***************************************************************************//
    // Check if the ingredients are dragged into the deliveryPlate. Otherwise delete them.
    //***************************************************************************//
    void checkCorrectPlacement()
    {
        PlateController pc = target.GetComponent<PlateController>(); //cache this component

        //if this ingredient is close enough to serving plate, we can add it to main queue. otherwise drop and delete it.
        float distanceToPlate = Vector3.Distance(target.transform.position, gameObject.transform.position);
        //print("distanceToPlate: " + distanceToPlate);

        if (distanceToPlate < minDeliveryDistance && !pc.deliveryQueueIsFull)
        {
            //close enough to land on plate
            transform.parent = target.transform;
            transform.position = new Vector3(
                target.transform.position.x,
                target.transform.position.y + (itemsDistanceOnPlate * pc.deliveryQueueItems),
                target.transform.position.z - (0.2f * pc.deliveryQueueItems + 0.1f));

            pc.deliveryQueueItems++;
            pc.deliveryQueueItemsContent.Add(factoryID);
            //print("Delivery Queue Items: " + pc.deliveryQueueItemsContent);

            //change deliveryPlate's color back to normal
            target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);

            //we no longer need this ingredient's script (ProductMover class)
            GetComponent<ProductMover>().enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }

        //Not draggable anymore.
        isFinished = true;
    }


    //***************************************************************************//
    // Follow players mouse or finger position on screen.
    //***************************************************************************//
    private Vector3 _Pos;
    void followInputPosition()
    {
        _Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Custom offset. these objects should be in front of every other GUI instances.
        _Pos = new Vector3(_Pos.x, _Pos.y, -0.5f);
        //follow player's finger
        transform.position = _Pos + new Vector3(0, 0, 0);
    }


    //***************************************************************************//
    // Follow players mouse or finger position on screen.
    // This is an IEnumerator and run independent of game main cycle
    //***************************************************************************//
    IEnumerator followInputTimeIndependent()
    {
        while (IngredientsController.itemIsInHand || target.tag == "serverPlate")
        {
            tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
            transform.position = tmpPos + new Vector3(0, 0, 0);

            //if user release the input, check if we delivered the processed ingredient to the plate or we just release it nowhere!
            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {
                //if we delivered it to the plate
                if (Vector3.Distance(target.transform.position, gameObject.transform.position) <= minDeliveryDistance)
                {
                    print("Landed on Plate in: " + Time.time);

                    GrillController gc = grill.GetComponent<GrillController>();
                    gc.isEmpty = true;
                    gc.isWarm = false;
                    gc.isOn = false;

                    gameObject.tag = "deliveryQueueItem";
                    gameObject.GetComponent<MeshCollider>().enabled = false;
                    transform.position = new Vector3(
                        target.transform.position.x,
                        target.transform.position.y + (0.02f * target.GetComponent<PlateController>().deliveryQueueItems),
                        target.transform.position.z - (0.2f * target.GetComponent<PlateController>().deliveryQueueItems + 0.1f));

                    transform.parent = target.transform;
                    target.GetComponent<PlateController>().deliveryQueueItems++;
                    target.GetComponent<PlateController>().deliveryQueueItemsContent.Add(factoryID);
                    //change deliveryPlate's color back to normal
                    target.GetComponent<Renderer>().material.color = new Color(1, 1, 1);
                    //we no longer need this ingredient's script (ProductMover class)
                    GetComponent<ProductMover>().enabled = false;
                    yield break;

                }
                else
                {
                    //if we released it nowhere
                    //print ("Reset Position");
                    target = grill;
                    transform.parent = target.transform;
                    transform.position = new Vector3(target.transform.position.x,
                                                     target.transform.position.y + 0.75f,
                                                     target.transform.position.z - 0.1f);
                }
            }

            yield return 0;
        }
    }


    //***************************************************************************//
    // Follow players mouse or finger position on screen.
    // This is an IEnumerator and run independent of game main cycle
    //***************************************************************************//
    IEnumerator discardIngredient()
    {
        while (IngredientsController.itemIsInHand || target == trashBin)
        {
            print("target: " + target);
            TrashBinController tbc = trashBin.GetComponent<TrashBinController>();

            tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tmpPos = new Vector3(tmpPos.x, tmpPos.y, -0.5f);
            transform.position = tmpPos + new Vector3(0, 0, 0);

            //update trashbin door state
            float tmpDistanceToTrashbin = Vector3.Distance(target.transform.position, gameObject.transform.position);
            print("tmpDistanceToTrashbin: " + tmpDistanceToTrashbin);
            if (tmpDistanceToTrashbin <= minDeliveryDistance)
                tbc.updateDoorState(1);
            else
                tbc.updateDoorState(0);

            //if user release the input, check if we delivered the processed ingredient to the trashbin or we just release it nowhere!
            if (Input.touches.Length < 1 && !Input.GetMouseButton(0))
            {
                //if we delivered it to the trashbin
                if (tmpDistanceToTrashbin <= minDeliveryDistance)
                {

                    tbc.playSfx(tbc.deleteSfx); //play trash sfx

                    //New v1.7.2 - trash loss
                    MainGameController.totalMoneyMade -= MainGameController.globalTrashLoss;
                    GameObject money3d = Instantiate(money3dText,
                                                        trashBin.transform.position + new Vector3(0, 0, -0.8f),
                                                        Quaternion.Euler(0, 0, 0)) as GameObject;
                    money3d.GetComponent<TextMeshController>().myText = "- $" + MainGameController.globalTrashLoss.ToString();

                    GrillController gc = grill.GetComponent<GrillController>();
                    gc.isEmpty = true;
                    gc.isWarm = false;
                    gc.isOn = false;
                    gc.isOverburned = false;

                    Destroy(gameObject);
                    yield break;

                }
                else
                {
                    //if we released it nowhere
                    //print ("Reset Position");
                    target = grill;
                    transform.parent = target.transform;
                    transform.position = new Vector3(target.transform.position.x,
                                                     target.transform.position.y + 0.75f,
                                                     target.transform.position.z - 0.1f);

                    //2021 - bug fix - target should be trashbin again once the burned burger is back at the grill
                    target = trashBin;
                    yield break;
                }
            }

            yield return 0;
        }
    }

}