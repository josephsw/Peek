Peek
============

Forked from Peek by Andy Biar (andybiar) and buenopolis.

Features added by josephsw:
* Adjustable mesh frame rate (for cameras that export at a different framerate)
* Decoupled mesh from the application frame rate
* Ability to have multiple takes in one sequence
* Takes can play progressively
* Start frame number can be adjusted
* Sync video to audio track (if using actual frame numbers when exporting, it will sync with original audio from the video)
* Mesh visibility can be toggled (call playMesh externally)

============

Peek is a system of creating augmented reality content using the [Unity3D Game Engine](http://unity3d.com). Check it out on [Vimeo](http://vimeo.com/andybiar/peek).

Peek consists of several components, one of which is the RGBDShader, a Unity shader designed to render 3D models from image files. The [RGBD Toolkit](http://rgbdtoolkit.com) can export image files of the correct format, allowing you a complete workflow from filming animations with the Kinect to rendering them in Unity.

IMPORTANT NOTE: The RGBDShader is written in GLSL, so it only works on Macs at this time.

To see the shader in action, open the Unity Example Project in Unity, double click on the Peek_demo scene in the Assets folder, and press play!

![Screenshot of Example Unity Project](https://dl.dropboxusercontent.com/u/27507970/brad.png)

In the coming months we will publish a more thorough tutorial, but if you are interested in using Peek or collaborating with us, please feel free to poke around in the Example Project or contact us directly on Twitter @andybiar @buenopolis.
