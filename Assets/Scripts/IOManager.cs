using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class IOManager : MonoBehaviour
{
    // This is the string that will be used as the file name where
    // the data is stored. Currently the date-time is used.
    public static string participantID;

    // This is the randomisation number (#_param2.txt that is to be used
    // for order of instances for this participant)
    public static string randomisationID;

    public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

    public static string identifierName;

    //Is the question shown on scene 1?
    //private static int questionOn;

    //Input and Outout Folders with respect to the Application.dataPath;
    public static string inputFolder = "/StreamingAssets/Input/";
    public static string inputFolderKPInstances = "/StreamingAssets/Input/KPInstances/";
    public static string outputFolder = "/StreamingAssets/Output/";

    // Complete folder path of inputs and ouputs
    public static string folderPathLoad;
    public static string folderPathLoadInstances;
    public static string folderPathSave;
    
    /*
	 * Loads all of the instances to be uploaded form .txt files. Example of input file:
	 * Name of the file: i3.txt
	 * Structure of each file is the following:
	 * weights:[2,5,8,10,11,12]
	 * values:[10,8,3,9,1,4]
	 * capacity:15
	 * profit:16
	 *
	 * The instances are stored as kpinstances structures in the array of structures: kpinstances
	 */
    public static void LoadGame()
    {
        folderPathLoad = Application.dataPath + inputFolder;
        folderPathLoadInstances = Application.dataPath + inputFolderKPInstances;
        folderPathSave = Application.dataPath + outputFolder;
        LoadParameters();
        
        GameManager.kpinstances = new GameManager.KPInstance[GameManager.numberOfInstances];

        for (int k = 1; k <= GameManager.numberOfInstances; k++)
        {
            var dict = new Dictionary<string, string>();

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(folderPathLoadInstances + "i" + k + ".txt"))
                {

                    string line;
                    while (!string.IsNullOrEmpty((line = sr.ReadLine())))
                    {
                        string[] tmp = line.Split(new char[] { ':' }, 
                            StringSplitOptions.RemoveEmptyEntries);

                        // Add the key-value pair to the dictionary:
                        dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                    }
                    // Read the stream to a string, and write the string to the console.
                    //String line = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Debug.Log("The file could not be read: " + e.Message);
            }
            dict.TryGetValue("weights", out string weightsS);
            dict.TryGetValue("values", out string valuesS);
            dict.TryGetValue("capacity", out string capacityS);
            dict.TryGetValue("profit", out string profitS);
            dict.TryGetValue("solution", out string solutionS);

            GameManager.kpinstances[k - 1].weights = 
                Array.ConvertAll(weightsS.Substring(1, weightsS.Length - 2).Split(','), int.Parse);
            GameManager.kpinstances[k - 1].values = 
                Array.ConvertAll(valuesS.Substring(1, valuesS.Length - 2).Split(','), int.Parse);
            GameManager.kpinstances[k - 1].capacity = int.Parse(capacityS);
            GameManager.kpinstances[k - 1].profit = int.Parse(profitS);
            GameManager.kpinstances[k - 1].solution = int.Parse(solutionS);

            dict.TryGetValue("problemID", out GameManager.kpinstances[k - 1].id);
            dict.TryGetValue("instanceType", out GameManager.kpinstances[k - 1].type);

        }
    }

    //Loads the parameters form the text files: param.txt and layoutParam.txt
    private static void LoadParameters()
    {
        var dict = new Dictionary<string, string>();

        try
        {   // Open the text file using a stream reader.
            using (StreamReader sr = new StreamReader(folderPathLoad + "layoutParam.txt"))
            {
                // (This loop reads every line until EOF or the first blank line.)
                string line;
                while (!string.IsNullOrEmpty((line = sr.ReadLine())))
                {
                    // Split each line around ':'
                    string[] tmp = line.Split(new char[] { ':' }, 
                        StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                }
            }


            using (StreamReader sr1 = new StreamReader(folderPathLoad + "param.txt"))
            {

                // (This loop reads every line until EOF or the first blank line.)
                string line1;
                while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
                {
                    // Split each line around ':'
                    string[] tmp = line1.Split(new char[] { ':' }, 
                        StringSplitOptions.RemoveEmptyEntries);
                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("The file could not be read: " + e.Message);
        }


        try
        {
            using (StreamReader sr2 = new StreamReader(folderPathLoadInstances + 
                randomisationID + "_param2.txt"))
            {
                // (This loop reads every line until EOF or the first blank line.)
                string line2;
                while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
                {
                    // Split each line around ':'
                    string[] tmp = line2.Split(new char[] { ':' }, 
                        StringSplitOptions.RemoveEmptyEntries);

                    // Add the key-value pair to the dictionary:
                    dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("The file could not be read: " + e.Message);
        }

        AssignVariables(dict);
    }


    //Assigns the parameters in the dictionary to variables
    private static void AssignVariables(Dictionary<string, string> dictionary)
    {

        //Assigns Parameters
        dictionary.TryGetValue("timeRest1min", out string timeRest1minS);
        dictionary.TryGetValue("timeRest1max", out string timeRest1maxS);
        dictionary.TryGetValue("timeRest2", out string timeRest2S);
        dictionary.TryGetValue("timeQuestion", out string timeQuestionS);
        dictionary.TryGetValue("timeAnswer", out string timeAnswerS);

        dictionary.TryGetValue("decision", out string decisionS);
        dictionary.TryGetValue("numberOfTrials", out string numberOfTrialsS);
        dictionary.TryGetValue("numberOfBlocks", out string numberOfBlocksS);
        dictionary.TryGetValue("numberOfInstances", out string numberOfInstancesS);
        dictionary.TryGetValue("instanceRandomization", out string instanceRandomizationS);
        
        GameManager.timeRest1min = Convert.ToSingle(timeRest1minS);
        GameManager.timeRest1max = Convert.ToSingle(timeRest1maxS);
        GameManager.timeRest2 = Convert.ToSingle(timeRest2S);
        GameManager.timeQuestion = int.Parse(timeQuestionS);
        GameManager.timeAnswer = int.Parse(timeAnswerS);

        GameManager.decision = int.Parse(decisionS);
        GameManager.numberOfTrials = int.Parse(numberOfTrialsS);
        GameManager.numberOfBlocks = int.Parse(numberOfBlocksS);
        GameManager.numberOfInstances = int.Parse(numberOfInstancesS);
        
        int[] instanceRandomizationNo0 = 
            Array.ConvertAll(instanceRandomizationS.Substring(1, 
            instanceRandomizationS.Length - 2).Split(','), int.Parse);

        Debug.Log(instanceRandomizationNo0.Length);
        GameManager.instanceRandomization = new int[instanceRandomizationNo0.Length];

        for (int i = 0; i < instanceRandomizationNo0.Length; i++)
        {
            GameManager.instanceRandomization[i] = instanceRandomizationNo0[i] - 1;
        }
        
        ////Assigns LayoutParameters
        dictionary.TryGetValue("columns", out string columnsS);
        dictionary.TryGetValue("rows", out string rowsS);
        
        BoardManager.columns = Int32.Parse(columnsS);
        BoardManager.rows = Int32.Parse(rowsS);
    }

    /// <summary>
    /// Saves the headers for both files (Trial Info and Time Stamps)
    /// In the trial file it saves:  1. The participant ID. 2. Instance details.
    /// In the TimeStamp file it saves: 1. The participant ID. 
    /// 2.The time onset of the stopwatch from which the time stamps are measured. 
    /// 3. the event types description.
    /// </summary>
    private static void SaveHeaders()
    {
        identifierName = participantID + "_" + dateID + "_" + "Dec" + "_";
        string folderPathSave = Application.dataPath + outputFolder;


        //Saves InstanceInfo
        string[] lines3 = new string[GameManager.numberOfInstances + 2];
        lines3[0] = "PartcipantID:" + participantID;
        lines3[1] = "instanceNumber" + ";c" + ";p" + ";w" + ";v" + ";id" + ";type" + ";sol";
        int l = 2;
        int ksn = 1;
        foreach (GameManager.KPInstance ks in GameManager.kpinstances)
        {
            //With instance type and problem ID
            lines3[l] = ksn + ";" + ks.capacity + ";" + ks.profit + ";" + 
                string.Join(",", ks.weights.Select(p => p.ToString()).ToArray()) + 
                ";" + string.Join(",", ks.values.Select(p => p.ToString()).ToArray())
                + ";" + ks.id + ";" + ks.type + ";" + ks.solution;
            l++;
            ksn++;
        }
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + 
            identifierName + "InstancesInfo.txt", true))
        {
            foreach (string line in lines3)
                outputFile.WriteLine(line);
        }


        // Trial Info file headers
        string[] lines = new string[2];
        lines[0] = "PartcipantID:" + participantID;
        lines[1] = "block;trial;answer;correct;timeSpent;instanceNumber;xyCoordinates;error";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + 
            identifierName + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }

        // Time Stamps file headers
        string[] lines1 = new string[3];
        lines1[0] = "PartcipantID:" + participantID;
        lines1[1] = "InitialTimeStamp:" + GameManager.initialTimeStamp;
        lines1[2] = "block;trial;eventType;elapsedTime";
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + 
            identifierName + "TimeStamps.txt", true))
        {
            foreach (string line in lines1)
                outputFile.WriteLine(line);
        }
    }

    // Saves the data of a trial to a .txt file with the participants ID as 
    // filename using StreamWriter.
    // If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
    // Each line in the File has the following structure: "trial;answer;timeSpent".
    // itemsSelected in the final solutions (irrespective if it was submitted); 
    // xycorrdinates; Error message if any.".
    public static void SaveTrialInfo(int answer, string itemsSelected, float timeSpent, string error)
    {
        string xyCoordinates = BoardManager.GetItemCoordinates();

        // Get the instance n umber for this trial and add 1 because the 
        // instanceRandomization is linked to array numbering in C#, which starts at 0;
        int instanceNum = GameManager.instanceRandomization[GameManager.TotalTrials - 1] + 1;

        int solutionQ = GameManager.kpinstances[instanceNum - 1].solution;

        string dataTrialText = GameManager.block + ";" + GameManager.trial + 
            ";" + itemsSelected + ";" + timeSpent + ";" + BoardManager.ReverseButtons + ";" + instanceNum + ";" 
            + xyCoordinates + ";" + error;

        string[] lines = { dataTrialText };
        string folderPathSave = Application.dataPath + outputFolder;

        // This location can be used by unity to save a file if u open the 
        // game in any platform/computer: Application.persistentDataPath;

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + 
            identifierName + "TrialInfo.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }

        //Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
    }

    /// <summary>
    /// Saves the time stamp for a particular event type to the "TimeStamps" File
    /// </summary>
    /// Event type: 1=ItemsWithQuestion;2=AnswerScreen;3=InterTrialScreen;
    /// 4=InterBlockScreen;5=EndScreen
    public static void SaveTimeStamp(string eventType)
    {

        string dataTrialText = GameManager.block + ";" + GameManager.trial + 
            ";" + eventType + ";" + GameManager.TimeStamp();

        string[] lines = { dataTrialText };
        string folderPathSave = Application.dataPath + outputFolder;
        
        using (StreamWriter outputFile = new StreamWriter(folderPathSave + 
            identifierName + "TimeStamps.txt", true))
        {
            foreach (string line in lines)
                outputFile.WriteLine(line);
        }
    }


    // Saves the time stamp of every click made on the items 
    // block ; trial ; clicklist (i.e. item number ; itemIn? 
    // (1: selecting; 0:deselecting; 2: reset) ; 
    // time of the click with respect to the begining of the trial)
    public static void SaveClicks(List<BoardManager.Click> itemClicks)
    {
        string folderPathSave = Application.dataPath + outputFolder;

        string[] lines = new string[itemClicks.Count];
        int i = 0;
        foreach (BoardManager.Click click in itemClicks)
        {
            lines[i] = GameManager.block + ";" + GameManager.trial + 
                ";" + click.ItemNumber + ";" + click.State + ";" + click.time;
            i++;
        }

        using (StreamWriter outputFile = new StreamWriter(folderPathSave + 
            identifierName + "Clicks.txt", true))
        {
            WriteToFile(outputFile, lines);
        }

    }

    // Helper function to write lines to an outputfile
    private static void WriteToFile(StreamWriter outputFile, string[] lines)
    {
        foreach (string line in lines)
        {
            outputFile.WriteLine(line);
        }

        outputFile.Close();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
