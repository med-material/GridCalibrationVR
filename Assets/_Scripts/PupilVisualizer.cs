using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PupilVisualizer
{
    private PupilDataGetter pupilDataGetter;
    private Vector2 norm_pos_left;
    private Vector3 pupil_center_left;
    private Vector2 norm_pos_right;
    private Vector3 pupil_center_right;
    private float diameter_left;
    private float rotation_left;
    private float diameter_right;
    private float rotation_right;
    private GameObject left_pupil;
    private GameObject right_pupil;

    public GameObject left_container;
    public GameObject right_container;

    public PupilVisualizer()
    {
        pupilDataGetter = new PupilDataGetter();
        pupilDataGetter.startSubscribe(new List<string> { "pupil." });
        UpdateData();

        left_pupil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        right_pupil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        left_container = GameObject.Find("LeftEyePupilContainer");
        right_container = GameObject.Find("RightEyePupilContainer");

        SetPupil(left_pupil, norm_pos_left, diameter_left, left_container.transform);
        SetPupil(right_pupil, norm_pos_right, diameter_right, right_container.transform);
    }

    private void SetPupil(GameObject pupil, Vector3 norm_pos, float diameter, Transform container)
    {
        pupil.transform.parent = container;
        SetColorPupil(pupil, norm_pos);
        pupil.transform.localPosition = new Vector3(norm_pos.x * 10, 0, norm_pos.y * 10);
        pupil.transform.localRotation = Quaternion.Euler(0, 0, 0);
        pupil.transform.localScale = new Vector3(1.0f, 0.01f, 1.0f);
    }

    private void SetColorPupil(GameObject pupil, Vector3 norm_pos)
    {
        if (norm_pos.x > 0.4f && norm_pos.x < 0.6f && norm_pos.y > 0.4f && norm_pos.y < 0.6f)
            pupil.GetComponent<Renderer>().material.color = Color.green;
        else
            pupil.GetComponent<Renderer>().material.color = Color.red;
    }

    public void UpdatePupilsData()
    {
        UpdateData();
        UpdatePupilData(left_pupil, norm_pos_left, diameter_left);
        UpdatePupilData(right_pupil, norm_pos_right, diameter_right);
    }

    public void UpdateData()
    {
        norm_pos_left = pupilDataGetter.norm_pos_left;
        pupil_center_left = pupilDataGetter.pupil_center_left;
        diameter_left = pupilDataGetter.diameter_left;
        rotation_left = pupilDataGetter.pupil_angle_left;

        norm_pos_right = pupilDataGetter.norm_pos_right;
        pupil_center_right = pupilDataGetter.pupil_center_right;
        diameter_right = pupilDataGetter.diameter_right;
        rotation_right = pupilDataGetter.pupil_angle_right;
    }

    private void UpdatePupilData(GameObject pupil, Vector3 norm_pos, float diameter)
    {
        pupil.transform.localPosition = new Vector3(norm_pos.x * 10, 0, norm_pos.y * 10);
        pupil.transform.localRotation = Quaternion.Euler(0, 0, 0);
        pupil.transform.localScale = new Vector3(1.0f, 0.01f, 1.0f);
    }

    public void DisablePupilData()
    {
        left_container.SetActive(false);
        right_container.SetActive(false);
    }
}