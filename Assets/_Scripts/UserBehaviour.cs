using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserBehaviour : MonoBehaviour{

    public GridController ctrl;

    private Vector2 pixelUV;
    private Vector2 prevPixelUV;
    private List<Vector2> hitpoints;
    private bool control;
    private float dispersion;

    private float timer = 0;


	// Use this for initialization
	void Start () {
        hitpoints = new List<Vector2>();
	}
	
	// Update is called once per frame
	void Update () {
        print(" Distance : " + dispersion);

        if (ctrl.GetCurrentCollider().collider)
        {
            if (ctrl.GetCurrentCollider().collider.name == "Cylinder")
            {
                pixelUV = ctrl.getCurrentColliderPosition(ctrl.GetCurrentCollider());

                if(prevPixelUV != null && pixelUV != prevPixelUV)
                {
                    control = true;
                    timer = 0;
                    prevPixelUV = pixelUV;
                }
                else
                {
                    prevPixelUV = pixelUV;
                }

                timer += Time.deltaTime;
                
                if(timer > 0.20f && control)
                {
                    hitpoints.Add(pixelUV);
                    if (hitpoints.Count == 2)
                    {
                        hitDispersion(hitpoints[0], hitpoints[1]);
                        Vector2 newPoint = hitpoints[1];
                        hitpoints.Clear();
                        hitpoints.Add(newPoint);
                    }
                    control = false;
                }
                else
                {
                    dispersion = 0;
                }
               
            }
            else
            {
                if(hitpoints != null)
                {
                    hitpoints.Clear();
                }
            }
        }
		
	}


    private void hitDispersion(Vector2 dis1, Vector2 dis2)
    {
        dispersion = Vector2.Distance(dis1, dis2);
    }

    public float getDispersion()
    {
        return dispersion;
    }
}
