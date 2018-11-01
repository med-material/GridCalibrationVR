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
    public float fix_base_data;
    public float fix_confidence;
    #endregion

    private List<string> topics;
    
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
            //PupilTools.SubscribeTo("gaze");
            //PupilTools.SubscribeTo("pupil.");
            //PupilTools.SubscribeTo("fixation"); // TODO verify that the plugin is enabled on Pupil Capture

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
        //PupilTools.UnSubscribeFrom("gaze");
        //PupilTools.UnSubscribeFrom("pupil.");
        //PupilTools.UnSubscribeFrom("fixation");

        PupilTools.OnReceiveData -= CustomReceiveData;
    }

    private void CustomReceiveData(string topic, Dictionary<string, object> dictionary, byte[] thirdFrame = null)
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
        else if (topic.StartsWith("fixation"))
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
                        //norm_pos: Normalized position of the fixation’s centroid
                        //base_data: Gaze data that the fixation is based on
                        //duration: Exact fixation duration, in milliseconds
                        //dispersion: Dispersion, in degrees
                }
            }
        }
    }
}