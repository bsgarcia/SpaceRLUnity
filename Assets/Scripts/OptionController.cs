using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;


public class OptionController : MonoBehaviour
{

	public GameObject explosion;
	public int scoreValue;
	public int counterscoreValue;
    public int outcomeOpt1;
    public int outcomeOpt2;
    public int choice;
    public int choseLeft;
    public int corr;

    // set probability of the forcefield option to be destroyed
    public double pDestroy = 10;
    public double otherPDestroy = 10;
    
    // the random p
    private double randomP = 10;

    public bool shootable = false;

    public Stopwatch st = new Stopwatch();

	private GameController gameController;

	void Awake()
	{
		GameObject gameControllerObject = GameObject.FindWithTag(
            "GameController");
        gameController = gameControllerObject.GetComponent<GameController>();

    }

    public void SetProbability(double P_, double otherP_)
    {
        Debug.Log("SetProbability");

        pDestroy = P_;
        otherPDestroy = otherP_;
        
    }

    void OnTriggerEnter(Collider other)
    {   
        // make the option shootable if it crosses the upper boundary
        if (other.tag == "BoundaryShootable")
        {
            shootable = true;
            st.Start();


        }

        // the option is shot
        if (other.tag == "Bolt" && shootable)
        {

            // record reaction time
            st.Stop();

            // explosion of the asteroid
            Instantiate(explosion, transform.localPosition, transform.localRotation);

            GameObject otherOption;

            // link this object to the gameController;
            gameController.SetOptionController(this);
            // if the forcefield option is chosen, it is destroyed with probability p1
            // pick a random p between 0 and 1
            randomP = ((double) Random.Range(0, 100))/100;
            if (randomP > pDestroy)
            {
                // only destroy the bolt
                Destroy(other.gameObject);
                return;
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

            choseLeft = transform.position.x == -4 ? 1 : 0;

            // destroy not chosen option
            //if (gameController.feedbackInfo == 1)

            gameController.FadeAndDestroyOption(otherOption, 1.5f);
            //else
               // Destroy(otherOption);
           
            gameController.PrintFeedback(
                scoreValue, counterscoreValue, transform.position);

            gameController.AddScore(scoreValue);

            gameController.AllowSendData(true);

            // destroy chosen option + laser shot
            Destroy(gameObject);
            Destroy(other.gameObject);

         
        }

    }


}
