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
        if (grCtrl.GetCircleCollider().collider && gmCtrl.canDetectCircle) //If the collider is the target and if any changes (depending on the mode) on the target is over
        {

            if (prevTarg == null || !System.Object.ReferenceEquals(prevTarg, gmCtrl.last_target))  //If the target has changed
            {
                prevTarg = gmCtrl.last_target;
                touchOnce = false;
            }

            if (!touchOnce) //if the user's gaze has touched a target (before it disappeared)
            {
                //We increment the touch count everytime the gaze is on another target
                if (touchCount < 2)
                {
                    touchCount++;
                }
                else
                {
                    touchCount = 1;
                }

                touchOnce = true;
            }

            //We create an invisible copy of the target in its original size, in aim to calculate the dispersion of the user (the data would be biaised with the changing size of the target)
            if (createCopy) //If a copy of a target has not been created, we do it
            {
                copyTarget(gmCtrl.last_target.circle);
                createCopy = false;
            }


            if (GameObject.Find("Copy") != null) //if there is a copy of the target
            {
                if (Physics.Raycast(grCtrl.transform.position, Vector3.forward, out hitLayer, 10000, layerMask, QueryTriggerInteraction.Ignore))
                {
                    pixelUV = grCtrl.getCurrentColliderPosition(hitLayer); //We get the current position of the gaze point
                }
            }

            //Every 0.1s we add a new gaze point coordinate to a hitpoints list, with it we calculate the dispersion (how much the gaze points are (or aren't) centered
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

            if (hasCirclePulsated) //The highlight pulsation has stopped so we put the width of the outline to 0
            {
                gmCtrl.last_target.highlightWidth = 0;
                hasCirclePulsated = false;
            }

            if (!gmCtrl.calib_end && gmCtrl.chooseCircleMode == 0) //In the first mode, touching the target triggers the pulsation of the red dot in its center
            {
                gmCtrl.last_target.outlinePulse(gmCtrl.last_target.target_center_material[1], 1f, 0.1f);
            }

        }
        else
        {
            touchCount = 0;
            //We clear the lists while not touching the target anymore
            if (hitpoints != null)
            {
                hitpoints.Clear();
            }

            if (distances != null)
            {
                distances.Clear();
            }

            if (gmCtrl.last_target != null && !gmCtrl.last_target.was_looked && !gmCtrl.calib_end) //In the first mode, not touching the target triggers the pulsation of the target's outline
            {
                hasCirclePulsated = true;
                if (gmCtrl.chooseCircleMode == 0)
                {
                    gmCtrl.last_target.outlinePulse(gmCtrl.last_target.target_material[1], 0.14f, 0.01f);
                }
            }

            Destroy(copy);
            createCopy = true;
        }

        if (touchCount > 0 && touchCount < 3) //While the gaze point is moving between two target (it has touched one or two targets)
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
                totalGazePointsDistance = getTotalGazePointsDistance(gazePointsDistance); //We get the total distance made by the gaze point from one target to another
            }
        }
    }


    private void hitDispersion(Vector2 dis1, Vector2 dis2) //Add the distance between the two vectors in the distances list and the calculate the moyenne of the distances
    {
        distances.Add(Vector2.Distance(dis1, dis2));
        gmCtrl.last_target.dispersion = disMoy(distances);
    }

    private float disMoy(List<float> dis) //Calculate the moyenne of the dis argument
    {
        float sum = 0;
        foreach(float d in dis)
        {
            sum += d;
        }

        return sum / dis.Count;
    }

    private void copyTarget(GameObject target) //Copy the target
    {
        copy = GameObject.Instantiate(target, target.transform.parent);
        copy.name = "Copy";
        copy.layer = 9;
        copy.GetComponent<Renderer>().enabled = false;
    }

    private void logGazePointCoord(RaycastHit collider) 
    {
        //We add the distance between the two last gaze point in a list of distances (we take the gaze coord from a list too, in which we had the new collider position)
        if(gazePointsCoord.Count > 1)
        {
            gazePointsDistance.Add(Vector3.Distance(gazePointsCoord[gazePointsCoord.Count - 1], collider.transform.localPosition));
        }

        gazePointsCoord.Add(collider.transform.localPosition);
    }

    private float getTotalGazePointsDistance(List<float> gz) //Calculate the sum of the distances between gaze points
    {
        float sum = 0;
        foreach (float d in gz)
        {
            sum += d;
        }

        return sum;
    }

    public float getDispersion() //Getter for the dispersion
    {
        return gmCtrl.last_target.dispersion;
    }

}
