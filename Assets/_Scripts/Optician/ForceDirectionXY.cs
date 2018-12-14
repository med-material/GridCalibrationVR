using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ForceDirectionXY : MonoBehaviour
{

    private RectTransform rt;

    [Tooltip("This will force this particular element to always have the specified absolute X,Y rotation. Use 0 for a straight upwards facing.")]
    public float forcedXYRotation = 0f;

    private Vector3 rot = Vector3.zero;

    void Awake()
    {

        rot.x = forcedXYRotation;
        rot.y = forcedXYRotation;
        rt = GetComponent<RectTransform>();


    }

    // Use this for initialization
    void Start()
    {


    }

    // Update is called once per frame
    void LateUpdate()
    {
        rot.x = forcedXYRotation;
        rot.y = forcedXYRotation;

        if (rt.eulerAngles != rot)
            rt.eulerAngles = rot;
    }
}
