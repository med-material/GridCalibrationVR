# SteamVR

For the steamVR documentation you can see the documentation in the `SteamVR` folder, `SteamVR Unity Plugin - Input System`. This documentation go through how to access input data, how to create new actions sets and how to bind new inputs for controllers.

The SteamVR plugin documentation is very light, but it is very fast forward to get ready to use it. You simply need to download the plugin, set the Player settings to `Virtual Reality supported`.

You can then drop a `CameraRig` into your scene to handle the controllers. You should also use a script called `Steam VR_Activate Action Set On Load.cs` on any script. It is useful to load and activate the action set on the load of the GameObject.

![Steam VR_Activate Action Set On Load](Resources/Documentation/SteamVRAction.png "Steam VR_Activate Action Set On Load")

Thanks to the SteamVR Input Window you can access to a window to create multiple things.

!["Steam VR window access](Resources/Documentation/SteamVRInput.png "Steam VR window access")

This window allows you to create new binding thanks to the button `Open binding UI`. It will open a web interface described in the SteamVR documentation presented at the beggining of this document.

!["Steam VR window access](Resources/Documentation/SteamVRInputWindow.png "Steam VR window access")