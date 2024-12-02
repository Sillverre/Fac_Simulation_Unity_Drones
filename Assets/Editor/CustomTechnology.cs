using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Technology), true)]
[System.Serializable]
public class CustomTechnology : Editor {
    [SerializeField]
    Technology _technology;

	void OnEnable()
	{
		_technology = (Technology)target;

    }

    public override void OnInspectorGUI(){
        _technology = (this.target as Technology);
        GUILayout.BeginHorizontal ();
        GUILayout.Label("Name: ");
		_technology.Name = GUILayout.TextField(_technology.Name);
        GUILayout.EndHorizontal ();

		_technology.LookAt = EditorGUILayout.Vector3Field("LookAt", _technology.LookAt);
        _technology.Position = EditorGUILayout.Vector3Field("Position", _technology.Position);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(_technology);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
	