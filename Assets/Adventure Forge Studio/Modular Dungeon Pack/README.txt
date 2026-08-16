Thank you for purchasing Adventure Forge's Modular Dungeon Pack.

Due to changes in the Unity engine we've had to do some changes in this package. From Unity 2018.1 and onwards Unity no longer supports Allegorithmics substances out of the box.
This prompted Allegorithmic to create a plugin, that does not work with pre-2018.1 projects and completely messes up older projects by resetting the Substance and unlinking and deleting
any materials created in the substance.
Our solution has been to make a new version of our package for Unity 2018.1 users. We will continue to support both the Unity 5.5 version and the Unity 2018.1 version for some time.
Both will be available under the same package, we've simply uploaded updated packages from both Unity 5.5 and Unity 2018.1.

There will however be some differences between the two versions. Mainly that the 2018.1 version is using a new version of the substance. This is done to better work around 
Allegorithmic's plugin. There are no content differences between the two versions, just the way the Substance is set up. We've included the new version of our Substance in both
versions of our package, but it is not applied in Unity 5.5 to ensure that we do not mess up any ongoing projects for our customers


!!!-- Unity 2018.x USERS--!!!
'
If you are using Unity 2018.x you need to download and import Allegorithmics Substance in Unity plugin.
https://assetstore.unity.com/packages/tools/utilities/substance-in-unity-110555
This should be done before importing this package.

!!!-- Unity 2018.x USERS--!!!


Summary of package:

In this package you'll find many variations of different objects, such as walls and pillars.
They are categorised as 'single', 'bottom', 'middle' and top' floor. 

'Single' floor models are meant to be used with a floor model at the bottom and a ceiling model on the top, making a single story room.

'Bottom' floor models are meant to be used with a floor model at the bottom and a 'Middle' 
or 'Top' model above it, making it the start of a multi story room.

In the texture folder you'll find a texture called 'FloorMatchup_Helper'. This can be used to better match up the floor tiles to make sure that
the texture tiles correct.

For textures this package uses Allegorithmic substances, which are procedural textures. The substances that comes with this package creates PBR textures. 
But all the models, with the exception of non-substance using models such as the doors or tourch, should work just fine with any tiled textures.

Demo Scene:

To get the demo scene working as intended you need to import the 'Characters' from Unity's standard assets.
To set it up correctly follow these steps:

1a.	Turn off Anti Aliasing in Edit/Project settings/Quality
1b.	Change Color Space from Gamma to Linear in Edit/Project settings/Player

2.	Open up the demo scene found at_ Adventure Forge Studio/Modular Dungeon Pack/Scene

2a.	Drag and drop the standard 'FPSController' into the scene and place it where you want to start(remeber to import it from Unity's standard assets). The FPSController is found at: Standard Assets/Characters/FirstPersonController/Prefabs
2b.	Locate the child of the 'FPSController' called FisrtPersonCharacter. This is the camera which we are going to use in the demo scene.

3.	Locate the 'Game Controller' gameobject in the scene hierarchy. On it there is a script called 'Trigger door'. 

4.	Drag and drop the camera child(FirstPersonCharacter) of the 'FPSController' onto the 'Cam' gameobject slot on the 'Trigger door' script. What this does
	is to simply let the script know what camera it is working on.

5.	Disable the 'WarningCamera' and 'Warning Canvas' in the scene hierarchy.

6.	Hit play and explorer the demo scene and press E to open doors.

To get a better feel for the dungeon we suggest importing Unity's Post Processing Stack package and use bloom, depth of field, Ambient Occlusion and fog.
This package is found on Unity's Asset store. Simply search for 'Post Processing Stack'.
These are the effects we used when taking the screenshots for this package.
Also how you decide to bake the light will also impact the visuals greatly. We suggest having Dirrectional Mode, found under the General GI light setting, set to 'Directional Specularity'.
This will ensure good baking, the other modes does not give good results from what we've experienced with Unity v5.5.0p1

Using the package:

There are many prefabs that can be used to build out your level. What is commom for all of the is that they have been made with Unity's snapping function in mind.
Unity's snapping function is simply used by wolding down left-ctrl when moving/rotating/scaling and object. This mean that all the pieces in this package fits together as long as the snapping function is used and the transform happens in integers.
To make sure snapping happens correctly, please make sure prefabs that are dropped into scene hierarchy has a transform with whole numbers. ex transform x: 1 :y 2 z: 15. 
This ensures that when you move the prefab using the snapping tool it will align perfectly with other prefabs placed in tha same way.

We've added a texture to help match up the floor tiles better, called 'FloorMatchup_Helper'. This is because the textures tile Y to Y and X to X. Y axis does not match up with the X axis of the textures.
The simple fix is then to use this texture in the albedo of a material, add it to the floor gameobject temporarily, rotate the floor tile 90 degrees at a time to match up with neighbouring tiles. Then re-apply the floor material.

If you are not using Unity's standard shader but still want to use the textures produced by the substance you should still be able to access these below the substance materials in the MDP_Substance folder, though this has not been tested with other shaders than Unity's Standard Shader.
What you need to do is set up new materials using the shader of your choosing, apply the textures created by our substance to this material and apply this material to any prefabs you are goig to use. This should allow you to use any shader
you like and still be able to use the procedurally generated textures from the substance. We've tested this with setting up a separate Standard Shader and having Load behavior to 'Bake and keep Substance' in the substance, but this should work
the same way if you use a custom shader.

We have planed to record tutorial videos on the different ways to use the content of the package.

If you have questions or problems you can contact us on support@adventureforgestudio.com

Version History

v1.4
	Updated Substances to support new Substance in Unity Plugin
	Added The upcoming chain pack as bonus content. This is a collection of the current chain props in this package and some new props.
	Added new version of our Substance, v2.3.8. This is being used in the Unity 2018.1 version, but not in Unity 5.5 version. This is due to new Substance plugin, used in Unity 2018.1.

v1.2
	Fixed substance producing white metallic texture instead of black for the non metallic surfaces.
	Improved Substance performance.
	Implemented new Height Based Ambient Occlusion into substance. Giving mutch better performance and better visual quality.
	Implemented more customzation options into substance.
	Re-orginized substance parameters to make it easier to work with.
	Fixed error messages concerning animations not being set to legacy. These are now set up correctly and should no longer give console messages.
	Added detail normal map to be used with stone materials. This needs to be applied manually to new substance materials as Substance Designer does not allow for this to be automatically applied to Unity Materials. Though we've applied it to the base materials.
	Added prebaked textures and materials option that can be used in stead of using substances. These need to be manually applied to the prefabs as those are set up to use the substances out of the box.
	Removed height map from StoneSlab and Stone substance, it is still on the StoneBricks substance. This was done' because it didn't look right with Unity's standard shader.
	Fixed some materials that was wrongly applied.
	Added 12 new decorative cutout chains

v1.1
	Added Dungeon Trap Package as part of this package, the Modular Dungeon Package.
	Added 2 decorating objects; Cutout chains(uses cutout tecture from Dungeon Trap Pack) and Large Wood Beam.

v1.00
	Full release