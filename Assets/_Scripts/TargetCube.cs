using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCube
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
    public GameObject cube;
    public bool was_looked;
    public bool calibration_max = false;
    private int calib_failed;
    public bool cube_created;
    public TargetCube(float x_min, float x_max, float y_min, float y_max)
    {
        default_x_min = x_min;
        default_x_max = x_max;
        default_y_min = y_min;
        default_y_max = y_max;
        previous_scale = new Vector3(0.3f, 0.3f, 0.1f);
        calib_failed = 0;
    }
    public void CreateTarget(GameObject wall, bool centered)
    {
        // Create the Cube 
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube_created = true;
        // Set the cube as child of the wall
        cube.transform.parent = wall.transform;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = previous_scale;
        if (was_looked)
        {
            if (cube.transform.localScale.x > 0.005f || cube.transform.localScale.y > 0.005f)
            {
                cube.transform.localScale /= 1.6f;
            }
            else
            {
                calib_failed = 3;
            }
        }
        else
        {
            cube.transform.localScale = previous_scale;
            calib_failed++;
        }
        CalculateOffset();
        if (centered)
        {
            // Place the cube at the center of the cell, for the end process
            cube.transform.localPosition = new Vector3((x_max + x_min) / 2, (y_max + y_min) / 2, -0.5f);
        }
        else
        {
            cube.transform.localPosition = new Vector3(Random.Range(x_min, x_max), Random.Range(y_min, y_max), -0.5f);
        }

        previous_scale = cube.transform.localScale;
        was_looked = false;

        if (calib_failed > 2)
        {
            calibration_max = true;
        }
    }
    private void CalculateOffset()
    {
        x_min = default_x_min + cube.transform.localScale.x / 2;
        x_max = default_x_max - cube.transform.localScale.x / 2;
        y_min = default_y_min + cube.transform.localScale.y / 2;
        y_max = default_y_max - cube.transform.localScale.y / 2;
    }
    public void DestroyTarget()
    {
        cube_created = false;
        Object.Destroy(cube);
    }
}
