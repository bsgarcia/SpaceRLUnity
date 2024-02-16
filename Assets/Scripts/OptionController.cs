using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;
using static System.Random;


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
    public float option1PDestroy = 1;
    public float option2PDestroy = 1;
    
    public bool shootable = false;
    public int missed = 0; 
    public bool forcefield = false;
   

	private GameController gameController;

    public bool showFeedback = true;
    public bool addToScore = true;
    
    public GameObject option1;
    public GameObject option2;
    
    private bool isLeaving = false;
    
    public bool destroyed = false;
    
    public GameObject forcefieldPrefab;

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
    
    public void Reset() 
    {
        missed = 0;
        choice = 0;
        scoreValue = outcomeOpt1;
        counterscoreValue = outcomeOpt2;
        destroyed = false;
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

    public void SetPDestroy(float pDestroy1, float pDestroy2)
    {
        option1PDestroy = pDestroy1;
        option2PDestroy = pDestroy2;
        
    }
    
    public (Color color1, Color color2, int colorIdx1, int colorIdx2) GetColor()
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
        
        int colorIdx1 = Random.Range(0, colors.Length);
        int colorIdx2 = Random.Range(0, colors.Length);
        // pick 2 colors from the list (randomly)
        Color color1 = colors[colorIdx1];
        Color color2 = colors[colorIdx2];

        return (color1, color2, colorIdx1, colorIdx2);
    }
    
    public void AttachForceFields(GameObject option1, GameObject option2)
    {
        option1.GetComponent<MeshRenderer>().enabled = false;
        option2.GetComponent<MeshRenderer>().enabled = false;

        // Instantiate the prefab
        GameObject ff1 = Instantiate(forcefieldPrefab);
        GameObject ff2 = Instantiate(forcefieldPrefab);
        // Set the parent of the instantiated prefab to the specified GameObject
        ff1.transform.SetParent(option1.transform);
        ff2.transform.SetParent(option2.transform);
        // set Correct position and rotation, scale        
        // ff1.transform.position = new Vector3(0.13f, 0f, 0.2f);
        // ff2.transform.position = new Vector3(0.13f, 0f, 0.2f);
        ff1.transform.localPosition = new Vector3(0.13f, 0f, 0.2f);
        ff2.transform.localPosition = new Vector3(0.13f, 0f, 0.2f);

        // ff1.transform.rotation = new Vector3(4f, 45f, 180f);
        // ff2.transform.rotation = new Vector3(4f, 45f, 180f);
        ff1.transform.rotation = Quaternion.Euler(4f, 45f, 180f);
        ff2.transform.rotation = Quaternion.Euler(4f, 45f, 180f);

        ff1.transform.localScale = new Vector3(.5f, .5f, .5f);
        ff2.transform.localScale = new Vector3(.5f, .5f, .5f);
    }

    public void SetForceFields(bool value, int idx = 0)
    {
        forcefield = value;

        option1 = gameController.option1;
        option2 = gameController.option2;
                
        if (!value) {
            // disable mesh rendered of option1 and option2
            getChildGameObject((GameObject) option1, (string) "Torus").GetComponent<MeshRenderer>().enabled = false;
            getChildGameObject((GameObject) option2, (string) "Torus").GetComponent<MeshRenderer>().enabled = false;
            // option1.Find("flat_cut/Torus").GetComponent<MeshRenderer>().enabled = false;
            // option2.Find("flat_cut/Torus").GetComponent<MeshRenderer>().enabled = false;
            return;
        }

        // if prefab is not attached to spaceship
        // AttachForceFields(option1, option2);

        // Material[] mat1 = option1.GetComponent<MeshRenderer>().materials;
        // Material[] mat2 = option2.GetComponent<MeshRenderer>().materials;
        

        // (Color color1, Color color2, int colorIdx1, int colorIdx2) = GetColor();

        // double[] p = new double[] {0.11920292202211755, 0.16798161486607552,
        //  0.23147521650098238, 0.35434369377420455, 0.45016600268752216,
        //  0.549833997312478, 0.6456563062257954, 0.7685247834990175,
        //  0.8320183851339245, 0.8807970779778823};
        float[] p = new float[2] {TaskParameters.probabilities[idx].x, TaskParameters.probabilities[idx].y};
        
        SetPDestroy(p[0], p[1]);

        // mat1[1] = option1.GetComponent<OptMaterials>().GetForceField(color1, 1);
        // mat2[1] = option1.GetComponent<OptMaterials>().GetForceField(color2, 2);
        // 
        getChildGameObject((GameObject) option1, (string) "Torus").GetComponent<MeshRenderer>().materials[0].SetFloat("_Proportion", (float) p[0]);
        getChildGameObject((GameObject) option2, (string) "Torus").GetComponent<MeshRenderer>().materials[0].SetFloat("_Proportion", (float) p[1]);

        // option2.GetComponent<MeshRenderer>().materials = mat2;
        // option1.GetComponent<MeshRenderer>().materials = mat1;

    }
    static public GameObject getChildGameObject(GameObject fromGameObject, string withName) {
		//Author: Isaac Dart, June-13.
		Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>();
		foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
    
    
    public void SetChoice(string tag, Collider other)
    // CALLED BY OptionShot.cs
    {
        // reset the missed variable
        missed = -1;
        // get the option
        GameObject option = GameObject.FindWithTag(tag);
        // get the other option
        GameObject otherOption = tag == "Opt1" ? GameObject.FindWithTag("Opt2") : GameObject.FindWithTag("Opt1");

        // if the forcefield option is chosen, it is destroyed with probability p1
        // pick a random p between 0 and 1
        // Create a Random object
        System.Random random = new System.Random();

        // Get a random probability between 0 and 1
        double randomP = random.NextDouble();

        if ((randomP > 1) || (randomP < 0))
        {
            throw new System.NotSupportedException("randomP invalid value");
        }
        double pDestroy = tag == "Opt1" ? option1PDestroy : option2PDestroy;
        
        // print all probabilities
        Debug.Log("randomP: " + randomP);
        Debug.Log("pDestroy: " + pDestroy);
        
        if ((randomP > pDestroy) && (forcefield))
        {
            other.GetComponent<Collider>().enabled = false;

            Debug.Log("Option survived");
            option.GetComponent<OptionShot>().DeviateShot(other);

            Instantiate(explosionFailed, other.transform.localPosition, other.transform.localRotation);

            option.GetComponent<OptionShot>().LeaveScreen();
            otherOption.GetComponent<OptionShot>().LeaveScreen();
            destroyed = false;
            gameController.MissedTrial();
            
        } 
        else 
        {
            Debug.Log("Option destroyed");
            destroyed = true;
            // missed = 0;
            Instantiate(explosion, option.transform.localPosition, option.transform.localRotation);
            otherOption.GetComponent<OptionShot>().LeaveScreen();
        }


        switch (tag)
        {
            case "Opt1":

                choseLeft = option.transform.position.x < 0 ? 1 : 0;
                otherOption = GameObject.FindWithTag("Opt2");
                scoreValue = outcomeOpt1;
                counterscoreValue = outcomeOpt2;
                corr = 1;
                choice = 1;
                
                break;

            case "Opt2":

                choseLeft = option.transform.position.x < 0 ? 1 : 0;
                otherOption = GameObject.FindWithTag("Opt1");
                scoreValue = outcomeOpt2;
                counterscoreValue = outcomeOpt1;
                choice = 2;
                corr = 0;

                break;

            default:
                //Debug.Log("Error: object not recognized.");
                otherOption = null;
                break;
        }


        if ((showFeedback) && (destroyed)) {
            gameController.PrintFeedback(
                scoreValue, counterscoreValue, option.transform.position);
            gameController.AddScore(scoreValue);
        }

        //if (addToScore) {
        //}

        gameController.AllowSendData(true);

        // destroy chosen option + laser shot
        if (destroyed)
        {
            Debug.Log("Chose left: " + choseLeft);
            Debug.Log("Position: " + transform.position.x);
            Destroy(option);
            Destroy(other.gameObject);
        }

    }
    
    public void MakeOptionsLeave() 
    {
        if (isLeaving) 
            return;
        
        StartCoroutine(Leave());
    }
    
    IEnumerator Leave() {
        isLeaving = true;
        yield return new WaitForSeconds(0.3f);
        option1.GetComponent<OptionShot>().LeaveScreen();
        option2.GetComponent<OptionShot>().LeaveScreen();
        isLeaving = false;
        yield return null;
    }
    
    public float GetOptionP(int cond, int idx)
    {
       return .5f; 
    }

}

