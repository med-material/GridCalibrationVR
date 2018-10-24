using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartHandler : MonoBehaviour
{

    public GridController gridController;
    public GameController gameController;
    private RaycastHit obj;
    public GameObject Menu;
    public GameObject startButton;
    public GameObject approxButton;
    public GameObject shrinkButton;
    public GameObject countDownText;
    private float timer;
    private float modeTimer;
    private float countDown = 3.0f;

    void Start()
    {
        ResetTimer();
        ResetModeTimer();
    }

    // Update is called once per frame
    void Update()
    {
        obj = gridController.GetCurrentCollider();
        if (obj.collider && timer > 0)
        {
            if (ReferenceEquals(obj.collider.gameObject, startButton))
            {
                SetShader();
                timer -= Time.deltaTime;
            }
            else if(ReferenceEquals(obj.collider.gameObject, approxButton)) {
                SetShader();
                shrinkButton.GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
                modeTimer -= Time.deltaTime;
            }
            else if(ReferenceEquals(obj.collider.gameObject, shrinkButton)) {
                SetShader();
                approxButton.GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
                modeTimer -= Time.deltaTime;
            }
            else
            {
                startButton.GetComponent<Renderer>().material.shader = Shader.Find("Diffuse");
                ResetTimer();
                ResetModeTimer();
            }
        }
        if (timer < 0)
        {
            // Print the timer, the seconds is rounded to have 3,2,1 value like seconds
            countDownText.GetComponent<TextMesh>().text = Math.Ceiling(System.Convert.ToDouble(countDown)).ToString();
            print(Math.Ceiling(System.Convert.ToDouble(countDown)).ToString());
            countDown -= Time.deltaTime;
			Menu.transform.position += new Vector3(0,0,-10.0f);
            if (countDown < 0)
            {
                print("START");
                gameController.is_started = true;
                ResetTimer();
           		Menu.SetActive(false);
				countDownText.SetActive(false);
            }
        }
        if(modeTimer < 0) {
            if(ReferenceEquals(obj.collider.gameObject, approxButton)) {
                gameController.choosenMode = "approx";
            }
            else if(ReferenceEquals(obj.collider.gameObject, shrinkButton)) {
                gameController.choosenMode = "shrink";
            }
        }
    }
    private void ResetTimer()
    {
        timer = 1.0f;
    }
    private void ResetModeTimer()
    {
        modeTimer = 0.5f;
    }
    private void SetShader() {
        obj.collider.gameObject.GetComponent<Renderer>().material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
    }
}
