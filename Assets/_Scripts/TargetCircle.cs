using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCirle
{
    private Vector3 previous_scale;
    private float default_x_max;
    private float default_y_max;
    private float default_x_min;
    private float default_y_min;
    private float x_min;
    private float y_min;
    private float x_max;
    private float y_max;
    public GameObject circle;
    public bool was_looked;
    public bool calibration_max = false;
    private int calib_failed;
    public bool circle_created;
    public TargetCirle(float x_min, float x_max, float y_min, float y_max)
    {
        default_x_min = x_min;
        default_x_max = x_max;
        default_y_min = y_min;
        default_y_max = y_max;
        previous_scale = new Vector3(0.25f, 0.01f, 0.3f);
        calib_failed = 0;
    }
    public void CreateTarget(GameObject wall, bool centered)
    {
        // Create the Circle 
        circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle_created = true;
        // Set the Circle as child of the wall
        circle.transform.parent = wall.transform;
        circle.transform.localRotation = Quaternion.Euler(90,0,0);
        circle.transform.localScale = previous_scale;
        if (was_looked)
        {
            if (circle.transform.localScale.x > 0.005f || circle.transform.localScale.y > 0.005f)
            {
                circle.transform.localScale /= 1.6f;
            }
            else
            {
                calib_failed = 3;
            }
        }
        else
        {
            circle.transform.localScale = previous_scale;
            calib_failed++;
        }
        CalculateOffset();
        if (centered)
        {
            // Place the circle at the center of the cell, for the end process
            circle.transform.localPosition = new Vector3((x_max + x_min) / 2, (y_max + y_min) / 2, -0.5f);
        }
        else
        {
            circle.transform.localPosition = new Vector3(Random.Range(x_min, x_max), Random.Range(y_min, y_max), -0.5f);
        }

        previous_scale = circle.transform.localScale;
        was_looked = false;

        if (calib_failed > 2)
        {
            calibration_max = true;
        }
    }
    private void CalculateOffset()
    {
        x_min = default_x_min + circle.transform.localScale.x / 2;
        x_max = default_x_max - circle.transform.localScale.x / 2;
        y_min = default_y_min + circle.transform.localScale.z / 2;
        y_max = default_y_max - circle.transform.localScale.z / 2;
    }
    public void DestroyTarget()
    {
        circle_created = false;
        Object.Destroy(circle);
    }
}
