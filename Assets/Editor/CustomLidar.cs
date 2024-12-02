using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Lidar), true)]
[System.Serializable]
public class CustomLidar : CustomTechnology
{
    [SerializeField]
    Lidar _lidar;

    void OnEnable()
    {
        _lidar = (Lidar)target;
    }

    public override void OnInspectorGUI()
    {
        _lidar = this.target as Lidar;
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Range: ");
        _lidar.Range = float.Parse(GUILayout.TextField(_lidar.Range.ToString()));
        EditorGUILayout.EndHorizontal();

        _lidar.IsDebug = EditorGUILayout.Toggle ("Debug: ", _lidar.IsDebug);

        if (_lidar.IsDebug)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Color: ");
            _lidar.ColorDebug = EditorGUILayout.ColorField(_lidar.ColorDebug);
            GUILayout.EndHorizontal();
        }

    }
}