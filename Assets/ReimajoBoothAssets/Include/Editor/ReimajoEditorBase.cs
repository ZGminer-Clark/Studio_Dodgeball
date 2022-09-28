#region Usings
using UnityEngine;
using UnityEditor;
#endregion Usings
/// <summary>
/// Script from Reimajo purchased at https://reimajo.booth.pm/, to be used in the worlds of the person who bought the asset only.
/// Make sure to join my discord to receive update notifications for this asset and support: https://discord.gg/SWkNA394Mm
/// If you have any issues, please contact me on Discord (https://discord.gg/SWkNA394Mm) or Booth or Twitter https://twitter.com/ReimajoChan
/// Do not give any of the asset files or parts of them to anyone else.
/// </summary>
namespace ReimajoBoothAssetsEditorScripts
{
    //############################## Last changed on 31.05.2021 ##########################################
    /// <summary>
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// This class is included in all my assets to be used for my custom editors.
    /// You might experience missing functions if you import an outdated asset in 
    /// a project with a new asset, please always use the latest versions from my 
    /// booth page to avoid this.
    /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// </summary>
    public class ReimajoEditorBase : Editor
    {
        /// <summary>
        /// Adds the default header for Reimajo Booth Assets.
        /// </summary>
        /// <param name="productName">Name of the product in my store</param>
        /// <param name="version">Next version of this product that is yet unpublished</param>
        /// <param name="documentation">Link to the documentation of this product</param>
        /// <param name="target">Editor target</param>
        public static void AddStandardHeader(string productName, string version, string documentation, UnityEngine.Object target)
        {
            if (UdonSharpEditor.UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            EditorGUILayout.Space();

            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = new Color(0, 0.9f, 0, 0.9f);
            EditorGUILayout.LabelField($"{productName} {version}", style);
            style.normal.textColor = Color.magenta;
            EditorGUILayout.LabelField("Reimajo Booth Asset from https://reimajo.booth.pm/", style);
            style.normal.textColor = Color.grey;
            EditorGUILayout.LabelField("Join my Discord to get notified about available updates (link is in the documentation)", style);

            GUILayout.BeginHorizontal(GUIStyle.none, GUILayout.Height(25));
            if (GUILayout.Button("Open Documentation", GUILayout.Height(25f)))
            {
                Application.OpenURL(documentation);
            }
            if (GUILayout.Button("Open Webshop", GUILayout.Height(25f)))
            {
                Application.OpenURL(@"https://reimajo.booth.pm/");
            }
            GUILayout.EndHorizontal();
            DrawUILine(Color.gray);
        }
        /// <summary>
        /// Draws a UI line on a custom inspector
        /// </summary>
        /// <param name="color"></param>
        /// <param name="thickness"></param>
        /// <param name="padding"></param>
        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            rect.height = thickness;
            rect.y += padding / 2;
            rect.x -= 2;
            rect.width += 6;
            EditorGUI.DrawRect(rect, color);
            EditorGUILayout.Space();
        }
        /// <summary>
        /// Draws a text label
        /// </summary>
        public static void DrawLabelField(Color color, string text)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = color;
            EditorGUILayout.LabelField(text, style);
        }
        /// <summary>
        /// Draws a property array between two lines
        /// </summary>
        /// <param name="lastHasLineDrawn">if a line has been drawn after the last property</param>
        /// <param name="property">the array property</param>
        public static void DrawArray(bool lastHasLineDrawn, SerializedProperty property)
        {
            if (!lastHasLineDrawn)
            {
                DrawUILine(Color.gray);
            }
            if (property.arraySize > 0)
            {
                property.isExpanded = true;
            }
            EditorGUILayout.PropertyField(property, property.isExpanded);
            DrawUILine(Color.gray);
        }
        /// <summary>
        /// Add a tag to the project if this tag doesn't exist yet, returns true if the tag was newly added.
        /// </summary>
        public static bool AddTagIfNeeded(string tagName)
        {
            SerializedObject unityTagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty allExistingTags = unityTagManager.FindProperty("tags");
            //check if the tag is already present
            bool tagFound = false;
            for (int i = 0; i < allExistingTags.arraySize; i++)
            {
                SerializedProperty tag = allExistingTags.GetArrayElementAtIndex(i);
                if (tag.stringValue.Equals(tagName)) { tagFound = true; break; }
            }
            //if not, add it
            if (!tagFound)
            {
                int index = allExistingTags.arraySize + 1;
                allExistingTags.arraySize++;
                allExistingTags.InsertArrayElementAtIndex(index);
                SerializedProperty newTag = allExistingTags.GetArrayElementAtIndex(index);
                newTag.stringValue = tagName;
                //save the changes
                unityTagManager.ApplyModifiedProperties();
                Debug.Log($"[ReimajoEditorBase] The tag '{tagName}' was added to this project.");
            }
            return !tagFound;
        }
        /// <summary>
        /// Draws a property, arrays are drawn as expanded if they have values & have a line before/after.
        /// The UdonBehaviour header returns true if a line has been drawn.
        /// </summary>
        public static bool Default_HandleProperty(SerializedProperty property, bool lastHasLineDrawn)
        {
            bool isdefaultScriptProperty = property.name.Equals("m_Script") && property.type.Equals("PPtr<MonoScript>") && property.propertyType == SerializedPropertyType.ObjectReference && property.propertyPath.Equals("m_Script");
            if (isdefaultScriptProperty)
                return false;
            if (property.isArray && property.propertyType != SerializedPropertyType.String)
            {
                DrawArray(lastHasLineDrawn, property);
                return true;
            }
            else
            {
                EditorGUILayout.PropertyField(property, property.isExpanded);
                return false;
            }
        }
        /// <summary>
        /// The default way of handling OnInspectorGUI() if no adjustements are needed
        /// </summary>
        public static void Default_OnInspectorGUI(string productName, string version, string documentation, Object target, SerializedObject serializedObject)
        {
            ReimajoEditorBase.AddStandardHeader(productName, version, documentation, target);
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
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }
        /// <summary>
        /// The default way of handling other properties if no adjustements are needed
        /// </summary>
        public static void Default_HandleProperties(SerializedObject serializedObject, bool lastHasLineDrawn = false)
        {
            Color cachedGuiColor = GUI.color;
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            bool isVisible = property.NextVisible(true);
            if (isVisible)
                do
                {
                    GUI.color = cachedGuiColor;
                    lastHasLineDrawn = ReimajoEditorBase.Default_HandleProperty(property, lastHasLineDrawn);
                } while (property.NextVisible(false));
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
