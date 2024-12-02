using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Triple
{
    public float Angle { get; set; }
    public float Value { get; set; }
    public string Tag { get; set; }

    public Triple(float angle, float value, string tag)
    {
        Angle = angle;
        Value = value;
        Tag = tag;
    }

    public Triple Clone()
    {
        return new Triple(Angle, Value, Tag);
    }
}

[AddComponentMenu("Robot/LidarRotative")]
[System.Serializable]
public class LidarRotative : Lidar
{
    private List<Triple> Values { get; set; }

	[SerializeField]
    private float _angleMin;
    public float AngleMin
    {
        get
        {
            return _angleMin;
        }
        set
        {
            _angleMin = value;
            if (value < -180)
                _angleMin = -180;
            if (value > 180)
                _angleMin = 180;

            CalculateStep();
        }
    }

	private float Step;

	[SerializeField]
    private float _angleMax;
    public float AngleMax
    {
        get
        {
            return _angleMax;
        }
        set
        {
            _angleMax = value;
            if (value < -180)
                _angleMax = -180;
            if (value > 180)
                _angleMax = 180;
            CalculateStep();

        }
    }

	[SerializeField]
	private float _angleRoll;

	public float AngleRoll {
		get {
			return _angleRoll;
		}
		set {
			_angleRoll = value;
			if (value < -180)
				_angleRoll = -180;
			if (value > 180)
				_angleRoll = 180;
		}
	}

	[SerializeField]
	private float _anglePitch;
	public float AnglePitch {
		get {
			return _anglePitch;
		}
		set {
			_anglePitch = value;
			if (value < -180)
				_anglePitch = -180;
			if (value > 180)
				_anglePitch = 180;
		}
	}

    private float nextTime = -1;

	[SerializeField]
    private int _resolution;

    public int Resolution
    {
        get
        {
            return _resolution;
        }
        set
        {   
            if (value < 1)
            {
                value = 1;
            }
            _resolution = value;
            CalculateStep();
        }
    }

    [SerializeField]
	private int _frequence;
	public int Frequence { get {
            if (_frequence < 1)
            {
                _frequence = 1;
            }
            else if (_frequence > 100)
            {
                _frequence = 100;
            }
            return _frequence; }
        set {
            if (value < 1)
            {
                value = 1;
            }
            else if (value > 100)
            {
                value = 100;
            }
            _frequence = value; }
    }

	[SerializeField]
    private bool _allValue;
	public bool AllValue { get { return  _allValue; } set  { _allValue = value; }} 

    public LidarRotative() : base()
    {
        AngleMin = 0;
        AngleMax = 0;
        Values = new List<Triple>();
        Frequence = 0;
        Resolution = 0;
        AllValue = true;
    }

    public LidarRotative(string name, Vector3 lookAt, Vector3 position, float range, bool debug, float angleMin, float angleMax, int freq, int nbRay, bool allValue) : base(name, lookAt, position, range, debug)
    {
        Values = new List<Triple>();
        AngleMin = angleMin;
        AngleMax = angleMax;
        Frequence = freq;
        AllValue = allValue;
        Resolution = nbRay;
    }

    public List<Triple> GetData()
    {
        if (AllValue)
            return getAllValues();
        else
            return getPartialValues();
    }


    public List<Triple> getAllValues()
    {
        if (Time.time > nextTime ||  nextTime == -1.0)
        {

            Values.Clear();

			Vector3 AxeFront = Quaternion.AngleAxis (  AnglePitch, this.transform.right) * this.transform.forward ;
			 
			Vector3 AxeRight = Quaternion.AngleAxis (  -AngleRoll, AxeFront) * this.transform.right ;

            for (float i = AngleMin; i < AngleMax  || (AngleMax == AngleMin && i == AngleMax); i+=Step)
            {
				Vector3 dir =  Quaternion.AngleAxis(i, Vector3.Cross (AxeFront, AxeRight) ) * AxeFront ;
                RaycastHit hit;

				if (Physics.Raycast(transform.position, dir, out hit, Range))
                {
                    Values.Add(new Triple(i, hit.distance, hit.transform.tag));
                    if (IsDebug)
                    {
                        Debug.DrawLine(transform.position, (transform.position + dir * hit.distance), ColorDebug, 0.5f);
                    }
                }
                else
                {
					Values.Add(new Triple(i, Mathf.Infinity, "null"));
                    if (IsDebug)
                    {
                        Debug.DrawLine(transform.position, (transform.position + dir * Range), ColorDebug, 0.5f);
                    }

                }
            }
            nextTime = Time.time + (1 / Frequence);
        }
        return new List<Triple>(Values);
    }

	private List<Triple> getPartialValues()
	{
		return new List<Triple> (Values);
	}

    private void CalculateStep()
    {
        if (Resolution != 0)
            Step = (Mathf.Abs(AngleMin) + Mathf.Abs(AngleMax)) / Resolution;
        if (Step == 0)
        {
            Step = 1;
        }
    }

	public new void Start(){
		CalculateStep();
	}
    public void Update()
    {
        getAllValues();
    }
}
