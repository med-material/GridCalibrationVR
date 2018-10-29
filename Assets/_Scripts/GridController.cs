using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{

    private Camera sceneCamera;
    private GazePrint gazePrint;
    private TextMesh positionText;
    private LineRenderer heading;
    private Vector3 standardViewportPoint = new Vector3(0.5f, 0.5f, 10);
    private Vector2 gazePointLeft;
    private Vector2 gazePointRight;
    private Vector2 gazePointCenter;
    public Material shaderMaterial;
    private RaycastHit hit;

    void Start()
    {
        PupilData.calculateMovingAverage = false;
        positionText = gameObject.GetComponent<TextMesh>();
        sceneCamera = gameObject.GetComponent<Camera>();
        gazePrint = gameObject.GetComponent<GazePrint>();
        heading = gameObject.GetComponent<LineRenderer>();
    }

    void OnEnable()
    {
        if (PupilTools.IsConnected)
        {
            PupilTools.IsGazing = true;
            PupilTools.SubscribeTo("gaze");
            PupilTools.SubscribeTo("fixation");
            PupilTools.SubscribeTo("pupil.");

            PupilTools.OnReceiveData += CustomReceiveData;
        }
    }


    void Update()
    {
        Vector3 viewportPoint = standardViewportPoint;

        if (PupilTools.IsConnected && PupilTools.IsGazing)
        {
            gazePointLeft = PupilData._2D.GetEyePosition(sceneCamera, PupilData.leftEyeID);
            gazePointRight = PupilData._2D.GetEyePosition(sceneCamera, PupilData.rightEyeID);
            gazePointCenter = PupilData._2D.GazePosition;
            viewportPoint = new Vector3(gazePointCenter.x, gazePointCenter.y, 1f);
        }

        if (heading.enabled)
        {
            heading.SetPosition(0, sceneCamera.transform.position - sceneCamera.transform.up);

            Ray ray = sceneCamera.ViewportPointToRay(viewportPoint);
            if (Physics.Raycast(ray, out hit))
            {
                heading.SetPosition(1, hit.point);

                positionText.text = sceneCamera.transform.InverseTransformDirection(hit.point).ToString();
            }
            else
            {
                heading.SetPosition(1, ray.origin + ray.direction * 50f);
            }
        }
    }
    void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        if (topic.StartsWith("pupil"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "confidence":
                        print("Confidence : " + PupilTools.FloatFromDictionary(dictionary, item.Key));
                        break;
                    case "norm_pos": // Origin 0,0 at the bottom left and 1,1 at the top right.
                        //print("Norm : " + PupilTools.VectorFromDictionary(dictionary, item.Key));
                        break;
                    default:
                        break;
                }
            }
        }
    }


    public RaycastHit GetCurrentCollider()
    {
        return hit;
    }
}
