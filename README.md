# GridCalibrationVR
GridCalibration VR Game made with Unity
========

The GridCalibrationVR environment provides a test for the Pupil Labs Calibration environment, used with the Pupil Labs plugin for eye-tracking in VR.
This test helps to understand how much the user's gaze is tracked on different area (Cases of a 3*3 grid here) of the Field Of View. 
The user will have to stare at circles which will randomly appear in the grid's cases. Once the user's gaze is on the target/circle, the circle'size will
shrink until the gaze is no longer on it. At the end, all the circles appear with their last size (when the gaze point was on it), indicating 
the area in which the gaze point is badly tracked by Pupil Labs.
If the user "fails" to stare at circle when it appears 3 times, it will no longer appear.
This test environment was developed for further environments about medical researches and rehabilitation of visual neglects patients.
In order to "attract" their gaze, there is 3 test mode.

Mode 1 : Normal mode
When appearing, the circle will be pulsating until the gaze point is on it, it will then shrink while the center of the circle (red dot)
will pulsate, in order to keep the user's attention on it.

Mode 2 : Declining mode
When appearing, the circle will have a much bigger size, in order for the user not to miss it. It will then slowly shrink down to its
original size, and shrink again of the gaze point is on it.

Mode 3 : Moving circle mode
Once the user has stared at a circle, it will not disappear, it will slowly move to the next case, while growing back to its original size.
In this mode, the user will just have to keep its eyes on the target during the test, it will bring it's gaze to the next location.
The operator (people not using the headset) will be able to modify the speed of the moving target via the Unity Editor.

In the menu, the operator can choose the mode using the key "1/2/3" (for each mode). Pressing "r" during a mode will restart it,
pressing "m" will get back to the menu, to choose another mode.

For better understanding of where the user is looking, for the operator, the Unity's line renderer will be visible only in the Unity Editor,
not in the headset.
Same is for the grid, the user will only see the targets.

![calib_start](https://user-images.githubusercontent.com/3967945/56736752-3fbba580-6769-11e9-853e-f744814c1158.png)
![78](https://user-images.githubusercontent.com/3967945/56736733-34687a00-6769-11e9-8f63-9a3baaa2250a.png)
![34](https://user-images.githubusercontent.com/3967945/56736734-34687a00-6769-11e9-9b3e-8ea4453980c4.png)
![45](https://user-images.githubusercontent.com/3967945/56736736-34687a00-6769-11e9-84f7-527411bec79b.png)
![56](https://user-images.githubusercontent.com/3967945/56736737-34687a00-6769-11e9-86ac-84aa474e0c1f.png)
![67](https://user-images.githubusercontent.com/3967945/56736738-34687a00-6769-11e9-81fa-7f3309e31458.png)




Installation
------------

You must have installed SteamVR and the Pupil Labs plugin in Unity : https://github.com/pupil-labs/hmd-eyes
Go to the 2DCalibrationDemo Scene, and in the Pupil Manager's editor, go the Pupil Manger Script.
Make it Size 1 and put the GridCalibration Test at Element 0.

Before launching, start the Pupil Service software. Once the user's eyes are tracked, you can launch the 2dCalibrationDemo scene.
It will start with the origin Pupil Labs calibration, and then it will go the Grid test environment.

