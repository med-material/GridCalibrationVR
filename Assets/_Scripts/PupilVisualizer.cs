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
    private Vector3 rotation_left;
    private float diameter_right;
    private Vector3 rotation_right;
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

        SetPupil(left_pupil, norm_pos_left, diameter_left, rotation_left, left_container.transform);
        SetPupil(right_pupil, norm_pos_right, diameter_right, rotation_right, right_container.transform);
    }

    private void SetPupil(GameObject pupil, Vector3 norm_pos, float diameter, Vector3 rotation, Transform container)
    {
        pupil.GetComponent<Renderer>().material.color = Color.red;
        pupil.transform.parent = container;
        pupil.transform.localPosition = new Vector3(norm_pos.x * 10,0,norm_pos.y * 10);
        pupil.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        //pupil.transform.localScale = new Vector3(diameter/container.transform.parent.GetComponent<Renderer>().bounds.max.x, 0.1f, diameter/container.transform.parent.GetComponent<Renderer>().bounds.max.z);
        pupil.transform.localScale = new Vector3(2.0f, 0.01f, 2.0f);
    }

    public void UpdatePupilsData()
    {
        UpdateData();
        UpdatePupilData(left_pupil, norm_pos_left, rotation_left, diameter_left);
        UpdatePupilData(right_pupil, norm_pos_right, rotation_right, diameter_right);
    }

    public void UpdateData()
    {
        norm_pos_left = pupilDataGetter.norm_pos_left;
        pupil_center_left = pupilDataGetter.pupil_center_left;
        diameter_left = pupilDataGetter.diameter_left;
        rotation_left = pupilDataGetter.rotation_left;

        norm_pos_right = pupilDataGetter.norm_pos_right;
        pupil_center_right = pupilDataGetter.pupil_center_right;
        diameter_right = pupilDataGetter.diameter_right;
        rotation_right = pupilDataGetter.rotation_right;
    }

    private void UpdatePupilData(GameObject pupil, Vector3 norm_pos, Vector3 rotation, float diameter)
    {
        pupil.transform.localPosition = new Vector3(norm_pos.x * 10,0,norm_pos.y * 10);
        pupil.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
        //pupil.transform.localScale = new Vector3(diameter, 0.1f, diameter);
        pupil.transform.localScale = new Vector3(2.0f, 0.01f, 2.0f);
    }

    public void DisablePupilData()
    {
        left_container.SetActive(false);
        right_container.SetActive(false);
    }
}