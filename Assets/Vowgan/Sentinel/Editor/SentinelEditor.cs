using System;
using System.Collections;
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Vowgan
{
    [CustomEditor(typeof(SentinelManager))]
    public class SentinelEditor : Editor
    {
        
        private SentinelManager script;
        
        private void OnEnable()
        {
            script = target as SentinelManager;
            if (script == null) return;
            script.transform.name = "Sentinel";
        }
        
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            
            if (GUILayout.Button("Set Count"))
            {
                Undo.RecordObject(script, "Initialized Max Player Count");
                List<int> newIds = new List<int>();
                List<Transform> newTransforms = new List<Transform>();
                
                while (script.transform.childCount > 0)
                {
                    Undo.DestroyObjectImmediate(script.transform.GetChild(0).gameObject);
                }
                
                for (int i = 0; i < script.MaxCount; i++)
                {
                    GameObject newChild = Instantiate(script.PlayerPrefab, script.transform);
                    newChild.name = script.PlayerPrefab.name + $" {i}";
                    Undo.RegisterCreatedObjectUndo(newChild, "Created Object");
                    newIds.Add(-1);
                    newTransforms.Add(newChild.transform);
                }
                
                script.PlayerIds = newIds.ToArray();
                script.PlayerTransforms = newTransforms.ToArray();
            }
            
            base.OnInspectorGUI();
        }
    }
}