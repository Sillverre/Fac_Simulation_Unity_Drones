using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Network), true)]
[System.Serializable]
[CanEditMultipleObjects]
public class CustomNetwork : Editor {
	[SerializeField]
	Network _network;

	void OnEnable()
	{
		_network = (Network)target;

	}

	public override void OnInspectorGUI(){
		_network = (this.target as Network);

        _network.Name = EditorGUILayout.TextField("Name:", _network.Name);

        _network.Range = EditorGUILayout.FloatField("Range:", _network.Range);
        _network.LosePackagePourcent = EditorGUILayout.FloatField("Pourcentage Perte:", _network.LosePackagePourcent);
        _network.FluctuationSignal = EditorGUILayout.FloatField("Fluctuation Signal:", _network.FluctuationSignal);


        if (GUI.changed)
		{
			EditorUtility.SetDirty(_network);
			serializedObject.ApplyModifiedProperties();
		}

		_network.IsDebug = EditorGUILayout.Toggle ("Debug: ", _network.IsDebug);

		if (_network.IsDebug)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Color: ");
			_network.ColorDebug = EditorGUILayout.ColorField(_network.ColorDebug);
			GUILayout.EndHorizontal();
		}
	}
}
