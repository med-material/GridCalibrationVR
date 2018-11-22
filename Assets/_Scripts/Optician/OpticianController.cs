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
    private Color textColor = new Color(0.6415094f, 0.6415094f, 0.6415094f, 1.0f);


    void Start()
    {

        //// ACUITY SETUP 
        l_rotation = new List<int> { 0, -90, 180, 90 }; // Right, Down, Left, Up
        keyCodes = new List<KeyCode> { rightArrow, downArrow, leftArrow, upArrow }; // have to stay same order than rotation list !!

        //// FOV SETUP
        FOVTargetRenderer = FOVTarget.GetComponent<Renderer>();
        FOVTargetRenderer.material.color = Color.black;
        savedFOVTargetpos = FOVTarget.transform.position;
        FOVEdgePoints = new List<Vector3>();
        moveDirections = new List<string> { "right", "down", "left", "up" };
        FOVPoints = new List<Vector3>();
        explainText.text = "Please look at the target while it moves \n to determine your max FOV";
        explainText.color = textColor;
        nbDirectionEnded = 0;
        ResetFOVTimer();
        moveDirection = moveDirections[nbDirectionEnded];
    }

    void Update()
    {
        userHit = gridController.GetCurrentCollider();

        if (isFOVCalibEnded)
        {
            UpdateAcuityCalibration();
        }
        else
        {
            UpdateMaxFOVCalibration();
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
        if (explainText.isActiveAndEnabled && explainText.color.a == 1.0f)
            StartCoroutine("FadeText");
        switch (moveDirection)
        {
            case "left":
                Move(new Vector3(-0.01f, 0.0f, 0.0f));
                break;
            case "down":
                Move(new Vector3(0.0f, -0.01f, 0.0f));
                break;
            case "right":
                Move(new Vector3(0.01f, 0.0f, 0.0f));
                break;
            case "up":
                Move(new Vector3(0.0f, 0.01f, 0.0f));
                break;
            default:
                break;
        }
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
        FOVTarget.transform.position += vector;
    }

    private void UpdateAcuityCalibration()
    {
        if (!almostCircle.activeSelf)
        {
            explainText.text = "Press the arrow corresponding to the \n open circle, for exemple right for this one. \n Press the right arrow to start";
            explainText.enabled = true;
            explainText.color = textColor;
            almostCircle.SetActive(true);
        }

        if (Input.GetKeyDown(rightArrow) && !isAcuityCalibStarted)
        {
            isAcuityCalibStarted = true;
            StartCoroutine("FadeText");
        }
        if (Input.anyKeyDown && isAcuityCalibStarted)
        {
            keyCodeIndex = GetKeyCodeIndexPressed();
            if (rotatIndex == keyCodeIndex) // if the user pressed the good arrow
            {
                SetRandomCircleOrientation();
                if (errors > 1)
                    SetRandomEdgePosition();
            }
            else if (rotatIndex != keyCodeIndex)
            {
                SetRandomCircleOrientation();
                errors++;
                print("WRONG KEY !" + errors);
            }
        }
    }

    private void SetRandomEdgePosition()
    {
        //FOVPoints
        CalculateEdgePoints();
        int rand_index = GetRandomIndex(FOVEdgePoints, -1);
        int rand_index_2 = GetRandomIndex(FOVEdgePoints, rand_index);
        Vector3 randPos = new Vector3(UnityEngine.Random.Range(FOVEdgePoints[rand_index].x, FOVEdgePoints[rand_index_2].x),
         UnityEngine.Random.Range(FOVEdgePoints[rand_index].y, FOVEdgePoints[rand_index_2].y), FOVEdgePoints[rand_index].z);
        almostCircle.transform.position = randPos;
    }

    private void CalculateEdgePoints()
    {
        FOVEdgePoints.Add(new Vector3(FOVPoints[0].x, FOVPoints[1].y, FOVPoints[0].z)); // Bottom right edge point
        FOVEdgePoints.Add(new Vector3(FOVPoints[2].x, FOVPoints[1].y, FOVPoints[0].z)); // Bottom left edge point
        FOVEdgePoints.Add(new Vector3(FOVPoints[2].x, FOVPoints[3].y, FOVPoints[0].z)); // Top left edge point
        FOVEdgePoints.Add(new Vector3(FOVPoints[0].x, FOVPoints[3].y, FOVPoints[0].z)); // Top right edge point
    }
    private int GetKeyCodeIndexPressed()
    {
        foreach (KeyCode k in keyCodes)
        {
            if (Input.GetKeyDown(k))
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
