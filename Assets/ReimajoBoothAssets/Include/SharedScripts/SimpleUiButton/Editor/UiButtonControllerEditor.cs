#region Usings
using UnityEngine;
using UnityEditor;
#endregion Usings
/// <summary>
/// Script from Reimajo purchased at https://reimajo.booth.pm/
/// Make sure to join my discord to receive update notifications for this asset and support: https://discord.gg/SWkNA394Mm
/// If you have any issues, please contact me on Discord (https://discord.gg/SWkNA394Mm) or Booth or Twitter https://twitter.com/ReimajoChan
/// </summary>
namespace ReimajoBoothAssetsEditorScripts
{
    #region AffectedScripts
    [CustomEditor(typeof(ReimajoBoothAssets.UiButtonController), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class UiButtonControllerEditor : Editor
    {
    #endregion AffectedScripts
        //############################################
        private const string VERSION = "V1.0";
        private const string PRODUCT_NAME = "UI Button Controller";
        private const string DOCUMENTATION = @"https://docs.google.com/document/d/1Uy7QSG-RXM_fUysAaFBcDumLKNSA32ClLPcfr_v2Evk/";
        //############################################
        #region BaseEditor
        public override void OnInspectorGUI()
        {
            ReimajoEditorBase.AddStandardHeader(PRODUCT_NAME, VERSION, DOCUMENTATION, target);
            ReimajoEditorBase.DrawLabelField(Color.green, "This example button is functionally compatible with my pushable button asset");
            ReimajoEditorBase.DrawUILine(Color.gray);
            ReimajoEditorBase.Default_HandleProperties(serializedObject, lastHasLineDrawn: true);
        }
    }
    #endregion BaseEditor
}