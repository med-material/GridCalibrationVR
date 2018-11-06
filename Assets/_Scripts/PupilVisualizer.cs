using System;
using System.Collections;
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
        pupilDataGetter = PupilDataGetter.GetPupilDataGetter();
        norm_pos_left = pupilDataGetter.norm_pos_left;
        pupil_center_left = pupilDataGetter.pupil_center_left;
        diameter_left = pupilDataGetter.diameter_left;
        rotation_left = pupilDataGetter.rotation_left;

        norm_pos_right = pupilDataGetter.norm_pos_right;
        pupil_center_right = pupilDataGetter.pupil_center_right;
        diameter_right = pupilDataGetter.diameter_right;
        rotation_right = pupilDataGetter.rotation_right;

        left_pupil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        right_pupil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        left_container = GameObject.Find("LeftEyeContainer");
        right_container = GameObject.Find("RightEyeContainer");

        SetPupil(left_pupil, pupil_center_left, diameter_left, rotation_left, left_container.transform);
        SetPupil(right_pupil, pupil_center_right, diameter_right, rotation_right, right_container.transform);
    }

    private void SetPupil(GameObject pupil, Vector3 pupil_center, float diameter, Vector3 rotation, Transform container)
    {
        pupil.GetComponent<Renderer>().material.color = Color.red;
        pupil.transform.parent = container;
        pupil.transform.localPosition = pupil_center;
        pupil.transform.localRotation = Quaternion.Euler(rotation.x + 90, rotation.y, rotation.z);
        pupil.transform.localScale = new Vector3(diameter, 0.1f, diameter);
    }

    public void UpdatePupilData()
    {
        UpdatePupilsData(left_pupil, pupil_center_left, rotation_left, diameter_left);
        UpdatePupilsData(right_pupil, pupil_center_right, rotation_right, diameter_right);
    }

    private void UpdatePupilsData(GameObject pupil, Vector3 pupil_center, Vector3 rotation, float diameter)
    {
        pupil.transform.localPosition = pupil_center;
        pupil.transform.localRotation = Quaternion.Euler(rotation.x + 90, rotation.y, rotation.z);
        pupil.transform.localScale = new Vector3(diameter, 0.1f, diameter);
    }

    public void DisablePupilData() {
        left_container.SetActive(false);
        right_container.SetActive(false);
    }
}