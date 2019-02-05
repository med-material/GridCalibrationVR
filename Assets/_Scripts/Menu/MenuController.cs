using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private Button baguetteButton;
    private const string baguetteButtonName = "LaBaguetteButton";
    private Button wamButton;
    private const string wamButtonName = "WAMButton";
    private Button calibTestButton;
    private const string calibTestButtonName = "PupilLabCalibrationTestButton";
    private Button acuityButton;
    private const string acuityButtonName = "AcuityStartButton";
    private Button pupilButton;
    private const string pupilButtonName = "PupilLabCalibrationButton";
    private Button calibArthurButton;
    private const string calibArthurButtonName = "Calibration test (Arthur)";
    private Image statusDot;
    private Text statusText;
    private bool saveStatus;
    private Text infoText;
    private Dictionary<Button, string> dicButtonTooltip;
    private string defaultTooltipValue = "Hover buttons to print information here.";
    private string defaultStatusValue = "Pupil Connection status";

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (FindObjectsOfType(GetType()).Length > 1)
            Destroy(FindObjectsOfType(GetType())[1]);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;

        // Setup event for mouse hover on button to toggle tooltip
        MouseController.onMouseEnter += ToggleTooltip;
        MouseController.onMouseExit += ToggleDefaultTooltip;
        MouseController.onMouseClick += StartScene;
    }

    void Start()
    {
        PupilTools.OnConnected += DotStatusOn;
        PupilTools.OnDisconnecting += DotStatusOff;
        PupilTools.OnCalibrationEnded += ActivateButtonsCalib;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("Menu transition", LoadSceneMode.Additive);
        if(saveStatus)
            DotStatusOn();
    }

    // TODO: Add the event click() for the WAMButton and LaBaguetteButton on the inspector
    private void DotStatusOn()
    {
        if (statusDot != null)
        {
            statusDot.color = new Color32(0, 255, 0, 255);
            statusText.text = "Pupil is Connected";
            saveStatus = false;
        }
        else {
            saveStatus = true;
        }

    }

    private void DotStatusOff()
    {
        if (statusDot != null)
        {
            statusDot.color = new Color32(255, 0, 0, 255);
            statusText.text = "Pupil is not connected";
        }
        else 
            saveStatus  = false;
    }

    public void StartScene(GameObject button)
    {
        switch (button.name)
        {
            case pupilButtonName:
                SceneManager.LoadScene("PupilLabCalibration");
                break;
            case calibTestButtonName:
                SceneManager.LoadScene("GridCalibration");
                break;
            case acuityButtonName:
                SceneManager.LoadScene("OpticianCalibration");
                break;
            default:
            case calibArthurButtonName:
            case wamButtonName:
            case baguetteButtonName:
                print("No actions registered for this button.");
                break;
        }
    }

    private void ToggleTooltip(GameObject button)
    {
        Button btn = button.GetComponent<Button>();
        if (dicButtonTooltip.ContainsKey(btn))
            infoText.text = dicButtonTooltip[btn];
    }

    private void ToggleDefaultTooltip(GameObject button)
    {
        if (dicButtonTooltip.ContainsKey(button.GetComponent<Button>()))
            infoText.text = defaultTooltipValue;
    }

    // Activate all buttons if the pupil calibration is done
    private void ActivateButtonsCalib()
    {
        foreach (KeyValuePair<Button, string> entry in dicButtonTooltip)
        {
            entry.Key.interactable = true;
        }
    }

    void onDisable()
    {
        // get the notification if pupil service is started and change the Dot color regardings that info
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        PupilTools.OnConnected -= DotStatusOn;
        PupilTools.OnDisconnecting -= DotStatusOff;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu transition")
        {
            dicButtonTooltip = new Dictionary<Button, string>();
            // Get the GameObjects like this to keep them even after a reload
            baguetteButton = GameObject.Find(baguetteButtonName).GetComponent<Button>();
            wamButton = GameObject.Find(wamButtonName).GetComponent<Button>();
            calibTestButton = GameObject.Find(calibTestButtonName).GetComponent<Button>();
            acuityButton = GameObject.Find(acuityButtonName).GetComponent<Button>();
            pupilButton = GameObject.Find(pupilButtonName).GetComponent<Button>();
            statusDot = GameObject.Find("PupilStatusDot").GetComponent<Image>();
            statusText = GameObject.Find("PupilStatusText").GetComponent<Text>();
            infoText = GameObject.Find("Info text").GetComponent<Text>();

            // Build the tooltip for every button
            dicButtonTooltip.Add(baguetteButton, "Requires pupil Calibration. Bisectional test with Baguette and knife.");
            dicButtonTooltip.Add(wamButton, "Requires pupil Calibration. Whack-a-mole-like rehabilitation game.");
            dicButtonTooltip.Add(calibTestButton, "Requires pupil Calibration. Test the pupil calibration with heatmap and video result.");
            dicButtonTooltip.Add(acuityButton, "Help to put the HMD on head. Test the user's max acuity zone.");
            dicButtonTooltip.Add(pupilButton, "Starts the pupil calibration. Required for several tests.");

            //Setup the tooltip default value
            statusText.text = defaultStatusValue;
            infoText.text = defaultTooltipValue;
        }

    }
}
