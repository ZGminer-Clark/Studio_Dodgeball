#region CompilerSettings
//#define DEBUG_TEST //uncomment if you want to debug this button
#endregion CompilerSettings
#region Usings
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
#endregion Usings
/// <summary>
/// Script from Reimajo, purchased at https://reimajo.booth.pm/, to be used in the worlds of the person who bought the asset only.
/// Join my Discord Server to receive update notifications & support for this asset: https://discord.gg/SWkNA394Mm
/// If you have any issues, please contact me on Discord or Booth or Twitter https://twitter.com/ReimajoChan
/// Do not give any of the asset files or parts of them to anyone else.
/// </summary>
namespace ReimajoBoothAssets
{
    /// <summary>
    /// Super simple button script to be used with an UI button, emulates parts of the way more complex pushable buttons from my Booth store.
    /// This script is included with all products to allow users to have a better example button for my assets if they do not own my Button & Slider asset.
    /// It may not be used in all example scenes, but it is included by default. This button can easily be switched with my pushable button.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class UiButtonController : UdonSharpBehaviour
    {
        #region SerializedFields
        /// <summary>
        /// Read-Only access to the current button state. DO NOT MODIFY.
        /// </summary>
        [HideInInspector, System.NonSerialized]
        public bool _isOn;
        /// <summary>
        /// If the button is on / green at start (can be used to set the default state in editor, 
        /// do not modify this at runtime, only use _TurnButtonOn() and _TurnButtonOff() for this)
        /// </summary>
        [SerializeField, Tooltip("If the button is on / green at start (can be used to set the default state in editor, do not modify this at runtime, only use _TurnButtonOn() and _TurnButtonOff() for this)")]
        private bool _isOnAtStart = false;
        /// <summary>
        /// Target script that receives <see cref="_buttonDownEventName"/> calls
        /// </summary>
        [SerializeField, Tooltip("(Optional) Target script that receives _buttonDownEventName calls. Can be empty.")]
        private UdonBehaviour _targetScript = null;
        /// <summary>
        /// Name of the event that is fired at <see cref="_targetScript"/> when the button is pressed down.
        /// Begin the event name with an underscore to protect this from being called on the network by malicious client users.
        /// </summary>
        [SerializeField, Tooltip("Name of the event that is fired at _targetScript when the button is pressed down. Begin the event name with an underscore to protect this from being called on the network by malicious client users.")]
        private string _buttonDownEventName = "_ButtonDownEvent";
        /// <summary>
        /// Image component of the background of the UI button.
        /// </summary>
        [SerializeField, Tooltip("Image component of the background of the UI button.")]
        private Image _buttonBackgroundImage;
        /// <summary>
        /// Background color of the UI button when it is on
        /// </summary>
        [SerializeField, Tooltip("Background color of the UI button when it is on")]
        private Color _colorButtonOn;
        /// <summary>
        /// Background color of the UI button when it is off
        /// </summary>
        [SerializeField, Tooltip("Background color of the UI button when it is off")]
        private Color _colorButtonOff;
        #endregion SerializedFields
        #region StartSetup
        /// <summary>
        /// Is called once at start to setup the button
        /// </summary>
        void Start()
        {
            _isOn = _isOnAtStart;
            UpdateVisualButtonState();
            if (_buttonDownEventName.Trim() == "")
            {
                Debug.LogError($"[UiButtonController] No button event name assigned to button script '{this.name}', will us the default name instead.");
                _buttonDownEventName = "_ButtonDownEvent";
            }
            if (_targetScript == null)
            {
                Debug.LogError($"[UiButtonController] No target script assigned to button script '{this.name}', this button will crash when being clicked in game.");
            }
        }
        #endregion StartSetup
        #region ExternalControl
        /// <summary>
        /// Call this event from your own scripts to toggle the button
        /// </summary>
        public void _ToggleButton()
        {
#if DEBUG_TEST
            Debug.Log($"[UiButtonController] '{this.name}' is TOGGLED to {(_isOn ? "ON" : "OFF")} by an external script.");
#endif
            _isOn = !_isOn;
            SendButtonDownEventToTargetScript();
            UpdateVisualButtonState();
        }
        /// <summary>
        /// Call this event from your own scripts to turn this button on.
        /// </summary>
        public void _TurnButtonOn()
        {
            if (!_isOn)
            {
#if DEBUG_TEST
                Debug.Log($"[UiButtonController] '{this.name}' is set to ON by an external script.");
#endif
                _isOn = true;
                SendButtonDownEventToTargetScript();
                UpdateVisualButtonState();
            }
#if DEBUG_TEST
            else
            {
                Debug.Log($"[UiButtonController] '{this.name}' is set to ON by an external script but is already ON, skipped event.");
            }
#endif
        }
        /// <summary>
        /// Call this event from your own scripts to turn this button off.
        /// </summary>
        public void _TurnButtonOff()
        {
            if (_isOn)
            {
#if DEBUG_TEST
                Debug.Log($"[UiButtonController] '{this.name}' is set to OFF by an external script.");
#endif
                _isOn = false;
                SendButtonDownEventToTargetScript();
                UpdateVisualButtonState();
            }
#if DEBUG_TEST
            else
            {
                Debug.Log($"[UiButtonController] '{this.name}' is set to OFF by an external script but is already OFF, skipped event.");
            }
#endif
        }
        #endregion ExternalControl
        #region InternalEvents
        /// <summary>
        /// Is called internally when the button is pressed to send the event to the receiving script
        /// </summary>
        private void SendButtonDownEventToTargetScript()
        {
#if DEBUG_TEST
            if (_targetScript != null)
                Debug.Log($"[UiButtonController] '{this.name}' sending event '{_buttonDownEventName}' to script '{_targetScript.name}'");
#endif
            if (Utilities.IsValid(_targetScript))
                _targetScript.SendCustomEvent(_buttonDownEventName);
        }
        /// <summary>
        /// Updates the visual state of the button to the current <see cref="_isOn"/> state.
        /// </summary>
        private void UpdateVisualButtonState()
        {
            if (_isOn)
            {
                _buttonBackgroundImage.color = _colorButtonOn;
            }
            else
            {
                _buttonBackgroundImage.color = _colorButtonOff;
            }
        }
        #endregion InternalEvents
    }
}