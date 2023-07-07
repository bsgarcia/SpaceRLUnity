using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour
{
    private GameController gameController;

    void Start () 
    {

        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
    
    }

	void OnTriggerExit(Collider other)
    {
        // Debug.Log("BoundaryController: " + tag);
        if (tag == "BoundaryMissed")
        {  
            gameController.MissedTrial();
            gameController.AllowWave(true);
           Destroy(other.gameObject);
        }

        if ((tag == "BoundaryShootable") &&  (other.tag == "Bolt"))
        {
            gameController.FadeAndDestroyOption(other.gameObject, 1f);

        }   
    }
        
    void OnTriggerEnter(Collider other)
    {
        if ((tag == "BoundaryShootable") &&  (other.tag == "Opt1" || other.tag == "Opt2"))
        {
            // Debug.Log("BoundaryController: " + other.tag);
            gameController.GetPlayerController().AllowShot(true);

        }

    }
}
