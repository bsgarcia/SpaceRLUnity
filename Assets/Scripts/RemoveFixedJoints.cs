using UnityEngine;
using Random = System.Random;
using System.Collections;
using System.Collections.Generic; // Add this line to include the List<T> type
// import parallel job system
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEditor;


[ExecuteInEditMode] // Allows script to run in editor mode
public class RemoveFixedJoints : MonoBehaviour
{

    void Start()
    {
        // Ensure this runs only in editor mode, not in play mode
        if (Application.isPlaying) return;

        List<string> prefabs = new List<string>();
        // Add all prefabs in the Resources folder to the list

        prefabs.Add("FractureShip1_");
        prefabs.Add("FractureShip3_");
        prefabs.Add("FractureShip8_");
        prefabs.Add("FractureShip12_");
        
        foreach (var option in prefabs)
        {
            if (option != null)
            {
               var obj = GameObject.Find(option); 
               // now remove all fixed joints from the object
                if (obj != null)
                {
                     var joints = obj.GetComponents<FixedJoint>();
                     foreach (var joint in joints)
                     {
                          DestroyImmediate(joint);
                     }
                }
                
            }
        }
    }

   // context menu to remove fixed joints
    [ContextMenu("Remove Fixed Joints")] 
    void Run() {
       Start(); 
    }
}