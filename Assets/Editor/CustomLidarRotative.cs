using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LidarRotative), true)]
[System.Serializable]
[CanEditMultipleObjects]
public class CustomLidarRotative : CustomLidar
{
    [SerializeField]
    LidarRotative _lidarRotative;

    void OnEnable()
    {
        _lidarRotative = (LidarRotative)target;
    }

    public override void OnInspectorGUI()
    {
        _lidarRotative = this.target as LidarRotative;
        base.OnInspectorGUI();

        _lidarRotative.AngleMin = EditorGUILayout.FloatField("Angle Min:", _lidarRotative.AngleMin);

        _lidarRotative.AngleMax = EditorGUILayout.FloatField("Angle Max:", _lidarRotative.AngleMax);

        GUILayout.BeginHorizontal();
        _lidarRotative.Resolution = EditorGUILayout.IntField("Resolution:", _lidarRotative.Resolution);
        _lidarRotative.Frequence = EditorGUILayout.IntField("Frequence", _lidarRotative.Frequence);
        GUILayout.EndHorizontal();



		GUILayout.BeginHorizontal();
		_lidarRotative.AngleRoll = EditorGUILayout.FloatField("Roll:", _lidarRotative.AngleRoll);
		_lidarRotative.AnglePitch = EditorGUILayout.FloatField("Pitch", _lidarRotative.AnglePitch);
		GUILayout.EndHorizontal();

		_lidarRotative.AllValue = EditorGUILayout.Toggle ("All values: ", _lidarRotative.AllValue);

    }
}