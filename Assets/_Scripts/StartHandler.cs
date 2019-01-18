using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartHandler : MonoBehaviour
{
    public GameController gameController;
    private ResetHandler rst;
    private GameObject normal, declining, moving, currentSelection;
    private Material[] target_material = new Material[2];
    public GameObject Menu;
    public GameObject countDownText;
    public float timer;
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
           if (rst.restart) //If it's a restart
            {
                timer = -1;
            }
        
        //SELECTION OF THE MODE

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

        if (Input.GetKeyDown("return") && currentSelection != null) //Launching a mode
        {
            startCountdown = true;
        }
            if (timer < 0 || startCountdown)
        {
            // Print the timer, the seconds is rounded to have 3,2,1 value like seconds
            countDownText.GetComponent<TextMesh>().text = Math.Ceiling(System.Convert.ToDouble(countDown)).ToString();
            countDown -= Time.deltaTime;
            Menu.transform.position += new Vector3(0, 0, -10.0f);

            if (countDown < 0) //After the countdown, we launch the mode
            {
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
    }
    private void ResetTimer()
    {
        timer = 2.0f;
    }

    private void makeItShine(GameObject current) //Highlight the button when selected (with 1/2/3)
    {
        target_material[0] = (Material)Resources.Load("Start button");
        target_material[1] = (Material)Resources.Load("Shiny");
        currentSelection.GetComponent<MeshRenderer>().materials = target_material;
    }

}
