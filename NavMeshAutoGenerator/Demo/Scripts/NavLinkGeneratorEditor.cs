using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace NavLinkGeneration
{
    [CustomEditor(typeof(NavLinkGenerator))]
    [System.Serializable]
    public class NavLinkGeneratorEditor : Editor
    {


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            NavLinkGenerator script = (NavLinkGenerator)target;

            //SerializedProperty Width = serializedObject.FindProperty("Width");
            if (GUILayout.Button("CreateLinksOnBoxCollider"))
            {
                if (script.GetComponent<BoxCollider>() != null)
                    script.CreateLinksOnBox(script.GetComponent<BoxCollider>(), script.transform, 0);
                else
                    Debug.Log("There is no box collider on this object");
            }
            if (GUILayout.Button("Create Links On Child Box Colliders"))
            {
                script.CreateLinksOnChildBoxColliders();
            }
            if (GUILayout.Button("Switch Bidirectional"))
            {
                script.SwitchBidirectional();
            }
            //if (GUILayout.Button("Swap End and Start"))
            //{
            //    script.SwapDirections();
            //}
            if (GUILayout.Button("Reset"))
            {
                script.ClearLinks();
            }
        }
    }
}