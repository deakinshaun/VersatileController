# VersatileController
A 3 DoF virtual reality controller supporting VR development on Unity

![Example application using the controller](Screenshots/applicationcontroller.png)
![Example application using the controller, with the hand skin](Screenshots/applicationhand.png)

The versatile controller is an application for a phone that turns the phone into a controller for virtual reality applications. The supporting software provides a virtual representation of the controller that can be integrated into Unity applications.

![View of the controller application](Screenshots/controllercontrols.png)
![View of the controller application, with the hand skin](Screenshots/controllerhand.png)

There are some dependencies that have to be installed into any Unity project that employs the flexible controller. The instructions for adding these are below.

By default this project uses a single (free and low capacity) license key for the photon library (which manages the network communication between controller and unity package). You can use it in this mode, but may run into issues if too many other users are using the package at the same time. Instructions near the bottom of the document describe how to get your own license key, and how to rebuild the android application.

Project Structure
=================

This repository contains the following Unity projects:
- VersatileController: This is the source for the controller application, capable of running on desktop, android phone (natively), or phone (via a supported web browser).
- BalloonPop-Desktop-Events: A sample application using the controller, that runs on the desktop (e.g., in the Unity editor), and interacts with controllers using a set of events specific to the versatile controller.
- XRInteractionToolkit: A sample application that can run on the desktop, or on virtual reality headsets. This uses the Unity input system to communicate with the controllers (i.e., the controllers can replace the native controllers supplied with the headset). The controller ID must be: VersatileControllerHead to control the camera view and VersatileController to manipulate the controllers.
- Puppeteer-Desktop-InputSystem: A sample application that can run on the desktop. Several controllers are used to manipulate the head, hands and feet of the puppet. This uses the Unity input system. The controller ID must be: VersatileControllerHead for the head controls, VersatileController for the hands, and VersatileControllerFeet for the feet.

Basic Installation Instructions
===============================

These instructions assume you are using the versatilecontroller.apk which is part of the repository. This needs to be installed on the android phone you are using.

This phone needs to be equipped with a gyroscope (most models are, but some do not, so check the phone specifications if there is an issues tracking rotation of the controller). The phone also needs to be connected to the Internet, as does the application running on Unity (via the editor, or via a build). The version of the controller apk that is supplied uses ARCore for tracking both position and orientation. Your device needs to be able to support this. If your device doesn't support ARCore but has a gyroscope then you can build a version of the android application that tracks only orientation (see the instructions below).

1. Open Unity with the project you want to add the controller to.

2. Install Photon Fusion 2 (2.0.6 required if using runnable applications provide). When prompted, provide the key: e7b26908-a034-49cd-bf45-5e05f1ec4f2a
If you miss this step, the key can be set in Assets/Photon/Fusion/Resources/PhotonAppSettings.asset, as the App Id Fusion (open the Server/Cloud Settings if you can't see this).

3. You may receive a prompt to install TextMesh Pro. The TMP Essentials are required, so install these. 

4. Copy the VersatileControllerInterface folder from one of the example projects in this repository (e.g., in the XRInteractionToolkit project) into the Assets folder of your Unity project. 

5. Add the VersatileControllerManager prefab under this folder to your project. You will need to set the Avatar Prefab property to the VersatileControllerAvatar prefab.

6. You should now be able to run your Unity project, start the application on your phone and see the virtual version of the controller manipulated by the physical device.

Using the Controller in your own application
============================================

The sample BalloonPop component demonstrates how to utilize the controller in your application. Specifically:

- All functions that you should need to use are available via the VersatileControllerVirtual class.
- You can register a callback to be notified of new controllers joining your application using the subscribeNewControllers method. This is a static
method so does not need an instance of the class.
- When a new controller connects, this will provide a GameObject containing a VersatileControllerVirtual component.
- You can subscribe to button events (Down and Up), and pose events using this component. 
- The button callbacks provide the name of the button, and identify the controller object that produced the event. The state of the button is implicit
in the callback (whether it is the down or up callback event).
- The pose callback provides the orientation and position of the controller. For 3 DoF controllers, the position is always zero.
- Controllers that disconnect will have their VersatileControllerVirtual (and GameObject) become null. Do check for this before attempting to 
access the controller.

The TemplateScene and the ControllerTemplate.cs provide a minimalistic scene and code component that is recommended as a starting point for any new applications using the VersatileController. 

Using the Controller with your own license key
==============================================

The android application needs to be rebuilt if you use a different key. This key needs to be the same in the android application, and in the Unity project that the controller connects to.

Building the controller application
===================================

The project is set to build the controller application (as an apk) directly.

Use File/Build And Run to start this. Make sure the ControllerScene is the only scene enabled in the Scene List.

By default, the project builds the version that uses ARCore and ARFoundation to provide rotational and positional tracking (a 6 DoF controller).

If your device does not support this, you can still use the 3 DoF rotational tracking. In particular, to build the WebGL version you do need to switch to 3 DoF. Changes required include:
- Disable ARCore under the project settings (XR Plug-in Management).
- Edit the VersatileControllerPrefab under the VersatileControllerAndroid/Prefabs/Resources folder in the Assets. Disable the UseAR checkbox.

A relatively recent apk is included in the repository, so you can install this directly to your phone without needing to build the controller application. 

A WebGL version can be accessed at: https://rawcdn.githack.com/deakinshaun/VersatileController/main/WebControllerV2.02/index.html

Acknowledgements
================

Buttons: http://www.holshousersoftware.com/glass/

Universal Controller: https://github.com/Unity-Technologies/XR-Interaction-Toolkit-Examples/, Unity Companion License

Hand Tracking: https://huggingface.co/unity/inference-engine-blaze-hand, Apache 2 License, Unity Evaluation License
