
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SentinelManager : UdonSharpBehaviour
    {
        
        public GameObject PlayerPrefab;
        public byte MaxCount = 100;
        [UdonSynced] public int[] PlayerIds;
        public Transform[] PlayerTransforms;
        
        private VRCPlayerApi playerLocal;
        private VRCPlayerApi[] playerList;
        private GameObject[] playerObjects;
        
        
        private void Start()
        {
            playerLocal = Networking.LocalPlayer;
            playerList = VRCPlayerApi.GetPlayers(new VRCPlayerApi[MaxCount]);
            
            playerObjects = new GameObject[MaxCount];
            for (int i = 0; i < MaxCount; i++)
            {
                playerObjects[i] = PlayerTransforms[i].gameObject;
            }
            
            OnDeserialization();
        }
        
        public override void OnDeserialization()
        {
            for (int i = 0; i < MaxCount; i++)
            {
                if (PlayerIds[i] == -1)
                {
                    playerList[i] = null;
                    playerObjects[i].SetActive(false);
                }
                else
                {
                    playerObjects[i].SetActive(true);
                    playerList[i] = VRCPlayerApi.GetPlayerById(PlayerIds[i]);
                }
            }
        }
        
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!playerLocal.isMaster) return;
            int id = player.playerId;
            for (int i = 0; i < MaxCount; i++)
            {
                if (PlayerIds[i] != -1) continue;
                AddPlayer(i, id);
                break;
            }
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!playerLocal.isMaster) return;
            int id = player.playerId;
            for (int i = 0; i < MaxCount; i++)
            {
                if (PlayerIds[i] != id) continue;
                RemovePlayer(i);
                break;
            }
        }
        
        public override void PostLateUpdate()
        {
            for (int i = 0; i < MaxCount; i++)
            {
                VRCPlayerApi targetPlayer = playerList[i];
                if (!Utilities.IsValid(targetPlayer)) continue;
                PlayerTransforms[i].position = targetPlayer.GetPosition();
            }
        }
        
        private void AddPlayer(int index, int id)
        {
            if (!playerLocal.isMaster) return;
            PlayerIds[index] = id;
            RequestSerialization();
            OnDeserialization();
        }
        
        private void RemovePlayer(int index)
        {
            if (!playerLocal.isMaster) return;
            PlayerIds[index] = -1;
            RequestSerialization();
            OnDeserialization();
        }
        
    }
}