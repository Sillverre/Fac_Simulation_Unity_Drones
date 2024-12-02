using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class DroneIA : MonoBehaviour
{
    private DroneMovementScript controller;
    private Wifi wifi;
    private LidarRotative[] lidarRotative;
    private Communication communication;

    public int id = 0;
    public bool active = true;
    public static bool detectFailure = true;

    private float z_coord = 0;
    private float altitude = 30;

    private List<Waste> wasteList = new List<Waste>();

    private List<Vector3> route = new List<Vector3>();
    private List<Vector3> obstacleAvoidance = new List<Vector3>();


    private float lastMsgReceivedTime = 0;
    private float lastMsgTime = 0;

    // Use this for initialization
    void Start()
    {
        controller = GetComponent<DroneMovementScript>();
        wifi = GetComponent<Wifi>();
        lidarRotative = GetComponents<LidarRotative>();
        communication = new Communication(wifi);

        z_coord = 50 * id + 25; 
        generateRoute();
    }

    // Update is called once per frame
    void Update()
    {
        if (active) // drone ok (failure simulation)
        {
            receiveData(wifi);

            Vector3 targetPos = nextWaypoint();

            int obst = obstacleDetection(lidarRotative[0].GetData());
            if (obst != 0)
            {
                if (obstacleAvoidance.Count == 0)
                {
                    float dx = targetPos.x - transform.position.x;
                    float dz = targetPos.z - transform.position.z;

                    obstacleAvoidance.Add(new Vector3(transform.position.x, 50, transform.position.z));
                    float xVal = transform.position.x;
                    if (dx > 25)
                        xVal += 20;
                    else if(dx < -25)
                        xVal -= 20;

                    float zVal = transform.position.z;
                    if (dz > 25)
                        zVal += 20;
                    else if (dz < -25)
                        zVal -= 20;

                    if (xVal > 290)
                        xVal = 290;
                    else if(xVal < 10)
                        xVal = 10;

                    if (zVal > 290)
                        zVal = 290;
                    else if (zVal < 10)
                        zVal = 10;

                    obstacleAvoidance.Add(new Vector3(xVal, 50, zVal));

                    targetPos = nextWaypoint();
                }
            }

            controller.GoToPoint(targetPos);


            wasteDetection(lidarRotative[1]);
            if (lidarRotative[1].AngleRoll < 180)
            {
                lidarRotative[1].AngleRoll = lidarRotative[1].AngleRoll + 1;
            }
            else
            {
                lidarRotative[1].AngleRoll = -180;
            }

            wasteList = Util.cleanWasteList(wasteList);
            communication.sendData(wasteList);


            if(Time.time - lastMsgTime > 5)
            {
                communication.sendAlive(id);
                lastMsgTime = Time.time;
            }
        }
    }


    void receiveData(Wifi wifi)
    {
        if (wifi.isWaiting())
        {
            byte[] msg = wifi.getMessage();

            if (msg[0] == 1) // message type: waste information (position, type, ...)
            {
                int msgIndex = 1;
                float x = BitConverter.ToSingle(msg, msgIndex);
                msgIndex += sizeof(float);
                float y = BitConverter.ToSingle(msg, msgIndex);
                msgIndex += sizeof(float);
                float z = BitConverter.ToSingle(msg, msgIndex);
                msgIndex += sizeof(float);
                float at = BitConverter.ToSingle(msg, msgIndex);
                msgIndex += sizeof(float);


                Vector3 position = new Vector3(x, y, z);
                int tagLength = msg.Length - (4 * sizeof(float)) - 2;
                byte[] tagBytes = new byte[tagLength];
                System.Array.ConstrainedCopy(msg, msgIndex, tagBytes, 0, tagLength);
                String tag = System.Text.Encoding.ASCII.GetString(tagBytes);

                Waste w = new Waste(tag, position, at, (msg[5] == 1 ? true : false));

                if (Util.findWasteInList(w, wasteList) == null)
                {
                    wasteList.Add(w);
                }
                else
                {
                    Waste waste = Util.findWasteInList(w, wasteList);
                    if (w.At > waste.At)
                    {
                        waste.At = w.At;
                        waste.Pickedup = w.Pickedup;
                    }
                }
            }
            else
            {
                lastMsgReceivedTime = Time.time;
            }
        }
    }

    int obstacleDetection(List<Triple> data)
    {
        foreach (Triple triple in data)
        {
            if (triple.Angle > -90 && triple.Angle < 90 && triple.Value < 10)
            {
                float dx = (float)Math.Sin(triple.Angle * Mathf.Deg2Rad) * triple.Value;

                float dxVar = 2.5f;
                if (triple.Value > 4)
                    dxVar = 2.5f;
                else
                    dxVar = 1.8f;


                if (dx < dxVar && dx > -dxVar)
                {
                    if (dx < 0)
                        return -1;
                    else
                        return 1;
                }
            }
        }
        return 0;
    }

    void wasteDetection(LidarRotative lidarRotative)
    {
        foreach (Triple triple in lidarRotative.GetData())
        {
            if (triple.Tag == "Metal" || triple.Tag == "Glass")
            {
                Vector3 axe1 = Quaternion.AngleAxis(lidarRotative.AngleRoll, transform.up) * transform.forward;
                Vector3 axe2 = Quaternion.AngleAxis(triple.Angle, axe1) * -transform.up;
                
                // add the desired distance to the direction
                Vector3 addDistanceToDirection = axe2 * triple.Value;
                // add the distance and direction to the current position to get the final destination
                Vector3 position = transform.position + addDistanceToDirection;

                Waste w = new Waste(triple.Tag, position, Time.time);
                if (Util.findWasteInList(w, wasteList) == null)
                {
                    wasteList.Add(w);
                }
            }
        }
    }

    // drone target position 
    Vector3 nextWaypoint()
    {
        float x_min = transform.position.x - 5;
        float x_max = transform.position.x + 5;
        float y_min = transform.position.y - 5;
        float y_max = transform.position.y + 5;
        float z_min = transform.position.z - 5;
        float z_max = transform.position.z + 5;

        Vector3 targetPosition;
        if (obstacleAvoidance.Count > 0)
        {
            if (x_min <= obstacleAvoidance[0].x && x_max >= obstacleAvoidance[0].x && y_min <= obstacleAvoidance[0].y && y_max >= obstacleAvoidance[0].y
            && z_min <= obstacleAvoidance[0].z && z_max >= obstacleAvoidance[0].z)
            {
                    obstacleAvoidance.RemoveAt(0);
            }
        }
        else if (route.Count > 0)
        {
            if (x_min <= route[0].x && x_max >= route[0].x && z_min <= route[0].z && z_max >= route[0].z)
            {
                route.RemoveAt(0);
            }
        }
        else
        {
            generateRoute();
            if (x_min <= route[0].x && x_max >= route[0].x && z_min <= route[0].z && z_max >= route[0].z)
            {
                route.RemoveAt(0);
            }
        }

        if (obstacleAvoidance.Count > 0)
        {
            targetPosition = obstacleAvoidance[0];
        }
        else if (route.Count > 0)
        {
            targetPosition = route[0];
        }
        else
        {
            generateRoute();
            targetPosition = route[0];
        }
        return targetPosition;
    }

    // generate the route to follow
    void generateRoute()
    {
        if (Time.time - lastMsgReceivedTime > 30 && id!=0 && detectFailure) // 
        {
            route.Add(new Vector3(285, altitude, z_coord-50));
            route.Add(new Vector3(15, altitude, z_coord - 50));
            route.Add(new Vector3(15, altitude, z_coord));
            route.Add(new Vector3(285, altitude, z_coord));
        }
        else
        {
            route.Add(new Vector3(15, altitude, z_coord));
            route.Add(new Vector3(285, altitude, z_coord));
        }     
    }


}
