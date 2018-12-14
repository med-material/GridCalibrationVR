using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class ResetHandler : MonoBehaviour {

    public string mode = "";
    public bool restart = false;
    private GameController gmctrl;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
    }


    void Update () {

        if (Input.GetKeyDown("m"))  //Go back to menu
        {
            if (gmctrl == null)
            {
                gmctrl = GameObject.Find("GameController").GetComponent<GameController>(); //not the best but it's the only way to keep an instance of the game controller
            }

            if (gmctrl.is_started)
            {
                SceneManager.LoadScene("GridCalibrationTest", LoadSceneMode.Single);
            }

        } else if (Input.GetKeyDown("r")) //Retry the selected mode
        {
            if (gmctrl == null)
            {
                gmctrl = GameObject.Find("GameController").GetComponent<GameController>(); //not the best but it's the only way to keep an instance of the game controller
            }
            if (gmctrl.is_started)
            {
                mode = gmctrl.choosenMode;
                SceneManager.LoadScene("GridCalibrationTest", LoadSceneMode.Single);
                restart = true;
            }
        }

    }
}
