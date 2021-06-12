GPU Instancer - Crowd Animations v0.9.4
Copyright ©2019 GurBu Technologies
---------------------------------
Thank you for supporting GPU Instancer!

---------------------------------
DOCUMENTATION
---------------------------------
Please read our online documentation for more in-depth explanations and customization options at:
https://wiki.gurbu.com/

---------------------------------
SETUP
---------------------------------
Please make sure that you have imported the latest version of GPU Instancer asset to your project first.
If you import GPU Instancer after you imported Crowd Animations, please reimport the GPUInstancer-CrowdAnimations folder.

1. Add Crowd Manager to your scene
Tools -> GPU Instancer -> Add Crowd Manager

2. In the Inspector window, press the "?" button at the top-right corner to get detailed information about setting up the manager.
Or press the "Wiki" button to read more about the Crowd Manager.

---------------------------------
SUPPORT
---------------------------------
If you have any questions, requests or bug reports, please email us at: support@gurbu.com
Unity Forum Thread: https://forum.unity.com/threads/gpu-instancer-crowd-animations.669724/

---------------------------------
MINIMUM REQUIREMENTS
---------------------------------
- DirectX 11 or DirectX 12 and Shader Model 5.0 GPU (Windows, Windows Store)
- Metal (macOS)
- OpenGL Core 4.3 (Windows, Linux)
- Modern Consoles (PS4, Xbox One)

---------------------------------
DEMO SCENES
---------------------------------
You can find demo scenes that showcase GPU Instancer - Crowd Animations capabilities in the "GPUInstancer-CrowdAnimations/Demos" folder. 
These scenes are only for demonstration and you can safely remove this folder from your builds.

---------------------------------
CHANGE LOG
---------------------------------

v0.9.4

IMPORTANT: Requires GPU Instancer v1.4.0 or later
IMPORTANT: All animations need to be re-baked after this update

New: Added HDRP and URP versions of Demo scenes
New: Added support for HDRP 7 Lit shader
New: Added animations test shader for HDRP 7
New: Added "Optional Renderers" feature to be able to disable/enable child Skinned Mesh Renderers per instance at runtime
New: Added demo scene showing use of enabling/disabling skinned mesh renderers during runtime
New: Added option to bake animation clip lists for prefabs without an Animator component
New: Added Animation Data ScriptableObject to store the baked animation data which allows to use the same baked data for multiple prototypes
New: Added Bake All/Missing buttons
New: Added option to delete baked data while deleting a prototype
New: Added multi-select feature to Frame Rate slider

Changed: Updated support for URP 7 Lit shader and Animations Test shader (re-export package needed)

v0.9.3

New: Added support for URP 7 Lit shader
New: Added animations test shader for URP 7

Changed: Crowd Manager now uses the GPUI shaders if original material uses URP Lit.

v0.9.2

New: Added support for HDRP 6 Lit shader 
New: Added animations test shader for HDRP 6
New: Added support for LWRP 6 Lit shader 
New: Added animations test shader for LWRP 6

Changed: Crowd Manager now uses the GPUI shaders if original material uses LWRP Lit or HDRP Lit.

Fixed: Auto. Add/Remove functionality errors when there is insufficient buffer size

v0.9.1

New: Option to disable loop for animation clips while using Crowd Animator workflow
New: Custom editor to define animation events from the Crowd Manager
New: Added optional float, int and string parameters to animation event system

Fixed: Insufficient animation data buffer size when adding new instances at runtime
Fixed: Mesh vertex color data being lost
Fixed: Animation events for the first frame not repeating when in loop

v0.9.0

New: Added Animation Event system for Crowd Animator workflow
New: Added a demo scene showcasing animation events
New: Added two new scenes that show the usage of material variation for crowd instances

Fixed: Animations getting stuck after setting speed to 0 once
Fixed: Active clip count calculation error on animation blending
Fixed: Wrong animation time calculation while exiting animation blending with transition
Fixed: CPU - GPU frame index synchronization issues when an animation runs for a long time
Fixed: Adding/removing causing other instances to switch to incorrect animation clip/frame

v0.8.5

Fixed: Animator error with multiple prototypes
Fixed: Default Clip option not showing all the clips when there are multiple clips with the same name

v0.8.4

Changed: Baked texture includes 1 more additional frame to include both first and last frames (requires re-bake)

Fixed: Animation - root motion syncronization problem
Fixed: Speed change causing syncornization problem when blending animations

v0.8.3

Changed: C# and HLSL code refactoring for better multi-platform support

Fixed: Incorrect mesh information on macOS
Fixed: Incorrect skinning when instance is removed with Auto Add/Remove feature
Fixed: Test animations are not visible

v0.8.2

New: Added Get/Set AnimationTime API methods
New: Added transition functionality for the Crowd Animator API methods
New: Added time parameter for animation blending

Fixed: Error when the child skinned mesh renderer is disabled on the original prefab
Fixed: Compatibility issues for PSSL
Fixed: Compatibility issues for Metal
Fixed: Shadows not rendering with Standard shaders when LOD Cross Fade enabled
Fixed: Flickering between animation frames
Fixed: Root motion not calculating animation speed
Fixed: Root motion not calculating instance scale

v0.8.1

Changed: Added comments to demo scene scripts

Fixed: Multiple skinned mesh renderers with different bind pose values for the same bones causing incorrect skinning
Fixed: Test Animations button causing errors when there are scripts with dependencies on the prefab

v0.8.0

First release