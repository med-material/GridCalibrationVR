using UnityEngine;
using System.Collections;
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
    public bool isCalibEnded = false;
    public Text explainText;

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
    void SetupPointingSystem()
    {
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
                    start = true;
                    SetupPointingSystem();
                }
            }
        }
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
        if (start)
        {
            Ray raycast = new Ray(transform.position, transform.forward);

            bool rayHit = Physics.Raycast(raycast, out hitObject);
            if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any))
            {
                AddPoint();
            }
            float beamLength = GetBeamLength(rayHit, hitObject);
            SetPointerTransform(beamLength, thickness);
            if (SteamVR_Input._default.inActions.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any))
            {
                if (GameObject.ReferenceEquals(hitObject.collider.gameObject, StartButton))
                {
                    isCalibEnded = true;
                }
            }
        }
    }

    private void AddPoint()
    {
        if (hitObject.collider != null)
        {
            handPoint = hitObject.collider.transform.worldToLocalMatrix.MultiplyPoint3x4(hitObject.point);
            handPoints.Add(handPoint);
        }
    }
}