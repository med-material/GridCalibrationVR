using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PupilTestData : MonoBehaviour {

	 private void OnEnable()
    {
        PupilTools.OnConnected += StartPupilSubscription;
        PupilTools.OnDisconnecting += StopPupilSubscription;

        PupilTools.OnReceiveData += CustomReceiveData;
    }
	
    void StartPupilSubscription()
    {
        PupilTools.CalibrationMode = Calibration.Mode._2D;
        PupilTools.SubscribeTo("pupil.");
        //PupilTools.SubscribeTo("fixation");
    }

    void StopPupilSubscription()
    {
        PupilTools.UnSubscribeFrom("pupil.");
        //PupilTools.UnSubscribeFrom("fication");
    }

    void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        if (topic.StartsWith("pupil"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "confidence":
                        print("Confidence : " + PupilTools.FloatFromDictionary(dictionary, item.Key));
                        break;
                    case "norm_pos": // Origin 0,0 at the bottom left and 1,1 at the top right.
                        print("Norm : " + PupilTools.VectorFromDictionary(dictionary, item.Key));
                        break;
                    case "ellipse":
                        var dictionaryForKey = PupilTools.DictionaryFromDictionary(dictionary, item.Key);
                        foreach (var pupilEllipse in dictionaryForKey)
                        {
                            switch (pupilEllipse.Key.ToString())
                            {
                                case "angle":
                                    var angle = (float)(double)pupilEllipse.Value;
                                    // Do stuff
                                    break;
                                case "center":
                                    print("Pupil center : " + PupilTools.ObjectToVector(pupilEllipse.Value));
                                    break;
                                case "axes":
                                    var vector = PupilTools.ObjectToVector(pupilEllipse.Value);
                                    // Do stuff
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        //print(item.Key);
                        break;
                }
            }
        }
        /**else if (topic.StartsWith("fixation"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "topic":
                        print("Topic : " + PupilTools.StringFromDictionary(dictionary, item.Key));
                        break;
                    default:
                        print(item.Key);
                        break;
                }
            }
        }*/
    }

    void OnDisable()
    {
        PupilTools.OnConnected -= StartPupilSubscription;
        PupilTools.OnDisconnecting -= StopPupilSubscription;
        PupilTools.OnReceiveData -= CustomReceiveData;
    }
}
