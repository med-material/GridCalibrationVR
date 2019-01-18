using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to print Acuity Max Field LineRenderer in any scene.
/// </summary>
public class acuityMaxFieldPrint : MonoBehaviour
{

    [SerializeField]
    private fovScriptableObject FOVPointsSave;
    private LineRenderer lr;
    public bool acuity;

    void Start()
    {
        // Get the good Point in the scriptable object and draw the lineRenderer
        FOVPoints pt = FOVPointsSave.fOVPts.FirstOrDefault(p => p.id == Convert.ToInt32(FOVPointsSave.previous_HMDeye_distance));
        if (pt != null)
        {
            List<Vector3> lst;
            Color color;
            if (acuity)
            {
                lst = pt.acuityFieldPoints;
                color = Color.red;
            }
            else
            {
                lst = pt.points;
                color = Color.blue;
            }
            // Setup the LineRenderer with color, width and hide it from the user bot not the operator
            lr = GetComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.02f;
            lr.gameObject.layer = 11; // 11 = OperatorUI

            DrawFOV(lr, lst, color);

        }
        else
            print("No points to print for this HMD eye distance. Please verify the OpticianCalibration Scene and add points for the distance" + FOVPointsSave.previous_HMDeye_distance / 2 + ".");
    }
    /// <summary>
    /// Function to render a lineRenderer with positions and color
    /// </summary>
    /// <param name="lrender">LineRenderer</param>
    /// <param name="pos_list">List of Vector3 positions</param>
    /// <param name="color">Color for the entire lineRenderer</param>
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
    }
}
