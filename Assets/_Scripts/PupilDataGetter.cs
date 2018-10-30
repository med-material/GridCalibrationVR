using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PupilDataGetter
{
    # region public_data
    public float confidence;
    public Vector3 norm_pos;

    public

    # endregion
    private bool oui = true;
    public void startSubscribe()
    {
        if (PupilTools.IsConnected)
        {
            PupilTools.SubscribeTo("gaze");
            PupilTools.SubscribeTo("pupil.");

            PupilTools.OnReceiveData += CustomReceiveData;
        }
    }

    private void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        if (topic.StartsWith("pupil"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "confidence":
                        confidence = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        break;
                    case "norm_pos": // Origin 0,0 at the bottom left and 1,1 at the top right.
                        norm_pos = PupilTools.VectorFromDictionary(dictionary, item.Key);
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
                                    //print("Center : " + PupilTools.ObjectToVector(pupilEllipse.Value));
                                    break;
                                case "axes":
                                    //print("Axes : " + PupilTools.ObjectToVector(pupilEllipse.Value));
                                    // Do stuff
                                    break;
                                default:
                                    break;
                            }
                        }
                        // Do stuff
                        break;
                    default:
                        break;
                }
            }
        }
        if (topic.StartsWith("gaze") && oui)
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "2D":
                        var dictionaryForKey = PupilTools.DictionaryFromDictionary(dictionary, item.Key);
                        foreach (var twoDEllipse in dictionaryForKey)
                        {
                            //print(twoDEllipse.Key + " : " + twoDEllipse.Value);
                        }
                        break;
                    case "gaze.2d.0.":
                    case "gaze.2d.1.":
                        //print(item.Value);
                        break;
                    default:
                        break;
                }
                //print(item.Key + " : " + item.Value);

            }
            oui = false;
        }
    }

}