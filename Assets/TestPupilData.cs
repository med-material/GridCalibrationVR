using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPupilData : MonoBehaviour
{

    // Use this for initialization
    void onEnable()
    {
        PupilTools.OnConnected += OnConnected;
        PupilTools.OnDisconnecting += OnDisconnecting;
    }

    void Update()
    {
        print(PupilTools.IsConnected);
    }

    void AddCustomReceive()
    {
        print("Method added to event");
        PupilTools.OnReceiveData += CustomReceiveData;
    }

    void RemoveCustomReceive()
    {
        PupilTools.OnReceiveData -= CustomReceiveData;
    }

    void OnConnected()
    {
        print("Connected!!!!!");
        AddCustomReceive();
    }

    void OnDisconnecting()
    {
        RemoveCustomReceive();
    }


    void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        print("Event");
        /**if (topic.StartsWith("pupil"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "confidence":
                        print("Confidence : " + PupilTools.FloatFromDictionary(dictionary, item.Key));
                        break;
                    case "norm_pos": // Origin 0,0 at the bottom left and 1,1 at the top right.
                        //print("Norm : " + PupilTools.VectorFromDictionary(dictionary, item.Key));
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
                                    //print("Pupil center : " + PupilTools.ObjectToVector(pupilEllipse.Value));
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
                        break;
                }
            }
        }*/
    }
}
