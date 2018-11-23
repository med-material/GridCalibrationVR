using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserBehaviour : MonoBehaviour{

    public GridController grCtrl;

    private GameController gmCtrl;
    private GameObject copy;
    private Vector2 pixelUV;
    private Vector2 prevPixelUV;
    private List<Vector2> hitpoints;
    private List<float> distances;
    private bool control = false;
    private bool createCopy = true;
    private int layerMask;
    private RaycastHit hitLayer;

    private float timer = 0;

	// Use this for initialization
	void Start () {
        hitpoints = new List<Vector2>();
        distances = new List<float>();
        gmCtrl = GetComponent<GameController>();
        layerMask = LayerMask.GetMask("Copy");
    }

    // Update is called once per frame
    void Update()
    {
        if (grCtrl.GetCurrentCollider().collider)
        {
            if (grCtrl.GetCurrentCollider().collider.name == "Cylinder")
            {

                if (createCopy)
                {
                    copyTarget(gmCtrl.last_target.circle);
                    createCopy = false;
                }

          
                if(GameObject.Find("Copy") != null)
                {
                    if (Physics.Raycast(grCtrl.transform.position, Vector3.forward, out hitLayer, 10000, layerMask, QueryTriggerInteraction.Ignore))
                    {
                        pixelUV = grCtrl.getCurrentColliderPosition(hitLayer);
                    }
                }

                timer += Time.deltaTime;
                if (timer > 0.1f)
                {
                    hitpoints.Add(pixelUV);
                    if (hitpoints.Count >= 2)
                    {
                        if (hitpoints[hitpoints.Count - 2] != hitpoints[hitpoints.Count - 1])
                        {
                            hitDispersion(hitpoints[hitpoints.Count - 2], hitpoints[hitpoints.Count - 1]);
                        }

                    }
                    timer = 0;
                }

            }
            else
            {
                if (hitpoints != null)
                {
                    hitpoints.Clear();
                }

                if (distances != null)
                {
                    distances.Clear();
                }

                if(gmCtrl.last_target != null && !gmCtrl.last_target.was_looked && !gmCtrl.calib_end)
                {
                    gmCtrl.last_target.outlinePulse();
                }

                Destroy(copy);
                createCopy = true;
            }

        }
    }


    private void hitDispersion(Vector2 dis1, Vector2 dis2)
    {
        distances.Add(Vector2.Distance(dis1, dis2));
        gmCtrl.last_target.dispersion = disMoy(distances);
    }

    private float disMoy(List<float> dis)
    {
        float sum = 0;
        foreach(float d in dis)
        {
            sum += d;
        }

        return sum / dis.Count;
    }

    private void copyTarget(GameObject target)
    {
        copy = GameObject.Instantiate(target, target.transform.parent);
        copy.name = "Copy";
        copy.layer = 9;
        copy.GetComponent<Renderer>().enabled = false;
    }

    public float getDispersion()
    {
        return gmCtrl.last_target.dispersion;
    }
}
