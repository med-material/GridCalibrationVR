using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu, ExecuteInEditMode]
public class fovScriptableObject : ScriptableObject
{
    public List<FOVPoints> fOVPts;

    public float previous_HMDeye_distance;

}

