using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using Random = UnityEngine.Random;
using Random2 = System.Random;

using Project.Scripts.Fractures;

// we need to use PrefabUtility to save the parent object as a prefab
using UnityEditor;


public class FractureSpaceships : MonoBehaviour {
    
    // create an array of pairs of game objects to set in the unity editor
    public List<GameObject> prefabs = new List<GameObject>(new GameObject[4]);
    private List<GameObject> options = new List<GameObject>();

    void Start()
    {
        Debug.Log("Fracturing Spaceships");
        
        int offset = 0;
        // for each option in the list of options
        foreach (var option in prefabs)
        {
            // if the option is not null
            if (option != null)

            {
                Debug.Log("Fracturing " + option.name);
                // add the option to the list of options

                var optionInstance = Instantiate(option);
                options.Add(option);
                // the option has a Mover script attached to it, set the value of speed to 0
                // optionInstance.GetComponent<Mover>().speed = 0;
                // Remove option shot script

                // put the option in front of the camera
                // optionInstance.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
                
                // offset += 10;
                // offset the position of the option
                // optionInstance.transform.position += new Vector3(offset, 0, 0);
                
                // put the option in a new parent object
                var parent = new GameObject();
                optionInstance.transform.SetParent(parent.transform);
                parent.name = option.name;
                
                // add fracturethis script to the parent object
                parent.AddComponent<FractureThis>();
                
                StartCoroutine(LoadChunkObject(optionInstance, parent));
                
                // parent.gameObject.SetActive(true);

                // find the Fracture game object that is at the rooot level
                // var fracture = GameObject.Find(parent.name + "_Fracture");
                // fracture.transform.SetParent(parent.transform);
                                
            }
        }
    }
    
    IEnumerator LoadChunkObject(GameObject option, GameObject parent)
    {
        GameObject fracture = null;
        while (fracture == null)
        {
            Debug.Log("Finding " + parent.name + "_Fracture");
            fracture = GameObject.Find(parent.name + "_Fracture");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Found " + parent.name + "_Fracture");
        
        // put it in the parent object
        fracture.transform.SetParent(parent.transform);
        
        // get number in parent name
        string number = parent.name.Substring(parent.name.Length - 1);

        /* MeshFilter meshFilter = parent.GetComponent<MeshFilter>();

        // Save the mesh as an asset if it's generated at runtime
        if (meshFilter && meshFilter.sharedMesh)
        {
            string meshPath = "Assets/Meshes/RuntimeMesh.asset";
            AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
            AssetDatabase.SaveAssets();
        } */

        // save the parent as a prefab in Models/WithFracture
        // PrefabUtility.SaveAsPrefabAsset(parent, "Assets/Models/WithFracture/FractureShip" + number.ToString() + ".prefab");
        yield return null;

    }

}


// public class SaveRuntimeMesh : MonoBehaviour
// {
//     public GameObject objectToSave;

//     public void SaveAsPrefabWithMesh()
//     {
//         MeshFilter meshFilter = objectToSave.GetComponent<MeshFilter>();

//         // Save the mesh as an asset if it's generated at runtime
//         if (meshFilter && meshFilter.sharedMesh)
//         {
//             string meshPath = "Assets/Meshes/RuntimeMesh.asset";
//             AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
//             AssetDatabase.SaveAssets();
//         }

//         // Save the GameObject as a prefab
//         string prefabPath = "Assets/Prefabs/MyPrefab.prefab";
//         PrefabUtility.SaveAsPrefabAsset(objectToSave, prefabPath);
//     }
// }