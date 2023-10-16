using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.IO;

public class ApiCalls : MonoBehaviour
{
    public static ApiCalls instance;

    public string userDataJson;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    /// <summary>
    /// Starts the SendWalletAddress coroutine
    /// </summary>
    public void SendWalltetAddressFunction(string key, string value)
    {
        StartCoroutine(SendWalletAddress(key, value));
    }



    /// <summary>
    /// Starts the GetAllUserData coroutine
    /// </summary>
    public void GetAllUserDataFunction()
    {
        StartCoroutine(GetAllUserData());
    }



    /// <summary>
    /// Starts the SaveUserData coroutine
    /// </summary>
    public void SaveUserDataFunction(string key1,string walletAddress,string key2, int value)
    {
        StartCoroutine(SaveUserData(key1, walletAddress, key2, value));// Testing with custom values
    }



    /// <summary>
    /// Starts the GetPangination coroutine
    /// </summary>
    public void GetPanginationFunction()
    {
        StartCoroutine(GetPangination());
    }



    /// <summary>
    /// This will update the wallet address of the logged in user to the database.
    /// </summary>
    /// <returns></returns>
    IEnumerator SendWalletAddress(string key, string value)
    {
        WWWForm form = new WWWForm();
        form.AddField(key, value);

        using UnityWebRequest www = UnityWebRequest.Post("http://34.218.125.67:5050/cookdano/userLogin", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            //Debug.Log(www.error);
        }
        else
        {
            //Debug.Log("Form upload complete!");
        }
    }



    /// <summary>
    /// This will update the wallet address of the logged in user to the database.
    /// </summary>
    /// <returns></returns>
    IEnumerator GetAllUserData()
    {
        WWWForm form = new WWWForm();
        form.AddField("", "");

        using UnityWebRequest webRequest = UnityWebRequest.Post("http://34.218.125.67:5050/cookdano/getAllUsers", form);
        // Request and wait for the desired page.
        yield return webRequest.SendWebRequest();

        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(": HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                //Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                userDataJson = webRequest.downloadHandler.text;
                //Debug.Log("userDataJson" + userDataJson);
                break;
        }
    }



    /// <summary>
    /// This will let us save the changed score to the database
    /// </summary>
    /// <returns></returns>
    IEnumerator SaveUserData(string key1,string walletAddress,string key2, int value)
    {
        WWWForm form = new WWWForm();
        form.AddField(key1, walletAddress);
        form.AddField(key2, value);

        using UnityWebRequest www = UnityWebRequest.Post("http://34.218.125.67:5050/cookdano/updateUserData", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Data has not been saved");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Data has been saved");
            Debug.Log(www.downloadHandler.text);
        }
    }



    /// <summary>
    /// This will let us showleaderboard of a specific page.
    /// </summary>
    /// <returns></returns>
    IEnumerator GetPangination()
    {
        WWWForm form = new WWWForm();
        form.AddField("", "");

        using (UnityWebRequest www = UnityWebRequest.Post("http://34.218.125.67:5050/cookdano/getPageWithNumber", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }
}
