# SteamVR

## SteamVR installation

In order to use SteamVR you need to install it on `Steam`. Then you also need the SteamVR plugin on the Unity Asset Store. ![Steam VR on Steam](Resources/Documentation/SteamVR_Unity.png "SteamVR Unity plugin")

[Link to the asset store SteamVR plugin](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)

## Unity steamVR

For the steamVR documentation you can see the documentation in the `SteamVR` folder, `SteamVR Unity Plugin - Input System` or with :
[this link to the SteamVR plugin documentation](https://valvesoftware.github.io/steamvr_unity_plugin/). This documentation go through how to access input data, how to create new actions sets and how to bind new inputs for controllers.

The SteamVR plugin documentation is very light, but it is very fast forward to get ready to use it. You simply need to download the plugin, set the Player settings to `Virtual Reality supported` in XR.

You can then drop a `CameraRig` into your scene to handle the controllers. You should also use a script called `Steam VR_Activate Action Set On Load.cs` on any script. It is useful to load and activate the action set on the load of the GameObject.

![Steam VR_Activate Action Set On Load](Resources/Documentation/SteamVRAction.png "Steam VR_Activate Action Set On Load")

Thanks to the SteamVR Input Window you can access to a window to create multiple things.

!["Steam VR window access](Resources/Documentation/SteamVRInput.png "Steam VR window access")

This window allows you to create new binding thanks to the button `Open binding UI`. It will open a web interface described in the SteamVR documentation presented at the beggining of this document.

!["Steam VR window access](Resources/Documentation/SteamVRInputWindow.png  "Steam VR window access")

## Actions example : 

First of all you will need this line in your code : 
`using Valve.VR;` to be able to call any components of the plugin in your scripts.

`SteamVR_Input._default.inActions.GrabPinch.GetStateDown(SteamVR_Input_Sources.Any)` this code line is an example of how to get the trigger press down input action from any controller. `_default` is the name of the action set, `inActions` are the incoming inputs from the user, you can also use `outActions` to access the haptic output. `GrabPinch` is the action binded to the controller's trigger. You can access to all actions by replacing the `GrabPinch` action by another action name. The `GetStateDown` is the state event you are listenning to, this function needs a source in parameter (eg: `SteamVR_Input_Sources.Any|LeftHand|RightHand`).   
