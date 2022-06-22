# VersatileController
A 3 DoF virtual reality controller supporting VR development on Unity

The versatile controller is an application for an android phone that turns the phone into a controller for virtual reality applications. The supporting software provides a virtual representation of the controller that can be integrated into Unity applications.

There are some dependencies that have to be installed into any Unity project that employs the flexible controller. The instructions for adding these are below.

By default this project uses a single (free and low capacity) license key for the photon library (which manages the network communication between controller and unity package). You can use it in this mode, but may run into issues if too many other users are using the package at the same time. Instructions near the bottom of the document describe how to get your own license key, and how to rebuild the android application.

Basic Installation Instructions
===============================

These instructions assume you are using the versatilecontroller.apk which is part of the repository. This needs to be installed on the android phone you are using.

This phone needs to be equipped with a gyroscope (most models are, but some do not, so check the phone specifications if there is an issues tracking rotation of the controller). The phone also needs to be connected to the Internet, as does the application running on Unity (via the editor, or via a build).

1. Open Unity with the project you want to add the controller to.

2. Install Photon Pun2. When prompted, provide the key: 0b3d611a-0158-4804-a33e-18a9e4456aff
If you miss this step, the key can be set in Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset, as the App Id PUN (open the Server/Cloud Settings if you can't see this).

3. You may receive a prompt to install TextMesh Pro. The TMP Essentials are required, so install these if asked.

4. Copy the VersatileControllerUnity folder from this repository into the Assets folder of your Unity project. 

5. Add the VersatileController prefab under this folder to your project.

6. You should now be able to run your Unity project, start the application on your phone and see the virtual version of the controller manipulated by the physical device.

Using the Controller with your own license key
==============================================

The android application needs to be rebuilt if you use a different key. This key needs to be the same in the android application, and in the Unity project that the controller connects to.

