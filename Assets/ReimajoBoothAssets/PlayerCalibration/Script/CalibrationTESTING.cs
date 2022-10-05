#region CompilerSettings
//#define DEBUG_TEST //leave this commented out if you don't want to debug this script
#define DEBUG_PRINTS //comment out if you don't want to see non-error debug prints from this script
#endregion CompilerSettings
#region Usings
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#endregion Usings
/// <summary>
/// Script from Reimajo purchased at https://reimajo.booth.pm/
/// Make sure to join my discord to receive update notifications for this asset and support: https://discord.gg/SWkNA394Mm
/// If you have any issues, please contact me on Discord (https://discord.gg/SWkNA394Mm) or Booth or Twitter https://twitter.com/ReimajoChan
/// </summary>
namespace ReimajoBoothAssets
{
    /// <summary>
    /// Script that measures the current player height in intervalls to detect avatar changes and sends 
    /// this value to all other UdonBehaviours in the list as well as sending them the current main finger 
    /// bones that are available on that new avatar. Afterwards, the event OnAvatarChanged() is sent to 
    /// all scripts in the list to inform them about the change and allow more complex calibrations when needed.
    /// The event calls are queued, so that this script doesn't cause noticeable CPU spikes.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class PlayerCalibration : UdonSharpBehaviour
    {
        #region SerializedFields
        /// <summary>
        /// We send the current player height to the Udon Behaviours on those GameObjects
        /// </summary>
        public GameObject[] _sendPlayerHeight = new GameObject[0];
        /// <summary>
        /// We send the current main finger bones to the Udon Behaviours on those GameObjects
        /// </summary>
        public GameObject[] _sendPlayerMainFingerBones = new GameObject[0];
        /// <summary>
        /// We send the OnAvatarChanged() event to the Udon Behaviours on those GameObjects
        /// </summary>
        public GameObject[] _sendAvatarChanged = new GameObject[0];
        /// <summary>
        /// We adjust object positions on those game objects
        /// </summary>
        public Transform[] _adjustObjectsPosition = new Transform[0];
        #endregion SerializedFields
        #region Settings
        /// <summary>
        /// We assume that the height of an object should be the result of the playerHeight multiplied by this value
        /// </summary>
        private const float ASSUMED_AVATAR_HEIGHT_OFFSET = 0.9f;
        /// <summary>
        /// We assume that loading the world takes this long until we should run expensive code for the first time
        /// </summary>
        private const float ASSUMED_LEVEL_LOADING_TIME = 3f;
        /// <summary>
        /// This is the standard update intervall in seconds at which we check if the avatar has changed
        /// </summary>
        private const float LONG_UPDATE_INTERVALL = 10f;
        /// <summary>
        /// This is a shorter update intervall in which we check if the avatar finished loading after the world has loaded
        /// </summary>
        private const float START_UPDATE_INTERVALL = 1f;
        /// <summary>
        /// Name of the child under the same parent like the _AOP object which defines the minimal height position of this object
        /// </summary>
        private const string MIN_POS_CHILD_NAME = "AOP_MIN";
        /// <summary>
        /// Name of the child under the same parent like the _AOP object which defines the maximal height position of this object
        /// </summary>
        private const string MAX_POS_CHILD_NAME = "AOP_MAX";
        /// <summary>
        /// If the avatar size changed only by this amount, we assume the avatar hasn't changed.
        /// </summary>
        private const float PLAYER_HEIGHT_ERROR = 0.05f;
        /// <summary>
        /// The maximum vertical height of a calibrated gameObject, only used when no <see cref="MAX_POS_CHILD_NAME"/> is found
        /// </summary>
        private const float DEFAULT_MAX_VERTICAL_POSITION = 2.5f;
        /// <summary>
        /// The minimal vertical height of a calibrated gameObject, only used when no <see cref="MIN_POS_CHILD_NAME"/> is found
        /// </summary>
        private const float DEFAULT_MIN_VERTICAL_POSITION = 0.5f;
        /// <summary>
        /// Default floor height of the world, only used when no <see cref="MAX_POS_CHILD_NAME"/> or <see cref="MIN_POS_CHILD_NAME"/> 
        /// or parent object is found
        /// </summary>
        private const float DEFAULT_FLOOR_HEIGHT = 0f;
        /// <summary>
        /// The smallest avatar height that we would send to other scripts.
        /// Setting this to 0 might crash your script for very small avatars because of ASSUMED_AVATAR_HEIGHT_OFFSET
        /// </summary>
        private const float MIN_AVATAR_HEIGHT = 0.01f;
        /// <summary>
        /// For how many seconds the short update intervall after the first update is kept until switching to the long update intervall
        /// </summary>
        private const float SHORT_UPDATE_INTERVALL_DURATION = 60f;
        /// <summary>
        /// Assumed "standard height" of an avatar if the measurement failed because required bones are missing
        /// </summary>
        private const float ASSUMED_AVATAR_STANDARD_HEIGHT = 1.3f;
        #endregion Settings
        #region PrivateFields
        private float _currentUpdateIntervall = START_UPDATE_INTERVALL;
        private float[] _minPosHeight;
        private float[] _maxPosHeight;
        private float[] _floorHeight;
        private bool _firstTime = true;
        private float _firstUpdateTime;
        private int _currentUpdateIndex = 0;
        private VRCPlayerApi _localPlayer;
        private HumanBodyBones _leftIndexBone = HumanBodyBones.LeftIndexDistal;
        private HumanBodyBones _rightIndexBone = HumanBodyBones.RightIndexDistal;
        private HumanBodyBones _leftIndexBoneTransmitted = HumanBodyBones.Head; //forcing the script to transmit the first value
        private HumanBodyBones _rightIndexBoneTransmitted = HumanBodyBones.Head; //forcing the script to transmit the first value
        private float _lastTransmittedPlayerHeight = float.MaxValue; //forcing the script to transmit the first value
        private float _currentPlayerHeight;
        private float _currentObjectHeight;
        #endregion PrivateFields
        #region Start
        private void Start()
        {
#if DEBUG_TEST
            Debug.Log($"[PlayerHeightDetector] is now in Start() with update intervall {_currentUpdateIntervall}");
#endif
            _localPlayer = Networking.LocalPlayer;
            if (_localPlayer == null)
            {
                //making sure this script doesn't run in editor where it would just crash
                this.gameObject.SetActive(false);
                return;
            }
            int objectCount = _adjustObjectsPosition.Length;
            //initialize the arrays where we store the min/max positions in
            _minPosHeight = new float[objectCount];
            _maxPosHeight = new float[objectCount];
            _floorHeight = new float[objectCount];
            //since we've assigned with .Length, the first index is objectCount - 1
            _currentUpdateIndex = objectCount - 1;
#if DEBUG_TEST
            Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_ReadNextMinMaxPositions)}");
#endif
            //now reading the min/max object positions frame by frame
            SendCustomEventDelayedFrames(nameof(_ReadNextMinMaxPositions), 1);
        }
        /// <summary>
        /// Reads the min/max AOP positions for each object frame-by-frame.
        /// When finished, <see cref="_NextUpdate"/> is called the next frame.
        /// </summary>
        public void _ReadNextMinMaxPositions()
        {
            //index 0 is the lowest index, below that means we've reached the end already
            if (_currentUpdateIndex >= 0)
            {
                Transform objToAdjust = _adjustObjectsPosition[_currentUpdateIndex];
                if (Utilities.IsValid(objToAdjust))
                {
                    Transform parent = objToAdjust.parent;
                    if (parent != null)
                    {
                        Transform minPos = GetChildWithName(parent, MIN_POS_CHILD_NAME);
                        if (minPos == null)
                        {
#if DEBUG_TEST
                            Debug.Log($"[PlayerHeightDetector] No AOP_MIN specified for object number {_currentUpdateIndex}, using default values instead.");
#endif
                            AssignDefaultAOPValuesToCurrentUpdateIndex();
                        }
                        else
                        {
                            //assign the minimal height 
                            Transform maxPos = GetChildWithName(parent, MAX_POS_CHILD_NAME);
                            if (maxPos == null)
                            {
#if DEBUG_TEST
                                Debug.Log($"[PlayerHeightDetector] No AOP_MAX specified for object number {_currentUpdateIndex}, using default values instead.");
#endif
                                AssignDefaultAOPValuesToCurrentUpdateIndex();
                            }
                            else
                            {
                                _minPosHeight[_currentUpdateIndex] = minPos.position.y;
                                _maxPosHeight[_currentUpdateIndex] = maxPos.position.y;
                                _floorHeight[_currentUpdateIndex] = parent.position.y;
                            }
                        }
                    }
                    else
                    {
#if DEBUG_TEST
                        Debug.Log($"[PlayerHeightDetector] No parent available for nr. {_currentUpdateIndex}, taking default values instead.");
#endif
                        AssignDefaultAOPValuesToCurrentUpdateIndex();
                    }
                }
                _currentUpdateIndex--;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_ReadNextMinMaxPositions)}");
#endif
                SendCustomEventDelayedFrames(nameof(_ReadNextMinMaxPositions), 1);
            }
            else
            {
                //store the time at which the first update will occur
                _firstUpdateTime = Time.time + ASSUMED_LEVEL_LOADING_TIME;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_NextUpdate)} in {ASSUMED_LEVEL_LOADING_TIME} seconds");
#endif
                //don't run next update while we assume the level is still loading since this would otherwise lag the player even more
                SendCustomEventDelayedSeconds(nameof(_NextUpdate), ASSUMED_LEVEL_LOADING_TIME);
            }
        }
        /// <summary>
        /// Assigning the default values for the <see cref="_currentUpdateIndex"/>
        /// since either parent, AOP_MIN or AOP_MAX is missing
        /// </summary>
        private void AssignDefaultAOPValuesToCurrentUpdateIndex()
        {
            _floorHeight[_currentUpdateIndex] = DEFAULT_FLOOR_HEIGHT;
            _minPosHeight[_currentUpdateIndex] = DEFAULT_MIN_VERTICAL_POSITION;
            _maxPosHeight[_currentUpdateIndex] = DEFAULT_MAX_VERTICAL_POSITION;
        }
        /*
         * Currently not supported, but "under review" https://vrchat.canny.io/vrchat-udon-closed-alpha-bugs/p/udonbehaviour-array-type-is-not-defined
         * 
        /// <summary>
        /// Reading all UdonBehaviours from the gameObjects array and storing it in the udonBehaviours array
        /// </summary>
        private UdonBehaviour[] ReadUdonBehavioursFromObjects(GameObject[] gameObjects)
        {
            if (gameObjects != null)
            {
                int lenght = gameObjects.Length;
                UdonBehaviour[] udonBehaviours = new UdonBehaviour[lenght];
                for (int i = 0; i < lenght; i++)
                {
                    UdonBehaviour script = (UdonBehaviour)gameObjects[i].GetComponent(typeof(UdonBehaviour));
                    if (script == null)
                        Debug.LogError($"[PlayerHeightDetector] No script found on {gameObjects[i].name}");
                    else
                        udonBehaviours[i] = script;
                }
                return udonBehaviours;
            }
            else
            {
                Debug.LogError($"[PlayerHeightDetector] GameObjects array was null, this should never happen.");
                return null;
            }
        }
        */
        #endregion Start
        #region Update 
        /// <summary>
        /// Is first called after the setup completed and <see cref="ASSUMED_LEVEL_LOADING_TIME"/> has passed.
        /// </summary>
        public void _NextUpdate()
        {
            //measure current player height and clamp it
            _currentPlayerHeight = Mathf.Max(MIN_AVATAR_HEIGHT, MeasurePlayerHeight(_localPlayer));
            //check if value has actually changed because we don't want to loop over all objects for nothing
            if (Mathf.Abs(_lastTransmittedPlayerHeight - _currentPlayerHeight) > PLAYER_HEIGHT_ERROR)
            {
                _lastTransmittedPlayerHeight = _currentPlayerHeight;
#if DEBUG_PRINTS
                Debug.Log($"[PlayerHeightDetector] Avatar has a new height: {_currentPlayerHeight}m, queuing _adjustObjectsPosition");
#endif
                //this counts down
                _currentUpdateIndex = _adjustObjectsPosition.Length - 1;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_AdjustNextObjectHeight)}");
#endif
                _currentObjectHeight = _currentPlayerHeight * ASSUMED_AVATAR_HEIGHT_OFFSET;
                //this splits _AdjustNextObjectHeight() over several frames
                SendCustomEventDelayedFrames(nameof(_AdjustNextObjectHeight), 1);
                return;
            }
            else
            {
                //we don't need to adjust the height, but need to check if the fingerbones changed
                if (!SendFingerBonesIfNeeded())
                {
                    //nothing has changed
                    QueueNextUpdate();
                }
            }
        }
        /// <summary>
        /// Determines if fingerbones changed, if yes, _SendNextMainFingerBone() calls are called next
        /// and it returnes true, else it returns false
        /// </summary>
        private bool SendFingerBonesIfNeeded()
        {
            _leftIndexBone = GetLeftHandIndexBone();
            _rightIndexBone = GetRightHandIndexBone();
            if (_leftIndexBone != _leftIndexBoneTransmitted || _rightIndexBone != _rightIndexBoneTransmitted)
            {
                _leftIndexBoneTransmitted = _leftIndexBone;
                _rightIndexBoneTransmitted = _rightIndexBone;
                //this counts down
                _currentUpdateIndex = _sendPlayerMainFingerBones.Length - 1;
#if DEBUG_PRINTS
                Debug.Log($"[PlayerHeightDetector] Queuing _sendPlayerMainFingerBones, new index bones are '{_rightIndexBone}' (Right) and '{_leftIndexBone}' (Left)");
#endif
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendNextMainFingerBone)}");
#endif
                //this splits _SendNextMainFingerBone() over several frames
                SendCustomEventDelayedFrames(nameof(_SendNextMainFingerBone), 1);
                return true;
            }
            else
            {
                return false;
            }
        }
        //----------------------------------------------------- 1/4 -------------------------------------------------
        /// <summary>
        /// Adjusts the height of all objets to the new measured avatar height + ASSUMED_AVATAR_HEIGHT_OFFSET
        /// </summary>
        public void _AdjustNextObjectHeight()
        {
            //index 0 is the lowest index, below that means we've reached the end already
            if (_currentUpdateIndex >= 0)
            {
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Adjusting object {_currentUpdateIndex}");
#endif
                Transform objToAdjust = _adjustObjectsPosition[_currentUpdateIndex];
                if (Utilities.IsValid(objToAdjust))
                {
                    float newObjectHeight = Mathf.Clamp(_currentObjectHeight + _floorHeight[_currentUpdateIndex],
                        _minPosHeight[_currentUpdateIndex], _maxPosHeight[_currentUpdateIndex]);
                    Vector3 currentPosition = objToAdjust.position;
                    currentPosition.y = newObjectHeight;
                    objToAdjust.position = currentPosition;
                }
                _currentUpdateIndex--;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_AdjustNextObjectHeight)}");
#endif
                SendCustomEventDelayedFrames(nameof(_AdjustNextObjectHeight), 1);
            }
            else
            {
#if DEBUG_PRINTS
                Debug.Log("[PlayerHeightDetector] Queueing _sendPlayerHeight");
#endif
                //this counts down
                _currentUpdateIndex = _sendPlayerHeight.Length - 1;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendNextAvatarHeight)}");
#endif
                //this splits _SendNextAvatarHeight() over several frames which is nessasary since we have no control 
                //over other scripts and their execution times and we don't want to cause frametime spikes
                SendCustomEventDelayedFrames(nameof(_SendNextAvatarHeight), 1);
            }
        }
        //----------------------------------------------------- 2/4 -------------------------------------------------
        /// <summary>
        /// Sends the current avatar height to the next script in the queue
        /// </summary>
        public void _SendNextAvatarHeight()
        {
            //index 0 is the lowest index, below that means we've reached the end already
            if (_currentUpdateIndex >= 0)
            {
                GameObject scriptObj = _sendPlayerHeight[_currentUpdateIndex];
                if (Utilities.IsValid(scriptObj))
                {
                    UdonBehaviour script = (UdonBehaviour)scriptObj.GetComponent(typeof(UdonBehaviour));
                    if (script != null)
                    {
                        script.SetProgramVariable("_avatarHeight", _currentPlayerHeight);
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHeightDetector] There is no UdonBehaviour attached to '{scriptObj.name}'");
                    }
                }
                _currentUpdateIndex--;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendNextAvatarHeight)}");
#endif
                SendCustomEventDelayedFrames(nameof(_SendNextAvatarHeight), 1);
            }
            else
            {
                //we need to check if the fingerbones changed. This function will return true if those are needed and queued
                if (!SendFingerBonesIfNeeded())
                {
                    //fingerbones didn't change, so directly proceed with AvatarChanged() calls (since the height changed)
#if DEBUG_PRINTS
                    Debug.Log("[PlayerHeightDetector] Queueing _sendAvatarChanged");
#endif
                    //this counts down
                    _currentUpdateIndex = _sendAvatarChanged.Length - 1;
#if DEBUG_TEST
                    Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendAvatarChangedToNextScript)}");
#endif
                    //this splits OnAvatarChanged() over several frames which is nessasary since we have no control 
                    //over other scripts and their execution times and we don't want to cause frametime spikes
                    SendCustomEventDelayedFrames(nameof(_SendAvatarChangedToNextScript), 1);
                }
            }
        }
        //----------------------------------------------------- 3/4 -------------------------------------------------
        /// <summary>
        /// Sends the main finger bones to the next script in the queue
        /// </summary>
        public void _SendNextMainFingerBone()
        {
            //index 0 is the lowest index, below that means we've reached the end already
            if (_currentUpdateIndex >= 0)
            {
                GameObject scriptObj = _sendPlayerMainFingerBones[_currentUpdateIndex];
                if (Utilities.IsValid(scriptObj))
                {
                    UdonBehaviour script = (UdonBehaviour)scriptObj.GetComponent(typeof(UdonBehaviour));
                    if (script != null)
                    {
                        script.SetProgramVariable("_rightIndexBone", _rightIndexBone);
                        script.SetProgramVariable("_leftIndexBone", _leftIndexBone);
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHeightDetector] There is no UdonBehaviour attached to '{scriptObj.name}'");
                    }
                }
                _currentUpdateIndex--;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendNextMainFingerBone)}");
#endif
                SendCustomEventDelayedFrames(nameof(_SendNextMainFingerBone), 1);
            }
            else
            {
#if DEBUG_PRINTS
                Debug.Log("[PlayerHeightDetector] Queueing _sendAvatarChanged");
#endif
                //this counts down
                _currentUpdateIndex = _sendAvatarChanged.Length - 1;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendAvatarChangedToNextScript)}");
#endif
                //this splits OnAvatarChanged() over several frames which is nessasary since we have no control 
                //over other scripts and their execution times and we don't want to cause frametime spikes
                SendCustomEventDelayedFrames(nameof(_SendAvatarChangedToNextScript), 1);
            }
        }
        //----------------------------------------------------- 4/4 -------------------------------------------------
        /// <summary>
        /// Sends the OnAvatarChanged event to the next script in the queue
        /// </summary>
        public void _SendAvatarChangedToNextScript()
        {
            //index 0 is the lowest index, below that means we've reached the end already
            if (_currentUpdateIndex >= 0)
            {
                GameObject objWithScript = _sendAvatarChanged[_currentUpdateIndex];
                if (Utilities.IsValid(objWithScript))
                {
                    UdonBehaviour script = (UdonBehaviour)objWithScript.GetComponent(typeof(UdonBehaviour));
                    if (script != null)
                    {
                        script.SendCustomEvent("_OnAvatarChanged");
                    }
                    else
                    {
                        Debug.LogError($"[PlayerHeightDetector] There is no UdonBehaviour attached to '{objWithScript.name}'");
                    }
                }
                //Slightly more efficient to count down so we don't have to keep track of the length... And efficiency is all that matters!
                _currentUpdateIndex--;
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_SendAvatarChangedToNextScript)}");
#endif
                SendCustomEventDelayedFrames(nameof(_SendAvatarChangedToNextScript), 1);
            }
            else
            {
#if DEBUG_PRINTS
                Debug.Log("[PlayerHeightDetector] Finished updating all components.");
#endif
                //we finished sending _OnAvatarChanged() calls and can now queue the next update
                QueueNextUpdate();
            }
        }
        //----------------------------------------------------- End -------------------------------------------------
        /// <summary>
        /// Queue the next update which occurs in <see cref="_currentUpdateIntervall"/> seconds
        /// </summary>
        private void QueueNextUpdate()
        {
            //we accept overhead for the first 60 seconds until we reduce the update limit
            if (_firstTime)
            {
                if (Time.time - _firstUpdateTime > SHORT_UPDATE_INTERVALL_DURATION)
                {
#if DEBUG_TEST
                    Debug.Log($"[PlayerHeightDetector] Now changing update intervall to {LONG_UPDATE_INTERVALL} seconds");
#endif
                    _currentUpdateIntervall = LONG_UPDATE_INTERVALL;
                    _firstTime = false; //for the first time we don't want to wait LONG_UPDATE_INTERVALL seconds, but afterwards it is fine
                }
            }
#if DEBUG_TEST
            Debug.Log($"[PlayerHeightDetector] Next calling {nameof(_NextUpdate)} in {_currentUpdateIntervall} seconds");
#endif
            SendCustomEventDelayedSeconds(nameof(_NextUpdate), _currentUpdateIntervall);
        }
        #endregion Update
        #region Measurement
        /// <summary>
        /// Estimates the player height according to certain bone measurements (foot-to-head bone chain)
        /// </summary>
        private float MeasurePlayerHeight(VRCPlayerApi player)
        {
            //reading player bone positions
            Vector3 head = player.GetBonePosition(HumanBodyBones.Head);
            Vector3 spine = player.GetBonePosition(HumanBodyBones.Spine);
            Vector3 rightUpperLeg = player.GetBonePosition(HumanBodyBones.RightUpperLeg);
            Vector3 rightLowerLeg = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 rightFoot = player.GetBonePosition(HumanBodyBones.RightFoot);
            //making sure all bones are valid
            if (head == Vector3.zero || spine == Vector3.zero || rightUpperLeg == Vector3.zero || rightLowerLeg == Vector3.zero || rightFoot == Vector3.zero)
            {
#if DEBUG_TEST
                Debug.Log($"[PlayerHeightDetector] Avatar is missing bones, so we assume it has standard height.");
#endif
                return ASSUMED_AVATAR_STANDARD_HEIGHT; //assuming the player has "standard height" is all we can do here
            }
            else
            {
                float playerHeight = Vector3.Distance(rightFoot, rightLowerLeg) + Vector3.Distance(rightLowerLeg, rightUpperLeg) +
                Vector3.Distance(rightUpperLeg, spine) + Vector3.Distance(spine, head);
                return playerHeight;
            }
        }
        #endregion Measurement
        #region FingerIndexBoneDetermination
        /// <summary>
        /// Determines which bone should be used as the right index finger bone, going through all finger bones
        /// and using the hand bone as the last fallback
        /// </summary>
        private HumanBodyBones GetRightHandIndexBone()
        {
            //Check right hand index finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightIndexDistal) != Vector3.zero)
            {
                return HumanBodyBones.RightIndexDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightIndexIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.RightIndexIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightIndexProximal) != Vector3.zero)
            {
                return HumanBodyBones.RightIndexProximal;
            }
            //Check right hand middle finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightMiddleDistal) != Vector3.zero)
            {
                return HumanBodyBones.RightMiddleDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightMiddleIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.RightMiddleIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightMiddleProximal) != Vector3.zero)
            {
                return HumanBodyBones.RightMiddleProximal;
            }
            //Check right hand ring finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightRingDistal) != Vector3.zero)
            {
                return HumanBodyBones.RightRingDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightRingIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.RightRingIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightRingProximal) != Vector3.zero)
            {
                return HumanBodyBones.RightRingProximal;
            }
            //Check right hand little finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightLittleDistal) != Vector3.zero)
            {
                return HumanBodyBones.RightLittleDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightLittleIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.RightLittleIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightLittleProximal) != Vector3.zero)
            {
                return HumanBodyBones.RightLittleProximal;
            }
            //Check right hand thumb finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightThumbDistal) != Vector3.zero)
            {
                return HumanBodyBones.RightThumbDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightThumbIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.RightThumbIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightThumbProximal) != Vector3.zero)
            {
                return HumanBodyBones.RightThumbProximal;
            }
            //Hand as last fallback
            if (_localPlayer.GetBonePosition(HumanBodyBones.RightHand) != Vector3.zero)
            {
                return HumanBodyBones.RightHand;
            }
            //if everything fails, we set the bone to none (RightLowerArm), allowing the receiving script to e.g. go into "desktop" mode
            return HumanBodyBones.RightLowerArm;
        }
        /// <summary>
        /// Determines which bone should be used as the left index finger bone, going through all finger bones
        /// and using the hand bone as the last fallback
        /// </summary>
        private HumanBodyBones GetLeftHandIndexBone()
        {
            //Check left hand index finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftIndexDistal) != Vector3.zero)
            {
                return HumanBodyBones.LeftIndexDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftIndexIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.LeftIndexIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftIndexProximal) != Vector3.zero)
            {
                return HumanBodyBones.LeftIndexProximal;
            }
            //Check left hand middle finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftMiddleDistal) != Vector3.zero)
            {
                return HumanBodyBones.LeftMiddleDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftMiddleIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.LeftMiddleIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftMiddleProximal) != Vector3.zero)
            {
                return HumanBodyBones.LeftMiddleProximal;
            }
            //Check left hand ring finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftRingDistal) != Vector3.zero)
            {
                return HumanBodyBones.LeftRingDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftRingIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.LeftRingIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftRingProximal) != Vector3.zero)
            {
                return HumanBodyBones.LeftRingProximal;
            }
            //Check left hand little finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftLittleDistal) != Vector3.zero)
            {
                return HumanBodyBones.LeftLittleDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftLittleIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.LeftLittleIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftLittleProximal) != Vector3.zero)
            {
                return HumanBodyBones.LeftLittleProximal;
            }
            //Check left hand thumb finger chain
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftThumbDistal) != Vector3.zero)
            {
                return HumanBodyBones.LeftThumbDistal;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftThumbIntermediate) != Vector3.zero)
            {
                return HumanBodyBones.LeftThumbIntermediate;
            }
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftThumbProximal) != Vector3.zero)
            {
                return HumanBodyBones.LeftThumbProximal;
            }
            //Hand as last fallback
            if (_localPlayer.GetBonePosition(HumanBodyBones.LeftHand) != Vector3.zero)
            {
                return HumanBodyBones.LeftHand;
            }
            //if everything fails, we set the bone to "none" (LeftLowerArm), allowing the receiving script to e.g. go into "desktop" mode
            return HumanBodyBones.LeftLowerArm;
        }
        #endregion FingerIndexBoneDetermination
        #region GeneralFunctions
        /// <summary>
        /// Returns the first child which name contains the string under the parent, returns null if no such child was found
        /// </summary>
        private Transform GetChildWithName(Transform parent, string name)
        {
            int childCount = parent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (parent.GetChild(i).name.Equals(name))
                    return parent.GetChild(i);
            }
            return null;
        }
        #endregion
        #region SpecialFunction
        /// <summary>
        /// Creates a Point between Parent and Child Bones to use as a reference point for a Capsule Collider
        /// /summary>
        private Vector3 LerpByDistance(Vector3 parent, Vector3 child, float x)
        {
            Vector3 P = x * Vector3.Normalize(child - parent) + parent;
            return P;
        }
        /// <summary>
        /// Create a capsule collider at float x from vector 3 above
        /// </summary>

        Get private point from above
            create capsule collider at point
            feed capsule collider distance to parent vector3 for length


        public class Example : MonoBehaviour
        {
            //Make sure there is a CapsuleCollider component attached to your GameObject
            CapsuleCollider m_Collider;
            float m_ScaleX, m_ScaleY, m_ScaleZ;
            public Slider m_SliderX, m_SliderY, m_SliderZ;


            void Start()
            {
                //Fetch the CapsuleCollider from the GameObject
                m_Collider = GetComponent<CapsuleCollider>();
                //These are the starting sizes for the Collider component
                m_ScaleX = 1.0f;
                m_ScaleY = 1.0f;
                m_ScaleZ = 1.0f;

                //Set all the sliders max values to 20 so the size values don't go above 20
                m_SliderX.maxValue = 20;
                m_SliderY.maxValue = 20;
                m_SliderZ.maxValue = 20;

                //Set all the sliders min values to 1 so the size don't go below 1
                m_SliderX.minValue = 1;
                m_SliderY.minValue = 1;
                m_SliderZ.minValue = 1;
            }

            void Update()
            {
                m_ScaleX = m_SliderX.value;
                m_ScaleY = m_SliderY.value;
                m_ScaleZ = m_SliderZ.value;

                m_Collider.size = new Vector3(m_ScaleX, m_ScaleY, m_ScaleZ);
                Debug.Log("Current BoxCollider Size : " + m_Collider.size);
            }
        }
    }
}