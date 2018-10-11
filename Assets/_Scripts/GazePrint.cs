using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazePrint : MonoBehaviour {

	void OnEnable()
	{
		if (PupilTools.IsConnected)
		{
			PupilGazeTracker.Instance.StartVisualizingGaze ();		
			print ("We are gazing");
		}
	}
}
