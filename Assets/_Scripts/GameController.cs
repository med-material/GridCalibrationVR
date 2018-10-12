using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    private float CENTER_X_L;
    private float CENTER_Y_C;
    private float CENTER_X_R;
    private float CENTER_Y_T;
    public GridController gridController;
    public GameObject wall;
    public List<TargetCube> targets;
    private float wall_width = 1;
    private float wall_height = 1;
    private static System.Random rand = new System.Random();
    private int last_index;
    private int target_index;
    private float timeLeft = 1.0f;
    private TargetCube last_target;
    private RaycastHit current_target;

    void Start()
    {
        //target_script = GetComponent<TargetCube>();

        // Create const value for x, y min and max
        CENTER_X_L = wall_width / 3;
        CENTER_Y_C = wall_height / 3;
        CENTER_X_R = wall_width - wall_width / 3;
        CENTER_Y_T = wall_height - wall_height / 3;

        // Create the targets
        targets = new List<TargetCube>();
        targets.Add(new TargetCube(0, CENTER_X_L, 0, CENTER_Y_C));
        targets.Add(new TargetCube(CENTER_X_L, CENTER_X_R, 0, CENTER_Y_C));
        targets.Add(new TargetCube(CENTER_X_R, wall_width, 0, CENTER_Y_C));
        targets.Add(new TargetCube(0, CENTER_X_L, CENTER_Y_C, CENTER_Y_T));
        targets.Add(new TargetCube(CENTER_X_L, CENTER_X_R, CENTER_Y_C, CENTER_Y_T));
        targets.Add(new TargetCube(CENTER_X_R, wall_width, CENTER_Y_C, CENTER_Y_T));
        targets.Add(new TargetCube(0, CENTER_X_L, CENTER_Y_T, wall_height));
        targets.Add(new TargetCube(CENTER_X_L, CENTER_X_R, CENTER_Y_T, wall_height));
        targets.Add(new TargetCube(CENTER_X_R, wall_width, CENTER_Y_T, wall_height));

        // gridController = gameObject.GetComponent<GridController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get the current object looked at
        current_target = gridController.GetCurrentCollider();
        //print(current_target.collider.tag);

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            // Reset timer 
            timeLeft = 1.0f;
            // Destroy last target
            if (last_target != null)
            {
                last_target.DestroyTarget();
            }
            // Get a random target and spawn it
            TargetCube trgt = selectItem();
             if (current_target.collider.name == "Cube")
            {
                last_target.was_looked = true;
            }
            trgt.CreateTarget(wall);
            last_target = trgt;


        }
    }

    public TargetCube selectItem()
    {
        // Get a random target in the list
        do
        {
            target_index = rand.Next(targets.Count);
        } while (last_index == target_index);
        last_index = target_index;
        TargetCube target = targets[target_index];
        return target;
    }

}
