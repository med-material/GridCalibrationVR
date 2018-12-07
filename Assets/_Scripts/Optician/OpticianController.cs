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

    public GameObject almostCircle;
    public Text explainText;
    public GameObject FOVTarget;
    public GridController gridController;
    public List<Vector3> FOVPointsLocal;
    public Transform OperatorPlane;
    public PointingSystem pointingSystem;
    public bool handlerMode = false;

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

    void Start()
    {
        // GENERAL SETUP
        lineRenderer = GetComponent<LineRenderer>();
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
            almostCircle.SetActive(false);
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
        }
    }

    private void UpdateMaxFOVCalibration()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
            almostCircle.transform.position += vector;
            almostCircle.transform.localPosition = new Vector3(almostCircle.transform.localPosition.x, almostCircle.transform.localPosition.y, -3.0f);
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
            savedTargetposList[currentTargetIndex] = almostCircle.transform.localPosition;
        }
        else
        {
            savedTargetposList.Insert(currentTargetIndex, almostCircle.transform.localPosition);
        }
    }

    private void UpdateAcuityCalibration()
    {
        if (!almostCircle.activeSelf)
        {
            explainText.text = "Press the up or down arrow to increase or reduce size of the \n circle to the minimum size for wich you can still see "
            + "the open side of it. \n Press space bar to start";
            explainText.enabled = true;
            explainText.color = textColor;
            almostCircle.SetActive(true);
        }

        if (!isSizeOk)
        {
            if (Input.GetKeyDown(downArrow) && !isConfirmingPosition)
            {
                ReduceCircleSize();
                SetRandomCircleOrientation();
            }
            else if (Input.GetKeyDown(upArrow) && !isConfirmingPosition)
            {
                IncreaseCircleSize();
                SetRandomCircleOrientation();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                isConfirmingPosition = !isConfirmingPosition;
            }
            else if (isConfirmingPosition)
            {
                keyCodeIndex = GetKeyCodeIndexPressed();
                if (keyCodeIndex == rotatIndex)
                {
                    isSizeOk = true;
                    explainText.text = "";
                }
            }
        }
        else // Size is well set by user
        {
            offSetTimer += Time.deltaTime;
            if (!isCircleSet)
            {
                offSetTimer = 0;
                SetTargetPosition(); // Set the target position
                isCircleSet = true;
            }
            else if (!hasTargetMoved)
            {
                keyCodeIndex = GetKeyCodeIndexPressed();
                if (keyCodeIndex != -1) // move the target in the field of view of the user
                {
                    moveDirection = moveDirections[keyCodeIndex]; // set the direction corresponding to the user input
                    if (offSetTimer > 0.1f) // avoid the circle to move just after spawning, getting the input from the previous circle
                        MoveTarget();
                }
                if (Input.GetKeyDown(KeyCode.Space)) // confirm the target is visible
                {
                    SavePos();
                    hasTargetMoved = true;
                }
            }
            else
            {
                keyCodeIndex = GetKeyCodeIndexPressed();
                if (rotatIndex == keyCodeIndex) // if the user pressed the good arrow
                {
                    SaveTargetPosition();
                    hasTargetMoved = false;
                    isCircleSet = false;
                    changePos = true;
                    if (currentTargetIndex + 1 == FOVEdgePoints.Count)
                    {
                        calibrationIsOver = true;
                        //OnPostRender();
                    }
                    else
                    {
                        currentTargetIndex++;
                    }
                    return;
                }
                else if (rotatIndex != keyCodeIndex)
                {
                    if (keyCodeIndex != -1)
                    {
                        errors++;
                        isCircleSet = false;
                        hasTargetMoved = false;
                        changePos = false;
                    }

                }
            }
        }
    }

    private void ReduceCircleSize()
    {
        almostCircle.transform.localScale *= 0.8f;
    }

    private void IncreaseCircleSize()
    {
        almostCircle.transform.localScale /= 0.8f;
    }

    private void SetTargetPosition()
    {
        //FOVPoints
        if (!pointingSystem.start)
        {
            if (FOVEdgePoints.Count == 0)
                CalculateAllPos();

            if (changePos)

                SetRandomCircleOrientation();

            SetPos();
        }
        else if (pointingSystem.start)
        {
            if (FOVEdgePoints.Count == 0)
                CalculateAllPosFromPointing();

            if (changePos)
                SetRandomCircleOrientation();

            SetPosFromPointing();

        }
    }

    private void SetPos()
    {
        almostCircle.transform.position = FOVEdgePoints[currentTargetIndex];
        Vector3 pt = almostCircle.transform.localPosition;
        if (pt.x == 0)
            pt.y = pt.y > 0 ? pt.y - 0.04f : pt.y + 0.04f;
        else if (pt.y == 0)
            pt.x = pt.x > 0 ? pt.x - 0.04f : pt.x + 0.04f;
        pt.z = -3.0f;
        almostCircle.transform.localPosition = pt;
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

        // TODO : Test it tomorrow morning.
    }

    private void SetPosFromPointing()
    {


    }
    private int GetKeyCodeIndexPressed()
    {
        if (Input.GetKey(leftArrow))
        {
            if (Input.GetKey(upArrow))
            {
                return 6;
            }
            else if (Input.GetKey(downArrow))
            {
                return 7;
            }
        }
        else if (Input.GetKey(rightArrow))
        {
            if (Input.GetKey(upArrow))
            {
                return 4;
            }
            else if (Input.GetKey(downArrow))
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
        }
        return -1;
    }

    private void SetRandomCircleOrientation()
    {
        rotatIndex = GetRandomIndex(l_rotation, rotatIndex);
        int rotat = l_rotation[rotatIndex];
        almostCircle.transform.localRotation = Quaternion.Euler(0, 0, rotat);
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
