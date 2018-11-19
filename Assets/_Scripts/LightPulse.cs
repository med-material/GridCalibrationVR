using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [RequireComponent(typeof(LineRenderer))]
public class LightPulse : MonoBehaviour {

    private int vertexCount = 40;
    private float lineWidth = 0.2f;
    private float radius = 4f;
    private bool circleFillScreen;

    private LineRenderer lineRenderer;

	void Awake () {
        lineRenderer = GetComponent<LineRenderer>();
	}

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        float deltaTheta = (2f * Mathf.PI) / 40f;
        float theta = 0f;

        Vector3 oldPos = Vector3.zero;

        for(int i = 0; i < vertexCount +1; i++)
        {
            Vector3 pos = new Vector3(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta), 0f);
            Gizmos.DrawLine(oldPos, transform.position + pos);
            oldPos = transform.position + pos;

            theta += deltaTheta;
        }
    }

#endif
}
