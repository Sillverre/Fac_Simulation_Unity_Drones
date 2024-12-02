using UnityEngine;

[System.Serializable]
public class Technology : MonoBehaviour {

	[SerializeField]
	private Vector3 _lookAt;
	public Vector3 LookAt { get { return _lookAt;} set{ _lookAt = value; } }

	[SerializeField]
	private Vector3 _position;
	public Vector3 Position { get { return _position; } set{ _position = value; } }

    [SerializeField]
    private string _name;
	public string Name { get { return _name; } set { _name = value; }
        }

	public Technology(){
		LookAt = new Vector3 (0f, 0f, 0f);
		Position = new Vector3 (0f, 0f, 0f);
		Name = "";
	}

	public Technology(string name, Vector3 lookAt, Vector3 position)
	{
		this.Name = name;
		this.LookAt = lookAt;
		this.Position = position;
	}

    public void Start() { }

}
