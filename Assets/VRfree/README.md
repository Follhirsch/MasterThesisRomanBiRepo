# VRfree Unity Plugin
## Introduction
This project contains eveything you need to get started with development for the VRfree Glove for Unity, i. e. the VRfree plugin for Windows, UWP, and Android, as well as some samples to showcase its useage and kickstart your development process.

## Getting Started
Download a copy of (recommended Unity version 2019.2.6f1 or newer). Drag and drop the VRfreePlugin.unitypackage in your Project window. 
Go to "Edit -> Project Settings -> Player" and make sure to set "Scripting Runtime Version" to ".Net 4.x Equivalent" in "Other Settings" and tick "Virtual Reality Supported" in the "XR Settings".
In order for the grabbing scene to work you also need to go to "Edit -> Project Settings -> Physics" and set "Contact Pairs Mode" to "Enable Kinemaic Kinematic Pairs".

## UWP Settings
When building for UWP, go to Project Settings->Player->UWP Settings->Publishing Settings->Capabilities and enable InternetClient, InternetClientServer, and PrivateNetworkClientServer. Delete all previous contents of build folder or build to new folder for settings to take effect.

## Folders

### Common
This folder contains assets that are essential and/or used in multiple or most of the samples. This includes the rigged hand model, a prefab and progress bar to help with hand calibration as well as important scripts that interface with the plugin. 

### Plugins
This folder contains the VRfree plugin for Windows, UWP, and Android.

### Samples
This contains some basic samples that adress some of the basic things you can do with the gloves. Each folder contains one scene file and the assets that are used only in that scene (others are found in the "Common" folder).
#### ArHeadModuleOffset
Sample scene for AR devices such as the Hololens 2. Includes an AR head module offset calibration. Make sure to read the Instructions.txt.
#### GestureDetection
Building upon HandsBasic. This scene shows off a simple gesture detection system. It allows to create, detect, and modify gestures.
#### Grabbing
Building upon HandDimensions. This scene includes the necessary scripts and prefabs for multiple essential interactions with the virtual environment, such as grabbing objects, hinges, drawers and knobs.
#### HandDimensions
Building upon HandsBasic. In this scene, the hand models are moved using the Unity physics, which improves the way they interact with other rigidbodies. The hand models will also load and display the hand and finger sizes that are set in the VRfree Setup application.
#### HandsBasic
This is the most basic scene. Other than a (VR) camera (with VRfreeCamera script) and a directional light, it contains only the hand models with VRfreeGlove scripts attached and the means to do the hand pose calibration. 
#### Stylus
Building upon TrackerScene. The stylus is a VRfree tracker with a specific shape. The Paint3dScript additionally allows drawing into the air by pressing the button on the stylus.
#### Tracker
The TrackerScene contains the "Tracker" GameObject with the VRfreeTracker script attached. Read the Instructions.txt to match the trackerId of your tracker and VRfreeTracker script.

## Remarks
In Unity Versions 2019.3 or newer, when using XR Plugin Management, add a TrackedPoseDriver to the camera GameObject.
On devices running Android, such as the Oculus Quest (2), the write external storage permission is required, which can be enabled under Project Settings -> Player -> Android -> Other Settings -> Write Permission -> External (SDCard).