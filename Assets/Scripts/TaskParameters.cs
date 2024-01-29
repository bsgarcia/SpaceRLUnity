using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using Random = UnityEngine.Random;


// Custom exception class for indicating common elements
public class CommonElementsFoundException : Exception
{
    public CommonElementsFoundException(string message) : base(message) { }
}

public class TaskParameters : MonoBehaviour
{

    public int nTrialsPerceptionPerPair;
    public int nPerceptualPairs = 16;
    public int nTrialsPerConditionFull;
    public int nTrialsPerConditionTrainingRL;
    
    public int session;

    public static int sessionIdx;

    public float fbTime;
    public bool interleaved;
    public static bool online = true;
    // public int n_conditions;
    // public int feedback_info;

    public static int nTrialsFull;
    public static int nTrialsTrainingRL;
    
    public static int nTrialsPerceptualTraining;

    public static int nConds;
    public static float feedbackTime;

    public float minReward;
    public float maxReward;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option1;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option2;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option3;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option4;

    public int std;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition1;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition2;


    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int ConditionTraining1;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int ConditionTraining2;

    public static List<List<int>> pairs = new List<List<int>>();
    public static List<List<int>> trainingPairs = new List<List<int>>();

    public static List<Vector2> colors = new List<Vector2>();

    public static List<Vector3> options = new List<Vector3>();
    public static List<Vector3> conditions = new List<Vector3>();
    public static List<Vector3> conditionsTraining = new List<Vector3>();
    public static List<int> conditionIdx;
    public static List<int> conditionTrainingIdx;

    public static List<List<List<int>>> rewards = new List<List<List<int>>>();
    public static List<List<List<int>>> rewardsTraining = new List<List<List<int>>>();

    private List<int> availableOptions = new List<int>();
    public static List<Vector2> symbols = new List<Vector2>();
    public static List<Vector2> symbolsTransfer = new List<Vector2>();
    public List<Vector2> proba = new List<Vector2>
    {
        new Vector2(0.12f, 0.88f),
        new Vector2(0.12f, 0.71f),
        new Vector2(0.12f, 0.45f),
        new Vector2(0.12f, 0.22f),
        new Vector2(0.16f, 0.84f),
        new Vector2(0.16f, 0.63f),
        new Vector2(0.22f, 0.37f),
        new Vector2(0.29f, 0.88f),
        new Vector2(0.29f, 0.71f),
        new Vector2(0.37f, 0.84f),
        new Vector2(0.37f, 0.63f),
        new Vector2(0.37f, 0.45f),
        new Vector2(0.55f, 0.88f),
        new Vector2(0.55f, 0.63f),
        new Vector2(0.63f, 0.78f),
        new Vector2(0.78f, 0.88f)
    };

    public static List<Vector2> probabilities;

    public static int[] ffPairIdx;
    public int[] probPairIdx;

    void Start()
    {
        #if UNITY_WEBGL
            // get from window.session
            session = 
        #endif
        Random.seed = (int) System.DateTime.Now.Ticks;

        GameController gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        for (int i = 0; i < gameController.RLHazard.Count/2; i++)
        {
            availableOptions.Add(i);
        }

        // Debug.Log("availableOptions: " + availableOptions.Count);
        // Debug.Log("gameController.RLHazard.Count: " + gameController.RLHazard.Count);   
        Shuffle2(availableOptions);
        
        // create a list of list<int>
        // each list<int> is a pair of options
        // split the options into pairs of 2 (in a list)
        for (int i = 0; i < availableOptions.Count; i += 2)
        {
            List<int> pair = new List<int>();
            pair.Add(availableOptions[i]);
            pair.Add(availableOptions[i + 1]);
            pairs.Add(pair);
        }
        
        List<int> availableOptions2 = new List<int>();

        for (int i = availableOptions.Count; i < gameController.RLHazard.Count; i++)
        {
            availableOptions2.Add(i);
        }

        // Debug.Log("availableOptions2: " + availableOptions2.Count);
        Shuffle2(availableOptions2);
        
        // create a list of list<int>
        // each list<int> is a pair of options
        // split the options into pairs of 2 (in a list)
        for (int i = 0; i < availableOptions2.Count; i += 2)
        {
            List<int> pair = new List<int>();
            pair.Add(availableOptions2[i]);
            pair.Add(availableOptions2[i + 1]);
            trainingPairs.Add(pair);
        }
        
        // Check for common elements between availableOptions and availableOptions2
        IEnumerable<int> commonElements = availableOptions.Intersect(availableOptions2);

        // If common elements exist, throw an exception
        if (commonElements.Any())
        {
            throw new CommonElementsFoundException("Common elements found between availableOptions and availableOptions2.");
        }

        options.Add(Option1);
        options.Add(Option2);
        options.Add(Option3);
        options.Add(Option4);

        conditions.Add(Condition1);
        conditions.Add(Condition2);

        conditionsTraining.Add(ConditionTraining1);
        conditionsTraining.Add(ConditionTraining2);

        nConds = conditions.Count;
        nTrialsFull = nTrialsPerConditionFull*conditions.Count;
        feedbackTime = fbTime;

        if (nPerceptualPairs != proba.Count) {
            Debug.Log("Warning!: nPerceptualPairs != proba.Count");
        }
        nTrialsPerceptualTraining = nTrialsPerceptionPerPair*nPerceptualPairs;
        probPairIdx = new int[nTrialsPerceptualTraining];

        nTrialsTrainingRL = nTrialsPerConditionTrainingRL*conditionsTraining.Count;
        probabilities = proba;

        sessionIdx = session;

        Debug.Log("Start computing probabilities");
        MakeProbPairs();
        Debug.Log("Start computing conditions");
        MakeConditionsIdx();
        Debug.Log("Start computing rewards");
        MakeDistributionRewards();
    }

    private void MakeDistributionRewards() {
        for (int c = 0; c < nConds; c++) {
            // 2 options
            rewards.Add(new List<List<int>>()); // Initialize the innermost list
            rewardsTraining.Add(new List<List<int>>());
            for (int i = 0; i < 2; i++) {
                // rewards[c].Add(
                    // RandomGaussian(conditions[c][i], std, minReward, maxReward, nTrialsPerConditionFull));
                // rewardsTraining[c].Add(
                    // RandomGaussian(conditionsTraining[c][i], std, minReward, maxReward, nTrialsPerConditionTrainingRL));
                rewards[c].Add(new List<int>() {5});
                rewardsTraining[c].Add(new List<int>() {5});
            }
        }
    }
    
    private void MakeProbPairs() {
        // Create the array using Enumerable.Repeat and SelectMany
        int repeatCount = nTrialsPerceptionPerPair;
        int[] probPairIdx = Enumerable.Repeat(Enumerable.Range(0, nPerceptualPairs).ToArray(), repeatCount)
                                   .SelectMany(x => x)
                                   .ToArray();
        Shuffle2(probPairIdx);
         
        ffPairIdx = probPairIdx;

    }
    
    public static  int GetOptionMean(int c, int option) {
        if (c<0) {
            Debug.Log("c: " + 0 + " option: " + option);
            return (int) conditionsTraining[0][option];
        }
        Debug.Log("c: " + c + " option: " + option);
        return (int) conditions[c][option];
    }


    private void MakeConditionsIdx()
    {
        List<List<int>> conditionIdxTemp = new List<List<int>>();
        List<List<int>> conditionTrainingIdxTemp = new List<List<int>>();

        for (int c = 0; c < nConds; c++)//conditions.Count; c++)
        {
            List<int> x1 = Enumerable.Repeat(c, nTrialsPerConditionFull).ToList();
            conditionIdxTemp.Add(x1);
            List<int> x2 = Enumerable.Repeat(c, nTrialsPerConditionTrainingRL).ToList();
            conditionTrainingIdxTemp.Add(x2);
        }

        Shuffle2(conditionIdxTemp);
        Shuffle2(conditionTrainingIdxTemp);

        conditionIdx = conditionIdxTemp.SelectMany(i => i).ToList<int>();
        conditionTrainingIdx = conditionTrainingIdxTemp.SelectMany(i => i).ToList<int>();

        if (interleaved)
        {
            Shuffle2(conditionIdx);
            Shuffle2(conditionTrainingIdx);
        }

    }

    private void Shuffle2<T>(IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static List<int> RandomGaussian(float mean, float std, float min, float max, float size) {
        List<int> y = new List<int>();
        for (int i = 0; i < size; i++) {
            float x;
            do {
                x = mean + NextGaussian() * std;
            } while (x < min || x > max);
            y.Add((int) Mathf.Round(x));
        }
        return y;
    }
    
    public static float NextGaussian() {
        float v1, v2, s;
        do {
            v1 = 2.0f * UnityEngine.Random.Range(0f,1f) - 1.0f;
            v2 = 2.0f * UnityEngine.Random.Range(0f,1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);

        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
     
        return v1 * s;
    }
    
    public void GetOption()
    {
        return;
    }


}