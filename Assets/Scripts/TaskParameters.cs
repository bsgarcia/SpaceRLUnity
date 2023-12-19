using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

public class TaskParameters : MonoBehaviour
{

    public int nTrialsPerception;
    public int nTrialsPerConditionFull;
    public int nTrialsPerConditionTrainingRL;

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


    void Start()
    {
        GameController gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        for (int i = 0; i < gameController.RLHazard.Count; i++)
        {
            availableOptions.Add(i);
        }

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

        nTrialsPerceptualTraining = nTrialsPerception;
        nTrialsTrainingRL = nTrialsPerConditionTrainingRL*conditionsTraining.Count;

        MakeConditionsIdx();
        MakeDistributionRewards();
    }

    private void MakeDistributionRewards() {
        for (int c = 0; c < nConds; c++) {
            // 2 options
            rewards.Add(new List<List<int>>()); // Initialize the innermost list
            rewardsTraining.Add(new List<List<int>>());

            for (int i = 0; i < 2; i++) {
                rewards[c].Add(
                    RandomGaussian(conditions[c][i], std, minReward, maxReward, nTrialsPerConditionFull));
                rewardsTraining[c].Add(
                    RandomGaussian(conditionsTraining[c][i], std, minReward, maxReward, nTrialsPerConditionTrainingRL));
            }
        }
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