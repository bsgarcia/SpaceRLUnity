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

    public GameObject perceptionHazard;
    public List<GameObject> RLHazard = new List<GameObject>(new GameObject[2]);
    public List<GameObject> fullHazard = new List<GameObject>(new GameObject[2]);

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

    public List<GameObject> GetOptions()
    {
        return new List<GameObject>(){option1, option2};
    }
    public OptionController GetOptionController()
    {
        return optionController;
    }
    
    public void DisplayFeedback(bool value)
    {
        optionController.showFeedback = value;

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

        // GetSubID();
        subID = "test";

        //StartCoroutine(SpawnWaves()); 
        dataController = GameObject.FindWithTag("DataController").GetComponent<DataController>();
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        pauseController = GameObject.FindWithTag("PauseController").GetComponent<PauseController>();
        optionController = GameObject.FindWithTag("OptionController").GetComponent<OptionController>();

    }

    public void RunWrapper()
    {
        StartCoroutine(Run());
    }

    public IEnumerator Run()
    {
        playButton.gameObject.SetActive(false);
        moveImage.gameObject.SetActive(true);

        while (!tutorialDone)
        {
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
        if (playButton.gameObject.active)
        {
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


    IEnumerator SetBoolWithDelay(System.Action<bool> assigner, bool value, float delay)
    {
        yield return new WaitForSeconds(delay);
        assigner.Invoke(value);
    }

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        Save("score", (int)score);
        if (feedbackInfo == 0)
        {

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

    public IEnumerator DestroyWithDelay(GameObject toDestroy, float delay)
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
        return;
        GameObject background = GameObject.FindWithTag("Background");
        background.GetComponent<MeshRenderer>().material.mainTexture =
            (Texture)Resources.Load("backgrounds/space");
        GameObject child = background.transform.GetChild(0).gameObject;
        child.GetComponent<MeshRenderer>().material.mainTexture =
        (Texture)Resources.Load("backgrounds/space");
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
        // pick 2 colors from the list (randomly)
        int colorIdx1 = Random.Range(0, colors.Length);
        int colorIdx2 = Random.Range(0, colors.Length);
        Color color1 = colors[colorIdx1];
        Color color2 = colors[colorIdx2];

        return (color1, color2, colorIdx1 + 1, colorIdx2 + 1);
    }

    public void SetForceFields(bool value=true)
    {
        OptionController optionController = GetOptionController();

        if (!value) {
            optionController.forcefield = false;
            return;

        }

        Material[] mat1 = option1.GetComponent<MeshRenderer>().materials;
        Material[] mat2 = option2.GetComponent<MeshRenderer>().materials;

        (Color color1, Color color2, int colorIdx1, int colorIdx2) = GetColor();


        optionController.forcefield = true;

        double[] p = new double[] {0.1, 0.14, 0.18, 0.2, 0.25, .3, .5, .75, .85, .9, 1};
        optionController.SetPDestroy(p[colorIdx1], p[colorIdx2]);

        mat1[1] = option1.GetComponent<OptMaterials>().GetForceField(color1, 1);
        mat2[1] = option1.GetComponent<OptMaterials>().GetForceField(color2, 2);

        option1.GetComponent<MeshRenderer>().materials = mat1;
        option2.GetComponent<MeshRenderer>().materials = mat2;

    }


    public void SpawnOptions(string phase="perception")
    {
        Quaternion spawnRotation = Quaternion.identity; //* Quaternion.Euler(45, 0, 0);


        float leftright;

        if (Random.value < 0.5f)
        {
            leftright = spawnValues.x;
        }
        else
        {
            leftright = -spawnValues.x;
        }
        // fixed 
        leftright = Mathf.Abs(leftright);
        //
        //Vector3 scaleChange = new Vector3(1f, 1f, 1f);

        Vector3 spawnPosition1 = new Vector3(leftright, spawnValues.y, spawnValues.z);

        GameObject hazard1;
        GameObject hazard2;
        // check that phase is either 'perception' or 'RL'
        // use a switch statement
        switch (phase)
        {
            case "perception":
                hazard1 = perceptionHazard;
                hazard2 = perceptionHazard;
                break;
            case "RL":
                hazard1 = RLHazard[0];
                hazard2 = RLHazard[1];
                break;
            case "full":
                hazard1 = fullHazard[0];
                hazard2 = fullHazard[1];
                break;

            default:
                hazard1 = perceptionHazard;
                hazard2 = perceptionHazard;
                break;
        }

        option1 = Instantiate(hazard1, spawnPosition1, spawnRotation);
        //option1.transform.position = spawnPosition1;
        option1.tag = "Opt1";
        //option1.transform.localScale = scaleChange;

        Vector3 spawnPosition2 = new Vector3(-leftright, spawnValues.y, spawnValues.z);
        option2 = Instantiate(hazard2, spawnPosition2, spawnRotation);
        ///option2.transform.position = spawnPosition2;
        //option2.transform.localScale = scaleChange;
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

        states.Add(new TrainingTestRL());
        states.Add(new TrainingTestFull());
        states.Add(new TrainingTestPerception());

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

public class TrainingTestPerception : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering perception training test");

    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {
        gameController.ChangeBackground();

        int[] condTrial = new int[TaskParameters.nConds];

        for (int t = 0; t < TaskParameters.nTrialsTrainingPerception; t++)
        {

            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }

            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int)TaskParameters.conditionIdx[t];

            gameController.feedbackInfo = (int)TaskParameters.conditions[cond][2];

            gameController.SpawnOptions(phase: "perception");
            gameController.SetForceFields();

            gameController.DisplayFeedback(false);
            
            // gameController.GetPlayerController().AllowShot(true);
            
            condTrial[cond]++;


            gameController.AllowWave(false);
            gameController.AllowSendData(false);

            while (!gameController.sendData)
            {
                yield return new WaitForSeconds(.5f);

            }
            
            if (TaskParameters.online) {
                // once the option is shot we can get the option controller and gather the data 
                OptionController optionController = gameController.GetOptionController();
                PlayerController playerController = gameController.GetPlayerController();

                gameController.Save("con", (int)cond + 1);
                gameController.Save("t", t);
                gameController.Save("session", 1);

                gameController.Save("choice", (int)optionController.choice);
                gameController.Save("outcome", (int)optionController.scoreValue);
                gameController.Save("cfoutcome", (int)optionController.counterscoreValue);
                // gameController.Save("rt", (int)optionController.st.ElapsedMilliseconds);
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

            gameController.AllowWave(true);
            yield return new WaitForSeconds(.5f);

        }

        isDone = true;
    }
    public void Exit()
    {
        Debug.Log("Exiting learning test");

    }

}

public class TrainingTestRL : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering RL training test");

    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {
        gameController.ChangeBackground();

        int[] condTrial = new int[TaskParameters.nConds];

        for (int t = 0; t < TaskParameters.nTrialsTrainingRL; t++)
        {

            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }

            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int)TaskParameters.conditionIdx[t];

            gameController.feedbackInfo = (int)TaskParameters.conditions[cond][2];

            gameController.SpawnOptions(phase: "RL");

            gameController.DisplayFeedback(true);
            gameController.SetForceFields(false);
            //gameController.SetForceFields();

            // gameController.GetPlayerController().AllowShot(true);
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
            
            if (TaskParameters.online) {
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
            gameController.AllowWave(true);
        }

        isDone = true;
    }
public void Exit()
    {
        Debug.Log("Exiting learning test");

    }
}

public class TrainingTestFull : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering full training test");

    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {
        gameController.ChangeBackground();

        int[] condTrial = new int[TaskParameters.nConds];

        for (int t = 0; t < TaskParameters.nTrialsTrainingRL; t++)
        {

            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }

            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int)TaskParameters.conditionIdx[t];

            gameController.feedbackInfo = (int)TaskParameters.conditions[cond][2];

            gameController.SpawnOptions(phase: "full");

            gameController.DisplayFeedback(true);
            gameController.SetForceFields(true);
            //gameController.SetForceFields();

            // gameController.GetPlayerController().AllowShot(true);


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
            
            if (TaskParameters.online) {
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
            gameController.AllowWave(true);
        }

        isDone = true;
    }
public void Exit()
    {
        Debug.Log("Exiting learning test");

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
        gameController.ChangeBackground();

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
            gameController.SetForceFields();


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
            gameController.SetForceFields();

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
