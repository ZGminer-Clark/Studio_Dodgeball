#define SCRIPT_VERSION
#region Usings
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using VRC.Udon;
using System.Threading.Tasks;
using System;
#endregion Usings
/// <summary>
/// Script from Reimajo purchased at https://reimajo.booth.pm/
/// Make sure to join my discord to receive update notifications for this asset and support: https://discord.gg/SWkNA394Mm 
/// If you have any issues, please contact me on Discord (https://discord.gg/SWkNA394Mm) or Booth or Twitter https://twitter.com/ReimajoChan
/// </summary>
namespace ReimajoBoothAssetsEditorScripts
{
    internal static class PlayerCalibrationEditorSettings
    {
        //############################################
        public const string VERSION = "V2.5";
        public const string PRODUCT_NAME = "Player Calibration";
        public const string DOCUMENTATION = @"https://docs.google.com/document/d/1QFgTta0UZHG97IxiKJ7s18ZCcbWaqkKFIpm7WXsqWx4/";
        //############################################
        #region OtherSettings
        internal const string TAG_NAME_PLAYER_CALIBRATION = "PlayerCalibration";
        internal readonly static string[] TAG_NAMES_CALIBRATED_OBJECTS = new string[2] { "PlayerCalibration", "ToggledChair" };
        internal const string SEND_PLAYER_HEIGHT = "SPH"; //Send Player Height
        internal const string SEND_MAIN_FINGER_BONES = "SFB"; //Send main Finger Bones
        internal const string SEND_AVATAR_CHANGED = "SAC"; //Send Avatar Changed
        internal const string ADJUST_OBJECT_POSITION = "AOP"; //Adjust Object Position
        internal const bool AUTO_UPDATE = true;
        #endregion OtherSettings
    }
    #region EditorFunctions
    /// <summary>
    /// Editor functions to auto-assign the components to the PlayerCalibration-Script that are tagged with {TAG_NAME}
    /// </summary>
    public class PlayerCalibrationEditorFunctions : ScriptableObject
    {
        private const string TAG_NAME_PLAYER_CALIBRATION = PlayerCalibrationEditorSettings.TAG_NAME_PLAYER_CALIBRATION;
        private readonly static string[] TAG_NAMES_CALIBRATED_OBJECTS = PlayerCalibrationEditorSettings.TAG_NAMES_CALIBRATED_OBJECTS;
        private const string SEND_PLAYER_HEIGHT = PlayerCalibrationEditorSettings.SEND_PLAYER_HEIGHT;
        private const string SEND_MAIN_FINGER_BONES = PlayerCalibrationEditorSettings.SEND_MAIN_FINGER_BONES;
        private const string SEND_AVATAR_CHANGED = PlayerCalibrationEditorSettings.SEND_AVATAR_CHANGED;
        private const string ADJUST_OBJECT_POSITION = PlayerCalibrationEditorSettings.ADJUST_OBJECT_POSITION;
        private const bool AUTO_UPDATE = PlayerCalibrationEditorSettings.AUTO_UPDATE;
        #region SetupProject
        public static void SetupProject()
        {
            if (ReimajoEditorBase.AddTagIfNeeded(tagName: TAG_NAME_PLAYER_CALIBRATION))
                Debug.Log("[PlayerCalibration] Project is now setup for the PlayerCalibration-script.");
            if (AUTO_UPDATE)
                UpdateAfterDelay(30);
        }
        public async static void UpdateAfterDelay(float seconds)
        {
            await Task.Delay((int)(1000 * seconds)); //update the project after 30 seconds to ensure the scene has already loaded.
            UpdateProject();
        }
        #endregion SetupProject
        #region UpdateProject
        [MenuItem("Reimajo/PlayerCalibration/AutoAssignUpdate")]
        public static void UpdateProject()
        {
            List<GameObject> objectsWithTag = new List<GameObject>();
            GameObject[] objectsWithThatOneTag = null;
            foreach (string tag in TAG_NAMES_CALIBRATED_OBJECTS)
            {
                try
                {
                    objectsWithThatOneTag = GameObject.FindGameObjectsWithTag(tag);
                }
                catch
                {
                    Debug.Log($"[PlayerCalibration] Skipped tag '{tag}' since it isn't used in the project.");
                    continue;
                }
                if (objectsWithThatOneTag.Length > 0)
                {
                    Debug.Log($"[PlayerCalibration] Found {objectsWithThatOneTag.Length} GameObject{(objectsWithThatOneTag.Length == 1 ? "s" : "")} that {(objectsWithThatOneTag.Length == 1 ? "has" : "have")} the tag '{tag}' assigned");
                    objectsWithTag.AddRange(objectsWithThatOneTag);
                }
            }
            if (objectsWithTag.Count == 0)
            {
                Debug.Log($"[PlayerCalibration] Found no GameObject that has the tag '{TAG_NAME_PLAYER_CALIBRATION}' assigned.");
                return;
            }
            /// We send the current player height to the Udon Behaviours on those GameObjects
            List<GameObject> sendPlayerHeight = new List<GameObject>();
            /// We send the current main finger bones to the Udon Behaviours on those GameObjects
            List<GameObject> sendPlayerMainFingerBones = new List<GameObject>();
            /// We send the OnAvatarChanged() event to the Udon Behaviours on those GameObjects
            List<GameObject> sendAvatarChanged = new List<GameObject>();
            /// We adjust object positions on those game objects
            List<Transform> adjustObjectsPosition = new List<Transform>();

            bool playerCalibrationScriptFound = false;
            ReimajoBoothAssets.PlayerCalibration playerCalibrationScript = null;

            foreach (GameObject obj in objectsWithTag)
            {
                string objName = obj.name;
                if (objName == "PlayerCalibrationScript")
                {
                    playerCalibrationScript = obj.GetComponent<ReimajoBoothAssets.PlayerCalibration>();
                    if (playerCalibrationScript == null)
                    {
                        Debug.LogError($"[PlayerCalibration] Found a tagged object named {objName} but no PlayerCalibration script was found on it.");
                        continue;
                    }
                    if (playerCalibrationScriptFound)
                    {
                        Debug.LogError($"[PlayerCalibration] Found more then one tagged object named {objName}, but there should only be one instance of this script in your scene! Will only use the last one. You should remove one of them.");
                    }
                    playerCalibrationScriptFound = true;
                    continue;
                }
                string[] nameFields = objName.Split('_');
                foreach (string field in nameFields)
                {
                    switch (field.ToUpper())
                    {
                        case SEND_PLAYER_HEIGHT: //Send Player Height
                            {
                                UdonBehaviour udonBehaviour = (UdonBehaviour)obj.GetComponent(typeof(UdonBehaviour));
                                if (udonBehaviour == null)
                                {
                                    Debug.LogError($"[PlayerCalibration] Found a tagged object named {objName} with the field {field} in the name but no UdonBehaviour was found on it.");
                                    continue;
                                }
                                sendPlayerHeight.Add(obj);
                            }
                            break;
                        case SEND_MAIN_FINGER_BONES: //Send main Finger Bones
                            {
                                UdonBehaviour udonBehaviour = (UdonBehaviour)obj.GetComponent(typeof(UdonBehaviour));
                                if (udonBehaviour == null)
                                {
                                    Debug.LogError($"[PlayerCalibration] Found a tagged object named {objName} with the field {field} in the name but no UdonBehaviour was found on it.");
                                    continue;
                                }
                                sendPlayerMainFingerBones.Add(obj);
                            }
                            break;
                        case SEND_AVATAR_CHANGED: //Send Avatar Changed
                            {
                                UdonBehaviour udonBehaviour = (UdonBehaviour)obj.GetComponent(typeof(UdonBehaviour));
                                if (udonBehaviour == null)
                                {
                                    Debug.LogError($"[PlayerCalibration] Found a tagged object named {objName} with the field {field} in the name but no UdonBehaviour was found on it.");
                                    continue;
                                }
                                sendAvatarChanged.Add(obj);
                            }
                            break;
                        case ADJUST_OBJECT_POSITION: //Adjust Object Position
                            {
                                adjustObjectsPosition.Add(obj.transform);
                            }
                            break;
                    }
                }
            }
            if (sendPlayerHeight.Count != 0)
                Debug.Log($"[PlayerCalibration] Found {sendPlayerHeight.Count} GameObject{(sendPlayerHeight.Count == 1 ? "s" : "")} with the tag 'SPH'");
            if (sendPlayerMainFingerBones.Count != 0)
                Debug.Log($"[PlayerCalibration] Found {sendPlayerMainFingerBones.Count} GameObject{(sendPlayerMainFingerBones.Count == 1 ? "s" : "")} with the tag 'SFB'");
            if (sendAvatarChanged.Count != 0)
                Debug.Log($"[PlayerCalibration] Found {sendAvatarChanged.Count} GameObject{(sendAvatarChanged.Count == 1 ? "s" : "")} with the tag 'SAC'");
            if (adjustObjectsPosition.Count != 0)
                Debug.Log($"[PlayerCalibration] Found {adjustObjectsPosition.Count} GameObject{(adjustObjectsPosition.Count == 1 ? "s" : "")} with the tag 'AOP'");

            int count = sendPlayerHeight.Count + sendPlayerMainFingerBones.Count + sendAvatarChanged.Count + adjustObjectsPosition.Count;
            if (playerCalibrationScript != null)
            {
                try
                {
                    playerCalibrationScript.SetProgramVariable("_sendPlayerHeight", sendPlayerHeight.ToArray());
                    playerCalibrationScript.SetProgramVariable("_sendPlayerMainFingerBones", sendPlayerMainFingerBones.ToArray());
                    playerCalibrationScript.SetProgramVariable("_sendAvatarChanged", sendAvatarChanged.ToArray());
                    playerCalibrationScript.SetProgramVariable("_adjustObjectsPosition", adjustObjectsPosition.ToArray());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PlayerCalibration] Couldn't set all values on PlayerCalibrationScript's Udon variables: {ex.Message}");
                }
                if (count == 0)
                {
                    Debug.Log($"[PlayerCalibration] <color=yellow>No object found in the scene that has the tag '{TAG_NAME_PLAYER_CALIBRATION}' set and at least one of the options in it's name (_{SEND_PLAYER_HEIGHT}; _{SEND_MAIN_FINGER_BONES}; _{SEND_AVATAR_CHANGED}; _{ADJUST_OBJECT_POSITION})</color>");
                }
                else
                {
                    Debug.Log($"[PlayerCalibration] <color=green>Successfully set { (count == 1 ? "1 value" : $"all {count} values")} on PlayerCalibrationScript's Udon variables</color>");
                }
            }
            else
            {
                Debug.LogError($"[PlayerCalibration] The Scene doesn't have a 'PlayerCalibrationScript' object with the PlayerCalibration-Script assigned to it and the tag '{TAG_NAME_PLAYER_CALIBRATION}' set to it.");
            }
        }
        #endregion UpdateProject
    }
    #endregion EditorFunctions
    #region AutoStart
    /// <summary>
    /// We use this class to setup the project at start automatically 
    /// See https://docs.unity3d.com/Manual/RunningEditorCodeOnLaunch.html
    /// </summary>
    [InitializeOnLoad]
    public class StartupPlayerCalibration
    {
        /// <summary>
        /// This is called when the Editor is started and ensures that the project has the needed tag
        /// </summary>
        static StartupPlayerCalibration()
        {
            //don't run this in editor play mode
            if (EditorApplication.isPlaying)
                return;
            //adding a delay to ensure that the project is fully loaded
            UpdateAfterDelay(20);
        }
        public async static void UpdateAfterDelay(float seconds)
        {
            //update the project after x seconds to ensure the scene has already loaded.
            await Task.Delay((int)(1000 * seconds));
            //don't run this in editor play mode
            if (EditorApplication.isPlaying)
                return;
            PlayerCalibrationEditorFunctions.SetupProject();
        }
    }
    #endregion AutoStart
    #region CustomEditor
#if SCRIPT_VERSION
    [CustomEditor(typeof(ReimajoBoothAssets.PlayerCalibration), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class CustomEditorPlayerCalibration : Editor
    {
        private const string VERSION = PlayerCalibrationEditorSettings.VERSION;
        private const string PRODUCT_NAME = PlayerCalibrationEditorSettings.PRODUCT_NAME;
        private const string DOCUMENTATION = PlayerCalibrationEditorSettings.DOCUMENTATION;
        private const string TAG_NAME = PlayerCalibrationEditorSettings.TAG_NAME_PLAYER_CALIBRATION;
        private const string SEND_PLAYER_HEIGHT = PlayerCalibrationEditorSettings.SEND_PLAYER_HEIGHT;
        private const string SEND_MAIN_FINGER_BONES = PlayerCalibrationEditorSettings.SEND_MAIN_FINGER_BONES;
        private const string SEND_AVATAR_CHANGED = PlayerCalibrationEditorSettings.SEND_AVATAR_CHANGED;
        private const string ADJUST_OBJECT_POSITION = PlayerCalibrationEditorSettings.ADJUST_OBJECT_POSITION;
        private const string INFO_TEXT = "Assign the object tag '" + TAG_NAME + "' to a gameObject and then\n" +
            "put at least one of the following options in it's name:\n\n" +
            "- " + SEND_PLAYER_HEIGHT + " to send the player height to this UdonBehaviour\n" +
            "- " + SEND_MAIN_FINGER_BONES + " to send the current main finger bones to this UdonBehaviour\n" +
            "- " + ADJUST_OBJECT_POSITION + " to adjust this object's height to the player\n" +
            "- " + SEND_AVATAR_CHANGED + " to send the _OnAvatarChanged() event to this UdonBehaviour\n\n" +
            "Example object name: Piano_" + SEND_PLAYER_HEIGHT + "_" + SEND_MAIN_FINGER_BONES + "_" + SEND_AVATAR_CHANGED +
            "\n(an underscore before/after each option is required).\n\n" +
            "Afterwards, press the update button above to auto-assign all those\nobjects to this script here. " +
            "The script will also auto-assign all\nobjects each time you load the scene.";
        public override void OnInspectorGUI()
        {
            ReimajoEditorBase.AddStandardHeader(PRODUCT_NAME, VERSION, DOCUMENTATION, target);
            //auto-update button section
            GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(25));
            if (GUILayout.Button("Auto Assign Update", GUILayout.Height(25f)))
            {
                //I have no idea why this works, but it definitely doesn't work without that delay for unknown reasons
                PlayerCalibrationEditorFunctions.UpdateAfterDelay(0.5f);
            }
            GUILayout.EndHorizontal();
            ReimajoEditorBase.DrawUILine(Color.gray);
            //tips and settings information section
            EditorGUILayout.LabelField(INFO_TEXT, GUILayout.Height(230f), GUILayout.ExpandWidth(true));
            Color cachedGuiColor = GUI.color;
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            bool lastHasLineDrawn = false;
            bool isVisible = property.NextVisible(true);
            if (isVisible)
                do
                {
                    GUI.color = cachedGuiColor;
                    lastHasLineDrawn = ReimajoEditorBase.Default_HandleProperty(property, lastHasLineDrawn);
                } while (property.NextVisible(false));
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion CustomEditor
}