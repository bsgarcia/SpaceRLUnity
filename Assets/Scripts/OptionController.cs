﻿using System.Collections;
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
        List<GameObject> options = gameController.GetOptions();
        option1 = options[0];
        option2 = options[1];
    }
    
    void Update() 
    {
        List<GameObject> options = gameController.GetOptions();
        option1 = options[0];
        option2 = options[1];
    }

    public static int RandomGaussian(float mean, float std, float min, float max) {
        float x;
        do {
            x = mean + NextGaussian() * std;
        } while (x < min || x > max);
        return (int) Mathf.Round(x);
    }
    
    public static float NextGaussian() {
        float v1, v2, s;
        do {
            v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);

        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
     
        return v1 * s;
    }

    public static float RandomGaussian2(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    public void SetPDestroy(double pDestroy1, double pDestroy2)
    {
        option1PDestroy = pDestroy1;
        option2PDestroy = pDestroy2;
        
    }
    
    public (Color color1, Color color2, int colorIdx1, int colorIdx2) GetColor(int colorIdx1, int colorIdx2)
    {
        Color[] colors = new Color[]
        {
            new Color(0.12156863f, 0.46666667f, 0.70588235f, 1f),
            new Color(0.20036909f, 0.43221838f, 0.64559785f, 1f),
            new Color(0.27916955f, 0.39777009f, 0.58531334f, 1f),
            new Color(0.36078431f, 0.3620915f, 0.52287582f, 1f),
            new Color(0.43958478f, 0.32764321f, 0.46259131f, 1f),
            new Color(0.52119954f, 0.29196463f, 0.40015379f, 1f),
            new Color(0.6f, 0.25751634f, 0.33986928f, 1f),
            new Color(0.68161476f, 0.22183775f, 0.27743176f, 1f),
            new Color(0.76041522f, 0.18738947f, 0.21714725f, 1f),
            new Color(0.83921569f, 0.15294118f, 0.15686275f, 1f)
        };
        // pick 2 colors from the list (randomly)
        Color color1 = colors[colorIdx1];
        Color color2 = colors[colorIdx2];

        return (color1, color2, colorIdx1, colorIdx2);
    }

    public void SetForceFields(int value)
    {
        if (value == 0) {
            forcefield = false;
        } else {
            forcefield = true;
        }
        option1 = gameController.option1;
        option2 = gameController.option2;

        if (value==0) {
            // disable mesh rendered of option1 and option2
            option1.GetComponent<MeshRenderer>().enabled = false;
            option2.GetComponent<MeshRenderer>().enabled = false;
            return;
        }

        Material[] mat1 = option1.GetComponent<MeshRenderer>().materials;
        Material[] mat2 = option2.GetComponent<MeshRenderer>().materials;
        

        (Color color1, Color color2, int colorIdx1, int colorIdx2) = GetColor(value-1, value-1);


        double[] p = new double[] {0.1, 0.14, 0.18, 0.2, 0.25, .3, .5, .75, .85, .9, 1};
        SetPDestroy(p[colorIdx1], p[colorIdx2]);

        mat1[1] = option1.GetComponent<OptMaterials>().GetForceField(color1, 1);
        mat2[1] = option1.GetComponent<OptMaterials>().GetForceField(color2, 2);

        option1.GetComponent<MeshRenderer>().materials = mat1;
        option2.GetComponent<MeshRenderer>().materials = mat2;

    }
    
    
    public void SetChoice(string tag, Collider other)
    {
        // reset the missed variable
        missed = false;
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
                scoreValue = RandomGaussian(mean: 5, std: 2, min: 0, max: 10); // gameController.outcomeOpt1;
                counterscoreValue = gameController.outcomeOpt2;
                corr = 1;
                choice = 1;
                
                break;

            case "Opt2":

                otherOption = GameObject.FindWithTag("Opt1");
                scoreValue = RandomGaussian(mean: 0, std: 2, min: -5, max: 5); // gameController.outcomeOpt1;
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
            gameController.AddScore(scoreValue);
        }

        //if (addToScore) {
        //}

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

