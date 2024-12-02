using UnityEngine;

[AddComponentMenu("Robot/Lidar")]
[System.Serializable]
public class Lidar : Technology
{
	[SerializeField]
	private float _range;
	public float Range { get { return _range; } set { _range = value; } }

	[SerializeField]
	private bool _isDebug;
	public bool IsDebug { get { return _isDebug; } set { _isDebug = value; } }
	[SerializeField]
	private Color _color;
	public Color ColorDebug { get { return _color; } set { _color = value;  } }

    public Lidar() : base()
    {
        Range = 0;
        IsDebug = false;
    }

    public Lidar(string name, Vector3 lookAt, Vector3 position, float range, bool debug) : base(name, lookAt, position)
    {
        Range = range;
        IsDebug = debug;
    }

    /// <summary>
    /// Obtains the distance of the nearest object 
    /// </summary>
    /// <returns>return float distance or Infinity if not intersection </returns>
    public float GetDistance()
    {
        RaycastHit hit;
        Physics.Raycast(this.transform.position + Position, this.transform.forward + LookAt, out hit, Range);

        if (IsDebug)
        {
            Debug.DrawLine(this.transform.position + Position, hit.point - (this.transform.position + Position), ColorDebug);
        }

        return hit.distance;
    }

    /// <summary>
    /// Obtains the coordinate of the nearest object who intersect with a ray
    /// </summary>
    /// <returns>Coordinate of the point or (0,0,0) if not intersection</returns>
    public Vector3 GetPoint()
    {
        RaycastHit hit;
        bool res = Physics.Raycast(this.transform.position + Position, this.transform.forward + LookAt, out hit, Range);

        if (IsDebug)
        {
            Debug.DrawLine(this.transform.position + Position, hit.point - (this.transform.position + Position), ColorDebug);
        }

        return res ? hit.point : new Vector3(0, 0, 0);
    }

}
