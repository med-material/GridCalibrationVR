// originally created by Theo and Kiefer (French interns at AAU Fall 2017) 
// modified by Bianca

// outcommented parts doesn't work in Pupil Labs Plugin, but is used in another project

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace RockVR.Video.Demo
{
    public class LoggerBehavior : MonoBehaviour
    {

        #region Public editor fields

        #endregion

        private static Logger _logger;
        private static List<object> _toLog;
        private Vector3 gazeToWorld;
        private static string CSVheader = AppConstants.CsvFirstRow;
        private Camera dedicatedCapture;
        GameController gameController;
        public static string sceneName = "_";

        #region Unity Methods

        private void Start()
        {
            PupilTools.OnConnected += StartPupilSubscription;
            PupilTools.OnDisconnecting += StopPupilSubscription;

            PupilTools.OnReceiveData += CustomReceiveData;


            gameController = GetComponent<GameController>();
            _toLog = new List<object>();
            dedicatedCapture = Camera.main;
        }

        private void Update()
        {
            DoLog();
            AddToLog();
        }


        private void AddToLog()
        {
            if (PupilData._2D.GazePosition != Vector2.zero)
            {
                gazeToWorld = dedicatedCapture.ViewportToWorldPoint(new Vector3(PupilData._2D.GazePosition.x, PupilData._2D.GazePosition.y, Camera.main.nearClipPlane));
            }

            var tmp = new
            {
                // default variables for all scenes
                a = DateTime.Now,
                b = (int)(1.0f / Time.unscaledDeltaTime), // frames per second during the last frame, could calucate an average frame rate instead
                c = gameController.choosenMode,
                d = dedicatedCapture.transform.position.x,
                e = dedicatedCapture.transform.position.y,
                f = dedicatedCapture.transform.position.z,
                g = dedicatedCapture.transform.rotation.x,
                h = dedicatedCapture.transform.rotation.y,
                i = dedicatedCapture.transform.rotation.z,
                j = PupilData._2D.GazePosition != Vector2.zero ? PupilData._2D.GazePosition.x : double.NaN,
                k = PupilData._2D.GazePosition != Vector2.zero ? PupilData._2D.GazePosition.y : double.NaN,
                l = PupilData._2D.GazePosition != Vector2.zero ? gazeToWorld.x : double.NaN,
                m = PupilData._2D.GazePosition != Vector2.zero ? gazeToWorld.y : double.NaN,
                n = PupilData._2D.GazePosition != Vector2.zero ? PupilTools.FloatFromDictionary(PupilTools.gazeDictionary, "confidence") : double.NaN, // confidence value calculated after calibration 
                o = gameController.travel_time < 0 ? gameController.travel_time : double.NaN,
                p = gameController.last_target.circle.transform.position.x,
                q = gameController.last_target.circle.transform.position.y,

            };
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
            if (_toLog.Count == 0) //&& SceneManage.loadTestScene == -2) // check this in Profiler 
            {
                var firstRow = new { CSVheader };
                _toLog.Add(firstRow);
            }
            _logger.Log(_toLog.ToArray());
            _toLog.Clear();
        }

        void StartPupilSubscription()
        {
            PupilTools.CalibrationMode = Calibration.Mode._2D;

            PupilTools.SubscribeTo("pupil.");
            PupilTools.SubscribeTo("fixation");
        }

        void StopPupilSubscription()
        {
            PupilTools.UnSubscribeFrom("pupil.");
        }

        void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
        {
            if (topic.StartsWith("pupil"))
            {
                foreach (var item in dictionary)
                {
                    switch (item.Key)
                    {
                        case "topic":
                            var textForKey = PupilTools.StringFromDictionary(dictionary, item.Key);
                            // Do stuff
                            break;
                        case "confidence":
                            print("Confidence : " + PupilTools.FloatFromDictionary(dictionary, item.Key));
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (topic.StartsWith("fixation"))
            {
                foreach (var item in dictionary)
                {
                    switch (item.Key)
                    {
                        case "base_data":
                            print("Base_data : " + PupilTools.StringFromDictionary(dictionary, item.Key));
                            break;
                        case "confidence":
                           print("Confidence : " +PupilTools.FloatFromDictionary(dictionary, item.Key)); 
                            break;
                        case "duration":
                            print("Duration : " +PupilTools.FloatFromDictionary(dictionary, item.Key) + "ms");
                            break;
                        default:
                            break;
                    }
                }
            }
        }
      

        #endregion
    }


}