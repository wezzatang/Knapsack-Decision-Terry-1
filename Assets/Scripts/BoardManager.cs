using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour
{
    // Resoultion width and Height
    // CAUTION! Modifying this does not modify the Screen resolution.
    // This is related to the unit grid on Unity.
    public static int resolutionWidth = 1600;
    public static int resolutionHeight = 900;
    public static int bottommargin = 100;
    public static int centremargin = 200;


    // Number of Columns and rows of the grid (the possible positions of the items).
    // 1920 x 1080; 16:9; 100 pixels should be sufficient
    public static int columns;
    public static int rows;

    //Prefab of the item interface configuration
    public static GameObject KSItemPrefab;
    public static GameObject WeightLimitPrefab;

    //A canvas where all the board is going to be placed
    private GameObject canvas;

    //If randomization of buttons:
    //1: No/Yes 0: Yes/No
    public static int ReverseButtons;

    // Current counters
    public Text ValueText;
    public Text WeightText;
    public Text WeightLeft;
    public Text TooHeavy;

    //The possible positions of the items;
    private List<Vector2> gridPositions = new List<Vector2>();

    //Weights and value vectors for this trial. CURRENTLY SET UP TO ALLOW ONLY INTEGERS.
    //ws and vs must be of the same length
    public static int[] ws;
    public static int[] vs;
    public static int nitems;
    public static string question;
    // to record optimal value
    public static int solution;

    // A list to store the previous item numbers
    public static List<int> previousitems = new List<int>();

    // Reset button
    public Button Reset;

    // Answer button
    public Button Answer;

    //These variables shouldn't be modified. They just state that the area of the value part of the item and the weight part are assumed to be 1.
    public static float minAreaBill = 1f;
    public static float minAreaWeight = 1f;

    //The total area of all the items. Separated by the value part and the weighy part. A good initialization for this variables is the number of items plus 1.
    public static int totalAreaBill = 8;
    public static int totalAreaWeight = 8;


    //Structure with the relevant parameters of an item.
    //gameItem: is the game object
    //coorValue1: The coordinates of one of the corners of the encompassing rectangle of the Value Part of the Item. The coordinates are taken relative to the center of the item.
    //coorValue2: The coordinates of the diagonally opposite corner of the Value Part of the Item.
    //coordWeight1 and coordWeight2: Same as before but for the weight part of the item.
    private struct Item
    {
        public GameObject gameItem;
        public Vector2 center;
        public int ItemNumber;
        public Button ItemButton;
    }

    //The items for the scene are stored here.
    private static Item[] items;
    

    // The list of all the button clicks. Each event contains the following information:
    // ItemNumber (a number between 1 and the number of items.)
    // Item is being selected In=1; Out=0 
    // Time of the click with respect to the beginning of the trial 
    public static List<Click> itemClicks = new List<Click>();

    public struct Click
    {
        // itemnumber (itemnumber or 100=Reset). State: In(1)/Out(0)/Invalid(2)/Reset(3). Time in seconds
        public int ItemNumber;
        public int State;
        public float time;
    }

    // To keep track of the number of items visited
    public static int itemsvisited = 0;

    // Current Instance number
    public static int currInstance;

    /// Macro function that initializes the Board
    public void SetupTrial()
    {
        previousitems.Clear();
        itemClicks.Clear();
        GameManager.valueValue = 0;
        GameManager.weightValue = 0;
        itemsvisited = 0;
        
        canvas = GameObject.Find("Canvas");

        SetKPInstance();

        //If the bool returned by LayoutObjectAtRandom() is false, then retry again:
        //Destroy all items. Initialize list again and try to place them once more.
        int nt = 0;
        bool itemsPlaced = false;
        while (nt <= 10 && !itemsPlaced)
        {
            GameObject[] items1 = GameObject.FindGameObjectsWithTag("Item");

            foreach (GameObject item in items1)
            {
                Destroy(item);
            }
            
            InitialiseList();
            itemsPlaced = LayoutObjectAtRandom();
            nt++;
        }


        if (itemsPlaced == false)
        {
            GameManager.ErrorInScene("Not enough space to place all items after" + nt + "tries");
        }
    }
    //Initializes the instance for this trial:
    //1. Sets the question string using the instance (from the .txt files)
    //2. The weight and value vectors are uploaded
    //3. The instance prefab is uploaded
    void SetKPInstance()
    {
        KSItemPrefab = (GameObject)Resources.Load("KSItem3");
        WeightLimitPrefab = (GameObject)Resources.Load("BigText");

        currInstance = GameManager.instanceRandomization[GameManager.TotalTrials - 1];
        question = "$" + GameManager.kpinstances[currInstance].profit + 
            Environment.NewLine + GameManager.kpinstances[currInstance].capacity + "kg";
    
        
        ws = GameManager.kpinstances[currInstance].weights;
        vs = GameManager.kpinstances[currInstance].values;

        solution = GameManager.kpinstances[currInstance].solution;
        
        // Display current value
        ValueText = GameObject.Find("ValueText").GetComponent<Text>();

        // Display current weight
        WeightText = GameObject.Find("WeightText").GetComponent<Text>();

        // Display current weight left
        WeightLeft = GameObject.Find("WeightLeft").GetComponent<Text>();

        // Show when weight is excessive
        TooHeavy = GameObject.Find("TooHeavy").GetComponent<Text>();

        // make reset button clickable
        Reset = GameObject.Find("Reset").GetComponent<Button>();
        Reset.onClick.AddListener(ResetClicked);

        // make answer button clickable
        Answer = GameObject.Find("Answer").GetComponent<Button>();
        Answer.onClick.AddListener(FinishTrial);

        SetTopRowText();
        
        // set question text
        Text Quest = GameObject.Find("Question").GetComponent<Text>();
        Quest.text = question;

        // Disable answer button if this is optimisation.
        if (GameManager.decision == 0)
        {
            GameObject answerbutton = GameObject.Find("Answer") as GameObject;
            answerbutton.SetActive(false);
        }
    }

    //This Initializes the GridPositions which are the possible places where the items will be placed.
    void InitialiseList()
    {
        gridPositions.Clear();

        // "Completely-Random" Grid
        for (int x = -resolutionWidth / 2; x < resolutionWidth / 2; x += resolutionWidth / columns)
        {
            for (int y = -resolutionHeight / 2 + bottommargin; y < resolutionHeight / 2; y += ((resolutionHeight - bottommargin) / rows))
            {
                if (Math.Abs(x) > centremargin || Math.Abs(y) > centremargin)
                {
                    gridPositions.Add(new Vector2(x, y));
                }
            }
        }

        //Debug.Log("Number of possible positions: " + gridPositions.Count);
    }


    //Returns a random position from the grid and removes the item from the list.
    Vector2 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector2 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }

    // Places all the objects from the instance (ws,vs) on the canvas. 
    // Returns TRUE if all items where positioned, FALSE otherwise.
    private bool LayoutObjectAtRandom()
    {
        int objectCount = ws.Length;
        items = new Item[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            bool objectPositioned = false;

            Item itemToLocate = GenerateItem(i, new Vector2(-2000, -2000));
            //Debug.Log("Local: " + itemToLocate.gameItem.transform.localPosition);
            //Debug.Log("Global: " + itemToLocate.gameItem.transform.position);
            while (!objectPositioned && gridPositions.Count > 0)
            {
                Vector2 randomPosition = RandomPosition();
                //Instantiates the item and places it.

                itemToLocate.gameItem.transform.localPosition = randomPosition;
                itemToLocate.center = new Vector2(itemToLocate.gameItem.transform.localPosition.x,
                    itemToLocate.gameItem.transform.localPosition.y);

                items[i] = itemToLocate;
                objectPositioned = true;
            }

            if (!objectPositioned)
            {
                Debug.Log("Not enough space to place all items... " +
                    "ran out of randomPositions");
                return false;
            }
        }
        return true;
    }


    /// <summary>
    /// Instantiates an Item and places it on the position from the input
    /// </summary>
    /// <returns>The item structure</returns>
    /// The item placing here is temporary; The real placing is done by the placeItem() method.
    Item GenerateItem(int itemNumber, Vector2 tempPosition)
    {

        //Instantiates the item and places it.
        GameObject instance = Instantiate(KSItemPrefab, tempPosition, 
            Quaternion.identity) as GameObject;
        instance.transform.SetParent(canvas.GetComponent<Transform>(), false);
        
        //Gets the subcomponents of the item 
        GameObject bill = instance.transform.Find("Bill").gameObject;
        GameObject weight = instance.transform.Find("Weight").gameObject;

        //Sets the Text of the items
        bill.GetComponentInChildren<Text>().text = "$" + vs[itemNumber];
        weight.GetComponentInChildren<Text>().text = "" + ws[itemNumber] + "kg";

        // Calculates the area of the Value and Weight sections of the item according to approach 2 
        // and then Scales the sections so they match the corresponding area.
        Vector3 curr_billscale = bill.transform.localScale;
        float billscale = (float) Math.Pow(vs[itemNumber] / vs.Average(), 0.6) * curr_billscale.x;

        if (billscale < 0.8f * curr_billscale.x)
        {
            billscale = 0.8f * curr_billscale.x;
        } else if (billscale > 1.2f * curr_billscale.x)
        {
            billscale = 1.2f * curr_billscale.x;
        }

        bill.transform.localScale = new Vector3(billscale,
            billscale, billscale);
        
        Vector3 curr_weightscale = weight.transform.localScale;
        float weightscale = (float)Math.Pow(ws[itemNumber] / ws.Average(), 0.6) * curr_weightscale.x;

        if (weightscale < 0.8f * curr_weightscale.x)
        {
            weightscale = 0.8f * curr_weightscale.x;
        }
        else if (weightscale > 1.2f * curr_weightscale.x)
        {
            weightscale = 1.2f * curr_weightscale.x;
        }

        weight.transform.localScale = new Vector3(weightscale,
            weightscale, weightscale);

        Item itemInstance = new Item
        {
            gameItem = instance,
        };

        itemInstance.ItemButton = itemInstance.gameItem.GetComponent<Button>();
        itemInstance.ItemNumber = itemNumber;

        itemInstance.ItemButton.onClick.AddListener(delegate {
            GameManager.gameManager.boardScript.ClickOnItem(itemInstance); });

        return (itemInstance);
    }

    void ClickOnItem(Item itemToLocate)
    {
        // Check if click is valid
        // Debug.Log("Item Clicked: " + itemToLocate.ItemNumber);
        Light myLight = itemToLocate.gameItem.GetComponent<Light>();

        if (myLight.enabled == true)
        {
            if ((GameManager.weightValue - ws[itemToLocate.ItemNumber]) <=
            GameManager.kpinstances[currInstance].capacity)
            {
                TooHeavy.text = " ";
            }
            myLight.enabled = false;
            RemoveItem(itemToLocate);
        }
        else
        {
            if (ClickValid(itemToLocate) == true)
            {
                myLight.enabled = true;
                AddItem(itemToLocate);
            }
        }
    }

    bool ClickValid(Item itemToLocate)
    {
         if ((GameManager.weightValue + ws[itemToLocate.ItemNumber]) >
            GameManager.kpinstances[currInstance].capacity)
        {
            TooHeavy.text = "Weight Limit Exceeded!";
            WeightLeft.text = "Exceeded";
        }
        else
        {
            TooHeavy.text = " ";
            WeightLeft.text = "Excess Capacity: " + (GameManager.kpinstances[currInstance].capacity -
            GameManager.weightValue).ToString() + "kg";
        }

        return true;
    }
   
    //Updates the timer rectangle size accoriding to the remaining time.
    public void UpdateTimer()
    {
        Image timer = GameObject.Find("Timer").GetComponent<Image>();
        timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
    }

    //Sets the triggers for pressing the corresponding keys
    //123: Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
    //changeToNextScene(answer) on game manager
    private void SetKeyInput()
    {

      if (GameManager.escena == "TrialAnswer")
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                //Left
                AnswerSelect("left");
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                //Right
                AnswerSelect("right");
            }
        }
        else if (GameManager.escena == "SetUp")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.SetTimeStamp();
                GameManager.ChangeToNextScene(itemClicks, false);
            }
        }
    }

    public void SetupInitialScreen()
    {
        //Button
        GameObject start = GameObject.Find("Start") as GameObject;
        start.SetActive(false);
        
        GameObject rand = GameObject.Find("RandomisationID") as GameObject;
        rand.SetActive(false);

        //Participant Input
        InputField pID = GameObject.Find("ParticipantID").GetComponent<InputField>();

        InputField.SubmitEvent se = new InputField.SubmitEvent();
        se.AddListener((value) => SubmitPID(value, start, rand));
        pID.onEndEdit = se;

        //Randomisation Input
        InputField rID = rand.GetComponent<InputField>();

        InputField.SubmitEvent se2 = new InputField.SubmitEvent();
        se2.AddListener((value) => SubmitRandID(value, start));
        rID.onEndEdit = se2;
    }

    private void SubmitPID(string pIDs, GameObject start, GameObject rand)
    {
        GameObject pID = GameObject.Find("ParticipantID");
        GameObject pIDT = GameObject.Find("ParticipantIDText");
        pID.SetActive(false);
        pIDT.SetActive(true);

        Text inputID = pIDT.GetComponent<Text>();
        inputID.text = "Randomisation Number";

        //Set Participant ID
        IOManager.participantID = pIDs;

        //Activate Randomisation Listener
        rand.SetActive(true);

    }

    private void SubmitRandID(string rIDs, GameObject start)
    {
        GameObject rID = GameObject.Find("RandomisationID");
        GameObject pIDT = GameObject.Find("ParticipantIDText");
        rID.SetActive(false);
        pIDT.SetActive(false);

        //Set Participant ID
        IOManager.randomisationID = rIDs;

        //Activate Start Button and listener
        start.SetActive(true);

        Button startButton = GameObject.Find("Start").GetComponent<Button>();
        startButton.onClick.AddListener(StartClicked);
    }


    public static string GetItemCoordinates()
    {
        string coordinates = "";
        foreach (Item it in items)
        {
            //Debug.Log("item");
            //Debug.Log(it.center);
            //Debug.Log(it.coordWeight1);
            coordinates = coordinates + "(" + it.center.x + "," + it.center.y + ")";
        }
        return coordinates;
    }

    public static void StartClicked()
    {
        Debug.Log("Start Button Clicked");
        GameManager.SetTimeStamp();
        GameManager.ChangeToNextScene(BoardManager.itemClicks, false);
    }

    // Function to display distance and weight in Unity
    void SetTopRowText()
    {
        CalcValue();
        ValueText.text = "Current Value: $" + GameManager.valueValue.ToString();

        CalcWeight();
        WeightText.text = "Current Weight: " + GameManager.weightValue.ToString() + "kg";
        if((GameManager.kpinstances[currInstance].capacity - GameManager.weightValue) > 0)
        {
            WeightLeft.text = "Excess Capacity: " + (GameManager.kpinstances[currInstance].capacity -
                GameManager.weightValue).ToString() + "kg";
        }
    }


    // Add current item to previous items
    void AddItem(Item itemToLocate)
    {
        previousitems.Add(itemToLocate.ItemNumber);
        itemsvisited = previousitems.Count();

        Click newclick;
        newclick.ItemNumber = itemToLocate.ItemNumber;
        newclick.State = 1;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    // Remove current item from previous items
    void RemoveItem(Item itemToLocate)
    {
        previousitems.Remove(itemToLocate.ItemNumber);
        itemsvisited = previousitems.Count();

        Click newclick;
        newclick.ItemNumber = itemToLocate.ItemNumber;
        newclick.State = 0;
        newclick.time = GameManager.timeQuestion - GameManager.tiempo;
        itemClicks.Add(newclick);
    }

    // Function to calculate total distance thus far
    public void CalcValue()
    {
        int[] individualvalues = new int[previousitems.Count()];
        if (previousitems.Count() > 0)
        {
            for (int i = 0; i <= (previousitems.Count() - 1); i++)
            {
                individualvalues[i] = vs[previousitems[i]];
            }

            GameManager.valueValue = individualvalues.Sum();
        }
        else
        {
            GameManager.valueValue = 0;
        }
    }

    // Function to calculate total WCSPP weight thus far
    public void CalcWeight()
    {
        int[] individualweights = new int[previousitems.Count()];
        if (previousitems.Count() > 0)
        {
            for (int i = 0; i <= (previousitems.Count() - 1); i++)
            {
                individualweights[i] = ws[previousitems[i]]; ;
            }

            GameManager.weightValue = individualweights.Sum();
        }
        else
        {
            GameManager.weightValue = 0;
        }
    }


    public void ResetClicked()
    {
        if (previousitems.Count() != 0)
        {
            Lightoff();
            previousitems.Clear();
            SetTopRowText();
            itemsvisited = 0;

            Click newclick;
            newclick.ItemNumber = 100;
            newclick.State = 2;
            newclick.time = GameManager.timeQuestion - GameManager.tiempo;
            itemClicks.Add(newclick);
        }
    }

    //Randomizes YES/NO button positions (left or right) and allocates corresponding script to save the correspondent answer.
    public static void RandomizeButtons(){
		Button btnLeft = GameObject.Find("Left").GetComponent<Button>();
		Button btnRight = GameObject.Find("Right").GetComponent<Button>();

        btnLeft.onClick.AddListener(delegate {
            AnswerSelect("left");
        });
        btnRight.onClick.AddListener(delegate {
            AnswerSelect("right");
        });

        ReverseButtons = Random.Range(0, 2);

		if (ReverseButtons == 1) {
			btnLeft.GetComponentInChildren<Text>().text = "No";
			btnRight.GetComponentInChildren<Text>().text = "Yes";
		} else {
			btnLeft.GetComponentInChildren<Text>().text = "Yes";
			btnRight.GetComponentInChildren<Text>().text = "No";
		}
	}

    public static void FinishTrial()
    {
        Debug.Log("Skipped to answer screen");
        IOManager.SaveTimeStamp("ParticipantSkip");
        GameManager.ChangeToNextScene(itemClicks, true);
    }

    public static void AnswerSelect(string LeftOrRight)
    {
        if ((LeftOrRight == "left" && ReverseButtons == 1) || (LeftOrRight == "right" && ReverseButtons == 0))
        {
            // reversed left, or unreversed right, means answer is NO.
            GameManager.answer = 0;
            Debug.Log("Trial number " + ((GameManager.block - 1) * GameManager.numberOfTrials +
                GameManager.trial) + ", Answer chosen: NO");
            GameManager.ChangeToNextScene(itemClicks, true);
        }
        else
        {
            // reversed right, or unreversed left, means answer is YES.
            GameManager.answer = 1;
            Debug.Log("Trial number " + ((GameManager.block - 1) * GameManager.numberOfTrials +
                GameManager.trial) + ", Answer chosen: YES");
            GameManager.ChangeToNextScene(itemClicks, true);
        }
    }


    private void Lightoff()
    {
        foreach (Item item in items)
        {
            Light myLight = item.gameItem.GetComponent<Light>();
            myLight.enabled = false;
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        SetKeyInput();

        if(GameManager.escena == "Trial")
        {
            SetTopRowText();
        }
    }
}