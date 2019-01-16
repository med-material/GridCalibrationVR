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
    [Range(0.4f, 2f)]
    public float SPEED_OF_CIRCLE = 0.4f;
    public Camera dedicatedCapture;

    # endregion

    # region private_value

    private ResetHandler rst;
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

    //private bool chooseCircleMode = false;
    public int chooseCircleMode;
    private bool wait = true;
    private Vector3 savedScale, savedPosition;

    public bool canDetectCircle = false;

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
        rst = GameObject.Find("SceneController").GetComponent<ResetHandler>();
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
            {
                chooseCircleMode = 2;
                StartShrinkMode();
            }              
            else if (choosenMode == "shrink")
            {
                chooseCircleMode = 1;
                StartShrinkMode();
            }
            else if (choosenMode == "normal")
            {
                chooseCircleMode = 0;
                StartShrinkMode();
            }
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
                t.CreateTarget(wall, true, chooseCircleMode, true);
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
            if (timeLeft < 0 || wait)
            {
                wait = false;
                if (last_target != null)
                {
                    if(last_target.circle.transform.localScale == last_target.scale_to_reach) //While changing the targets, if the old one's size hasn't changed, it means the user failed to stare at it, we increments a fail count
                    {
                        last_target.calib_failed++;
                    }
                    if (chooseCircleMode == 2) //We save the original scale and position for the mode
                    {
                        savedScale = last_target.circle.transform.localScale; 
                        savedPosition = last_target.circle.transform.localPosition;
                    }
                    last_target.DestroyTarget();
                }
                // If the target is not created, create it
                if (!trgt.circle_created)
                {
                    if (chooseCircleMode == 2) //If it is the declining mode
                    {
                        if (savedScale != Vector3.zero) //If we have a saved scale
                        {
                            trgt.CreateTarget(wall, true, chooseCircleMode, false, savedScale);
                            trgt.SPEED_OF_CIRCLE = SPEED_OF_CIRCLE; 
                        }
                        else
                        {
                            trgt.CreateTarget(wall, true, chooseCircleMode, false);
                        }
                    }
                    else
                    {
                        trgt.CreateTarget(wall, true, chooseCircleMode, false);
                    }

                    if (last_target != null && canDetectCircle) //We get the distraction when the user is starring at the target
                    {
                        prevPos = last_target.circle.transform.localPosition;

                        distraction = getDistraction(userbhv.totalGazePointsDistance, Vector3.Distance(prevPos, trgt.circle.transform.localPosition));
                    }
                    travel_time = 0.0f;
                    target_timer = 0.0f;
                }
                last_target = trgt;
                ResetTimer();
            }

            // Launch the circle mode selected
            if (last_target.circle_created)
            {
                if (chooseCircleMode == 1)
                {
                    canDetectCircle = last_target.bigCircleMode();
                }
                else
                {
                    if(savedScale != Vector3.zero)
                    {
                        canDetectCircle = last_target.movingCircleMode(savedScale, savedPosition);
                        if(canDetectCircle == true)
                        {
                            savedScale = Vector3.zero;
                            savedPosition = Vector3.zero;
                        }
                    }
                    else
                    {
                        canDetectCircle = true;
                    }
                }
            }

            target_timer += Time.deltaTime;
            // Get the current object looked at by the user
            looking_at_circle = gridController.GetCurrentCollider();
            lookings = gridController.GetCurrentColliders();

            for (int i = 0; i < lookings.Length; i++)
            {
                RaycastHit hit = lookings[i];
                if (hit.collider)
                {
                    if (hit.collider.name == "Cylinder" && canDetectCircle)
                    {
                        looking_at_circle = hit;
                    }
                }
            }
            // If the user is looking the target, reduce its scale 
            if (looking_at_circle.collider)
            {

                if (canDetectCircle) //if any changes (depending on the mode) on the target is over
                {
                    if (looking_at_circle.collider.name == "Cylinder") //if it's on the target
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

            } while (System.Object.ReferenceEquals(last_target, lst_trgt[target_index]));
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

    public float getDistanceBetweenCircle(Vector3 pos1, Vector3 pos2) //Distance btw two target's positions
    {
        return Vector3.Distance(pos1, pos2);
    }

    public float getDistraction(float dis1, float dis2) //Difference btw two distances (usually the shortest distance btw 2 targets and the distance travelled by the user 's gaze)
    {
        return dis1 - dis2;
    }

}
