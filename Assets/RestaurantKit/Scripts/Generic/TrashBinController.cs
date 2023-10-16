using UnityEngine;
using System.Collections;

public class TrashBinController : MonoBehaviour
{

    //***************************************************************************//
    // This class manages all thing related to TrashBin.
    // 
    //***************************************************************************//

    //AudioClip
    public AudioClip deleteSfx;

    //Flags
    internal bool canDelete = true;

    private GameObject[] deliveryPlates;        //all available deliveryPlates inside the game
    private float[] distanceToPlates;           //distance to all available plates
    private GameObject target;

    //Textures for open/closed states
    public Texture2D[] state;

    //Flag used to let managers know that player is intended to send the order to trashbin.
    public bool isCloseEnoughToTrashbin;//Do not modify this.

    private Renderer r;

    //***************************************************************************//
    // Simple Init
    //***************************************************************************//
    void Awake()
    {
        target = null;
        r = GetComponent<Renderer>();

        deliveryPlates = GameObject.FindGameObjectsWithTag("serverPlate");
        distanceToPlates = new float[deliveryPlates.Length];

        isCloseEnoughToTrashbin = false;
        r.material.mainTexture = state[0];
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

        for (int i = 0; i < deliveryPlates.Length; i++)
        {
            distanceToPlates[i] = Vector3.Distance(deliveryPlates[i].transform.position, gameObject.transform.position);
            //find the correct (nearest blender) target
            target = deliveryPlates[(int)findMinInArray(distanceToPlates).y];
        }

        //check if player wants to move the order to trash bin
        if (target.GetComponent<PlateController>().canDeliverOrder)
        {
            checkDistanceToDelivery();
        }
    }


    //***************************************************************************//
    // If player is dragging the deliveryPlate, check if maybe he wants to trash it.
    // we do this by calculation the distance of deliveryPlate and trashBin.
    //***************************************************************************//
    private float myDistance;
    void checkDistanceToDelivery()
    {
        myDistance = Vector3.Distance(transform.position, target.transform.position);
        //print("distance to trashBin is: " + myDistance + ".");

        //2.0f is a hardcoded value. specify yours with caution.
        if (myDistance < 2.0f)
        {
            isCloseEnoughToTrashbin = true;
            //change texture
            r.material.mainTexture = state[1];
        }
        else
        {
            isCloseEnoughToTrashbin = false;
            //change texture
            r.material.mainTexture = state[0];
        }
    }


    /// <summary>
    /// Allow other controllers to update the animation state of this trashbin object
    /// by controlling it's door state.
    /// </summary>
    public void updateDoorState(int _state)
    {
        if (_state == 1)
            r.material.mainTexture = state[1];
        else
            r.material.mainTexture = state[0];
    }


    //***************************************************************************//
    // Activate using trashbin again, after a few seconds.
    //***************************************************************************//
    IEnumerator reactivate()
    {
        yield return new WaitForSeconds(0.25f);
        canDelete = true;
    }


    //***************************************************************************//
    // Play audioclips.
    //***************************************************************************//
    public void playSfx(AudioClip _sfx)
    {
        GetComponent<AudioSource>().clip = _sfx;
        if (!GetComponent<AudioSource>().isPlaying)
            GetComponent<AudioSource>().Play();
    }

}