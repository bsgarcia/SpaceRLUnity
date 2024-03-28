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
    
    public void AttachForceFields(GameObject option1, GameObject option2, float space)
    {
        option1.GetComponent<MeshRenderer>().enabled = false;
        option2.GetComponent<MeshRenderer>().enabled = false;

        // Instantiate the prefab
        GameObject ff1 = Instantiate(forcefieldPrefab);
        GameObject ff2 = Instantiate(forcefieldPrefab);
        // Set the parent of the instantiated prefab to the specified GameObject
        // for Resources/ff1
        // ff1.transform.localScale = new Vector3(.91f, .91f, 5f);
        // ff2.transform.localScale = new Vector3(.91f, .91f, 5f);
        // for Resources/ff2
        // ff1.transform.localScale = new Vector3(.71f, .71f, 5f);
        // ff2.transform.localScale = new Vector3(.71f, .71f, 5f);
        // for Resources/ff2
        ff1.transform.localScale = new Vector3(.26f, .26f, 5f);
        ff2.transform.localScale = new Vector3(.26f, .26f, 5f);
// 
        ff1.transform.SetParent(option1.transform);
        ff2.transform.SetParent(option2.transform);
        // set Correct position and rotation, scale        
        // ff1.transform.localPosition = new Vector3(0f, 0f, 0f);
        // ff2.transform.localPosition = new Vector3(0f, 0f, 0f);
        
        // for Resources/ff1
        // float spacing = -1.6f;
        // for Resources/ff2
        float spacing = -space;

         // Get the dimensions of the spaceship
        Bounds bounds1 = option1.GetComponent<Renderer>().bounds;
        Bounds bounds2 = option2.GetComponent<Renderer>().bounds;
        
        // Calculate the position in front of the model
        Vector3 positionInFront1 = option1.transform.position + option1.transform.forward * (bounds1.size.z / 2 + spacing);
        Vector3 positionInFront2 = option2.transform.position + option2.transform.forward * (bounds2.size.z / 2 + spacing);

        // Place the ff at the calculated position
        ff1.transform.position = positionInFront1;
        ff2.transform.position = positionInFront2;

        ff1.transform.rotation = Quaternion.Euler(90f, 45f, 0f);
        ff2.transform.rotation = Quaternion.Euler(90f, 45f, 0f);
    }

    public void SetForceFields(bool value, int idx = 0, float space = 1.7f)
    {
        forcefield = value;

        option1 = gameController.option1;
        option2 = gameController.option2;
                
        if (!value) {
            // disable mesh rendered of option1 and option2
            // getChildGameObject((GameObject) option1, (string) "ff_prefab(Clone)").GetComponent<SpriteRenderer>().enabled = false;
            // getChildGameObject((GameObject) option2, (string) "ff_prefab(Clone)").GetComponent<SpriteRenderer>().enabled = false;
            option1.GetComponent<MeshRenderer>().enabled = false;
            option2.GetComponent<MeshRenderer>().enabled = false;
            return;
        }

        // if prefab is not attached to spaceship
        AttachForceFields(option1, option2, space);

        float[] p = new float[2] {TaskParameters.probabilities[idx].x, TaskParameters.probabilities[idx].y};
        
        SetPDestroy(p[0], p[1]);

        SpriteRenderer spriteRenderer1 = getChildGameObject(
            (GameObject) option1, (string) "ff_prefab(Clone)").GetComponent<SpriteRenderer>();
        SpriteRenderer spriteRenderer2 = getChildGameObject(
            (GameObject) option2, (string) "ff_prefab(Clone)").GetComponent<SpriteRenderer>();

        spriteRenderer1.sprite = Resources.Load<Sprite>("bw/" + p[0].ToString().Replace(",", "."));
        spriteRenderer2.sprite = Resources.Load<Sprite>("bw/" + p[1].ToString().Replace(",", "."));

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

            Instantiate(explosionFailed, other.transform.localPosition, other.transform.localRotation);

            Debug.Log("Option survived");
            option.GetComponent<OptionShot>().DeviateShot(other);

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
        // gameController.AllowWave(true);

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

