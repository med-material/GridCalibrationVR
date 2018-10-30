using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketWith2DCalibration : MonoBehaviour 
{
	private Camera sceneCamera;
	private LineRenderer heading;
	private Vector3 standardViewportPoint = new Vector3 (0.5f, 0.5f, 10);
	private Vector2 gazePointLeft;
	private Vector2 gazePointRight;
	private Vector2 gazePointCenter;

	public Material shaderMaterial;

	void Start () 
	{
		PupilData.calculateMovingAverage = false;

		sceneCamera = gameObject.GetComponent<Camera> ();
		heading = gameObject.GetComponent<LineRenderer> ();
	}

	void OnEnable()
	{
		if (PupilTools.IsConnected)
		{
			PupilTools.IsGazing = true;
			PupilTools.SubscribeTo ("gaze");
		}
	}

	void Update()
	{
		Vector3 viewportPoint = standardViewportPoint;

		if (PupilTools.IsConnected && PupilTools.IsGazing)
		{
			gazePointLeft = PupilData._2D.GetEyePosition (sceneCamera, PupilData.leftEyeID);
			gazePointRight = PupilData._2D.GetEyePosition (sceneCamera, PupilData.rightEyeID);
			gazePointCenter = PupilData._2D.GazePosition;
			viewportPoint = new Vector3 (gazePointCenter.x, gazePointCenter.y, 1f);
		}

		if (Input.GetKeyUp (KeyCode.L))
			heading.enabled = !heading.enabled;
		if (heading.enabled)
		{
			heading.SetPosition (0, sceneCamera.transform.position-sceneCamera.transform.up);

			Ray ray = sceneCamera.ViewportPointToRay (viewportPoint);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit))
			{
				heading.SetPosition (1, hit.point);
			} else
			{
				heading.SetPosition (1, ray.origin + ray.direction * 50f);
			}
		}
	}
}
