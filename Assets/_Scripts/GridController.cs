using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    public Camera eyeCamera;
    private Camera sceneCamera;
    private TextMesh positionText;
    public LineRenderer heading;
    private Vector3 standardViewportPoint = new Vector3(0.5f, 0.5f, 10);
    private Vector2 gazePointLeft;
    private Vector2 gazePointRight;
    private Vector2 gazePointCenter;
    private RaycastHit hit;
    private RaycastHit circleHit;
    private RaycastHit[] hits;
    LayerMask collisionCircleLayer;

    void Start()
    {
        PupilData.calculateMovingAverage = false;
        positionText = gameObject.GetComponent<TextMesh>();
        sceneCamera = gameObject.GetComponent<Camera>();
    }

    void OnEnable()
    {
        if (PupilTools.IsConnected)
        {
            PupilTools.IsGazing = true;
            PupilTools.SubscribeTo("gaze");
            eyeCamera.gameObject.AddComponent<FramePublishing>();
        }
        collisionCircleLayer = (1 << LayerMask.NameToLayer("Circle"));
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

            hits = Physics.RaycastAll(ray);
            if (Physics.Raycast(ray, out hit))
            {
                heading.SetPosition(1, hits[hits.Length - 1].point);
                positionText.text = sceneCamera.transform.InverseTransformDirection(hits[hits.Length - 1].point).ToString();
            }
            else
            {
                heading.SetPosition(1, ray.origin + ray.direction * 50f);
            }

            Physics.Raycast(ray, out circleHit, 10f, collisionCircleLayer);

        }
    }

    public RaycastHit GetCurrentCollider()
    {
        return hit;
    }

    public bool IsCollidingWithObj(GameObject collider_obj)
    {
        foreach (RaycastHit h in hits)
        {
            if (ReferenceEquals(h.collider.gameObject, collider_obj))
            {
                return true;
            }
        }
        return false;
    }

    public RaycastHit GetCircleCollider()
    {
        return circleHit;
    }
    public RaycastHit[] GetCurrentColliders()
    {
        return hits;
    }
    public Vector2 getCurrentColliderPosition(RaycastHit collider)
    {
        if (collider.transform.GetComponent<CapsuleCollider>())
        {
            collider.transform.GetComponent<CapsuleCollider>().enabled = false;
        }
        Renderer rend = collider.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = collider.collider as MeshCollider;
        Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2 pixelUV = collider.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;
        return pixelUV;
    }
}
