using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Valve.VR;
public class OpticianController : MonoBehaviour
{

    public GameObject landoltC;
    public Text explainText;
    public GameObject FOVTarget;
    public GridController gridController;
    public List<Vector3> FOVPointsLocal;
    public Transform OperatorPlane;
    public Transform FovContainer;
    public PointingSystem pointingSystem;
    public bool handlerMode = false;
    public SteamVR_Action_Vector2 touchPadAction;
    public SteamVR_Action_Boolean touchPadActionClick;
    public GameObject radialMenu;

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
    private int errors = 0;
    private float FOVTimer = 0;
    private RaycastHit userHit;
    private List<string> moveDirections;
    private string moveDirection;
    private int nbDirectionEnded;
    private List<Vector3> FOVPoints; // FOV points, the max points the user can reach in 4 direction
    private List<Vector3> FOVEdgePoints;
    private Vector3 savedFOVTargetpos;
    private List<Vector3> savedTargetposList;
    private Color textColor = new Color(0.6415094f, 0.6415094f, 0.6415094f, 1.0f);
    private bool isSizeOk = false;
    private bool isConfirmingPosition = false;
    private bool hasTargetMoved = false;
    private bool isCircleSet = false;
    private bool changePos = false;
    private int currentTargetIndex = 0;
    private bool calibrationIsOver;
    private Material lineMaterial;
    private float offSetTimer = 0;
    private LineRenderer lineRenderer;
    private LineRenderer lineRendererAcuity;
    private List<float> landolt_factor = new List<float>() { 2.0f, 1.5f, 1.33f, 1.25f, 1.20f, 1.1666667f, 1.1424f, 1.1261213f, 1.11f, 1.5f, 1.332f };
    private int landolt_current_index = 0;
    private List<string> acceptedDirection = new List<string>();
    private int calibStep = 1;
    private List<Vector3> FOVpt;

    void Start()
    {
        // GENERAL SETUP
        lineRenderer = FovContainer.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.gameObject.layer = 11; // 11 = OperatorUI

        lineRendererAcuity = OperatorPlane.GetComponent<LineRenderer>();
        lineRendererAcuity.material = new Material(Shader.Find("Sprites/Default"));
        lineRendererAcuity.widthMultiplier = 0.02f;
        lineRendererAcuity.gameObject.layer = 11; // 11 = OperatorUI


        //// ACUITY SETUP 
        l_rotation = new List<int> { 0, -90, 180, 90 }; // Right, Down, Left, Up
        keyCodes = new List<KeyCode> { rightArrow, downArrow, leftArrow, upArrow }; // have to stay same order than rotation list !!
        savedTargetposList = new List<Vector3>();

        //// FOV SETUP
        savedFOVTargetpos = FOVTarget.transform.localPosition;
        FOVEdgePoints = new List<Vector3>();
        moveDirections = new List<string> { "right", "down", "left", "up", "right-up", "right-down", "left-up", "left-down" };
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

        if (!lineMaterial)
        {
            lineMaterial = new Material(Shader.Find("Custom/GizmoShader"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
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
        lrender.loop = true;
    }

    void Update()
    {
        userHit = gridController.GetCurrentCollider();
        if (calibrationIsOver)
        {
            DrawFOV(lineRendererAcuity, savedTargetposList, Color.red);
            landoltC.SetActive(false);
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
                        DrawFOV(lineRenderer, pointingSystem.handPoints, Color.blue);
                        FOVTarget.SetActive(false);
                    }
                }
                else
                {
                    UpdateMaxFOVCalibration();
                }

            }

        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            // FIXME: Restart system
        }
    }

    private void UpdateMaxFOVCalibration()
    {
        if (Input.GetKeyDown(KeyCode.Space) || SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any))
        {
            if (FOVTimer != 0) // If the space bar has been pressed and is wasn't to start the game.
            {
                // Save the target position, set the next direction to come.
                SaveTargetPosition();
                FOVTarget.transform.localPosition = savedFOVTargetpos; // reset the target position at center before new direction
                nbDirectionEnded++;
                if (nbDirectionEnded == moveDirections.Count - 4)
                {
                    isFOVCalibEnded = true;
                    DrawFOV(lineRenderer, FOVPointsLocal, Color.blue);
                    FOVTarget.SetActive(false);
                }
                else
                    moveDirection = moveDirections[nbDirectionEnded];
            }
            else
            {
                FOVTimer = Time.deltaTime;
            }
        }
        if (FOVTimer != 0)
        {
            MoveTarget();
        }
    }

    private void SaveTargetPosition()
    {
        FOVPoints.Add(FOVTarget.transform.position);
        FOVPointsLocal.Add(FOVTarget.transform.localPosition);
    }

    private void MoveTarget()
    {
        Vector3 direction = new Vector3();
        if (explainText.isActiveAndEnabled && explainText.color.a == 1.0f)
            StartCoroutine("FadeText");
        switch (moveDirection)
        {
            case "left":
                direction = new Vector3(-0.009f, 0.0f, 0.0f);
                break;
            case "down":
                direction = new Vector3(0.0f, -0.009f, 0.0f);
                break;
            case "right":
                direction = new Vector3(0.009f, 0.0f, 0.0f);
                break;
            case "up":
                direction = new Vector3(0.0f, 0.009f, 0.0f);
                break;
            case "right-up":
                direction = new Vector3(0.009f, 0.009f, 0.0f);
                break;
            case "left-up":
                direction = new Vector3(-0.009f, 0.009f, 0.0f);
                break;
            case "right-down":
                direction = new Vector3(0.009f, -0.009f, 0.0f);
                break;
            case "left-down":
                direction = new Vector3(-0.009f, -0.009f, 0.0f);
                break;
            default:
                break;
        }
        Move(direction);
    }

    private void MoveTargetOnAxis()
    {
        Vector3 previous_pos = landoltC.transform.localPosition;
        //"right", "down", "left", "up", "right-up", "right-down", "left-up", "left-down"
        //   0        1      2       3       4            5            6          7
        switch (currentTargetIndex)
        {
            case 1:
            case 0:
            case 7:
                if (keyCodeIndex == 0) // right
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], 0.002f);
                else if (keyCodeIndex == 2) // left
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], -0.002f);
                break;
            case 3:
            case 4:
            case 5:
                if (keyCodeIndex == 2) // left
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], 0.002f);
                else if (keyCodeIndex == 0) // right
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], -0.002f);
                break;
            case 2:
                if (keyCodeIndex == 1) // bottom
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], 0.002f);
                else if (keyCodeIndex == 3) // top
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], -0.002f);
                break;
            case 6:
                if (keyCodeIndex == 3) // top
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], 0.002f);
                else if (keyCodeIndex == 1) // bottom
                    landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, FOVpt[currentTargetIndex], -0.002f);
                break;
        }
        float temp = Vector3.Distance(landoltC.transform.localPosition,savedFOVTargetpos);
        float temp1 = Vector3.Distance(FOVpt[currentTargetIndex], savedFOVTargetpos);
        if ( temp >= temp1)
            landoltC.transform.localPosition = previous_pos;
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

    private void Move(Vector3 vector)
    {
        if (isFOVCalibEnded)
        {
            Vector3 previous_pos = landoltC.transform.position;
            //landoltC.transform.position += vector;
            float temp = Vector3.Distance(savedFOVTargetpos, landoltC.transform.localPosition);
            float temp1 = Vector3.Distance(savedFOVTargetpos, FOVEdgePoints[currentTargetIndex]);
            Vector3 fovPt = FOVEdgePoints[currentTargetIndex];
            if (Vector3.Distance(savedFOVTargetpos, landoltC.transform.localPosition) >= Vector3.Distance(savedFOVTargetpos, FOVEdgePoints[currentTargetIndex]))
                // if the new position is further than the limit FOV, reset the position (block to FOV limit)
                landoltC.transform.position = previous_pos;
            else
                landoltC.transform.localPosition = Vector3.MoveTowards(landoltC.transform.localPosition, new Vector3(fovPt.x, fovPt.y, -3.0f), 0.01f);
            //landoltC.transform.localPosition = new Vector3(landoltC.transform.localPosition.x, landoltC.transform.localPosition.y, -3.0f);
        }
        else
        {
            FOVTarget.transform.position += vector;
            FOVTarget.transform.localPosition = new Vector3(FOVTarget.transform.localPosition.x, FOVTarget.transform.localPosition.y, -3.0f);
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

        if (!landoltC.activeSelf)
        {
            explainText.text = "Press the up or down arrow to increase or reduce size of the \n circle to the minimum size for wich you can still see "
            + "the open side of it. \n Press space bar to start";
            explainText.enabled = true;
            explainText.color = textColor;
            landoltC.SetActive(true);
        }

        if (!isSizeOk)
        {
            if ((Input.GetKeyDown(downArrow) || (touchPadClick && touchPadValue.y < -0.5)) && !isConfirmingPosition)
            {
                ReduceCircleSize();
                SetRandomLandoltOrientation();
            }
            else if ((Input.GetKeyDown(upArrow) || (touchPadClick && touchPadValue.y > 0.5)) && !isConfirmingPosition)
            {
                IncreaseCircleSize();
                SetRandomLandoltOrientation();
            }
            else if (Input.GetKeyDown(KeyCode.Space) || SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any))
            {
                isConfirmingPosition = !isConfirmingPosition;
            }
            else if (isConfirmingPosition)
            {
                keyCodeIndex = GetDirectionIndexPressed();
                if (keyCodeIndex == rotatIndex)
                {
                    isSizeOk = true;
                    explainText.text = "";
                    if (FOVEdgePoints.Count == 0)
                        CalculateAllPosFromPointing();
                }
            }
        }
        else // Size is well set by user
        {
            // TODO: Do all of the next comments
            // TODO: Add text explication for operator and patient for instruction
            // 1. Set the position to center
            // 2. Move the target slowly to have a smooth poursuit on the current axe
            // until the FOV limit is reached (patient with touchpad/operator with arrow)
            // 3. press space bar or trigger to stop the movement and change the Landolt C orientation
            // 4. The patient or operator moves the target closer to the center until the patient can detect the opening direction
            // 5. ask the patient to press the touchpad in the good direction (add visual help around the Landolt C simulating the touchpad)
            // if the patient pressed direction is good,
            //      if was the last axe, then proceed to result, draw the acuity field.
            //      else set the current axe to the next one and start to step 1.
            // else go to step 4.
            switch (calibStep)
            {
                case 1:
                    landoltC.transform.localPosition = landoltC.transform.localPosition != savedFOVTargetpos ? savedFOVTargetpos : landoltC.transform.localPosition; // 1.
                    calibStep++;
                    break;
                case 2:
                    // test if the direction moves the target closer or further from the center
                    keyCodeIndex = GetDirectionIndexPressed();
                    CalculateAcceptedMovingDirection(); // accepted two directions for current axe
                    if (keyCodeIndex != -1 && acceptedDirection.Contains(moveDirections[keyCodeIndex]))
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
                case 3:
                    // Visual help for patient touchpad touch WIP
                    // get the good direction click corresponding to Landolt C opened side
                    // Function to listen to click, get the index
                    keyCodeIndex = GetDirectionIndexPressed();
                    if (keyCodeIndex == rotatIndex)
                    {
                        calibStep++; // the patient pressed the good direction, move to next tt
                    }
                    break;
                case 4:
                    if (currentTargetIndex >= FOVEdgePoints.Count - 1)
                        calibrationIsOver = true;
                    else
                    {
                        currentTargetIndex++;
                        calibStep = 1;
                        ToggleRadialMenu(); //remove the radial Menu around the Landolt C
                    }
                    break;
            }
        }
    }

    private void CalculateAcceptedMovingDirection()
    {
        //"right", "down", "left", "up", "right-up", "right-down", "left-up", "left-down"

        acceptedDirection.Clear();
        switch (currentTargetIndex)
        {
            case 0:
            case 4:
                acceptedDirection.Add("right");
                acceptedDirection.Add("left");
                break;
            case 2:
            case 6:
                acceptedDirection.Add("down");
                acceptedDirection.Add("up");
                break;
            case 3:
            case 7:
                acceptedDirection.Add("right-up");
                acceptedDirection.Add("left-down");
                acceptedDirection.Add("down");
                acceptedDirection.Add("up");
                acceptedDirection.Add("right");
                acceptedDirection.Add("left");
                break;
            case 1:
            case 5:
                acceptedDirection.Add("right-down");
                acceptedDirection.Add("left-up");
                acceptedDirection.Add("down");
                acceptedDirection.Add("up");
                acceptedDirection.Add("right");
                acceptedDirection.Add("left");
                break;
            default:
                break;
        }
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

    private void SetTargetPosition()
    {
        //FOVPoints
        if (!pointingSystem.start)
        {
            if (FOVEdgePoints.Count == 0)
                CalculateAllPos();

            if (changePos)
                SetRandomLandoltOrientation();

            SetPos();
        }
        else if (pointingSystem.start)
        {
            if (FOVEdgePoints.Count == 0)
                CalculateAllPosFromPointing();

            if (changePos)
                SetRandomLandoltOrientation();

            SetPosFromPointing();

        }
    }

    private void SetPos()
    {
        landoltC.transform.position = FOVEdgePoints[currentTargetIndex];
        Vector3 pt = landoltC.transform.localPosition;
        if (pt.x == 0)
            pt.y = pt.y > 0 ? pt.y - 0.04f : pt.y + 0.04f;
        else if (pt.y == 0)
            pt.x = pt.x > 0 ? pt.x - 0.04f : pt.x + 0.04f;
        pt.z = -3.0f;
        landoltC.transform.localPosition = pt;
    }

    private void CalculateAllPos()
    {
        FOVEdgePoints.Add(FOVPoints[0]); // right point
        FOVEdgePoints.Add(FOVPoints[1]); // bottom point
        FOVEdgePoints.Insert(1, (FOVEdgePoints[1] + (FOVEdgePoints[0] - FOVEdgePoints[1]) / 2)); // Bottom right point
        FOVEdgePoints.Add(FOVPoints[2]); // left point
        FOVEdgePoints.Insert(3, (FOVEdgePoints[3] + (FOVEdgePoints[2] - FOVEdgePoints[3]) / 2)); // Bottom Left point 
        FOVEdgePoints.Add(FOVPoints[3]); // Top point
        FOVEdgePoints.Insert(5, (FOVEdgePoints[5] + (FOVEdgePoints[4] - FOVEdgePoints[5]) / 2)); // Top left point 
        FOVEdgePoints.Insert(7, (FOVEdgePoints[0] + (FOVEdgePoints[6] - FOVEdgePoints[0]) / 2)); // Right point
    }

    private void CalculateAllPosFromPointing()
    {
        // Calculate position for the target position from the point the user placed

        int index_left;
        Vector3 pt_left = new Vector3();
        int index_right;
        Vector3 pt_right = new Vector3();
        int index_top;
        Vector3 pt_top = new Vector3();
        int index_down;
        Vector3 pt_down = new Vector3();

        foreach (var pt in pointingSystem.handPoints)
        {
            if (pt.x > pt_right.x)
            {
                pt_right = pt;
                index_right = pointingSystem.handPoints.IndexOf(pt);
            }
            if (pt.x < pt_left.x)
            {
                pt_left = pt;
                index_left = pointingSystem.handPoints.IndexOf(pt);
            }
            if (pt.y > pt_top.y)
            {
                pt_top = pt;
                index_top = pointingSystem.handPoints.IndexOf(pt);
            }
            if (pt.y < pt_down.y)
            {
                pt_down = pt;
                index_down = pointingSystem.handPoints.IndexOf(pt);
            }
        }
        FOVEdgePoints.Add(pt_right);
        FOVEdgePoints.Add(pt_down);
        FOVEdgePoints.Insert(1, (FOVEdgePoints[1] + (FOVEdgePoints[0] - FOVEdgePoints[1]) / 2)); // Bottom right point
        FOVEdgePoints.Add(pt_left);
        FOVEdgePoints.Insert(3, (FOVEdgePoints[3] + (FOVEdgePoints[2] - FOVEdgePoints[3]) / 2)); // Bottom Left point 
        FOVEdgePoints.Add(pt_top);
        FOVEdgePoints.Insert(5, (FOVEdgePoints[5] + (FOVEdgePoints[4] - FOVEdgePoints[5]) / 2)); // Top left point 
        FOVEdgePoints.Insert(7, (FOVEdgePoints[0] + (FOVEdgePoints[6] - FOVEdgePoints[0]) / 2)); // Right point

        int i = 0;
        foreach (var item in FOVEdgePoints)
        {
            Vector3 pt = FOVEdgePoints[i];
            pt.z = -3.0f;
            FOVpt.Add(pt);
            i++;
        }
    }

    private void SetPosFromPointing()
    {
        //almostCircle.transform.localPosition = FOVEdgePoints[currentTargetIndex];
        Vector3 pt = FOVEdgePoints[currentTargetIndex];
        pt.z = -3.0f;
        FOVpt.Add(pt);
        landoltC.transform.localPosition = pt;
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
}
