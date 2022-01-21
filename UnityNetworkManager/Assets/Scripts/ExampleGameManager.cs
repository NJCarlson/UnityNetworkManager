using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using UnityEngine.UI;


/// <summary>
/// This script is a sanatized example of using the Network Manager & Objects, as well saving and loading data, 
/// </summary>
public class ExampleGameManager : MonoBehaviour
{
    //[SerializeField]
    //ImgsFillDynamic fillGraphic;
    [SerializeField]
    TextMeshProUGUI timerText;
    [SerializeField]
    float sliderValue;
    [SerializeField]
    GameObject popUpPanel;
    [SerializeField]
    TextMeshProUGUI popUpText;
    [SerializeField]
    NetworkManager netMan;
    [SerializeField] GameObject StopButton;
    [SerializeField] GameObject PlayButton;

    //End of practice popup
    [SerializeField] GameObject UserSavePopup;
    [SerializeField]
    TMP_InputField sessionID;

    //History Panel 
    [SerializeField]
    GameObject historyButtonPrefab;
    [SerializeField]
    GameObject historyScrollContent;
    [SerializeField]
    GameObject historyScrollPanel;

    //Display  Save
    [SerializeField]
    GameObject uiSavePanel;
    [SerializeField]
    TextMeshProUGUI SavetimeText;
    [SerializeField]
    TextMeshProUGUI SavePercentText;
    [SerializeField]
    TextMeshProUGUI studentNameText;
    [SerializeField]
    TextMeshProUGUI dateText;
    [SerializeField]
    Slider SaveSlider;

    //Delete Saves
    [SerializeField]
    GameObject DeletePopUp;

    //setting panel
    [SerializeField]
    GameObject settingsPanel;

    private GameObject buttonToDelete;
  
    private LoginManager logMan;

    private List<GameObject> saveButtons;

    ExampleNetworkObject theNetworkObject;


    // Start is called before the first frame update
    void Start()
    {
        saveButtons = new List<GameObject>();
        if (netMan == null)
        {
            netMan = GameObject.FindObjectOfType<NetworkManager>();
        }

        theNetworkObject = netMan.exampleNetworkObject;

        logMan = GameObject.FindObjectOfType<LoginManager>();

        popUpPanel.SetActive(false);
    }

    // Fixed Update is called once at the end of each frame
    void FixedUpdate()
    {
     
    }

    /// <summary>
    /// Formats the timer for display
    /// "00:00:00"
    /// </summary>
    /// <param name="elapsed"></param>
    /// <returns></returns>
    string FormatSeconds(float elapsed)
    {
        int d = (int)(elapsed * 100.0f);
        int minutes = d / (60 * 100);
        int seconds = (d % (60 * 100)) / 100;
        int hundredths = d % 100;
        return String.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, hundredths);
    }

    /// <summary>
    /// Starts the practice activity, as long as
    /// the device is connected
    /// </summary>
    public void StartTimer()
    {

        if (theNetworkObject.type == NetworkObject.NetworkObjectType.udp)
        {
            Message startMessage = new Message()
            {
                str_msg = "GO",
                timestamp = DateTime.Now,
                fromUnity = true,
                id = theNetworkObject.id,
                byte_msg = null
            };

            theNetworkObject.SetOutgoingMessage(startMessage);
        }

        //todo, add check for ble type, and make sure ble characteristic is valid / ble is connected

 
    }

    /// <summary>
    /// Called when the tiner stop button is pushed.
    /// Saves user data.
    /// Resets variables as needed.
    /// </summary>
    public void StopTimer()
    {
        if (theNetworkObject.type == NetworkObject.NetworkObjectType.udp)
        {
            Message startMessage = new Message()
            {
                str_msg = "ST",
                timestamp = DateTime.Now,
                fromUnity = true,
                id = theNetworkObject.id,
                byte_msg = null
            };

            theNetworkObject.SetOutgoingMessage(startMessage);
        }

        
        // UserSavePopup.gameObject.SetActive(true);
        SaveSession();
    }

    public void SaveSession()
    {
        //todo, save data 
        string completionText;
        if (true)
        {
            completionText = "Completed";
        }
        else
        {
            completionText = "Attempted";
        }

        if (string.IsNullOrEmpty(sessionID.text))
        {
            sessionID.text = "student";
        }

        ExerciseData data = new ExerciseData
        {
            actor = new Actor
            {
                mbox = sessionID.text,
                name = "Student " + sessionID.text,
                objectType = "Agent"
            },

            verb = new Verb
            {
                id = "",
                display = new Display
                {
                    EnGB = completionText,
                    EnUS = completionText
                }
            },

            @object = new Object
            {
                id = "",
                definition = new Definition
                {
                    description = new Description
                    {
                        EnUS = "Practiced correctly "
                    },
                    name = new Name
                    {
                        EnUS = " Practice Session"
                    },
                    type = "Practice"
                },
                objectType = "",
            },

            result = new Result
            {
                success = true,
                completion = true,
                duration = FormatSeconds(1.0f),
                response = ((int)1).ToString()
            },

            context = new Context
            {
                registration = "",
                contextActivities = new ContextActivities
                {

                },
                language = ""
            },
            stored = DateTime.Now.ToString(),
            authority = new Authority
            {
                account = new Account
                {

                },
                objectType = "Activity"
            },

            version = "",
            id = "",
            timestamp = DateTime.Now.ToString(),

            attachments = new List<Attachment>
            {

            }
        };

        SaveManager.SaveLocalExerciseDataJSON(data);
        StopButton.SetActive(false);
        PlayButton.SetActive(true);
    }

    /// <summary>
    /// Loads and displays all saved data on device
    /// </summary>
    public void loadHistory()
    {
        SaveManager.LoadSessionData();

        foreach (var item in saveButtons)
        {
            GameObject.Destroy(item);
        }

        saveButtons.Clear();

        for (int i = SaveManager.saves.Count - 1; i >= 0; i--)
        {
            //create session history slot and add it to scroll view content;
            GameObject SaveButton = GameObject.Instantiate(historyButtonPrefab, historyScrollContent.transform);
            SaveButton.GetComponentInChildren<Text>().text = SaveManager.saves[i].stored.ToString();
            SaveButton.GetComponent<HistoryButton>().curData = SaveManager.saves[i];
            saveButtons.Add(SaveButton);
        }
    }

    /// <summary>
    /// Displays the specified history data. 
    /// </summary>
    /// <param name="save"></param>
    public void DisplayHistory(ExerciseData save)
    {
        uiSavePanel.SetActive(true);
        studentNameText.text = save.actor.name;
        SavetimeText.text = save.result.duration;
        SavePercentText.text = save.result.response + "%";
        dateText.text = save.stored.ToString(); //todo format this
        float sliderval = 0.0f;
        float.TryParse(save.result.response, out sliderval);
        SaveSlider.value = sliderval;
    }

    /// <summary>
    /// This function is called when the user pushes
    /// the red X on the Activity History Page.
    /// Sets the button to be deleted, and displays 
    /// the "Are you sure ?" popup.
    /// </summary>
    /// <param name="save"></param>
    /// <param name="buttonToBeDeleted"></param>
    public void DisplayDeletePopUp(ExerciseData save, GameObject buttonToBeDeleted)
    {
        SaveManager.stagedToDelete = save.id;
        DeletePopUp.SetActive(true);
        buttonToDelete = buttonToBeDeleted;
    }

    /// <summary>
    /// Called when the user clicks "Yes" 
    /// Deletes the file that has been staged for deletion.
    /// </summary>
    public void DeleteFile()
    {
        try
        {
            if (SaveManager.DeleteStagedFile())
            {
                DeletePopUp.SetActive(false);
                GameObject.Destroy(buttonToDelete);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void OnDestroy()
    {
        Message stopMsg = new Message()
        {
            str_msg = "GO",
            timestamp = DateTime.Now,
            fromUnity = true,
            id = theNetworkObject.id,
            byte_msg = null
        };

        theNetworkObject.SetOutgoingMessage(stopMsg);
    }


    #region Ble Debug Menu


    #endregion

}
