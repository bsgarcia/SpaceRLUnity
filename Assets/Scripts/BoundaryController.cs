using System.ComponentModel;
using System.Xml.Serialization;
using System.Net;
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

	//void OnTriggerExit(Collider other)
    //{
    //    // Debug.Log("BoundaryController: " + tag);
    //    if (tag == "BoundaryMissed")
    //    {  
    //        if (gameController.GetOptionController().missed)
    //        {
    //            gameController.MissedTrial();
    //        }
    //        gameController.AllowWave(true);
    //        Destroy(other.gameObject);

    //        gameController.AllowWave(true);
    //        Destroy(other.gameObject);
    //    }

    //    if ((tag == "BoundaryShootable") &&  (other.tag == "Bolt"))
    //    {
    //        gameController.FadeAndDestroyOption(other.gameObject, 1f);

    //    }   
    //}
        
    void OnTriggerEnter(Collider other)
    {
        if ((tag == "BoundaryShootable") &&  (other.tag == "Opt1" || other.tag == "Opt2"))
        {
            gameController.GetPlayerController().AllowShot(true);
        }

        if ((tag == "BoundaryMissed") && (other.tag == "Opt1" || other.tag == "Opt2"))
        {   
            if (gameController.GetOptionController().missed)
            {
                gameController.MissedTrial();
            }
            gameController.AllowWave(true);
            Destroy(other.gameObject);
            gameController.GetPlayerController().AllowShot(false);
        }

        if (tag == "BoundaryLeave" && (other.tag == "Opt1" || other.tag == "Opt2") &&
        other.gameObject.GetComponent<OptionShot>().isLeaving == false)
        {
            Debug.Log("BoundaryController.cs: BoundaryLeave - " + other.tag + "is leaving");
            gameController.GetPlayerController().AllowShot(false);
            gameController.GetOptionController().MakeOptionsLeave();
            gameController.GetOptionController().missed = true;
        }

    }
}
