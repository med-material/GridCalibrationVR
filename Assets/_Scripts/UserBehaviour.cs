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

    private List<Vector3> gazePointsCoord;
    private List<float> gazePointsDistance;
    private Vector3 currentGazePointCoord;
    public float totalGazePointsDistance;

    private List<float> distances;
    private List<float> gazePoint;
    private bool control = false;
    private bool createCopy = true;
    private bool hasCirclePulsated = false;
    private int layerMask;
    private RaycastHit hitLayer;
    public int touchCount = 0;

    private float timer = 0;

    private bool touchOnce = false;
    private TargetCirle prevTarg = null;

    // Use this for initialization
    void Start () {
        hitpoints = new List<Vector2>();
        distances = new List<float>();
        gazePointsCoord = new List<Vector3>();
        gazePointsDistance = new List<float>();
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

                if(prevTarg == null || !System.Object.ReferenceEquals(prevTarg, gmCtrl.last_target))
                {
                    prevTarg = gmCtrl.last_target;
                    touchOnce = false;
                }

                if (!touchOnce)
                {

                    if (touchCount < 2)
                    {
                        touchCount++;
                    }
                    else
                    {
                        touchCount = 1;
                    }

                    print("Je touche : " + touchCount);
                    touchOnce = true;
                }


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

                if (hasCirclePulsated)
                {
                    gmCtrl.last_target.highlightWidth = 0;
                    hasCirclePulsated = false;
                }

                if (!gmCtrl.calib_end)
                {
                    gmCtrl.last_target.outlinePulse(gmCtrl.last_target.target_center_material[1], 1, 0.1f);
                }

            }
            else
            {
                touchCount = 0;

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
                    hasCirclePulsated = true;
                    gmCtrl.last_target.outlinePulse(gmCtrl.last_target.target_material[1], 0.14f, 0.01f);
                }

                Destroy(copy);
                createCopy = true;
            }

            if(touchCount > 0 && touchCount < 3)
            {
                if (Physics.Raycast(grCtrl.transform.position, Vector3.forward, out hitLayer, 10000, layerMask, QueryTriggerInteraction.Ignore))
                {
                    logGazePointCoord(hitLayer);
                }
            }
            else
            {
                if (gazePointsDistance.Count < 3)
                {
                    totalGazePointsDistance = getTotalGazePointsDistance(gazePointsDistance);
                }
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

    private void logGazePointCoord(RaycastHit collider)
    {
        if(gazePointsCoord.Count > 1)
        {
            gazePointsDistance.Add(Vector3.Distance(gazePointsCoord[gazePointsCoord.Count - 1], collider.transform.localPosition));
        }

        gazePointsCoord.Add(collider.transform.localPosition);
    }

    private float getTotalGazePointsDistance(List<float> gz)
    {
        float sum = 0;
        foreach (float d in gz)
        {
            sum += d;
        }

        return sum;
    }

    public float getDispersion()
    {
        return gmCtrl.last_target.dispersion;
    }

}
