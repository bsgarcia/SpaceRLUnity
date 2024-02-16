using System.ComponentModel;
using System.Xml.Serialization;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;


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
        if ((tag == "BoundaryMissedLeft") || (tag == "BoundaryMissedRight"))
        {
            Destroy(other.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if ((tag == "BoundaryShootable") &&  (other.tag == "Opt1" || other.tag == "Opt2"))
        {
            PlayerController playerController = gameController.GetPlayerController();
            playerController.AllowMove(true);
            playerController.AllowShot(true);
            playerController.fireTimer = new Stopwatch();
            playerController.moveTimer = new Stopwatch();
            playerController.fireTimer.Start();
            playerController.moveTimer.Start();

        }

        if ((tag == "BoundaryMissed") && (other.tag == "Opt1" || other.tag == "Opt2"))
        {   
            if (!gameController.GetOptionController().destroyed)
                gameController.MissedTrial();
            
            gameController.GetPlayerController().AllowShot(false);
            gameController.AllowWave(true);
            StartCoroutine(gameController.DestroyWithDelay(other.gameObject, 1f));
        }

        if ((tag == "BoundaryLeave") && (other.tag == "Opt1" || other.tag == "Opt2") &&
        !other.gameObject.GetComponent<OptionShot>().isLeaving)
        {
            Debug.Log("BoundaryController.cs: BoundaryLeave - " + other.tag + "is leaving");
            gameController.GetPlayerController().AllowShot(false);
            gameController.GetOptionController().MakeOptionsLeave();
            gameController.GetOptionController().missed = 1;
        }

    }
}