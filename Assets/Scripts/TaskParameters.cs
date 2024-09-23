using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using Random = UnityEngine.Random;
using Random2 = System.Random;


// Custom exception class for indicating common elements
public class CommonElementsFoundException : Exception
{
    public CommonElementsFoundException(string message) : base(message) { }
}

public class TaskParameters : MonoBehaviour
{

    public int nTrialsPerceptionPerPair;
    public int nPerceptualPairs = 16;
    
    public int perceptualReward_ = 50;
    public static int perceptualReward;
    
    public int session;
// 
    public static int sessionIdx;

    public float fbTime;
    public float fallSpeed;

    public bool interleaved;
    public static bool online = true;
    // public int n_conditions;
    // public int feedback_info;

    public static int nTrialsFull;

    public static int nTrialsTrainingRL;
    
    public static int nTrialsPerceptualTraining;
    
    public  bool trainingPerceptual;
    public  bool trainingRL;
    public  bool trainingFull;
    

    public static int nConds;
    public static float feedbackTime;
    public static float fallSpeed_;

    public float minReward;
    public float maxReward;

    /* [VectorLabels("mag", "proba", "val")]
    public Vector3 Option1;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option2;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option3;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option4;
 */
    public int std;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition1;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition2;


    // [VectorLabels("Opt1", "Opt2", "info")]
    // public Vector3Int ConditionTraining1;

    // [VectorLabels("Opt1", "Opt2", "info")]
    // public Vector3Int ConditionTraining2;

    public static List<List<int>> pairs = new List<List<int>>();
    public static List<List<int>> trainingPairs = new List<List<int>>();

    public static List<Vector2> colors = new List<Vector2>();

    public static List<Vector3> options = new List<Vector3>();
    public static List<Vector3> conditions = new List<Vector3>();
    public static List<List<int>> conditionsTraining = new List<List<int>>();
    public static List<int> conditionIdx;
    public static List<int> conditionTrainingIdx;

    public string expName_ = "default";
    public static string expName = "default";
    
    public string FFColorYellowBlue = "by";

    public static string FFColor = "bw";

    public static List<List<List<int>>> rewards = new List<List<List<int>>>();
    public static List<List<List<int>>> rewardsTraining = new List<List<List<int>>>();

    private List<int> availableOptions = new List<int>();
    public static List<Vector2> symbols = new List<Vector2>();
    public static List<Vector2> symbolsTransfer = new List<Vector2>();
    /* public List<Vector2> proba2 = new List<Vector2>
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
 */
    // public List<Vector2> proba = new List<Vector2>
    //     {
    //         new Vector2(0.16f, 0.29f),
    //         new Vector2(0.71f, 0.84f),
    //         new Vector2(0.22f, 0.37f),
    //         new Vector2(0.63f, 0.78f),
    //         new Vector2(0.55f, 0.78f),
    //         new Vector2(0.22f, 0.45f),
    //         new Vector2(0.37f, 0.63f),
    //         new Vector2(0.55f, 0.88f),
    //         new Vector2(0.12f, 0.45f),
    //         new Vector2(0.29f, 0.71f),
    //         new Vector2(0.16f, 0.63f),
    //         new Vector2(0.37f, 0.84f),
    //         new Vector2(0.12f, 0.71f),
    //         new Vector2(0.29f, 0.88f),
    //         new Vector2(0.16f, 0.84f),
    //         new Vector2(0.12f, 0.88f)
    //     };
    //     

    public List<Vector2> proba2 = new List<Vector2>
    {
        new Vector2(0.12f, 0.18f),
        new Vector2(0.12f, 0.27f),
        new Vector2(0.12f, 0.38f),
        new Vector2(0.12f, 0.5f),
        new Vector2(0.12f, 0.62f),
        new Vector2(0.12f, 0.73f),
        new Vector2(0.12f, 0.82f),
        new Vector2(0.12f, 0.88f),
        new Vector2(0.18f, 0.12f),
        new Vector2(0.18f, 0.27f),
        new Vector2(0.18f, 0.38f),
        new Vector2(0.18f, 0.5f),
        new Vector2(0.18f, 0.62f),
        new Vector2(0.18f, 0.73f),
        new Vector2(0.18f, 0.82f),
        new Vector2(0.18f, 0.88f),
        new Vector2(0.27f, 0.12f),
        new Vector2(0.27f, 0.18f),
        new Vector2(0.27f, 0.38f),
        new Vector2(0.27f, 0.5f),
        new Vector2(0.27f, 0.62f),
        new Vector2(0.27f, 0.73f),
        new Vector2(0.27f, 0.82f),
        new Vector2(0.27f, 0.88f),
        new Vector2(0.38f, 0.12f),
        new Vector2(0.38f, 0.18f),
        new Vector2(0.38f, 0.27f),
        new Vector2(0.38f, 0.5f),
        new Vector2(0.38f, 0.62f),
        new Vector2(0.38f, 0.73f),
        new Vector2(0.38f, 0.82f),
        new Vector2(0.38f, 0.88f),
        new Vector2(0.5f, 0.12f),
        new Vector2(0.5f, 0.18f),
        new Vector2(0.5f, 0.27f),
        new Vector2(0.5f, 0.38f),
        new Vector2(0.5f, 0.62f),
        new Vector2(0.5f, 0.73f),
        new Vector2(0.5f, 0.82f),
        new Vector2(0.5f, 0.88f),
        new Vector2(0.62f, 0.12f),
        new Vector2(0.62f, 0.18f),
        new Vector2(0.62f, 0.27f),
        new Vector2(0.62f, 0.38f),
        new Vector2(0.62f, 0.5f),
        new Vector2(0.62f, 0.73f),
        new Vector2(0.62f, 0.82f),
        new Vector2(0.62f, 0.88f),
        new Vector2(0.73f, 0.12f),
        new Vector2(0.73f, 0.18f),
        new Vector2(0.73f, 0.27f),
        new Vector2(0.73f, 0.38f),
        new Vector2(0.73f, 0.5f),
        new Vector2(0.73f, 0.62f),
        new Vector2(0.73f, 0.82f),
        new Vector2(0.73f, 0.88f),
        new Vector2(0.82f, 0.12f),
        new Vector2(0.82f, 0.18f),
        new Vector2(0.82f, 0.27f),
        new Vector2(0.82f, 0.38f),
        new Vector2(0.82f, 0.5f),
        new Vector2(0.82f, 0.62f),
        new Vector2(0.82f, 0.73f),
        new Vector2(0.82f, 0.88f),
        new Vector2(0.88f, 0.12f),
        new Vector2(0.88f, 0.18f),
        new Vector2(0.88f, 0.27f),
        new Vector2(0.88f, 0.38f),
        new Vector2(0.88f, 0.5f),
        new Vector2(0.88f, 0.62f),
        new Vector2(0.88f, 0.73f),
        new Vector2(0.88f, 0.82f)
                
    };
    
    // create an array of pairs of game objects to set in the unity editor
    public List<GameObject> pairRLTraining = new List<GameObject>(new GameObject[2]);
    public List<GameObject> pairFull1 = new List<GameObject>(new GameObject[2]);
    public List<GameObject> pairFull2 = new List<GameObject>(new GameObject[2]);
    public List<GameObject> pairFull3 = new List<GameObject>(new GameObject[2]);
    public List<GameObject> pairFull4 = new List<GameObject>(new GameObject[2]);


    public static List<Vector2> probabilities;

    public static int[] ffPairIdx;
    public int[] probPairIdx;
    
    public int nTrialPerCondition;
    
    public static List<List<GameObject>> pairsFullGameObject = new List<List<GameObject>>(); 
    public static List<List<GameObject>> pairsRLGameObject = new List<List<GameObject>>();

    public List<List<GameObject>> game2Pairings = new List<List<GameObject>>();
    
    public List<List<int>> game2PairingsRewards = new List<List<int>>();
    
    public static int nCondRL;

    // public static int[] conditionIdx;
    
    public static int[] fullFFPairsIdx;

    public int nControlsFF = 14;
    public int nControlsSS = 14;
    
    public int nRepeatTrainingRL;

    void Start()
    {
        expName = expName_;

        GameController gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();

        #if UNITY_WEBGL && !UNITY_EDITOR
            // get from window.session
            // session = GameController.GetSession();
            if (session == 0) {
                gameController.skipTuto = true;
            } else {
                gameController.skipTuto = true;
            }

        #endif
        #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
        #else
            Debug.unityLogger.logEnabled = false;
        #endif

        // Random.seed = (int) System.DateTime.Now.Ticks;

        
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
        // options.Add(Option3);
        // options.Add(Option4);

        conditions.Add(Condition1);
        conditions.Add(Condition2);

        // conditionsTraining.Add(ConditionTraining1);
        // conditionsTraining.Add(ConditionTraining2);
        conditionsTraining.Add(new List<int> {Condition1.x, Condition1.y});
        conditionsTraining.Add(new List<int> {Condition2.x, Condition2.y});
        // conditionsTraining.Add(Condition2_);
        nConds = conditions.Count;
        // nTrialsFull = nTrialsPerConditionFull*conditions.Count;
        feedbackTime = fbTime;

        if (nPerceptualPairs != proba2.Count) {
            Debug.Log("Warning!: nPerceptualPairs != proba.Count");
        }
        
   
        probPairIdx = new int[nTrialsPerceptualTraining];

        probabilities = proba2;

        sessionIdx = session;
        
        fallSpeed_ = fallSpeed;

        nTrialPerCondition = nTrialsPerceptionPerPair;

        // Shuffle2(pairFull1);
        // Shuffle2(pairFull2);        
        // Shuffle2(pairFull3);        
        // Shuffle2(pairFull4);        

        pairsFullGameObject.Add(pairFull1);
        pairsFullGameObject.Add(pairFull2);

        pairsRLGameObject.Add(pairFull1);
        pairsRLGameObject.Add(pairFull2);
        
        Debug.Log("pairsRLGameObject: " + pairsRLGameObject.Count);

        pairsFullGameObject.Add(pairFull3);
        pairsFullGameObject.Add(pairFull4);
        
        // Shuffle2(pairRLTraining);
        
        perceptualReward = perceptualReward_;
        
        List<GameObject> game2Spaceships = new List<GameObject>();
        List<float> game2SpaceshipsRewards = new List<float>();
        // first get all the game objects for the pairs
        for (int i = 0; i < pairsRLGameObject.Count; i++) {
            for (int j = 0; j < pairsRLGameObject[i].Count; j++) {
                game2Spaceships.Add(pairsRLGameObject[i][j]);
                game2SpaceshipsRewards.Add(conditionsTraining[i][j]);
            }
        }
        
        // now pit the game objects in pairs where A,B == B,A so no repetition
        // we have to check if the pair is already in the list in reverse order
        for (int i = 0; i < game2Spaceships.Count; i++) {
            for (int j = i+1; j < game2Spaceships.Count; j++) {

                if (game2Spaceships[i].name == game2Spaceships[j].name) {
                    continue;
                }

                List<GameObject> pair = new List<GameObject>();
                List<int> pairRewards = new List<int>();

                pair.Add(game2Spaceships[i]);
                pair.Add(game2Spaceships[j]);
                
                pairRewards.Add((int) game2SpaceshipsRewards[i]);
                pairRewards.Add((int) game2SpaceshipsRewards[j]);

                game2Pairings.Add(pair);
                game2PairingsRewards.Add(pairRewards);
            }
        }
        
        // in the end pairsRLGameObject will have all the pairs of game objects
        pairsRLGameObject = game2Pairings;
        
        nRepeatTrainingRL = 8; 
        nTrialsTrainingRL = 0;
        if (trainingRL) {
            // sessionIdx = 1;
            // session = 1;
            nTrialsTrainingRL = game2Pairings.Count*nRepeatTrainingRL;
        }
        Debug.Log("nTrialsTrainingRL: " + nTrialsTrainingRL);

        nTrialsPerceptualTraining = 0;
        if (trainingPerceptual) {
            // sessionIdx = 0;
            // session = 0;
            nTrialsPerceptualTraining = 48;
        }
        Debug.Log("nTrialsPerceptualTraining: " + nTrialsPerceptualTraining);
        
        nTrialsFull = 0;
        if (trainingFull) {
            nTrialsFull = nTrialsPerceptionPerPair*nPerceptualPairs*nConds + nControlsFF + nControlsSS;
        }
        Debug.Log("nTrialsFull: " + nTrialsFull);

        Debug.Log("Start computing probabilities");
        MakeProbPairs();
        Debug.Log("Start computing conditions");
        MakeConditionsIdx();
        Debug.Log("Start computing rewards");
        MakeDistributionRewards();
        
        nCondRL = game2Pairings.Count;
        conditionsTraining = game2PairingsRewards;
        
        AddControls();
        
        if (sessionIdx == 4) {
            FFColor = FFColorYellowBlue;
        }

    }

    private void MakeDistributionRewards() {
        for (int c = 0; c < nConds; c++) {
            // 2 options
            rewards.Add(new List<List<int>>()); // Initialize the innermost list
            // unpitted version
            // rewardsTraining.Add(new List<List<int>>());
            for (int i = 0; i < 2; i++) {
                rewards[c].Add(
                    RandomGaussian(conditions[c][i], std, minReward, maxReward, nPerceptualPairs+nControlsSS));
                
                // unpitted version
                // rewardsTraining[c].Add(
                        // RandomGaussian(conditionsTraining[c][i], std, minReward, maxReward, nPerceptualPairs));
                
                // deterministic version
                // rewards[c].Add(Enumerable.Repeat((int) conditions[c][i], nPerceptualPairs).ToList());
                // rewardsTraining[c].Add(Enumerable.Repeat((int) conditionsTraining[c][i], nPerceptualPairs).ToList());
                // rewardsTraining[c][i] = Enumerable.Repeat((int) conditionsTraining[c][i], nPerceptualPairs).ToList();

                // rewards[c].Add(new List<int>() {5});
                // rewardsTraining[c].Add(new List<int>() {5});
            }
        }
        
        // game 2 pitted version
        for (int c = 0; c < game2Pairings.Count ; c++) {
            // 2 options
            rewardsTraining.Add(new List<List<int>>());
            for (int i = 0; i < 2; i++) {
                rewardsTraining[c].Add(
                    RandomGaussian(game2PairingsRewards[c][i], std, minReward, maxReward, nRepeatTrainingRL));


            }
            
        }
        
        Debug.Log("rewardsTraining: " + rewardsTraining.Count);
        
    }
    
    private void MakeProbPairs() {
        // Create the array using Enumerable.Repeat and SelectMany
        int repeatCount = nTrialsPerceptionPerPair;
        int[] probPairIdx = Enumerable.Repeat(Enumerable.Range(0, nPerceptualPairs).ToArray(), repeatCount)
                                   .SelectMany(x => x)
                                   .ToArray();
        probPairIdx = Shuffle(probPairIdx).Take(nTrialsPerceptualTraining).ToArray();
         
        ffPairIdx = probPairIdx;
        
        repeatCount = nConds;
        probPairIdx = Enumerable.Repeat(Enumerable.Range(0, nPerceptualPairs).ToArray(), repeatCount)
                                   .SelectMany(x => x)
                                   .ToArray();
        probPairIdx = Shuffle(probPairIdx).ToArray();
        fullFFPairsIdx = probPairIdx;

    }
    
    public static void RandomizeFFPairs() {
        ffPairIdx = Shuffle(ffPairIdx).ToArray();
        fullFFPairsIdx = Shuffle(fullFFPairsIdx).ToArray();
    }
    
    public static void RandomizeConditions() {
        // if (conditionIdx.Count > 0) 
            conditionIdx = Shuffle(conditionIdx);
        // if (conditionTrainingIdx.Count > 0)
            conditionTrainingIdx = Shuffle(conditionTrainingIdx);
    }
    
    
    public static int GetOptionMean(int c, int option) {
        // if (c==-2) {
            // Debug.Log("c: " + 0 + " option: " + option);
            // return (int) conditionsTraining[0][option];
        // }
        
        if (sessionIdx==0) {
            return (int) perceptualReward;
        }
        if (sessionIdx==1) {
            Debug.Log("c: " + c + " option: " + option);
            return (int) conditionsTraining[c][option];
        }
        if (sessionIdx==2 || sessionIdx==3) {
            
            // control ff only
            if (c==-1) {
                return (int) perceptualReward;
            }
            // control ss only
            if (c==-2) {
                return -2;
            }
            return (int) conditions[c][option];

        }

        return (int) conditions[c][option];
    }


    private void MakeConditionsIdx()
    {
        List<List<int>> conditionIdxTemp = new List<List<int>>();
        List<List<int>> conditionTrainingIdxTemp = new List<List<int>>();

        for (int c = 0; c < nConds; c++)//conditions.Count; c++)
        {
            if (nTrialsFull == 0) {
                break;
            }
            List<int> x1 = Enumerable.Repeat(c, (nTrialsFull-(nControlsFF+nControlsSS))/nConds).ToList();
            conditionIdxTemp.Add(x1);
            // unpitted version
            // List<int> x2 = Enumerable.Repeat(c, nTrialsTrainingRL/nConds).ToList();
            // conditionTrainingIdxTemp.Add(x2);
        }
        
        for (int c = 0; c < game2Pairings.Count; c++)//conditions.Count; c++)
        {
            if (nTrialsTrainingRL == 0) {
                break;
            }
            List<int> x1 = Enumerable.Repeat(c, nTrialsTrainingRL/game2Pairings.Count).ToList();
            conditionTrainingIdxTemp.Add(x1);
        }

        conditionIdxTemp = Shuffle(conditionIdxTemp);
        conditionTrainingIdxTemp = Shuffle(conditionTrainingIdxTemp);

        conditionIdx = conditionIdxTemp.SelectMany(i => i).ToList<int>();
        conditionTrainingIdx = conditionTrainingIdxTemp.SelectMany(i => i).ToList<int>();

        if (interleaved)
        {
            conditionIdx = Shuffle(conditionIdx);
            conditionTrainingIdx = Shuffle(conditionTrainingIdx);
        }
        
        // print all conditionTrainingIdx elements
        /* for (int i = 0; i < conditionTrainingIdx.Count; i++)
        {
            Debug.Log("conditionTrainingIdx: " + conditionTrainingIdx[i]);
        } */

    }
    
    private void AddControls() {
        int nControls = nControlsFF;
        // only forcefield
        List<int> controlsFF = Enumerable.Repeat(-1, nControls).ToList();
        // only ss
        List<int> controlsSS = Enumerable.Repeat(-2, nControls).ToList();
        
        // merge the two lists
        controlsFF.AddRange(controlsSS);
        // merge controlsFF with conditionTrainingIdx
        controlsFF.AddRange(conditionIdx);
        
        conditionIdx = controlsFF;
        // shuffle the list
        conditionIdx = Shuffle(conditionIdx);
        
        // conditionTrainingIdx = Shuffle(conditionTrainingIdx);
        
        // create an array ranging from 0 to nPerceptualPairs
        int[] ffIdx = Enumerable.Range(0, nPerceptualPairs).ToArray();
        // select 24 random elements from the array
        ffIdx = Shuffle(ffIdx).Take(nControls*2).ToArray();
        
        // now add ffIdx to fullFFPairsIdx existing array (at the end)
        fullFFPairsIdx = ffIdx.Concat(fullFFPairsIdx).ToArray();
    }

    private static void Shuffle2<T>(IList<T> list)
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
    
    public static List<T> Shuffle<T>(IList<T> sequence)
    {
        Random2 random = new Random2();

        // T[] retArray = sequence.copy();
        List<T> retArray = new List<T>(sequence);


        for (int i = 0; i < retArray.Count - 1; i += 1)
        {
            int swapIndex = random.Next(i, retArray.Count);
            if (swapIndex != i) {
                T temp = retArray[i];
                retArray[i] = retArray[swapIndex];
                retArray[swapIndex] = temp;
            }
        }

        return new List<T>(retArray);
    }

    public static List<int> RandomGaussian(float mean, float std, float min, float max, float size) {
        List<int> y = new List<int>();
        for (int i = 0; i < size; i++) {
            float x;
            do {
                x = mean + NextGaussian() * std;
            } while (x < min || x > max || x < (mean-std) || x > (mean+std));
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