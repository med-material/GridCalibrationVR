In this folder you will find resources for the OpticianCalibration Scene.

The Landolt C is the sprite used as target for the user.
The FOVScriptable object is the object that stores the data between game and scene. 
It is used to store the FOV points and the acuity points, but also the previous HMD-eye distance
setup.

The FOVs prefab is used to display two lineRenderers : one for the Acuity Field (red) and one for the FOV (blue).

The prefab can be dropped in any scene as direct child of the camera and its transform property must stay the same. It use the layer 11 called "OperatorUI" to display
the lineRenderers only on this layer.