using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;


public class DataController : MonoBehaviour
{
    private Dictionary<string, object> data;

    private string url;


    private GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        data = new Dictionary<string, object>();
        GameObject gameControllerObject = GameObject.FindWithTag(
            "GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject
                .GetComponent<GameController>();
        }
        if (gameController == null)
        {
            Debug.Log("Cannot find 'GameController' script");
        }
        
        //url = Application.absoluteURL + gameController.serverURL;
        // remove double slashes
        //url = url.Replace("//", "/");
        url = gameController.serverURL;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save(string key, object value)
    {
        data[key] = value;

    }
    public void PrintData()
    {   
        string str = "";
        foreach (KeyValuePair<string, object> entry in data)
        {

            str += "\"" + entry.Key + "\": " + "\"" + entry.Value.ToString().Replace(",", ".") + "\" ,";
            // Debug.Log(entry.Key + " = " + entry.Value);
        }
        Debug.Log(str);
    }
    

    public IEnumerator SendToDB()
    {
        if (!gameController.online)
        {
            Debug.Log("Online: off");
            yield break;

        }


        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        int error = 0;
        bool success = false;
        string str = "{";

        // object obj = CreateObjectFromDictionary(data);
        foreach (KeyValuePair<string, object> entry in data)
        {
            // formData.Add(new MultipartFormDataSection(entry.Key, entry.Value.ToString().Replace(",", ".")));
            //string += entry.Key + ": " + entry.Value.ToString().Replace(",", ".") + "\n";
            // format as json string
            str += "\"" + entry.Key + "\": " + "\"" + entry.Value.ToString().Replace(",", ".") + "\" ,";
        }
        // add slash before each quote
        //str = str.Replace("\"", "\\\"");
        
        // remove last comma
        str = str.Substring(0, str.Length - 2);
        str += "}";

        // Convert the dictionary to a JSON string
        // PrintData();
                // Serialize the object to JSON
        // string json = JsonConvert.SerializeObject(obj);
        
        //Debug.Log("Sending to server: " + str);

        while ((error < 4) && !success)

        {
            UnityWebRequest www = UnityWebRequest.Post(url, str, "application/json");
            www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, Content-Type, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            //www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, PUT, OPTIONS");
            www.SetRequestHeader("Access-Control-Allow-Origin", "*");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                error++;
                if (error == 4)
                    gameController.DisplayNetworkError();
            }
           
            string response = www.downloadHandler.text;
            Debug.Log("Server response: " + response);

            if (response.ToLower().Contains("error"))
            {
                error++;
                if (error == 4)
                    gameController.DisplayServerError();
            } 
            else if (response.ToLower().Contains("success"))
            {
                success = true;
            }
            
            yield return new WaitForSeconds(.1f);
            
        } 
        
    }
}
