using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using UnityEngine;

public class GameController : MonoBehaviour
{
    # region public_value
    public GridController gridController;
    public Heatmap heatmap;
    public GameObject wall;
    public List<TargetCirle> targets;
    public TargetCirle last_target = null;
    public bool is_started = false;
    public float choosenTime;
    public float travel_time = -1.0f;
    public string choosenMode = "";
    public string test_name;
    public bool calib_end = false;

    # endregion

    # region private_value
    private Camera dedicatedCapture;
    private Vector3 gazeToWorld;
    private float CENTER_X_L;
    private float CENTER_Y_C;
    private float CENTER_X_R;
    private float CENTER_Y_T;
    private float wall_width = 1;
    private float wall_height = 1;
    private static System.Random rand = new System.Random();
    private int last_index;
    private int target_index;
    private float timeLeft;
    private RaycastHit looking_at_circle;
    private RaycastHit[] lookings;
    private RaycastHit looking_at_circle_before;
    private float target_timer;
    private float heat_timer;
    private bool only_one = true;
    private Color success_color = new Color(0.07f, 0.8f, 0.07f, 1);
    private Vector2 pixelUV;

    private Vector3 prevPos;

    private float distraction;

    #endregion

    # region log value
    public PupilDataGetter pupilDataGetter;
    private LoggerBehavior logger;
    private UserBehaviour userbhv;
    #endregion

    void Start()
    {
        // Set the timers's default time
        ResetTargetTimer();
        ResetTimer();

        dedicatedCapture = Camera.main;
        logger = GetComponent<LoggerBehavior>();
        userbhv = GetComponent<UserBehaviour>();
        heatmap.SetStartStop(false);

        CreateCalculValue();
        CreateTargets();
    }

    void OnEnable()
    {
        pupilDataGetter = new PupilDataGetter();
        pupilDataGetter.startSubscribe(new List<string> { "gaze", "pupil.", "fixation" });
    }

    void onDisable()
    {
        pupilDataGetter.stopSubscribe();
    }
    void Update()
    {
        if (is_started)
        {
            if (choosenMode == "approx")
                StartApproxMode();
            else if (choosenMode == "shrink")
                StartShrinkMode();
            else print("No mode is selected. Please verify you have selected a mode");
        }
    }

    private void CreateTargets()
    {
        // Create the targets
        targets = new List<TargetCirle>();
        targets.Add(new TargetCirle(0, CENTER_X_L, 0, CENTER_Y_C));
        targets.Add(new TargetCirle(CENTER_X_L, CENTER_X_R, 0, CENTER_Y_C));
        targets.Add(new TargetCirle(CENTER_X_R, wall_width, 0, CENTER_Y_C));
        targets.Add(new TargetCirle(0, CENTER_X_L, CENTER_Y_C, CENTER_Y_T));
        targets.Add(new TargetCirle(CENTER_X_L, CENTER_X_R, CENTER_Y_C, CENTER_Y_T));
        targets.Add(new TargetCirle(CENTER_X_R, wall_width, CENTER_Y_C, CENTER_Y_T));
        targets.Add(new TargetCirle(0, CENTER_X_L, CENTER_Y_T, wall_height));
        targets.Add(new TargetCirle(CENTER_X_L, CENTER_X_R, CENTER_Y_T, wall_height));
        targets.Add(new TargetCirle(CENTER_X_R, wall_width, CENTER_Y_T, wall_height));
    }

    private void CreateCalculValue()
    {
        // Create const value for x, y min and max
        CENTER_X_L = wall_width / 3;
        CENTER_Y_C = wall_height / 3;
        CENTER_X_R = wall_width - wall_width / 3;
        CENTER_Y_T = wall_height - wall_height / 3;
    }
    
    private void StartApproxMode()
    {
        // Check if calibration is ended, delete current target, create each target in centered position
        if (calib_end && only_one)
        {
            print("Calibration test end.");
            targets.ForEach(t =>
            {
                if (t.circle_created)
                {
                    t.DestroyTarget();
                }
                t.CreateTarget(wall, true);
            });
            only_one = false;
        }
        else if (only_one)
        {
            // Get the current object looked at
            looking_at_circle = gridController.GetCurrentCollider();
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                ResetTimer();
                // Destroy last target
                if (last_target != null)
                {
                    last_target.DestroyTarget();
                    travel_time = -1.0f;
                }
                // Get a random target and spawn it
                TargetCirle trgt = selectItem();
                if (trgt != null)
                {
                    trgt.CreateTarget(wall, false);
                    //print(trgt.circle.transform.localScale.ToString("F5"));
                    last_target = trgt;
                }
            }

            // Process the target looked at 
            if (looking_at_circle.collider)
            {
                if (looking_at_circle.collider.name == "Cylinder")
                {
                    if (looking_at_circle_before.collider)
                    {
                        if (System.Object.ReferenceEquals(looking_at_circle.collider, looking_at_circle_before.collider))
                        {
                            if (travel_time < 0)
                            {
                                travel_time = timeLeft;
                            }
                            // If the target looked at is the same as before, start time
                            target_timer -= Time.deltaTime;
                            if (target_timer < 10)
                            {
                                looking_at_circle.collider.GetComponent<Renderer>().material.color = success_color;
                            }
                        }
                        else
                        {
                            looking_at_circle.collider.GetComponent<Renderer>().material.color = Color.white;
                            ResetTargetTimer();
                        }
                    }
                    looking_at_circle_before = looking_at_circle;
                }
                else
                {
                    ResetTargetTimer();
                }
            }
            // If the target has been fixed
            if (target_timer < 0)
            {
                //print("The target was looked for 300ms");
                timeLeft = -1.0f;
                last_target.was_looked = true;
                ResetTargetTimer();
            }
        }
    }
    private void StartShrinkMode()
    {
        if (calib_end && only_one)
        {
            print("Calibration test end.");
            targets.ForEach(t =>
            {
                if (t.circle_created)
                {
                    t.DestroyTarget();
                }
                t.CreateTarget(wall, true);
            });
            //heatMap.setActive(true);
            only_one = false;
        }
        else if (only_one)
        {
            heatmap.SetStartStop(true);
            // Select a random target
            TargetCirle trgt = selectItem();
            // If no more time left and the target has not been looked.
            if (timeLeft < 0)
            {
                if (last_target != null)
                {
                    last_target.DestroyTarget();
                }
                // If the target is not created, create it
                if (!trgt.circle_created)
                {
                    trgt.CreateTarget(wall, true);
                    if(last_target != null)
                    {
                        prevPos = last_target.circle.transform.localPosition;

                            distraction = getDistraction(userbhv.totalGazePointsDistance, Vector3.Distance(prevPos, trgt.circle.transform.localPosition));
                            print("DISTRACTION :" + distraction);
                        
                        print("Premier cercle : "+ last_target.circle.transform.localPosition + " Nouveau cercle "+ trgt.circle.transform.localPosition + " Distance avec le cercle précèdent " + Vector3.Distance(prevPos, trgt.circle.transform.localPosition));
                    }
                    travel_time = 0.0f;
                    target_timer = 0.0f;
                }
                last_target = trgt;
                ResetTimer();  
            }
            target_timer += Time.deltaTime;
            // Get the current object looked at by the user
            looking_at_circle = gridController.GetCurrentCollider();
            lookings = gridController.GetCurrentColliders();

            for(int i = 0; i < lookings.Length; i++){
                RaycastHit hit = lookings[i];
                if(hit.collider.name == "Cylinder"){
                    looking_at_circle = hit;
                }
            }
            // If the user is looking the target, reduce its scale 
            if (looking_at_circle.collider)
            {
                //Vector3 posCircleHeatMap = new Vector3(looking_at_circle.transform.position.x, looking_at_circle.transform.position.y, looking_at_circle.transform.position.z - 0.5f);
                //Vector3 posCircleHeatMap = looking_at_circle.transform.position;
                if (looking_at_circle.collider.name == "Cylinder")
                {
                    LogData();
                    if (travel_time <= 0.0f)
                    {
                        travel_time = target_timer;
                    }
                    last_target.was_looked = true;

                    //We get the pixels location of the collider, depending of the distance from the center of the circle, we reduce or increase the shrinking speed.
                    //pixelUV = gridController.getCurrentColliderPosition(looking_at_circle);
                    //last_target.reduceSpeed(Vector2.Distance(new Vector2(2,2), pixelUV), 0.048f, 0);

                    //Depending of the dispersion, we reduce or increase the shrinking speed.
                    last_target.reduceSpeed(userbhv.getDispersion(), 0.038f, 1);
                    last_target.ReduceScale();  
                    looking_at_circle_before = looking_at_circle;
                }
                else
                {
                    timeLeft -= Time.deltaTime;
                }
                if (last_target != null)
                {
                    if (last_target.was_looked && looking_at_circle.collider.name != "Cylinder")
                    {
                        last_target.calibration_max = true;
                    }
                }
            }
        }
    }
    private TargetCirle selectItem()
    {
        // If all targets are minimum size, calibration is endend
        if (targets.All(trg => trg.calibration_max))
        {
            calib_end = true;
            heatmap.SetStartStop(false);
            return null;
        }
        // Get a random target in the list
        // exclude the previous target and target with calibration ended
        TargetCirle target;
        if (targets.Where(t => !t.calibration_max).ToList().Count <= 1)
        {
            target = targets.Find(t => !t.calibration_max);
        }
        else
        {
            List<TargetCirle> lst_trgt = targets.Where(t => !t.calibration_max).ToList();
            do
            {
                target_index = rand.Next(lst_trgt.Count);
            } while (last_index == target_index);
            last_index = target_index;
            target = lst_trgt[target_index];
        }
        return target;
    }
    private void ResetTargetTimer()
    {
        target_timer = 0.3f;
    }
    private void ResetTimer()
    {
        timeLeft = choosenTime;
    }
    private void LogData()
    {
        List<object> lst = new List<object>();
        lst.Add(pupilDataGetter.left_confidence); // confidence value on real time 
        lst.Add(pupilDataGetter.right_confidence);
        lst.Add(travel_time);
        lst.Add(last_target != null ? last_target.circle.transform.position.x : double.NaN);
        lst.Add(last_target != null ? last_target.circle.transform.position.y : double.NaN);
        lst.Add(CalculateCircleRadiusPercent());
        lst.Add(pupilDataGetter.fix_duration);
        lst.Add(pupilDataGetter.fix_dispersion);
        lst.Add(pupilDataGetter.fix_confidence);

        logger.AddObjToLog();
    }

    public float CalculateCircleRadiusPercent()
    {
        float current_scale = looking_at_circle.collider.gameObject.transform.localScale.x;
        float first_scale = last_target.previous_scales[0].x;
        return 100 - (((first_scale - current_scale) / first_scale) * 100);
    }

    public float getDistanceBetweenCircle(Vector3 pos1, Vector3 pos2)
    {
        return Vector3.Distance(pos1, pos2);
    }

    public float getDistraction(float dis1, float dis2)
    {
        return dis1 - dis2;
    }

}
