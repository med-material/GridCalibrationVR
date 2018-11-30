using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OpticianController : MonoBehaviour
{

    public GameObject almostCircle;
    public Text explainText;
    public GameObject FOVTarget;
    public GridController gridController;

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
    private string mode = "auto";
    private bool hasTargetMoved = false;
    private bool isCircleSet = false;
    private bool changePos = false;
    private int currentTargetIndex = 0;
    private bool calibrationIsOver;
    private Material lineMaterial;
    private float offSetTimer = 0;

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

        if (!lineMaterial)
        {
            lineMaterial = new Material(Shader.Find("Custom/GizmoShader"));
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }
    }

    void OnPostRender()
    {
        if (calibrationIsOver)
        {
            almostCircle.SetActive(false);
            DrawLines(savedTargetposList, Color.blue);
        }
        if (isFOVCalibEnded)
        {
            DrawLines(FOVEdgePoints, Color.red);
        }
    }

    // To show the lines in the editor
    void OnDrawGizmos()
    {
        if (calibrationIsOver)
        {
            almostCircle.SetActive(false);
            DrawLines(savedTargetposList, Color.blue);
        }
        if (isFOVCalibEnded)
        {
            DrawLines(FOVEdgePoints, Color.red);
        }
    }

    void Update()
    {
        userHit = gridController.GetCurrentCollider();
        if (calibrationIsOver)
        {
            almostCircle.SetActive(false);
        }
        else
        {
            if (isFOVCalibEnded)
                UpdateAcuityCalibration();
            else
                UpdateMaxFOVCalibrationAuto();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    private void DrawLines(List<Vector3> point_list, Color line_color)
    {
        foreach (Vector3 t in point_list)
        {
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);
            GL.Color(line_color);
            if (t.Equals(point_list.Last())) // if this is the last point, link to the first one to close the area
            {
                GL.Vertex3(t.x, t.y, t.z);
                GL.Vertex3(point_list[0].x, point_list[0].y, point_list[0].z);
            }
            else
            {
                Vector3 next_t = point_list[point_list.IndexOf(t) + 1];
                GL.Vertex3(t.x, t.y, t.z);
                GL.Vertex3(next_t.x, next_t.y, next_t.z);
            }

            GL.End();
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
                if (nbDirectionEnded == moveDirections.Count - 4)
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
        if (FOVEdgePoints.Count == 0)
        {
            CalculateAllPos();
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
        FOVEdgePoints.Add(new Vector3(FOVPoints[2].x, FOVPoints[1].y, FOVPoints[0].z)); // Bottom left edge point
        FOVEdgePoints.Insert(1, (FOVEdgePoints[1] + (FOVEdgePoints[0] - FOVEdgePoints[1]) / 2)); // Bottom middle point
        FOVEdgePoints.Add(new Vector3(FOVPoints[2].x, FOVPoints[3].y, FOVPoints[0].z)); // Top left edge point
        FOVEdgePoints.Insert(3, (FOVEdgePoints[3] + (FOVEdgePoints[2] - FOVEdgePoints[3]) / 2)); // Middle Right point
        FOVEdgePoints.Add(new Vector3(FOVPoints[0].x, FOVPoints[3].y, FOVPoints[0].z)); // Top right edge point
        FOVEdgePoints.Insert(5, (FOVEdgePoints[5] + (FOVEdgePoints[4] - FOVEdgePoints[5]) / 2)); // Top middle point
        FOVEdgePoints.Insert(7, (FOVEdgePoints[0] + (FOVEdgePoints[6] - FOVEdgePoints[0]) / 2)); // Right middle point
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
