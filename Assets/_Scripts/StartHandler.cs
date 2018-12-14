using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartHandler : MonoBehaviour
{

    public GridController gridController;
    public GameController gameController;
    private ResetHandler rst;
    private RaycastHit obj;
    public GameObject Menu;
    public GameObject startButton;
    public GameObject approxButton;
    public GameObject shrinkButton;
    public Image shrinkLoader;
    public Image approxLoader;
    public Image startLoader;
    public GameObject countDownText;
    public float timer;
    public bool isRestarted;
    private float countDown = 3.0f;

    void Start()
    {
        rst = GameObject.Find("SceneController").GetComponent<ResetHandler>();
        ResetTimer();
    }

    void Update()
    {
           if (rst.restart)
            {
                timer = -1;
            }   

        obj = gridController.GetCurrentCollider();
        if (obj.collider && timer > 0)
        {
            if (gridController.IsCollidingWithObj(startButton))
            {
                startLoader.fillAmount = 2.0f - timer;
                timer -= Time.deltaTime;
            }
            else if (gridController.IsCollidingWithObj(approxButton))
            {
                approxLoader.fillAmount = 2.0f - timer;
                timer -= Time.deltaTime;
            }
            else if (gridController.IsCollidingWithObj(shrinkButton))
            {
                shrinkLoader.fillAmount = 2.0f - timer;
                timer -= Time.deltaTime;
            }
            else
            {
                ResetFillAmount();
                ResetTimer();
            }
        }
        if (timer < 0)
        {
            // Print the timer, the seconds is rounded to have 3,2,1 value like seconds
            countDownText.GetComponent<TextMesh>().text = Math.Ceiling(System.Convert.ToDouble(countDown)).ToString();
            countDown -= Time.deltaTime;
            Menu.transform.position += new Vector3(0, 0, -10.0f);
            if (countDown < 0)
            {
                print("START");
                gameController.is_started = true;
                ResetTimer();
                Menu.SetActive(false);
                countDownText.SetActive(false);
                if (rst.restart)
                {
                    gameController.choosenMode = rst.mode;
                    rst.restart = false;
                }               
            }
        }
        if (timer < 0 && !rst.restart)
        {
            ResetFillAmount();
            if (gridController.IsCollidingWithObj(approxButton))
            {
                gameController.choosenMode = "approx";
            }
            else if (gridController.IsCollidingWithObj(startButton))
            {
                gameController.choosenMode = "normal";
            }
            else if (gridController.IsCollidingWithObj(shrinkButton))
            {
                gameController.choosenMode = "shrink";
            }
        }
    }
    private void ResetTimer()
    {
        timer = 2.0f;
    }

    private void ResetFillAmount()
    {
        startLoader.fillAmount = 0.0f;
        approxLoader.fillAmount = 0.0f;
        shrinkLoader.fillAmount = 0.0f;
    }

}
