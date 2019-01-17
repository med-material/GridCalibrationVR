using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Valve.VR;
using System.Globalization;
public class OpticianController : MonoBehaviour
{
    public Text directionLabel;
    public GameObject landoltC;
    public Text explainText;
    public GameObject FOVTarget;
    public List<Vector3> FOVPointsLocal;
    public Transform OperatorPlane;
    public Transform FovContainer;
    public PointingSystem pointingSystem;
    public SteamVR_Action_Vector2 touchPadAction;
    public SteamVR_Action_Boolean touchPadActionClick;
    public GameObject radialMenu;
    [SerializeField]
    private fovScriptableObject FOVPointsSave;
    [SerializeField]
    private Text HMDeyeDistanceText;
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private Image arrowImage;
    private Renderer FOVTargetRenderer;
    private bool isFOVCalibEnded;
    private KeyCode rightArrow = KeyCode.RightArrow;
    private KeyCode leftArrow = KeyCode.LeftArrow;
    private KeyCode upArrow = KeyCode.UpArrow;
    private KeyCode downArrow = KeyCode.DownArrow;
    private List<KeyCode> keyCodes;
    private List<int> l_rotation;
    private int rotatIndex = 0;
    private int keyCodeIndex;
    private float FOVTimer = 0;
    private List<string> moveDirections;
    private List<int> rotateAngles;
    private string moveDirection;
    private int nbDirectionEnded;
    private List<Vector3> FOVPoints; // FOV points, the max points the user can reach in 4 direction
    private List<Vector3> FOVEdgePoints;
    private Vector3 savedFOVTargetpos;
    private List<Vector3> savedTargetposList;
    private Color textColor = new Color(0.6415094f, 0.6415094f, 0.6415094f, 1.0f);
    private bool isSizeOk = false;
    private bool isConfirmingPosition = false;
    private bool changePos = false;
    private int currentTargetIndex = 0;
    private bool calibrationIsOver;
    private Material lineMaterial;
    private LineRenderer lineRenderer;
    private LineRenderer lineRendererAcuity;
    private List<float> landolt_factor = new List<float>() { 2.0f, 1.5f, 1.33f, 1.25f, 1.20f, 1.1666667f, 1.1424f, 1.1261213f, 1.11f, 1.5f, 1.332f };
    private int landolt_current_index = 0;
    private List<string> acceptedDirection = new List<string>();
    private int calibStep = 1;
    private List<Vector3> FOVpt;
    private float touchTimer;
    private float distHMDEye;

    void Start()
    {
        // GENERAL SETUP
        lineRenderer = OperatorPlane.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.gameObject.layer = 11; // 11 = OperatorUI

        lineRendererAcuity = FovContainer.GetComponent<LineRenderer>();
        lineRendererAcuity.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererAcuity.widthMultiplier = 0.02f;
        lineRendererAcuity.gameObject.layer = 11; // 11 = OperatorUI

        slider.value = FOVPointsSave.previous_HMDeye_distance;
        saveButton.interactable = false;
        arrowImage.enabled = false;

        //// ACUITY SETUP 
        l_rotation = new List<int> { 0, -90, 180, 90 }; // Right, Down, Left, Up
        keyCodes = new List<KeyCode> { rightArrow, downArrow, leftArrow, upArrow }; // have to stay same order than rotation list !!
        savedTargetposList = new List<Vector3>();

        //// FOV SETUP
        savedFOVTargetpos = FOVTarget.transform.localPosition;
        FOVEdgePoints = new List<Vector3>();
        moveDirections = new List<string> { "right", "down", "left", "up", "right-up", "right-down", "left-up", "left-down" };
        rotateAngles = new List<int> { 0, -45, -90, -135, 180, 135, 90, 45 };
        FOVPoints = new List<Vector3>();
        FOVPointsLocal = new List<Vector3>();
        FOVpt = new List<Vector3>();
        FOVTargetRenderer = FOVTarget.GetComponent<Renderer>();

        FOVTargetRenderer.material.color = Color.red;
        explainText.text = "Press space bar when the target is out of \n your field of view."
            + "\n Press space bar to start.";
        FOVTimer = 0;

        explainText.color = textColor;
        nbDirectionEnded = 0;
        moveDirection = moveDirections[nbDirectionEnded];
        directionLabel.gameObject.layer = 11;

        if (!lineMaterial)
        {
            lineMaterial = new Material(Shader.Find("Custom/GizmoShader"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        PointingSystem.OnAddPoint += DrawFOVPoint;
    }


    void OnEnable()
    {
        Debug.Log("Testing connection for devices");
        SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
    }

    // A SteamVR device got connected/disconnected
    private void OnDeviceConnected(int index, bool connected)
    {
        if (connected)
        {
            if (OpenVR.System != null)
            {
                //lets figure what type of device got connected
                ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass((uint)index);
                if (deviceClass == ETrackedDeviceClass.Controller)
                {
                    Debug.Log("Controller got connected at index:" + index);

                    if (!pointingSystem.start)
                    {
                        pointingSystem.start = true;
                        FOVTarget.SetActive(false);
                        pointingSystem.SetupPointingSystem();
                        LoadFOVPoints();
                    }
                }
            }
        }
    }

    // Draw the user FOV and acuity FOV to the operator
    private void DrawFOV(LineRenderer lrender, List<Vector3> pos_list, Color color)
    {
        lrender.startColor = color;
        lrender.endColor = color;
        lrender.positionCount = pos_list.Count;
        Vector3 pos;
        List<Vector3> temp_pos_list = new List<Vector3>();
        for (int index = 0; index < pos_list.Count; ++index)
        {
            pos = pos_list.ToArray()[index];
            pos.z = -3.0f;
            temp_pos_list.Add(pos);
        }
        lrender.SetPositions(temp_pos_list.ToArray());
    }

    void DrawFOVPoint()
    {
        lineRenderer.positionCount = 0;
        DrawFOV(lineRenderer, pointingSystem.handPoints, Color.blue);
    }

    void Update()
    {
        if (calibrationIsOver)
        {
            radialMenu.SetActive(false);
            FOVPoints pt = FOVPointsSave.fOVPts.FirstOrDefault(p => p.id == Convert.ToInt32(distHMDEye));
            if (pt != null)
                pt.acuityFieldPoints = savedTargetposList;

            DrawFOV(lineRendererAcuity, savedTargetposList, Color.red);
            landoltC.SetActive(false);
            arrowImage.enabled = false;
            explainText.text = "Calibration is over";
        }
        else
        {
            if (isFOVCalibEnded)
                UpdateAcuityCalibration();
            else
            {
                if (pointingSystem.start)
                {
                    if (pointingSystem.isCalibEnded)
                    {
                        isFOVCalibEnded = true;
                        lineRenderer.loop = true;
                        saveButton.interactable = true;
                        pointingSystem.StartButton.SetActive(false);
                        distHMDEye = FOVPointsSave.previous_HMDeye_distance;
                        FOVPoints pt = FOVPointsSave.fOVPts.FirstOrDefault(p => p.id == Convert.ToInt32(distHMDEye));
                        if (pt != null)
                        {
                            pt.selectedAxes = pointingSystem.selectedAxes;
                            pointingSystem.selectAxes = true;
                        }

                        // DrawFOV(lineRenderer, pointingSystem.handPoints, Color.blue);
                        FOVTarget.SetActive(false);
                    }
                }
                else
                {
                    explainText.text = "Please connect a controller to start.";
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            // FIXME: Restart system
            // TODO: Ask Romain for his solution
        }
    }

    private void MoveTargetOnAxis()
    {
        Vector3 previous_pos = landoltC.transform.localPosition;
        //"right", "down", "left", "up", "right-up", "right-down", "left-up", "left-down"
        //   0        1      2       3       4            5            6          7
        switch (currentTargetIndex)
        {
            case 0:// right
                if (keyCodeIndex == 0 || keyCodeIndex == 4 || keyCodeIndex == 5)
                    MoveTowards(1);
                else if (keyCodeIndex == 2 || keyCodeIndex == 7 || keyCodeIndex == 6)
                    MoveTowards(-1);
                break;
            case 1: //bottom-right
                if (keyCodeIndex == 0 || keyCodeIndex == 1 || keyCodeIndex == 5)
                    MoveTowards(1);
                else if (keyCodeIndex == 2 || keyCodeIndex == 3 || keyCodeIndex == 6)
                    MoveTowards(-1);
                break;
            case 2: //bottom
                if (keyCodeIndex == 1 || keyCodeIndex == 7 || keyCodeIndex == 5)
                    MoveTowards(1);
                else if (keyCodeIndex == 4 || keyCodeIndex == 3 || keyCodeIndex == 6)
                    MoveTowards(-1);
                break;
            case 3: //bottom-left
                if (keyCodeIndex == 2 || keyCodeIndex == 1 || keyCodeIndex == 7)
                    MoveTowards(1);
                else if (keyCodeIndex == 0 || keyCodeIndex == 3 || keyCodeIndex == 4)
                    MoveTowards(-1);
                break;
            case 4: //left
                if (keyCodeIndex == 2 || keyCodeIndex == 7 || keyCodeIndex == 6)
                    MoveTowards(1);
                else if (keyCodeIndex == 0 || keyCodeIndex == 4 || keyCodeIndex == 5)
                    MoveTowards(-1);
                break;
            case 5: //top-left
                if (keyCodeIndex == 3 || keyCodeIndex == 2 || keyCodeIndex == 6)
                    MoveTowards(1);
                else if (keyCodeIndex == 1 || keyCodeIndex == 0 || keyCodeIndex == 5)
                    MoveTowards(-1);
                break;
            case 6: //top
                if (keyCodeIndex == 3 || keyCodeIndex == 4 || keyCodeIndex == 6)
                    MoveTowards(1);
                else if (keyCodeIndex == 1 || keyCodeIndex == 7 || keyCodeIndex == 5)
                    MoveTowards(-1);
                break;
            case 7: //top-right
                if (keyCodeIndex == 3 || keyCodeIndex == 0 || keyCodeIndex == 4)
                    MoveTowards(1);
                else if (keyCodeIndex == 1 || keyCodeIndex == 2 || keyCodeIndex == 7)
                    MoveTowards(-1);
                break;
        }
        float temp = Vector3.Distance(landoltC.transform.localPosition, savedFOVTargetpos); // distance between landolt C and the center point
        float temp1 = Vector3.Distance(FOVpt[currentTargetIndex], savedFOVTargetpos);       // distance between the limit point and the center point
        if (temp >= temp1)
            landoltC.transform.localPosition = previous_pos;
    }

    private void MoveTowards(int mult)
    {
        if (mult < 0 && landoltC.transform.localPosition == savedFOVTargetpos) // Prevent the user to go the wrong way
            print("Not moving the right way");
        else
            landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], 0.002f * mult);
    }

    private IEnumerator FadeText()
    {
        while (explainText.isActiveAndEnabled)
        {
            if (explainText.color.a - 0.05f > 0)
                explainText.color -= new Color(0.0f, 0.0f, 0.0f, 0.05f);
            else
                explainText.enabled = false;
            yield return null;
        }
    }

    private void SavePos()
    {
        if (savedTargetposList.Count - 1 == currentTargetIndex)
        {
            savedTargetposList[currentTargetIndex] = landoltC.transform.localPosition;
        }
        else
        {
            savedTargetposList.Insert(currentTargetIndex, landoltC.transform.localPosition);
        }
    }

    private void UpdateAcuityCalibration()
    {
        bool touchPadClick = touchPadActionClick.GetStateDown(SteamVR_Input_Sources.Any);
        Vector2 touchPadValue = touchPadAction.GetAxis(SteamVR_Input_Sources.Any);
        Button right_button = radialMenu.transform.GetChild(0).GetComponent<RMF_RadialMenu>().elements[0].button;
        Button left_button = radialMenu.transform.GetChild(0).GetComponent<RMF_RadialMenu>().elements[2].button;

        if (!landoltC.activeSelf)
        {
            explainText.enabled = true;
            explainText.color = textColor;
            landoltC.SetActive(true);
            ToggleRadialMenu();
            right_button.interactable = false;
            left_button.interactable = false;
        }


        // TODO: Do all of the next comments
        // TODO: Add text explication for operator and patient for instruction
        // 1. Let the user choose the size of the Landolt checked
        // 2. Let the user confirm positions 
        // 3. Set the position to center
        // 4. Move the target slowly to have a smooth poursuit on the current axe
        // until the FOV limit is reached (patient with touchpad/operator with arrow)
        // 5. press space bar or trigger to stop the movement and change the Landolt C orientation
        // 6. The patient or operator moves the target closer to the center until the patient can detect the opening direction
        // 7. ask the patient to press the touchpad in the good direction (add visual help around the Landolt C simulating the touchpad)
        // if the patient pressed direction is good,
        //      if was the last axe, then proceed to result, draw the acuity field.
        //      else set the current axe to the next one and start to step 3.
        // else go to step 4.
        switch (calibStep)
        {
            case 1:
                explainText.text = "Increase or reduce size of the \n circle to the minimum size for wich you can still see "
               + "the open side of it. \n Press trigger/space bar to start";
                right_button.interactable = false;
                left_button.interactable = false;

                if (Input.GetKeyDown(downArrow) || (touchPadClick && touchPadValue.y < -0.5))
                {
                    ReduceCircleSize();
                    SetRandomLandoltOrientation();
                }
                else if (Input.GetKeyDown(upArrow) || (touchPadClick && touchPadValue.y > 0.5))
                {
                    IncreaseCircleSize();
                    SetRandomLandoltOrientation();
                }
                else if (Input.GetKeyDown(KeyCode.Space) || SteamVR_Input._default.inActions.GrabPinch.GetStateDown(SteamVR_Input_Sources.Any) && touchTimer <= 0)
                {
                    calibStep++;
                    touchTimer = 0.3f;
                    // Change text to explain this step
                }
                break;
            case 2:
                right_button.interactable = true;
                left_button.interactable = true;
                explainText.text = "Select opened side with the controller touchpad or keyboard arrows";
                if (Input.GetKeyDown(KeyCode.Space) || SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any) && touchTimer <= 0)
                {
                    calibStep--;
                    touchTimer = 0.3f;
                    // Change text to explain this step
                }
                keyCodeIndex = GetDirectionIndexPressed();
                if (keyCodeIndex == rotatIndex)
                {
                    isSizeOk = true;
                    explainText.text = "";
                    calibStep++;
                    if (FOVEdgePoints.Count == 0)
                        CalculateAllPosFromPointing();
                }
                break;
            case 3:
                ToggleRadialMenu(); // remove Radial Menu around landolt C
                landoltC.transform.localPosition = landoltC.transform.localPosition != savedFOVTargetpos ? savedFOVTargetpos : landoltC.transform.localPosition; // 1.
                calibStep++;
                break;
            case 4:
                arrowImage.enabled = true; // set the arrow in the good angle to show the user the direction to go
                int rotat = rotateAngles[currentTargetIndex];
                arrowImage.transform.localRotation = Quaternion.Euler(0, 0, rotat);
                // test if the direction moves the target closer or further from the center
                keyCodeIndex = GetDirectionIndexPressed();
                // CalculateAcceptedMovingDirection(); // accepted two directions for current axe
                if (keyCodeIndex != -1)
                {
                    moveDirection = moveDirections[keyCodeIndex];
                    MoveTargetOnAxis();
                }
                if (Input.GetKeyDown(KeyCode.Space) || SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any))
                {
                    calibStep++;
                    SetRandomLandoltOrientation();
                    ToggleRadialMenu(); //Add the radial Menu around the Landolt C
                }

                break;
            case 5:
                // Visual help for patient touchpad touch WIP
                // get the good direction click corresponding to Landolt C opened side
                // Function to listen to click, get the index
                keyCodeIndex = GetDirectionIndexPressed();
                if (keyCodeIndex == rotatIndex)
                {
                    SavePos();
                    calibStep++; // the patient pressed the good direction, move to next step
                }
                break;
            case 6:
                if (currentTargetIndex >= FOVEdgePoints.Count - 1)
                    calibrationIsOver = true;
                else
                {
                    currentTargetIndex++;
                    calibStep = 3;
                }
                break;
        }
        touchTimer -= Time.deltaTime;
    }

    private void ToggleRadialMenu()
    {
        // Activate the Radial Menu, center it on the landolt C
        radialMenu.SetActive(!radialMenu.activeSelf);
        radialMenu.transform.localPosition = landoltC.transform.localPosition;
    }

    private void ReduceCircleSize()
    {
        landoltC.transform.localScale /= landolt_factor[landolt_current_index];
        landolt_current_index = landolt_current_index >= landolt_factor.Count - 1 ? landolt_current_index : landolt_current_index + 1;
    }

    private void IncreaseCircleSize()
    {
        landoltC.transform.localScale *= landolt_factor[landolt_current_index];
        landolt_current_index = landolt_current_index - 1 > 0 ? landolt_current_index - 1 : landolt_current_index;
    }

    private void CalculateAllPosFromPointing()
    {
        // Calculate position for the target position from the point the user placed

        Vector3 pt_left = new Vector3();
        Vector3 pt_right = new Vector3();
        Vector3 pt_top = new Vector3();
        Vector3 pt_down = new Vector3();
        Vector3 pt_s_top_left = new Vector3();
        Vector3 pt_s_top_right = new Vector3();
        Vector3 pt_s_down_left = new Vector3();
        Vector3 pt_s_down_right = new Vector3();
        int index_right = -1;

        foreach (var pt in pointingSystem.handPoints) // Find the point on the right
        {
            if (pt.x > pt_right.x)
            {
                pt_right = pt;
                index_right = pointingSystem.handPoints.IndexOf(pt);
            }
        }
        if (index_right != 0 && index_right != -1)
        {
            pointingSystem.handPoints.AddRange(pointingSystem.handPoints.Take(index_right)); // set the first item list to the right point
            pointingSystem.handPoints.RemoveRange(0, index_right);
        }
        pt_right = new Vector3();

        foreach (var pt in pointingSystem.handPoints)
        {
            if (pt.x > pt_right.x)
                pt_right = pt;
            if (pt.x < pt_left.x)
                pt_left = pt;
            if (pt.y > pt_top.y)
                pt_top = pt;
            if (pt.y < pt_down.y)
                pt_down = pt;
        }
        foreach (var pt in pointingSystem.selectedAxes)
        {
            if (pt.y > pt_s_top_right.y && pt.x > 0)
                pt_s_top_right = pt;
            if (pt.y > pt_s_top_left.y && pt.x < 0)
                pt_s_top_left = pt;
            if (pt.y < pt_s_down_left.y && pt.x < 0)
                pt_s_down_left = pt;
            if (pt.y < pt_s_down_right.y && pt.x > 0)
                pt_s_down_right = pt;
        }

        FOVEdgePoints.Add(pt_right);
        FOVEdgePoints.Add(pt_s_down_right);
        FOVEdgePoints.Add(pt_down);
        FOVEdgePoints.Add(pt_s_down_left);
        FOVEdgePoints.Add(pt_left);
        FOVEdgePoints.Add(pt_s_top_left);
        FOVEdgePoints.Add(pt_top);
        FOVEdgePoints.Add(pt_s_top_right);

        int i = 0;
        foreach (var item in FOVEdgePoints)
        {
            Vector3 pt = FOVEdgePoints[i]; // CHANGE THIS TO ITEM
            pt.z = -3.0f;
            FOVpt.Add(pt);
            i++;
        }

        DrawFOV(lineRendererAcuity, FOVpt, Color.red);
    }

    // Vector3.MoveTowards
    public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 a = target - current;
        float magnitude = a.magnitude;
        if (magnitude <= maxDistanceDelta || magnitude == 0f)
        {
            return target;
        }
        return current + a / magnitude * maxDistanceDelta;
    }

    private bool NearlyEqual(float f1, float f2)
    {
        // Equal if they are within 0.01 of each other
        return Math.Abs(f1 - f2) < 0.01;
    }

    private int GetDirectionIndexPressed()
    {
        bool touchPadClick = touchPadActionClick.GetState(SteamVR_Input_Sources.Any);
        Vector2 touchPadValue = touchPadAction.GetAxis(SteamVR_Input_Sources.Any);

        if (Input.GetKey(leftArrow) || (touchPadClick && touchPadValue.x < -0.5))
        {
            if (Input.GetKey(upArrow) || (touchPadClick && touchPadValue.y > 0.5))
            {
                return 6;
            }
            else if (Input.GetKey(downArrow) || (touchPadClick && touchPadValue.y < -0.5))
            {
                return 7;
            }
        }
        else if (Input.GetKey(rightArrow) || (touchPadClick && touchPadValue.x > 0.5))
        {
            if (Input.GetKey(upArrow) || (touchPadClick && touchPadValue.y > 0.5))
            {
                return 4;
            }
            else if (Input.GetKey(downArrow) || (touchPadClick && touchPadValue.y < -0.5))
            {
                return 5;
            }
        }
        foreach (KeyCode k in keyCodes)
        {
            if (Input.GetKey(k))
            {
                return keyCodes.IndexOf(k);
            }
            if (touchPadClick && touchPadValue.x > 0.5)
                return 0;
            if (touchPadClick && touchPadValue.y < -0.5)
                return 1;
            if (touchPadClick && touchPadValue.x < -0.5)
                return 2;
            if (touchPadClick && touchPadValue.y > 0.5)
                return 3;
        }
        return -1;
    }

    private void SetRandomLandoltOrientation()
    {
        rotatIndex = GetRandomIndex(l_rotation, rotatIndex);
        int rotat = l_rotation[rotatIndex];
        directionLabel.text = moveDirections[rotatIndex];
        landoltC.transform.localRotation = Quaternion.Euler(0, 0, rotat);
    }

    private int GetRandomIndex<T>(List<T> lst, int previous_index)
    {
        System.Random rand = new System.Random();
        int temp_index;
        do
        {
            temp_index = rand.Next(0, lst.ToList().Count);
        } while (temp_index == previous_index);
        return temp_index;
    }

    public void SaveFOVPointsToScriptableObj()
    {
        int key = Convert.ToInt32(distHMDEye);

        if (FOVPointsSave.fOVPts == null) // test if list has been initialized
            FOVPointsSave.fOVPts = new List<FOVPoints>();
        FOVPoints FOVpt = null;
        if (FOVPointsSave.fOVPts.Count > 0)
            FOVpt = FOVPointsSave.fOVPts.FirstOrDefault(p => p.id == key); // look for object with the choosen id
        if (FOVpt == null)
        {
            FOVpt = new FOVPoints();
            FOVpt.id = key;
            FOVpt.points = pointingSystem.handPoints;
            FOVpt.selectedAxes = pointingSystem.selectedAxes;
            FOVPointsSave.fOVPts.Add(FOVpt);
        }
        else
        {
            FOVpt.points = pointingSystem.handPoints;
            FOVpt.selectedAxes = pointingSystem.selectedAxes;
        }
    }

    public void ResetFOVPoints()
    {
        FOVPointsSave.fOVPts.Clear();
    }

    public void ChangeHMDEyeDistanceValue(Slider slider)
    {
        distHMDEye = slider.value;
        FOVPointsSave.previous_HMDeye_distance = distHMDEye;
        HMDeyeDistanceText.text = (slider.value / 2).ToString();
        LoadFOVPoints();
    }

    // Function to load points from the scriptable object FOVPoints
    //     also draw the LineRenderer paths from the points
    private void LoadFOVPoints()
    {
        distHMDEye = FOVPointsSave.previous_HMDeye_distance;
        FOVPoints FOVpt = null;
        if (FOVPointsSave.fOVPts.Count > 0)
            FOVpt = FOVPointsSave.fOVPts.FirstOrDefault(p => p.id == Convert.ToInt32(distHMDEye)); // look for object with the choosen id
        if (FOVpt != null)
        {
            pointingSystem.handPoints = FOVpt.points;
            pointingSystem.selectedAxes = FOVpt.selectedAxes;
            DrawFOV(lineRenderer, pointingSystem.handPoints, Color.blue);
            pointingSystem.isCalibEnded = true;
        }
    }

    public void LoadNextScene(string nextScene)
    {
        //SceneManager.LoadSceneAsync(nextScene);
        //SceneManager.UnloadSceneAsync("OpticianCalibration");
    }

    private void OnDisable()
    {
        EditorUtility.SetDirty(FOVPointsSave);
    }
}
