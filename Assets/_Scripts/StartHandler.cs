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
    private GameObject normal, declining, moving, currentSelection;
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
    private bool startCountdown = false;

    void Start()
    {
        rst = GameObject.Find("SceneController").GetComponent<ResetHandler>();
        normal = GameObject.Find("Start button"); //1
        declining = GameObject.Find("Shrink button"); //2
        moving = GameObject.Find("Approx button"); //3
        ResetTimer();
    }

    void Update()
    {
           if (rst.restart)
            {
                timer = -1;
            }

        if (Input.GetKeyDown("1") || Input.GetKeyDown("[1]"))
        {

            if(currentSelection != null)
            {
                currentSelection.GetComponent<MeshRenderer>().materials[1] = null;
            }
            currentSelection = normal;
            makeItShine(currentSelection);
            gameController.choosenMode = "normal";
        }
        else if (Input.GetKeyDown("2") || Input.GetKeyDown("[2]"))
        {
            if (currentSelection != null)
            {
                currentSelection.GetComponent<MeshRenderer>().materials[1] = null;
            }
            currentSelection = declining;
            makeItShine(currentSelection);
            gameController.choosenMode = "shrink";
        }
        else if (Input.GetKeyDown("3") || Input.GetKeyDown("[3]"))
        {
            if (currentSelection != null)
            {
                currentSelection.GetComponent<MeshRenderer>().materials[1] = null;
            }
            currentSelection = moving;
            makeItShine(currentSelection);
            gameController.choosenMode = "approx";
        }

        if (Input.GetKeyDown("return") && currentSelection != null)
        {
            startCountdown = true;
        }

            /*obj = gridController.GetCurrentCollider();
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
            }*/
            if (timer < 0 || startCountdown)
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
                startCountdown = false;
                if (rst.restart)
                {
                    gameController.choosenMode = rst.mode;
                    startCountdown = false;
                    rst.restart = false;
                }               
            }
        }
        /*if (timer < 0 && !rst.restart)
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
        }*/
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

    private void makeItShine(GameObject current)
    {
        /*/Material[] listMat = new Material[2];
        listMat[0] = current.GetComponent<Renderer>().materials[0];
        listMat[1] = (Material)Resources.Load("Shiny");*/
        currentSelection.GetComponent<MeshRenderer>().materials[1] = (Material)Resources.Load("Shiny");
    }

}
