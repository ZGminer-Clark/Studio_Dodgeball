The UI Button Controller was created by Reimajo and is sold at https://reimajo.booth.pm/
Please don't share or redistribute these asset files in any way.
Only use them within your own VRChat worlds after you paid for them.

Make sure to join my discord to receive update notifications, additional information 
and help for this asset: https://discord.gg/SWkNA394Mm

#####################################################################################################
##  There is a better online version of this documentation which might have new information:
##  https://docs.google.com/document/d/1Uy7QSG-RXM_fUysAaFBcDumLKNSA32ClLPcfr_v2Evk/
#####################################################################################################

Before you import this asset:

Watch this video tutorial first: https://www.youtube.com/watch?v=OqEmOEOdMp8
This will explain how to set everything up.

If you have any issues, please contact me on Discord (https://discord.gg/SWkNA394Mm) or Booth or Twitter https://twitter.com/ReimajoChan

There is a sample scene included which shows the general setup. 
Simply copy all objects (except the “World” object) from the scene into your own scene as shown in the tutorial video.

The code documentation can be found by hovering with the cursor above a field in the Inspector in case you bought the script version and inside the script itself.

-------------------------------------------------------------------------------------------------------------------

The UI Button Controller is included in all my assets, even if it is not part of the asset itself. 
The purpose of it is to provide a simple drop-in replacement of my pushable Buttons from my 
Button & Slider asset (https://reimajo.booth.pm/items/2906380) which is functionally compatible, 
so users who own my pushable Buttons can easily replace the UI Button with them because they have 
the same internal events and variables. It’s included in all my assets simply because of the way I 
export asset packages, not because it is necessarily needed in all of them.

-------------------------------------------------------------------------------------------------------------------

Please make sure you have the newest SDK3-Worlds (https://vrchat.com/home/download) and UdonSharp (https://github.com/Merlin-san/UdonSharp/releases/latest) imported into your project if you use UdonSharp. In case you need to update your SDK or UdonSharp, please follow these steps:

0. Enter Playmode in each scene. If there are compile errors, remove the scripts that have an issue first.

1. Close the scene (e.g. by opening a new empty scene instead) and then close Unity (and Visual Studio if you have it open)

2. Backup your whole Unity Project folder, e.g. by zipping it

3. Delete the following files in "Assets":
```
VRCSDK.meta
VRChat Examples.meta
Udon.meta
UdonSharp.meta
```

4. Delete those folders in "Assets":
```
VRCSDK
VRChat Examples
Udon
UdonSharp
```

5. Open the project in Unity, ignore the console errors, DON'T open your world scene

6. Import newest VRCSDK3 for worlds (https://vrchat.com/home/download)

7. Import newest UdonSharp package (https://github.com/Merlin-san/UdonSharp/releases/latest)

8. Enter playmode in each of your world scenes now (!)