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
    public float left_confidence;
    public float right_confidence;
    public string current_topic;
    public Vector2 norm_pos;
    public float pupil_angle;
    public Vector3 pupil_axes;
    public Vector3 pupil_center;
    public Vector2 fix_norm_pos;
    public float fix_dispersion;
    public float fix_duration;
    public object fix_base_data;
    public float fix_confidence;
    #endregion

    private List<string> topics;
    private float gaze_timestamp;

    public PupilDataGetter()
    {
        topics = new List<string>();
    }

    public void startSubscribe(List<string> topics)
    {
        if (PupilTools.IsConnected)
        {
            this.topics.AddRange(topics);
            foreach (string topic in topics)
            {
                PupilTools.SubscribeTo(topic);
            }
            PupilTools.OnReceiveData += CustomReceiveData;
        }
    }

    public void stopSubscribe()
    {
        foreach (string topic in topics)
        {
            PupilTools.UnSubscribeFrom(topic);
        }
        topics.Clear();
        PupilTools.OnReceiveData -= CustomReceiveData;
    }

    private bool IsGazingAndFixing()
    {
        return topics.Contains("fixation") && topics.Contains("gaze");
    }
    private void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
    {
        GetGazeData(topic, dictionary);
        GetPupilData(topic, dictionary);
        GetFixationData(topic, dictionary);

        if (IsGazingAndFixing())
        {

        }
    }

    private void GetGazeData(String topic, Dictionary<string, object> dictionary)
    {
        if (topic.StartsWith("gaze"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "topic":
                        current_topic = PupilTools.StringFromDictionary(dictionary, item.Key);
                        break;
                    case "confidence":
                        confidence = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void GetPupilData(String topic, Dictionary<string, object> dictionary)
    {
        if (topic.StartsWith("pupil"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "topic":
                        current_topic = PupilTools.StringFromDictionary(dictionary, item.Key);
                        break;
                    case "confidence":
                        if (topic.StartsWith("pupil.1"))
                        {
                            left_confidence = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        }
                        else if (topic.StartsWith("pupil.0"))
                        {
                            right_confidence = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        }
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
                                    pupil_angle = (float)(double)pupilEllipse.Value;
                                    break;
                                case "center":
                                    pupil_center = PupilTools.ObjectToVector(pupilEllipse.Value);
                                    break;
                                case "axes":
                                    pupil_axes = PupilTools.ObjectToVector(pupilEllipse.Value);
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
        }
    }

    private void GetFixationData(String topic, Dictionary<string, object> dictionary)
    {
        if (topic.StartsWith("fixation"))
        {
            foreach (var item in dictionary)
            {
                switch (item.Key)
                {
                    case "topic":
                        current_topic = PupilTools.StringFromDictionary(dictionary, item.Key);
                        break;
                    case "norm_pos":
                        fix_norm_pos = PupilTools.VectorFromDictionary(dictionary, item.Key);
                        break;
                    case "dispersion":
                        fix_dispersion = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        break;
                    case "duration":
                        fix_duration = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        break;
                    case "confidence":
                        fix_confidence = PupilTools.FloatFromDictionary(dictionary, item.Key);
                        break;
                    default:
                        break;
                        // Other sub-topics :
                        //base_data: Gaze data that the fixation is based on
                }
            }
        }
    }
}