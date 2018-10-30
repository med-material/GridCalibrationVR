using System;
using System.Collections;

public class PupilDataGetter
{
    public void startSubscribe() {
        if (PupilTools.IsConnected)
        {
            PupilTools.SubscribeTo("gaze");
            PupilTools.SubscribeTo("pupil.");

            PupilTools.OnReceiveData += CustomReceiveData;
        }
    }
}