using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Waste
{
    public Vector3 Position { get; set; }
    public string Tag;
    public double Distance { get; set; }
    public bool Pickedup;
    public float At;

    public Waste(String tag, Vector3 position)
    {
        Position = position;
        Tag = tag;
        Distance = -1;
        Pickedup = false;
        At = -1;
    }

    public Waste(String tag, Vector3 position, float at)
    {
        Position = position;
        Tag = tag;
        Distance = -1;
        Pickedup = false;
        At = at;
    }

    public Waste(String tag, Vector3 position, float at, bool pickedup)
    {
        Position = position;
        Tag = tag;
        Distance = -1;
        Pickedup = pickedup;
        At = at;
    }

}
