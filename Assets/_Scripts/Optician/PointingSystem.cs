using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using Valve.VR;

public class PointingSystem : MonoBehaviour
{
    public enum AxisType
    {
        XAxis,
        ZAxis
    }

    public GameObject StartButton;
    public Color color;
    public float thickness = 0.002f;
    public AxisType facingAxis = AxisType.ZAxis;
    public float length = 100f;
    public bool showCursor = true;

    [HideInInspector]
    public bool isCalibEnded = false;
    public Text explainText;
    public delegate void OnAddPointdeleg();

    internal Vector3 handPoint;
    internal List<Vector3> handPoints;
    internal bool start;

    GameObject holder;
    GameObject pointer;
    GameObject cursor;
    RaycastHit hitObject;

    Vector3 cursorScale = new Vector3(0.05f, 0.05f, 0.05f);
    float contactDistance = 0f;
    Transform contactTarget = null;
    public bool selectAxes;
    private Text textButton;
    public List<Vector3> selectedAxes;

    public static event OnAddPointdeleg OnAddPoint;

    void Start()
    {
        textButton = StartButton.GetComponentInChildren<Text>();
        textButton.text = "SET FOV POINTS";
        selectedAxes = new List<Vector3>();
    }
    void SetPointerTransform(float setLength, float setThicknes)
    {
        //if the additional decimal isn't added then the beam position glitches
        float beamPosition = setLength / (2 + 0.00001f);

        if (facingAxis == AxisType.XAxis)
        {
            pointer.transform.localScale = new Vector3(setLength, setThicknes, setThicknes);
            pointer.transform.localPosition = new Vector3(beamPosition, 0f, 0f);
            if (showCursor)
            {
                cursor.transform.localPosition = new Vector3(setLength - cursor.transform.localScale.x, 0f, 0f);
            }
        }
        else
        {
            pointer.transform.localScale = new Vector3(setThicknes, setThicknes, setLength);
            pointer.transform.localPosition = new Vector3(0f, 0f, beamPosition);

            if (showCursor)
            {
                cursor.transform.localPosition = new Vector3(0f, 0f, setLength - cursor.transform.localScale.z);
            }
        }
    }

    // Use this for initialization
    internal void SetupPointingSystem()
    {
        StartButton.SetActive(true);
        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);

        holder = new GameObject();
        holder.transform.parent = this.transform;
        holder.transform.localPosition = Vector3.zero;

        pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer.transform.parent = holder.transform;
        pointer.GetComponent<MeshRenderer>().material = newMaterial;

        pointer.GetComponent<BoxCollider>().isTrigger = true;
        pointer.AddComponent<Rigidbody>().isKinematic = true;
        pointer.layer = 2;

        if (showCursor)
        {
            cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cursor.transform.parent = holder.transform;
            cursor.GetComponent<MeshRenderer>().material = newMaterial;
            cursor.transform.localScale = cursorScale;

            cursor.GetComponent<SphereCollider>().isTrigger = true;
            cursor.AddComponent<Rigidbody>().isKinematic = true;
            cursor.layer = 2;
        }

        explainText.text = "Place point with the controler \n with the back button at the \n limit of your FOV";

        handPoints = new List<Vector3>();

        SetPointerTransform(length, thickness);
    }


    float GetBeamLength(bool bHit, RaycastHit hit)
    {
        float actualLength = length;

        //reset if beam not hitting or hitting new target
        if (!bHit || (contactTarget && contactTarget != hit.transform))
        {
            contactDistance = 0f;
            contactTarget = null;
        }

        //check if beam has hit a new target
        if (bHit)
        {
            if (hit.distance <= 0)
            {

            }
            contactDistance = hit.distance;
            contactTarget = hit.transform;
        }

        //adjust beam length if something is blocking it
        if (bHit && contactDistance < length)
        {
            actualLength = contactDistance;
        }

        if (actualLength <= 0)
        {
            actualLength = length;
        }

        return actualLength;
    }

    void Update()
    {
        Ray raycast = new Ray(transform.position, transform.forward);

        bool rayHit = Physics.Raycast(raycast, out hitObject);
        if (start && !selectAxes)
        {
            if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any) && !GameObject.ReferenceEquals(hitObject.collider.gameObject, StartButton))
            {
                AddPoint();
            }
            float beamLength = GetBeamLength(rayHit, hitObject);
            SetPointerTransform(beamLength, thickness);
            if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any))
            {
                if (GameObject.ReferenceEquals(hitObject.collider.gameObject, StartButton))
                {
                    StartButton.SetActive(false);
                    // isCalibEnded = true;
                    selectAxes = true;
                    textButton.text = "SELECT AXES";
                }
            }
        }
        if (selectAxes)
        {
            if (selectedAxes.Count < 4)
            {
                if (SteamVR_Input._default.inActions.GrabPinch.GetStateDown(SteamVR_Input_Sources.Any) && !GameObject.ReferenceEquals(hitObject.collider.gameObject, StartButton))
                {
                    Vector3 selected_point = new Vector3();
                    if (hitObject.collider != null)
                    {
                        handPoint = hitObject.collider.transform.worldToLocalMatrix.MultiplyPoint3x4(hitObject.point);
                        float dist = float.MaxValue;

                        foreach (Vector3 point in handPoints)
                        {
                            float temp_dist = Vector3.Distance(point, handPoint);
                            if (temp_dist <= dist)
                            {
                                dist = temp_dist;
                                selected_point = point;
                            }
                        }
                    }
                    selectedAxes.Add(selected_point);
                }
            }
            if (selectedAxes.Count == 4)
            {
                isCalibEnded = true;
            }
        }
    }

    private void AddPoint()
    {
        if (hitObject.collider != null)
        {
            handPoint = hitObject.collider.transform.worldToLocalMatrix.MultiplyPoint3x4(hitObject.point);
            handPoints.Add(handPoint);
            if (OnAddPoint != null)
                OnAddPoint();
        }
    }
}