using System;
using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Robot/Wifi")]

public class Wifi : Network
{
	public override bool isAvailable (GameObject o) {
		return o.GetComponent<Wifi> ();
	}


	public Wifi () : base()
	{
	}

	public Wifi ( String name, float range, float losePackagePourcent, float fluctuation ) : base (name, range, losePackagePourcent, fluctuation) {}

}


