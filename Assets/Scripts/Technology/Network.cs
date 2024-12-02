using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor.VersionControl;
using System.Runtime.InteropServices;

[System.Serializable]
public abstract class Network : MonoBehaviour
{
	private List<byte[]> _waitMessage =  new List<byte[]>();

	[SerializeField]
	private float _range;
	public float Range {
		get { return _range;}
		set { _range = value;}
	}

    [SerializeField]
    private String _name;
	public String Name { get { return _name;} set { _name = value;} }

	[SerializeField]
	[Range (0, 100)]
	private float _losePackagePourcent;
	public float LosePackagePourcent { get { return _losePackagePourcent; } set { _losePackagePourcent = value; }}

	[SerializeField]
	private float _fluctuation;
	public float FluctuationSignal { get { return _fluctuation;} set{ _fluctuation = value;} }

	[SerializeField]
	private bool _isDebug;
	public bool IsDebug { get { return _isDebug; } set { _isDebug = value; } }
	[SerializeField]
	private Color _color;
	public Color ColorDebug { get { return _color; } set { _color = value; _color.a = 1; } }

	public Network(){
		Name = "";
		Range = 0;
		LosePackagePourcent = 0;
		FluctuationSignal = 0;
		_isDebug = false;
		ColorDebug = Color.red;
	}

	public Network(String name, float range, float losePackagePourcent, float fluctuation)
	{
		Name = name;
		Range = range;
		LosePackagePourcent = losePackagePourcent;
		FluctuationSignal = fluctuation;
		_isDebug = false;
		ColorDebug = Color.red;

	}

	public List<GameObject> getRobotAround()
	{
		
		GameObject[] allRobot = GameObject.FindGameObjectsWithTag("Robot");
		List<GameObject> l = new List<GameObject>();
		foreach (GameObject o in allRobot)
		{
			if ( o != this.gameObject &&  isAvailable(o) && Vector3.Distance(this.transform.position, o.transform.position) <= _range)
			{
				l.Add(o);
				if (IsDebug){
					Debug.DrawLine(this.transform.position , o.transform.position, ColorDebug, 1f);
				}
			}
		}
		return l;
	}


	public abstract bool isAvailable (GameObject o);

	public void SendAll(byte[] message)
	{
		List<GameObject> l  = getRobotAround();
		foreach (GameObject o in l) 
		{
			o.GetComponent<Network>().addMessage(message);
		}
	}

	public void Send(GameObject o, byte[] message)
	{

		Network n = o.GetComponent<Network>();
		if (n != null)    
			n.addMessage(message);
	}

	public byte[] getMessage()
	{
		if (!isWaiting())
		{
			return null;
		}
		byte[] tmp = _waitMessage[0];
		_waitMessage.RemoveAt(0);
		return tmp;
	}

	public bool isWaiting()
	{
		return _waitMessage.Count != 0;
	}

	protected void addMessage(byte[] mess){
		_waitMessage.Add(mess);
	}
    public void Start() { }

}