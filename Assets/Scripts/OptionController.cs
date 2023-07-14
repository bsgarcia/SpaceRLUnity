using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;


public class OptionController : MonoBehaviour
{

	public GameObject explosion;
	public GameObject explosionFailed;
	public int scoreValue;
	public int counterscoreValue;
    public int outcomeOpt1;
    public int outcomeOpt2;
    public int choice;
    public int choseLeft;
    public int corr;

    // set probability of the forcefield option to be destroyed
    public double option1PDestroy = 1;
    public double option2PDestroy = 1;
    
    // the random p
    private double randomP = 1;

    public bool shootable = false;
    public bool missed = true;
    public bool forcefield = false;

    public Stopwatch st = new Stopwatch();

	private GameController gameController;

    public bool showFeedback = true;
    public bool addToScore = true;
    
    public GameObject option1;
    public GameObject option2;
    
    private bool isLeaving = false;
    

	void Awake()
	{
		GameObject gameControllerObject = GameObject.FindWithTag(
            "GameController");
        gameController = gameControllerObject.GetComponent<GameController>();

    }
    
    void Update() 
    {
        List<GameObject> options = gameController.GetOptions();
        option1 = options[0];
        option2 = options[1];
    }

    public void SetPDestroy(double pDestroy1, double pDestroy2)
    {
        option1PDestroy = pDestroy1;
        option2PDestroy = pDestroy2;
        
    }
    
    
    public void SetChoice(string tag, Collider other)
    {
        // get the option
        GameObject option = GameObject.FindWithTag(tag);
        // get the other option
        GameObject otherOption = tag == "Opt1" ? GameObject.FindWithTag("Opt2") : GameObject.FindWithTag("Opt1");
        bool exploded = true;

        // if the forcefield option is chosen, it is destroyed with probability p1
        // pick a random p between 0 and 1
        randomP = ((double) Random.Range(0, 100))/100;
        double pDestroy = tag == "Opt1" ? option1PDestroy : option2PDestroy;
        
        // print all probabilities
        Debug.Log("randomP: " + randomP);
        Debug.Log("pDestroy: " + pDestroy);
        
        if ((randomP > pDestroy) && (forcefield))
        {
            other.GetComponent<Collider>().enabled = false;

            Debug.Log("Option survived");
            option.GetComponent<OptionShot>().DeviateShot(other);

            Instantiate(explosionFailed, option.transform.localPosition, option.transform.localRotation);

            option.GetComponent<OptionShot>().LeaveScreen();
            otherOption.GetComponent<OptionShot>().LeaveScreen();

            exploded = false;
            missed = true;
            
        } 
        else 
        {
            Debug.Log("Option destroyed");
            exploded = true;
            missed = false;
            Instantiate(explosion, option.transform.localPosition, option.transform.localRotation);
            otherOption.GetComponent<OptionShot>().LeaveScreen();
        }


        switch (tag)
        {
            case "Opt1":

                otherOption = GameObject.FindWithTag("Opt2");
                scoreValue = gameController.outcomeOpt1;
                counterscoreValue = gameController.outcomeOpt2;

                corr = 1;
                choice = 1;
                
                break;

            case "Opt2":

                otherOption = GameObject.FindWithTag("Opt1");

                scoreValue = gameController.outcomeOpt2;
                counterscoreValue = gameController.outcomeOpt1;
                choice = 2;
                corr = 0;

                break;

            default:
                //Debug.Log("Error: object not recognized.");
                otherOption = null;
                break;
        }

        choseLeft = transform.position.x < 0 ? 1 : 0;

        if ((showFeedback) && (exploded)) {
            gameController.PrintFeedback(
                scoreValue, counterscoreValue, option.transform.position);
        }

        if (addToScore) {
            gameController.AddScore(scoreValue);
        }

        gameController.AllowSendData(true);

        // destroy chosen option + laser shot
        if (exploded)
        {
            Destroy(option);
            Destroy(other.gameObject);
        }

        }
    
    public void MakeOptionsLeave() 
    {
        if (!isLeaving) 
        {
            isLeaving = true;
            
            option1.GetComponent<OptionShot>().LeaveScreen();
            option2.GetComponent<OptionShot>().LeaveScreen();

            isLeaving = false;
        }
    }

    }

