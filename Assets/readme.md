# Developer documentation

## _Scripts

> This is the folder holding all the scripts. 

`GameController.cs`
This scripts holds the control for the `GridCalibration` Scene. This is where the script subscribe to pupil data, log the data and controls the transition between calibration steps.

`GridController.cs`
This script is handling the collider between the gaze point with objects. `GetCurrentCollider()` is the function to get the current collider. `GetCurrentColliders()` is the function to get all the colliders hitted by the raycast.

`Heatmap.cs`
This script is creating a circle for each gaze position thanks to `AddCircle()` to create the Heatmap.

`PupilDataGetter.cs`
Is a utility class to subscribe to pupil plugin data thanks to the function `startSubscribe(List<string> topics)`.

`PupilVisualizer.cs`
Is a class to get data from pupil plugin and to create eye-frames. WIP

`ResetHandler.cs`
Is a class to handle the input from the user. `m` is the key to go back to the menu. `r` is the key to restart the current mode.

`StartHandler.cs`
Is the class handling the game menu, you can try modes by hitting the key `1` for normal mode, `2` for shrink mode or `3` smooth pursuit and then press `enter`.

`TargetCircle.cs`
Is the class attached to the gameObject `Target` and handle its position, the creation of the circle and the dot on the middle, change the scale, set the outline, the size for the big circle mode and its moving.

`UserBehaviour.cs`
Utility class to handle the pulsating, calculate the dispersion.

### Optician

 > In this folder you can find differents scripts usefull for the OpticianCalibration Scene.

`acuityMaxFieldPrint.cs`
Class used to print Acuity Max Field LineRenderer in any scene.

`FOVPoints.cs`
Is a Serializable class to describe objects for the scriptable objects

`fovScriptableObject.cs`
Is a ScriptableObject to store data relative to eye-lens distance, like eye-lens distance and the corresponding FOV points.

`OpticianController.cs`
Is the script that holds all the logical for the OpticianCalibration Scene.

`PointingSystem.cs`
Is the script that holds all the logical for the pointing system of the controller and is used to add points to different lists.


## Prefabs

> This is the folder that contains the Prefabs of all scene.

`FOVs` is the prefab for printing lineRenderers of the FOV field and the acuity max field.

`Heatmap` this is the prefab to create a heatmap, it can be dropped in any scene to create a heatmpa fron the gaze position. It needs a layer `HeatMapMesh` to draw particules on it.

## Radial Menu Framework

> This folder contains the radial Menu that is used on the `OpticianCalibration` scene. See the `RMF Documentation.pdf` to understand how it works precisely.

`RMF_RadialMenu.cs` is the main script for this framework. It handles the input (that I have modified to get controllers input).

`RMF_RadialMenuElement.cs` is the script handling the buttons of the radial menu.  

## Resources

> In this folder we have all the resources for all scenes. 

### OpticianCalibrationResources

> This folder is containing the resources for the  `OpticianCalibration` scene, like the fovScriptableObject asset.

## Scenes

> This forlder contains all our scenes and the option for the recorder.

## Other folders

> Other folders are the plugin folders, for example the SteamVR plugin, Unity Recorder and the pupil plugin.
