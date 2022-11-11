using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;


public class GameController : MonoBehaviour
{
    // public members
    public string serverURL;

    public bool online;
    public bool skipTuto;

    public GameObject hazard;
    public Vector3 spawnValues;
    public float startWait;
    public float waveWait;


    public Text scoreText;

    public Text restartText;
    public Text gameOverText;
    public Text missedTrialText;
    public Button playButton;

    public RawImage spaceImage;
    public RawImage moveImage;

    public Text rewardText;
    public Text counterText;


    public bool waveAllowed;

    public GameObject option1;
    public GameObject option2;

    public Object[] optionpath;


    public int outcomeOpt1;
    public int outcomeOpt2;

    public Animation anim1;
    public Animation anim2;

    public bool transfer;

    public int feedbackInfo;

    public string subID = "test";
    public int score;

    public int missedTrial = 0;

    public bool sendData = false;



    // private members

    private bool gameOver;
    private bool restart;

    private Texture symbol1;
    private Texture symbol2;


    private bool networkError;

    private bool KeyPhaseShootDone;
    private bool KeyPhaseMoveDone;

    private bool tutorialDone;

    private StateMachine stateMachine;

    private DataController dataController;
    private PlayerController playerController;
    private OptionController optionController;
    private PauseController pauseController;

    private bool isQuitting = false;


    // JS interactions
    [DllImport("__Internal")]
    private static extern void SetScore(int score);

    [DllImport("__Internal")]
    private static extern string GetSubID();

    [DllImport("__Internal")]
    private static extern void Alert(string text);

    //[DllImport("__Internal")]
    //private static extern void DisplayNextButton();


    // Getters / Setters
    // -------------------------------------------------------------------- //
    // Option controller is regenerated each trial, 
    // so we call this setter from the new generated object 
    // each time
    public void SetOptionController(OptionController obj)
    {
        optionController = obj;
    }

    public OptionController GetOptionController()
    {
        return optionController;
    }

    public void SetOutcomes(int v1, int v2)
    {
        outcomeOpt1 = v1;
        outcomeOpt2 = v2;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }


    public bool IsGameOver()
    {
        return gameOver;
    }

    public void SetGameOver()
    {
        gameOver = true;
    }

    public void AllowWave(bool value)
    {
        waveAllowed = value;
    }

    public void AllowSendData(bool value)
    {
        sendData = value;
    }

    public void Save(string key, object value)
    {
        dataController.Save(key, value);
    }

    public IEnumerator SendToDB()
    {
        //PrintData();
        yield return dataController.SendToDB();
        AfterSendToDB();

        Debug.Log("******************************************************");

    }

    public void PrintData()
    {
        dataController.PrintData();
    }

    public void AfterSendToDB()
    {
        playerController.ResetCount();
        missedTrial = 0;
        AllowWave(true);

    }


    void Start()
    {
        // optionpath = Resources.LoadAll("colors/"); 
        optionpath = Resources.LoadAll("colors/");

        score = 0;
        gameOver = false;
        restart = false;
        transfer = false;
        waveAllowed = true;
        restartText.text = "";
        gameOverText.text = "";
        rewardText.text = "";
        counterText.text = "";
        missedTrialText.text = "";

        gameOverText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        counterText.gameObject.SetActive(false);
        missedTrialText.gameObject.SetActive(false);
        moveImage.gameObject.SetActive(false);

        spaceImage.gameObject.SetActive(false);

        playButton.gameObject.SetActive(true);

        KeyPhaseShootDone = false;
        KeyPhaseMoveDone = false;
        tutorialDone = false;

        UpdateScore();

        try
        {
            subID = GetSubID();
        }
        catch
        {
            subID = "Unity";
        }

        //StartCoroutine(SpawnWaves()); 
        dataController = GameObject.FindWithTag("DataController").GetComponent<DataController>();
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        pauseController = GameObject.FindWithTag("PauseController").GetComponent<PauseController>();

    }

    public void RunWrapper() {
        StartCoroutine(Run());
    }

    public IEnumerator Run()
    {
        playButton.gameObject.SetActive(false);
        moveImage.gameObject.SetActive(true);

        while (!tutorialDone) {
            yield return new WaitForSeconds(2f);
        }

        stateMachine = new StateMachine(this);
        stateMachine.NextState();
        stateMachine.Update();

        gameOverText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        counterText.gameObject.SetActive(true);
        missedTrialText.gameObject.SetActive(true);
        
    }

    void ManageKeyPhase()
    {
        if (playButton.gameObject.active) {
            return;
        }
        if (skipTuto)
        {
            KeyPhaseMoveDone = true;
            KeyPhaseShootDone = true;
            tutorialDone = true;
            moveImage.gameObject.SetActive(false);
            spaceImage.gameObject.SetActive(false);
            return;

        }
        // Debug.Log(KeyPhaseMoveDone);
        if (!KeyPhaseMoveDone)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                KeyPhaseMoveDone = true;
                StartCoroutine(HideWithDelay(moveImage.gameObject, 3f));
                StartCoroutine(ShowWithDelay(spaceImage.gameObject, 5f));
                StartCoroutine(DisplayMsg("Good!", 2f, 3f));
            }

        }
        else if (!KeyPhaseShootDone && spaceImage.gameObject.active)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftControl))
            {

                KeyPhaseShootDone = true;

                StartCoroutine(HideWithDelay(spaceImage.gameObject, 3f));
                StartCoroutine(DisplayMsg("Perfect!\n Now  get  ready,\n  asteroids  are\n  coming!", 4f, 3f));
                StartCoroutine(SetBoolWithDelay(value => tutorialDone = value, true, 7f));
            }

        }

    }

    void Update()
    {
        ManageKeyPhase();

        if (isQuitting)
        {
            return;
        }

        if (IsGameOver())
        {
            StartCoroutine(DisplayGameOver());
            SetScore(score);
            StartCoroutine(QuitGame());
        }

        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        if (stateMachine != null && stateMachine.CurrentStateIsDone())
        {
            stateMachine.currentState.Exit();
            stateMachine.NextState();
            stateMachine.Update();
        }


    }

    IEnumerator QuitGame()
    {
        isQuitting = true;
        yield return new WaitForSeconds(1000f);
        Application.Quit();
    }

    // Graphical manager (to put in its own controller later)
    // ------------------------------------------------------------------------------------------------ //

    IEnumerator HideWithDelay(GameObject toHide, float delay)
    {
        yield return new WaitForSeconds(delay);
        toHide.SetActive(false);
    }
    IEnumerator ShowWithDelay(GameObject toShow, float delay)
    {
        yield return new WaitForSeconds(delay);
        toShow.SetActive(true);
    }


    IEnumerator SetBoolWithDelay(System.Action<bool> assigner, bool value,  float delay) {
        yield return new WaitForSeconds(delay);
        assigner.Invoke(value);
    }

    public void AddScore(int newScoreValue)
    {

        score += newScoreValue;
        Save("score", (int)score);
        if (feedbackInfo == 0) { 

            scoreText.gameObject.SetActive(false);
            return;
        }
        UpdateScore();
    }

    public void PrintFeedback(int newScoreValue, int counterScoreValue, Vector3 ScorePosition)
    {

        if (feedbackInfo == 0)
            return;

        rewardText.transform.position = ScorePosition;
        rewardText.text = "" + newScoreValue;

        if (feedbackInfo == 2)
        {
            counterText.transform.position = new Vector3(
                         -ScorePosition.x, ScorePosition.y, ScorePosition.z);
            counterText.text = "" + counterScoreValue;
        }

        StartCoroutine("DeleteFeedback", TaskParameters.feedbackTime);

    }

    IEnumerator DeleteFeedback(float feedbacktime)
    {
        yield return new WaitForSeconds(feedbacktime);
        rewardText.text = "";
        counterText.text = "";
        missedTrialText.text = "";
        // Destroy(option1);
        // Destroy(option2); 
        yield return null;
    }

    IEnumerator DestroyWithDelay(GameObject toDestroy, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(toDestroy);
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void DisplayNetworkError()
    {
        string msg = "Network error!\n" +
            "Check your \ninternet \nconnection and click to continue...";
        pauseController.PauseGame(msg);

    }
    public void DisplayServerError()
    {
        string msg = "Server error\n" +
            "Click to continue...";
        pauseController.PauseGame(msg);

    }

    public IEnumerator DisplayMsg(string txt, float delay, float delayBefore)
    {
        //missedTrialText.SetActive(true);
        yield return new WaitForSeconds(delayBefore);

        missedTrialText.gameObject.SetActive(true);
        missedTrialText.text = txt;
        yield return new WaitForSeconds(delay);
        missedTrialText.text = "";

    }

    IEnumerator DisplayGameOver()
    {
        yield return new WaitForSeconds(5);
        gameOverText.text = "End!";
    }


    public void MissedTrial()
    {
        missedTrial = 1;
        missedTrialText.text = "Missed trial!\n -2";
        AddScore(-1);
        AllowSendData(true);
        StartCoroutine("DeleteFeedback", TaskParameters.feedbackTime);
    }


    public void FadeAndDestroyOption(GameObject option, float delay)
    {
        option.GetComponent<Animation>().Play();
        option.GetComponent<Collider>().enabled = false;
        StartCoroutine(DestroyWithDelay(option, delay));
        //StartCoroutine(SendToDB());
    }


    public void ChangeBackground()
    {
        GameObject background = GameObject.FindWithTag("Background");
        background.GetComponent<MeshRenderer>().material.mainTexture =
            (Texture)Resources.Load("backgrounds/red");
        GameObject child = background.transform.GetChild(0).gameObject;
        child.GetComponent<MeshRenderer>().material.mainTexture =
        (Texture)Resources.Load("backgrounds/red");
    }

    public void SetSymbolsTexture(Vector2 id)
    {
        symbol1 = (Texture)optionpath[(int)id[0]];
        symbol2 = (Texture)optionpath[(int)id[1]];
        Material[] mat1 = option1.GetComponent<MeshRenderer>().materials;
        Material[] mat2 = option2.GetComponent<MeshRenderer>().materials;

        mat1[0].mainTexture = symbol1;
        mat2[0].mainTexture = symbol2;

        List<Color> colors = new List<Color>(){
            new Color(0.29803922f, 0.29f, 0.8f, 1f        ),
            new Color(0.41614764f, 0.4116263f , 0.59767782f, 1f        ),
            new Color(0.53425606f, 0.37619377f, 0.50515955f, 1f        ),
            new Color(0.65236448f, 0.34076125f, 0.41264129f, 1f        ),
            new Color(0.76862745f, 0.30588235f, 0.32156863f, 1f        )
       };

        mat1[1] = option1.GetComponent<OptMaterials>().GetForceField(colors[0], 1);
        mat2[1] = option1.GetComponent<OptMaterials>().GetForceField(colors[4], 2);

        option1.GetComponent<MeshRenderer>().materials = mat1;
        option2.GetComponent<MeshRenderer>().materials = mat2;

        //option2.GetComponent<MeshRenderer>().material.mainTexture = symbol2;

        //material[] mats = r.materials;  // copy of materials array.

        //mats[n] = newMaterial; // set new material

        //r.materials = mats; // assign updated array to materials array
        //Material mat1 = option1.GetComponent<OptMaterials>().GetForceField(new Color(0, 0, 0), 1);
        //option1.GetComponent<MeshRenderer>().material = mat1;
        //Material mat2 = option2.GetComponent<OptMaterials>().GetForceField(new Color(255, 255, 255), 2);
        //option2.GetComponent<MeshRenderer>().material = mat2;


        //option1.GetComponent<MeshRenderer>().material[1].SetFloat("_Color", color);


    }


    public void SpawnOptions()
    {
        Quaternion spawnRotation = Quaternion.identity;

        float leftright;

        if (Random.value < 0.5f)
        {
            leftright = spawnValues.x;
        }
        else
        {
            leftright = -spawnValues.x;
        }

        Vector3 spawnPosition1 = new Vector3(leftright, spawnValues.y, spawnValues.z);
        option1 = Instantiate(hazard, spawnPosition1, spawnRotation);
        option1.tag = "Opt1";

        Vector3 spawnPosition2 = new Vector3(-leftright, spawnValues.y, spawnValues.z);
        option2 = Instantiate(hazard, spawnPosition2, spawnRotation);
        option2.tag = "Opt2";

    }


}
// ------------------------------------------------------------------------------------------------//
// State Machine (to put in its own controller later)
// ------------------------------------------------------------------------------------------------ //

public interface IState
{
    void Enter();
    bool IsDone();
    IEnumerator Execute();
    void Exit();
}


public class StateMachine
{
    public IState currentState;
    private GameController owner;
    public List<IState> states;

    int stateNumber;


    public StateMachine(GameController owner)
    {
        this.owner = owner;
        states = new List<IState>();
        states.Add(new LearningTest());
        states.Add(new TransferTest());

        stateNumber = -1;
    }


    public void ChangeState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null && !CurrentStateIsDone())
        {
            owner.StartCoroutine(currentState.Execute());
        }
    }

    public void NextState()
    {
        stateNumber += 1;

        if (stateNumber < states.Count)
        {
            ChangeState(states[stateNumber]);

        }
        else
        {
            Debug.Log("Last state reached");
        }
    }

    public bool CurrentStateIsDone()
    {
        return currentState.IsDone();
    }
}


public class LearningTest : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering learning test");

    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {

        int[] condTrial = new int[TaskParameters.nConds];

        for (int t = 0; t < TaskParameters.nTrials; t++)
        {

            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }

            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int)TaskParameters.conditionIdx[t];

            gameController.feedbackInfo = (int)TaskParameters.conditions[cond][2];

            gameController.SpawnOptions();
            gameController.SetSymbolsTexture(TaskParameters.symbols[cond]);


            gameController.SetOutcomes(
                TaskParameters.rewards[cond * 2][condTrial[cond]],
                TaskParameters.rewards[cond * 2 + 1][condTrial[cond]]);

            condTrial[cond]++;


            gameController.AllowWave(false);
            gameController.AllowSendData(false);

            while (!gameController.sendData)
            {
                yield return new WaitForSeconds(.5f);

            }
            // once the option is shot we can get the option controller and gather the data 
            OptionController optionController = gameController.GetOptionController();
            PlayerController playerController = gameController.GetPlayerController();

            gameController.Save("con", (int)cond + 1);
            gameController.Save("t", t);
            gameController.Save("session", 1);

            gameController.Save("choice", (int)optionController.choice);
            gameController.Save("outcome", (int)optionController.scoreValue);
            gameController.Save("cfoutcome", (int)optionController.counterscoreValue);
            gameController.Save("rt", (int)optionController.st.ElapsedMilliseconds);
            gameController.Save("choseLeft", (int)optionController.choseLeft);
            gameController.Save("corr", (int)optionController.corr);

            gameController.Save("fireCount", (int)playerController.fireCount);
            gameController.Save("upCount", (int)playerController.upCount);
            gameController.Save("downCount", (int)playerController.downCount);
            gameController.Save("leftCount", (int)playerController.leftCount);
            gameController.Save("rightCount", (int)playerController.rightCount);

            gameController.Save("prolificID", gameController.subID);
            gameController.Save("feedbackInfo", (int)gameController.feedbackInfo);
            gameController.Save("missedTrial", (int)gameController.missedTrial);
            gameController.Save("score", (int)gameController.score);
            gameController.Save("optFile1",
                 (string)TaskParameters.symbols[cond][0].ToString() + ".tiff");
            gameController.Save("optFile2",
                 (string)TaskParameters.symbols[cond][1].ToString() + ".tiff");
            //gameController.Save("optFile2", (string)gameController.symbol2.ToString());


            // retrieve probabilities
            float p1 = TaskParameters.GetOption(cond, 1)[1];
            float p2 = TaskParameters.GetOption(cond, 2)[1];

            gameController.Save("p1", (float)p1);
            gameController.Save("p2", (float)p2);

            yield return gameController.SendToDB();

        }

        isDone = true;
    }


    public void Exit()
    {
        Debug.Log("Exiting learning test");

    }
}



public class TransferTest : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering transfer test");

    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(1.5f);
        //gameController.ChangeBackground();
        //yield return new WaitForSeconds(1.5f);
        int[] condTrial = new int[TaskParameters.nConds];


        for (int t = 0; t < TaskParameters.nTrials; t++)
        {
            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }


            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int)TaskParameters.conditionTransferIdx[t];

            gameController.feedbackInfo = (int)TaskParameters.conditionsTransfer[cond][2];

            gameController.SpawnOptions();
            gameController.SetSymbolsTexture(TaskParameters.symbolsTransfer[cond]);

            gameController.SetOutcomes(
                TaskParameters.rewardsTransfer[cond * 2][condTrial[cond]],
                TaskParameters.rewardsTransfer[cond * 2 + 1][condTrial[cond]]);
            condTrial[cond]++;

            gameController.AllowWave(false);
            gameController.AllowSendData(false);

            while (!gameController.sendData)
            {
                yield return new WaitForSeconds(.5f);

            }

            // once the option is shot we can get the option controller and gather the data 
            OptionController optionController = gameController.GetOptionController();
            PlayerController playerController = gameController.GetPlayerController();

            gameController.Save("con", (int)cond + 1);
            gameController.Save("t", t);
            gameController.Save("session", 2);

            gameController.Save("choice", (int)optionController.choice);
            gameController.Save("outcome", (int)optionController.scoreValue);
            gameController.Save("cfoutcome", (int)optionController.counterscoreValue);
            gameController.Save("rt", (int)optionController.st.ElapsedMilliseconds);
            gameController.Save("choseLeft", (int)optionController.choseLeft);
            gameController.Save("corr", (int)optionController.corr);


            gameController.Save("fireCount", (int)playerController.fireCount);
            gameController.Save("upCount", (int)playerController.upCount);
            gameController.Save("downCount", (int)playerController.downCount);
            gameController.Save("leftCount", (int)playerController.leftCount);
            gameController.Save("rightCount", (int)playerController.rightCount);

            gameController.Save("prolificID", gameController.subID);
            gameController.Save("feedbackInfo", (int)gameController.feedbackInfo);
            gameController.Save("missedTrial", (int)gameController.missedTrial);
            gameController.Save("score", (int)gameController.score);
            gameController.Save("optFile1",
                 (string)TaskParameters.symbolsTransfer[cond][0].ToString() + ".tiff");
            gameController.Save("optFile2",
                 (string)TaskParameters.symbolsTransfer[cond][1].ToString() + ".tiff");


            // retrieve probabilities
            float p1 = TaskParameters.GetOptionTransfer(cond, 1)[1];
            float p2 = TaskParameters.GetOptionTransfer(cond, 2)[1];

            gameController.Save("p1", (float)p1);
            gameController.Save("p2", (float)p2);

            yield return gameController.SendToDB();
        }
        isDone = true;
    }


    public void Exit()
    {
        Debug.Log("Exiting transfer test");
        gameController.SetGameOver();

    }


}
