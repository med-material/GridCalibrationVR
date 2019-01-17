Scenes : 
-----------------

GridCalibration : This scene is for the post-Pupil Lab calibration and is started by it. It tests the confidence of the pupil lab calibration.

-----------------

OpticianCalibration : First scene in the bundle of test to launch, it does not need the Pupil Lab calibration to work. Need a controller.
The user needs to use the slider to set the same value as the eye-lens distance setup on the HMD. Then the user needs to choose the size
of the target (Landolt C) and confirm by choosing the right opening side on the touchpad or with the keyboard arrows. 
After he (or the operator) needs to move (touchpad/arrows) the landolt C far from the center, following the given direction until he can´t distinguish the opening side.
Press the trigger button or space bar and then confirm the opening side (randomly regenerated) with touchpad or arrows. Repeat the process 7 times and then the Max Acuity Zone is saved.

-----------------

PupilLabCalibration : Pupil Lab calibration scene, needs the software Pupil Service or Capture started to work. The user has to gaze at different dots on a circle to complete 
the calibration and have at least 60% of confidence to move to next scene.