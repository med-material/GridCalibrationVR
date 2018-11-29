using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpticianController : MonoBehaviour
{

    public GameObject almostCircle;
    public Text explainText;
    public GameObject FOVTarget;
    public GridController gridController;

    private Renderer FOVTargetRenderer;
    private bool isFOVCalibEnded;
    private bool isAcuityCalibStarted;
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
    private string mode = "auto";
    private bool hasTargetMoved = false;
    private bool isCircleSet = false;
    private bool changePos = false;
    private int currentTargetIndex = 0;
    private bool calibrationIsOver;

    void Start()
    {

        //// ACUITY SETUP 
        l_rotation = new List<int> { 0, -90, 180, 90 }; // Right, Down, Left, Up
        keyCodes = new List<KeyCode> { rightArrow, downArrow, leftArrow, upArrow }; // have to stay same order than rotation list !!
        savedTargetposList = new List<Vector3>();
        //// FOV SETUP

        savedFOVTargetpos = FOVTarget.transform.position;
        FOVEdgePoints = new List<Vector3>();
        moveDirections = new List<string> { "right", "down", "left", "up", "right-up", "right-down", "left-up", "left-down" };
        FOVPoints = new List<Vector3>();
        FOVTargetRenderer = FOVTarget.GetComponent<Renderer>();
        if (mode == "auto")
        {
            FOVTargetRenderer.material.color = Color.red;
            explainText.text = "Please fix the red centered dot \n Press space bar when the target is out of \n your field of view."
                + "\n Press space bar to start.";
            FOVTimer = 0;
        }
        else
        {
            FOVTargetRenderer.material.color = Color.black;
            explainText.text = "Please look at the target while it moves \n to determine your max FOV";
            ResetFOVTimer();
        }
        explainText.color = textColor;
        nbDirectionEnded = 0;
        moveDirection = moveDirections[nbDirectionEnded];
    }

    void Update()
    {
        userHit = gridController.GetCurrentCollider();
        if (calibrationIsOver)
        {
            Debug.DrawLine(savedTargetposList[0], savedTargetposList[1], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[1], savedTargetposList[2], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[2], savedTargetposList[3], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[3], savedTargetposList[4], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[4], savedTargetposList[5], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[5], savedTargetposList[6], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[6], savedTargetposList[7], Color.blue, 200);
            Debug.DrawLine(savedTargetposList[7], savedTargetposList[0], Color.blue, 200);
            print("Calibration is over !");
        }
        else
        {
            if (isFOVCalibEnded)
                UpdateAcuityCalibration();
            else if (mode == "auto")
                UpdateMaxFOVCalibrationAuto();
            else
                UpdateMaxFOVCalibration();
            // The method UpdateMaxFOVCalibration use the gaze and suppose the calibration is good. Most of the time it is not.
            // Use the other method to detect the user max FOV.
        }

    }

    private void UpdateMaxFOVCalibrationAuto()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (FOVTimer != 0) // If the space bar has been pressed and is wasn't to start the game.
            {
                // Save the target position, set the next direction to come.
                SaveTargetPosition();
                FOVTarget.transform.position = savedFOVTargetpos; // reset the target position at center before new direction
                nbDirectionEnded++;
                if (nbDirectionEnded == moveDirections.Count)
                {
                    isFOVCalibEnded = true;
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

    private void UpdateMaxFOVCalibration()
    {
        // while timer is under x seconds and the user is looking at the target
        if (System.Object.ReferenceEquals(userHit.collider.gameObject, FOVTarget) && FOVTimer > 0f)
            MoveTarget();
        else if (System.Object.ReferenceEquals(userHit.collider.gameObject, FOVTarget))
            FOVTimer -= Time.deltaTime;
        else if (FOVTimer > 0)
            FOVTimer -= Time.deltaTime;
        else if (FOVTimer < 0)
        {
            SaveTargetPosition();
            FOVTarget.transform.position = savedFOVTargetpos; // reset the target position at center before new direction
            ResetFOVTimer();
            // timer is ended, go to next direction
            nbDirectionEnded++;
            if (nbDirectionEnded == moveDirections.Count)
            {
                isFOVCalibEnded = true;
                FOVTarget.SetActive(false);
            }
            else
                moveDirection = moveDirections[nbDirectionEnded];
        }
    }

    private void ResetFOVTimer()
    {
        FOVTimer = 3.0f;
    }
    private void SaveTargetPosition()
    {
        FOVPoints.Add(FOVTarget.transform.position);
    }

    private void MoveTarget()
    {
        Vector3 direction = new Vector3();
        if (explainText.isActiveAndEnabled && explainText.color.a == 1.0f)
            StartCoroutine("FadeText");
        switch (moveDirection)
        {
            case "left":
                direction = new Vector3(-0.01f, 0.0f, 0.0f);
                break;
            case "down":
                direction = new Vector3(0.0f, -0.01f, 0.0f);
                break;
            case "right":
                direction = new Vector3(0.01f, 0.0f, 0.0f);
                break;
            case "up":
                direction = new Vector3(0.0f, 0.01f, 0.0f);
                break;
            case "right-up":
                direction = new Vector3(0.01f, 0.01f, 0.0f);
                break;
            case "left-up":
                direction = new Vector3(-0.01f, 0.01f, 0.0f);
                break;
            case "right-down":
                direction = new Vector3(0.01f, -0.01f, 0.0f);
                break;
            case "left-down":
                direction = new Vector3(-0.01f, -0.01f, 0.0f);
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
            if (savedTargetposList.Count - 1 == currentTargetIndex)
            {
                savedTargetposList[currentTargetIndex] = almostCircle.transform.position;
            }
            else
            {
                savedTargetposList.Insert(currentTargetIndex, almostCircle.transform.position);
            }
        }
        else
            FOVTarget.transform.position += vector;
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
            if (Input.GetKeyDown(downArrow))
            {
                ReduceCircleSize();
                SetRandomCircleOrientation();
            }
            else if (Input.GetKeyDown(upArrow))
            {
                IncreaseCircleSize();
                SetRandomCircleOrientation();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                isSizeOk = true;
                explainText.text = "";
            }
        }
        else // Size is set by user
        {
            if (!isCircleSet)
            {
                SetTargetPosition(); // Set the target position
                isCircleSet = true;
            }
            else if (!hasTargetMoved)
            {
                keyCodeIndex = GetKeyCodeIndexPressed();
                if (keyCodeIndex != -1) // move the target in the field of view of the user
                {
                    moveDirection = moveDirections[keyCodeIndex]; // set the direction corresponding to the user input
                    MoveTarget();
                }
                if (Input.GetKeyDown(KeyCode.Space)) // confirm the target is visible
                {
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
                        return;
                    }
                    else
                    {
                        currentTargetIndex++;
                    }
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
        if (FOVEdgePoints.Count == 0)
        {
            CalculateAllPos();
            //CalculateAllPos(); // Use this for better corner FOV 
            Debug.DrawLine(FOVEdgePoints[0], FOVEdgePoints[1], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[1], FOVEdgePoints[2], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[2], FOVEdgePoints[3], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[3], FOVEdgePoints[4], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[4], FOVEdgePoints[5], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[5], FOVEdgePoints[6], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[6], FOVEdgePoints[7], Color.red, 200);
            Debug.DrawLine(FOVEdgePoints[7], FOVEdgePoints[0], Color.red, 200);
        }

        //CalculateRandomPos();
        if (changePos)
        {
            SetRandomCircleOrientation();
            almostCircle.transform.position = FOVEdgePoints[currentTargetIndex];
            // set the new pos
        }
        else
        {
            almostCircle.transform.position = FOVEdgePoints[currentTargetIndex];
            // set the same previous pos
        }
    }

    private void CalculateAllPos()
    {
        FOVEdgePoints.Add(new Vector3(FOVPoints[0].x, FOVPoints[1].y, FOVPoints[0].z)); // Bottom right edge point
        FOVEdgePoints.Add(FOVPoints[1]);
        FOVEdgePoints.Add(new Vector3(FOVPoints[2].x, FOVPoints[1].y, FOVPoints[0].z)); // Bottom left edge point
        FOVEdgePoints.Add(FOVPoints[2]);
        FOVEdgePoints.Add(new Vector3(FOVPoints[2].x, FOVPoints[3].y, FOVPoints[0].z)); // Top left edge point
        FOVEdgePoints.Add(FOVPoints[3]);
        FOVEdgePoints.Add(new Vector3(FOVPoints[0].x, FOVPoints[3].y, FOVPoints[0].z)); // Top right edge point
        FOVEdgePoints.Add(FOVPoints[0]);
    }

    private int GetKeyCodeIndexPressed()
    {
        if (Input.GetKey(leftArrow))
        {
            if (Input.GetKey(upArrow))
            {

            }
            else if (Input.GetKey(downArrow))
            {

            }
        }
        else if (Input.GetKey(rightArrow))
        {
            if (Input.GetKey(upArrow))
            {

            }
            else if (Input.GetKey(downArrow))
            {

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

    private void CalculateRandomPos()
    {
        int rand_index = GetRandomIndex(FOVEdgePoints, -1);
        int rand_index_2 = GetRandomIndex(FOVEdgePoints, rand_index);
        float rand_x = UnityEngine.Random.Range(FOVEdgePoints[rand_index].x, FOVEdgePoints[rand_index_2].x);
        print("X : " + rand_x);
        float rand_y = UnityEngine.Random.Range(FOVEdgePoints[rand_index].y, FOVEdgePoints[rand_index_2].y);
        print("Y : " + rand_y);
        Vector3 randPos = new Vector3(rand_x - almostCircle.transform.localScale.x, rand_y - almostCircle.transform.localScale.y, FOVEdgePoints[rand_index].z);
        almostCircle.transform.position = randPos;
    }


}
