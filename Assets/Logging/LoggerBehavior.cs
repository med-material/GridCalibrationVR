// originally created by Theo and Kiefer (French interns at AAU Fall 2017) 
// modified by Bianca

// outcommented parts doesn't work in Pupil Labs Plugin, but is used in another project

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoggerBehavior : MonoBehaviour
{

    #region Public editor fields

    #endregion

    private static Logger _logger;
    private static List<object> _toLog;
    private Vector3 gazeToWorld;
    private static string CSVheader = AppConstants.CsvFirstRow;
    private Camera dedicatedCapture;
    private GameController gameController;
    public static string sceneName = "_";

    #region Unity Methods

    private void Start()
    {
        _toLog = new List<object>();
        dedicatedCapture = Camera.main;
    }

    private void Update()
    {
    }

    private void AddToLog()
    {
        var gameController = gameObject.GetComponent<GameController>();
        if (PupilData._2D.GazePosition != Vector2.zero)
        {
            gazeToWorld = dedicatedCapture.ViewportToWorldPoint(new Vector3(PupilData._2D.GazePosition.x, PupilData._2D.GazePosition.y, Camera.main.nearClipPlane));
        }

        var tmp = new
        {
            // default variables for all scenes
            a = DateTime.Now,
            c = gameController.choosenMode != "" ? gameController.choosenMode : "",
            j = PupilData._2D.GazePosition != Vector2.zero ? PupilData._2D.GazePosition.x : double.NaN,
            k = PupilData._2D.GazePosition != Vector2.zero ? PupilData._2D.GazePosition.y : double.NaN,
            l = PupilData._2D.GazePosition != Vector2.zero ? gazeToWorld.x : float.NaN,
            m = PupilData._2D.GazePosition != Vector2.zero ? gazeToWorld.y : float.NaN,
            //n = PupilData._2D.GazePosition != Vector2.zero ? PupilTools.FloatFromDictionary(PupilTools.gazeDictionary, "confidence") : double.NaN, // confidence value calculated after calibration 
            o = gameController.travel_time,
            p = gameController.last_target != null ? gameController.last_target.circle.transform.position.x : double.NaN,
            q = gameController.last_target != null ? gameController.last_target.circle.transform.position.y : double.NaN

        };
        _toLog.Add(tmp);
    }

    public void AddObjToLog(object tmp)
    {

        DoLog();
        _toLog.Add(tmp);
    }

    private Vector3 CalculEyeGazeOnObject(RaycastHit hit)
    {
        return hit.transform.InverseTransformPoint(hit.point);
    }

    public static void DoLog()
    {
        CSVheader = AppConstants.CsvFirstRow;
        _logger = Logger.Instance;
        if (_toLog.Count == 0)
        {
            var firstRow = new { CSVheader };
            _toLog.Add(firstRow);
        }
        _logger.Log(_toLog.ToArray());
        _toLog.Clear();
    }
    #endregion
}
