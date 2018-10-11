using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public GridController gridController;

	private RaycastHit current_target;

	void Start() {
		// gridController = gameObject.GetComponent<GridController>();
	}

	// Update is called once per frame
	void Update () {
		current_target = gridController.GetCurrentCollider();
		print(current_target.collider.tag);

		// YO
	}
}
