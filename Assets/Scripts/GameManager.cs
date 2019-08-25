/* Unity 3D program that displays multiple interactive instances of the Knapsack Problem.
 * 
 * Optimal resolution 1920x1080. Future users of this program should consider updating the code to suit higher resolution displays.
 * 
 * Input files are stored in ./StreamingAssets/Input
 * User responses and other data are stored in ./StreamingAssets/Output
 * 
 * Based on Knapsack and TSP code written by Pablo Franco
 * Modifications (July 2019) by Anthony Hsu include:
 * click "Start" button to begin; items clickable; deleted various
 * unused assets and functions; added StreamingAssets folder.
 * 
 * Honours students should make further changes to suit their projects.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Stopwatch to calculate time of events.
    public static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
    // Time at which the stopwatch started. Time of each event is 
    // calculated according to this moment.
    public static string initialTimeStamp;

    // Game Manager: It is a singleton (i.e. it is always one and the 
    // same it is nor destroyed nor duplicated)
    public static GameManager gameManager = null;

    // The reference to the script managing the board (interface/canvas).
    public BoardManager boardScript;

    // Current Scene
    public static string escena;

    // Time spent so far on this scene
    public static float tiempo;

    // Some of the following parameters are a default to be used if 
    // they are not specified in the input files.
    // Otherwise they are rewritten (see loadParameters() )
    // Total time for these scene
    public static float totalTime;

    // Time spent at the instance
    public static float timeTaken;

    // Current trial initialization
    public static int trial = 0;

    // Current block initialization
    public static int block = 0;

    // Total trial
    public static int TotalTrials;

    private static bool showTimer;

    // Modifiable Variables:
    // Minimum and maximum for randomized interperiod Time
    public static float timeRest1min;
    public static float timeRest1max;

    // InterBlock rest time
    public static float timeRest2;

    // Time given for each trial (The total time the items are shown -With and without the question-)
    public static float timeQuestion;
    public static float timeAnswer;

    // IMPORTANT: DECISION or OPTIMISATION KP
    // Game skips answer screen if optimisation is chosen.
    // If Decision, set decision = 1 in x_param2.txt. 
    public static int decision;

    // Total number of trials in each block
    public static int numberOfTrials;

    // Total number of blocks
    public static int numberOfBlocks;

    //Number of instance file to be considered. From i1.txt to i_.txt..
    public static int numberOfInstances;

    //The order of the instances to be presented
    public static int[] instanceRandomization;

    //The order of the left/right No/Yes randomization
    public static int[] buttonRandomization;

    // To record answer in the decision KP
    // 0 if NO
    // 1 if YES
    // 2 if not selected
    // 100 if not applicable. i.e. optimisation KP.
    public static int answer;

    // Skip button in case user does not want a break
    public static Button skipButton;

    // A list of floats to record participant performance
    // Performance should always be equal to or greater than 1.
    // Due to the way it's calculated (participant answer/optimal solution), performance closer to 1 is better.
    public static List<double> perf = new List<double>();
    public static double performance;
    public static List<double> paylist = new List<double>();
    public static double pay;

    // Keep track of total payment
    // Default value is the show up fee
    public static double payAmount = 8.00;

    // current value
    public static int valueValue;

    // current weight
    public static int weightValue;

    // A structure that contains the parameters of each instance
    public struct KPInstance
    {
        public int capacity;
        public int profit;

        public int[] weights;
        public int[] values;

        public string id;
        public string type;

        public int solution;
    }

    // An array of all the instances to be uploaded form .txt files.
    public static KPInstance[] kpinstances;

    // Use this for initialization
    void Awake()
    {
        //Makes the Game manager a Singleton
        if (gameManager == null)
        {
            gameManager = this;
        }
        else if (gameManager != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        //Initializes the game
        boardScript = gameManager.GetComponent<BoardManager>();

        InitGame();
        if (escena != "SetUp")
        {
            IOManager.SaveTimeStamp(escena);
        }

    }


    //Initializes the scene. One scene is setup, other is trial, other is Break....
    void InitGame()
    {
        /*
		Scene Order: escena
		0=setup
		1=trial game
		2= intertrial rest
		3= interblock rest
		4= end
		*/
        Scene scene = SceneManager.GetActiveScene();

        escena = scene.name;

        Debug.Log("Current Scene: " + escena);

        if (escena == "SetUp")
        {
            //Only uploads parameters and instances once.
            boardScript.SetupInitialScreen();
        }

        else if (escena == "Trial")
        {
            trial++;
            TotalTrials = trial + (block - 1) * numberOfTrials;
            showTimer = true;
            boardScript.SetupTrial();


            tiempo = timeQuestion;
            totalTime = timeQuestion;
        }
        else if (escena == "TrialAnswer")
        {
            showTimer = true;
            answer = 2;

            BoardManager.RandomizeButtons();

            tiempo = timeAnswer;
            totalTime = timeAnswer;
        }
        else if (escena == "InterTrialRest")
        {
            showTimer = false;
            tiempo = Random.Range(timeRest1min, timeRest1max);
            totalTime = tiempo;
        }
        else if (escena == "InterBlockRest")
        {
            trial = 0;
            block++;
            showTimer = true;
            tiempo = timeRest2;
            totalTime = tiempo;
            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "End")
        {
            showTimer = false;

            skipButton = GameObject.Find("Skip").GetComponent<Button>();
            skipButton.onClick.AddListener(SkipClicked);
        }
        else if (escena == "Payment")
        {
            showTimer = false;

            Text perf = GameObject.Find("PerfText").GetComponent<Text>();
            perf.text = DisplayPerf();

            Text pay = GameObject.Find("PayText").GetComponent<Text>();
            pay.text = "Total Payment: $" + Math.Ceiling(payAmount).ToString();
        }

    }

    // Function to display user performance (last scene)
    public static string DisplayPerf()
    {
        string perfText = "Performance: ";

        for (int i = 0; i < numberOfInstances; i++)
        {
            // Payment calculation
            perfText += " $" + paylist[i] + ";";
        }
        return perfText;
    }

    // Update is called once per frame
    void Update()
    {

        if (escena != "SetUp")
        {
            StartTimer();
        }
    }
    
    //Takes care of changing the Scene to the next one (Except for when in the setup scene)
    public static void ChangeToNextScene(List<BoardManager.Click> itemClicks, bool skipped)
    {        
        /* Scene Order
         * 0= Setup
         * 1= Trial
         * 2= Intertrial rest
         * 3= Interblock rest
         * 5= End
         * 6= Payment
         */
        if (escena == "SetUp")
        {
            block++;
            IOManager.LoadGame();
            SceneManager.LoadScene("Trial");
        }
        else if (escena == "Trial")
        {
            if (skipped)
            {
                timeTaken = timeQuestion - tiempo;
            }
            else
            {
                timeTaken = timeQuestion;
            }            
            
            // Save participant answer
            // Calc Perf
            if ((float)valueValue > 0)
            {
                performance = (float)valueValue;
            }
            else
            {
                performance = 0;
            }

            perf.Add(performance);

            pay = Math.Pow(performance, 1);

            paylist.Add(pay);

            payAmount += pay;
            Debug.Log("current pay: $" + payAmount);
            
            IOManager.SaveTimeStamp("AnswerScreen");
            IOManager.SaveClicks(itemClicks);

            // Load next scene
            if (decision == 1)
            {
                SceneManager.LoadScene("TrialAnswer");
            }
            else if (decision == 0)
            {
                SceneManager.LoadScene("InterTrialRest");
            }
        }
        else if (escena == "TrialAnswer")
        {
            IOManager.SaveTrialInfo(answer, ExtractItemsSelected(itemClicks), timeTaken, "");

            if (answer != 2)
            {
                IOManager.SaveTimeStamp("ParticipantAnswer");
            }

            SceneManager.LoadScene("InterTrialRest");
        }
        else if (escena == "InterTrialRest")
        {
            ChangeToNextTrial();
        }
        else if (escena == "InterBlockRest")
        {
            SceneManager.LoadScene("Trial");
        }
        else if (escena == "End")
        {
            SceneManager.LoadScene("Payment");
        }

    }
    
    //Redirects to the next scene depending if the trials or blocks are over.
    private static void ChangeToNextTrial()
    {
        //Checks if trials are over
        if (trial < numberOfTrials)
        {
            SceneManager.LoadScene("Trial");
        }
        else if (block < numberOfBlocks)
        {
            SceneManager.LoadScene("InterBlockRest");
        }
        else
        {
            SceneManager.LoadScene("End");
        }
    }
    
    /// <summary>
    /// In case of an error: Skip trial and go to next one.
    /// Example of error: Not enough space to place all items
    /// </summary>
    /// Receives as input a string with the errorDetails which is saved in the output file.
    public static void ErrorInScene(string errorDetails)
    {
        Debug.Log(errorDetails);
        
        IOManager.SaveTrialInfo(answer, ExtractItemsSelected(BoardManager.itemClicks), 
            timeQuestion, errorDetails);
        ChangeToNextTrial();
    }


    // Extracts the items that were finally selected based on the sequence of clicks.
    private static string ExtractItemsSelected(List<BoardManager.Click> itemClicks)
    {
        List<int> itemsIn = new List<int>();
        foreach (BoardManager.Click click in itemClicks)
        {
            if (click.State == 1)
            {
                itemsIn.Add(Convert.ToInt32(click.ItemNumber));
            }
            else if (click.State == 0)
            {
                itemsIn.Remove(Convert.ToInt32(click.ItemNumber));
            }
            else if (click.State == 2)
            {
                itemsIn.Clear();
            }
        }

        string itemsInS = string.Empty;
        foreach (int i in itemsIn)
        {
            itemsInS = itemsInS + i + ",";
        }

        if (itemsInS.Length > 0)
        {
            itemsInS = itemsInS.Remove(itemsInS.Length - 1);
        }

        return itemsInS;
    }

    // Starts the stopwatch. Time of each event is calculated according to this moment.
    // Sets "initialTimeStamp" to the time at which the stopwatch started.
    public static void SetTimeStamp()
    {
        initialTimeStamp = @System.DateTime.Now.ToString("HH-mm-ss-fff");
        stopWatch.Start();
    }

    // Calculates time elapsed
    public static string TimeStamp()
    {
        long milliSec = stopWatch.ElapsedMilliseconds;
        return (milliSec / 1000f).ToString();
    }

    // Updates the timer (including the graphical representation)
    // If time runs out in the trial or the break scene. It switches to the next scene.
    void StartTimer()
    {
        tiempo -= Time.deltaTime;

        if (showTimer)
        {
            boardScript.UpdateTimer();
        }

        // When the time runs out:
        if (tiempo < 0)
        {
            ChangeToNextScene(BoardManager.itemClicks, false);
        }
    }
    
    // Change to next scene if the user clicks skip
    static void SkipClicked()
    {
        Debug.Log("Skip Clicked");
        ChangeToNextScene(BoardManager.itemClicks, true);
    }
}
