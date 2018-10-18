using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private float CENTER_X_L;
    private float CENTER_Y_C;
    private float CENTER_X_R;
    private float CENTER_Y_T;
    public GridController gridController;
    public GameObject wall;
    public List<TargetCirle> targets;
    private float wall_width = 1;
    private float wall_height = 1;
    private static System.Random rand = new System.Random();
    private int last_index;
    private int target_index;
    private float timeLeft;
    private TargetCirle last_target;
    private RaycastHit looking_at_circle;
    private RaycastHit looking_at_circle_before;
    private float target_timer;
    private bool calib_end = false;
    private bool only_one = true;
    public bool is_started = false;
    private Color success_color = new Color(0.07f, 0.8f, 0.07f, 1);
    public float choosenTime;

    void Start()
    {
        // Set the timers's default time
        ResetTargetTimer();
        ResetTimer();

        // Create const value for x, y min and max
        CENTER_X_L = wall_width / 3;
        CENTER_Y_C = wall_height / 3;
        CENTER_X_R = wall_width - wall_width / 3;
        CENTER_Y_T = wall_height - wall_height / 3;

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

    // Update is called once per frame
    void Update()
    {
        if (is_started)
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
                    }
                    // Get a random target and spawn it
                    TargetCirle trgt = selectItem();
                    if(trgt != null) {
                         trgt.CreateTarget(wall, false);
                         last_target = trgt;
                    }
                   
                }
                // Process the target looked at 
                if (looking_at_circle.collider)
                {
                    if (looking_at_circle.collider.name == "Cube")
                    {
                        if (looking_at_circle_before.collider)
                        {
                            if (System.Object.ReferenceEquals(looking_at_circle.collider, looking_at_circle_before.collider))
                            {
                                // If the target looked at is the same as before start time
                                target_timer -= Time.deltaTime;
                                if (target_timer < 10)
                                {
                                    looking_at_circle.collider.GetComponent<Renderer>().material.color = success_color;
                                }
                            }
                            else
                            {
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
                    print("The target was looked for 300ms");
                    timeLeft = -1.0f;
                    last_target.was_looked = true;
                    ResetTargetTimer();
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
            return null;
        }
        // Get a random target in the list
        // exclude the previous target and target with calibration ended
        TargetCirle target;
        if (targets.Where(t => !t.calibration_max).ToList().Count <= 1)
        {
            print("Only one remaining");
            target = targets.Find(t => !t.calibration_max);
        }
        else
        {
            List<TargetCirle> lst_trgt = targets.Where(t=>!t.calibration_max).ToList();
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

}
