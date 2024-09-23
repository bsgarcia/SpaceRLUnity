using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor; // Required for PrefabUtility and Editor functionality
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using Random = UnityEngine.Random;
using Random2 = System.Random;
using Project.Scripts.Fractures;

// [ExecuteInEditMode] // Allows script to run in editor mode
public class FractureSpaceshipsEditor : MonoBehaviour
{

    public List<GameObject> prefabs = new List<GameObject>(new GameObject[4]);
    private List<GameObject> options = new List<GameObject>();

    public bool stopLoop = false;

    void Start()
    {
        // Ensure this runs only in editor mode, not in play mode
        if (Application.isPlaying) return;

        Debug.Log("Fracturing Spaceships in Editor mode");

        int offset = 0;
        foreach (var option in prefabs)
        {
            if (option != null)
            {
                Debug.Log("Fracturing " + option.name);

                var optionInstance = Instantiate(option);
                options.Add(option);

                var parent = new GameObject();
                optionInstance.transform.SetParent(parent.transform);
                parent.name = option.name;

                parent.AddComponent<FractureThis>();

                StartCoroutine(LoadChunkObject(optionInstance, parent));
            }
        }
    }

    IEnumerator LoadChunkObject(GameObject option, GameObject parent)
    {
        GameObject fracture = null;
        while ((fracture == null) & (!stopLoop))
        {
            Debug.Log("Finding " + parent.name + "_Fracture");
            fracture = GameObject.Find(parent.name + "_Fracture");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Found " + parent.name + "_Fracture");

        fracture.transform.SetParent(parent.transform);
        // Iterate through all child meshes
        foreach (Transform child in parent.transform)
        {
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();

            if (meshFilter && meshFilter.mesh != null)
            {
                // Create a unique mesh path for each child
                string meshPath = $"Assets/Meshes/FracturedMesh_{child.name}.asset";

                // Save the mesh as a new asset in the project
                // AssetDatabase.CreateAsset(meshFilter.mesh, meshPath);
                // AssetDatabase.SaveAssets();

                Debug.Log("Mesh saved at: " + meshPath);
            }
        }

        string number = parent.name.Substring(parent.name.Length - 1);

        // Save the parent as a prefab in Models/WithFracture in the editor
        // PrefabUtility.SaveAsPrefabAsset(parent, "Assets/Models/WithFracture/FractureShip" + number + ".prefab");
        yield return null;
    }

    // Optional: This is useful if you want to trigger the fracturing outside of Start()
    [ContextMenu("Fracture Spaceships")]
    void FractureInEditor()
    {
        Start();
    }
}
