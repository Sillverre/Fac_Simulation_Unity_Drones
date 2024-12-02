using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Wifi), true)]
[System.Serializable]
[CanEditMultipleObjects]
public class CustomWifi : CustomNetwork
{
	[SerializeField]
	private Wifi _wifi;

	void OnEnable()
	{
		_wifi = (Wifi)target;
	}

	public override void OnInspectorGUI()
	{
		_wifi = this.target as Wifi;
		base.OnInspectorGUI();

	}
}