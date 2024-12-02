using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class RoverIA : MonoBehaviour
{
    public bool metal = true;
    public bool glass = false;
    public bool plastic = false;
    public bool trash = false;
    public float zoneX;
    public float zoneY;

    private Vector2 zoneLimitMin;
    private Vector2 zoneLimitMax;
    private Vector3 zoneCenter;
    private Vector2 mapLimitMin, mapLimitMax;

    private List<Waste> wasteList = new List<Waste>();

    private List<GameObject> trashList = new List<GameObject>();
    private bool trashPickedUp = false;
    private float lastTrashAt = 0;
    private GameObject targetTrash;
    private Vector3 trashTargetPos;
    private float trashDelay = 0;

    private RoverMovementScript controller;
    private Wifi wifi;
    private LidarRotative lidarRotative;
    private Communication communication;

    private int counter = 0;
    private bool obstacle = false;
    private bool obstacleMultiple = false;
    private Waste goToWaste = null;

    // Use this for initialization
    void Start()
    {
        controller = GetComponent<RoverMovementScript>();
        wifi = GetComponent<Wifi>();
        lidarRotative = GetComponent<LidarRotative>();
        communication = new Communication(wifi);

        mapLimitMin = new Vector2(0, 0);
        mapLimitMax = new Vector2(300, 300);

        if (trash) // trash rover type
        {
            metal = false;
            glass = false;
            plastic = false;
            foreach (GameObject o in GameObject.FindGameObjectsWithTag("Trash"))
            {
                trashList.Add(o);
            }
        }
        else // normal rover type
        {
            zoneLimitMin = new Vector2(zoneX * 150, zoneY * 150);
            zoneLimitMax = new Vector2((zoneX + 1) * 150, (zoneY + 1) * 150);
            zoneCenter = new Vector3(zoneX * 150 + 75, 0, zoneY * 150 + 75);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!trash) // normal rover type
        {
            receiveData(wifi);

            // send wastes position to others
            wasteList = Util.cleanWasteList(wasteList);
            communication.sendData(wasteList);

            // detect wastes and select waste to pick up
            goToWaste = wasteDetection(lidarRotative.GetData());
        }

        int obst = obstacleDetection(lidarRotative.GetData());
        if (obst == 1) // obstacle to the right
        {
            controller.ChangeMotorSpeed(-1, 1);
            obstacle = true;
            counter = 0;
        }
        else if (obst == -1) // obstacle to the left
        {
            controller.ChangeMotorSpeed(1, -1);
            obstacle = true;
            counter = 0;
        }
        else // if no obstacle detected
        {
            if (obstacle && counter < 200)
            {
                ++counter;
                controller.ChangeMotorSpeed(1, 1);
            }
            else
            {
                float val = 0;

                if (!trash) // normal rover type
                {
                    if (goToWaste != null)
                    {
                        float x_min = transform.position.x - 1;
                        float x_max = transform.position.x + 1;
                        float z_min = transform.position.z - 1;
                        float z_max = transform.position.z + 1;
                        if (x_min <= goToWaste.Position.x && x_max >= goToWaste.Position.x && z_min <= goToWaste.Position.z && z_max >= goToWaste.Position.z)
                        {
                            goToWaste.Pickedup = true;
                            goToWaste.At = Time.time;
                        }
                        else
                        {
                            val = goTo(goToWaste.Position);
                        }
                    }

                    if (transform.position.x < zoneLimitMin.x || transform.position.x > zoneLimitMax.x || 
                        transform.position.z < zoneLimitMin.y || transform.position.z > zoneLimitMax.y)
                    {
                        val = goTo(zoneCenter);
                    }
                }
                else // trash rover type
                {
                    if (trashPickedUp)
                    {
                        float x_min = transform.position.x - 1;
                        float x_max = transform.position.x + 1;
                        float z_min = transform.position.z - 1;
                        float z_max = transform.position.z + 1;

                        if (x_min <= trashTargetPos.x && x_max >= trashTargetPos.x && z_min <= trashTargetPos.z && z_max >= trashTargetPos.z) // target position reached
                        {
                            targetTrash.transform.position = new Vector3(transform.position.x - 3, 0, transform.position.z);
                            targetTrash.SetActive(true);
                            lastTrashAt = Time.time;
                            targetTrash = null;
                            trashPickedUp = false;
                            Renderer[] r = this.gameObject.GetComponentsInChildren<Renderer>();
                            r[2].material.color = Color.green;
                        }
                        else
                        {
                            val = goTo(trashTargetPos);
                        }
                    }
                    else if (!trashPickedUp && targetTrash != null) // go to the trash
                    {
                        val = goTo(targetTrash.transform.position);
                    }
                    else if(Time.time - lastTrashAt > trashDelay) 
                    {
                        targetTrash = trashList[UnityEngine.Random.Range(0, trashList.Count)];
                        val = goTo(targetTrash.transform.position);
                        trashDelay = UnityEngine.Random.Range(10, 30);
                    }
                }


                if (val == 0)
                    controller.ChangeMotorSpeed(1, 1);
                else if (val == 1)
                    controller.ChangeMotorSpeed(1, -1);
                else
                    controller.ChangeMotorSpeed(-1, 1);

                counter = 0;
                obstacle = false;
                obstacleMultiple = false;
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

                if (Util.findWasteInList(w, wasteList) == null) // add waste in the list
                {
                    wasteList.Add(w);
                }
                else // update waste information
                {
                    Waste waste = Util.findWasteInList(w, wasteList);
                    if (w.At > waste.At)
                    {
                        waste.At = w.At;
                        waste.Pickedup = w.Pickedup;
                    }
                }
            }
        }
    }

    bool canBePickedUp(String tag)
    {
        if ((tag == "Metal" && metal) || (tag == "Glass" && glass) || (tag == "Plastic" && plastic))
            return true;
        return false;
    }

    void OnCollisionEnter(Collision col)
    {
        if (canBePickedUp(col.gameObject.tag))
        {
            col.gameObject.SetActive(false);
            //col.gameObject.transform.position = new Vector3(UnityEngine.Random.Range(4, 20), col.gameObject.transform.position.y, UnityEngine.Random.Range(4, 20));
        }

        if (trash && col.gameObject.tag == "Trash")
        {
            trashPickedUp = true;
            col.gameObject.SetActive(false);
            Renderer[] r = this.gameObject.GetComponentsInChildren<Renderer>();
            r[2].material.color = Color.red;
            trashTargetPos = new Vector3(UnityEngine.Random.Range(mapLimitMin.x+5, mapLimitMax.x-5), 0, UnityEngine.Random.Range(mapLimitMin.y+5, mapLimitMax.y-5));
        }
    }

    int obstacleDetection(List<Triple> data)
    {
        // x position of the left nearest and right nearest obstacle  
        float dx_left_min = float.MinValue;
        float dx_right_min = float.MaxValue;

        foreach (Triple triple in data)
        {
            if (triple.Angle > -90 && triple.Angle < 90 && 
                ((!canBePickedUp(triple.Tag) && !trash) || (trash && (targetTrash ==null || triple.Tag !="Trash"))) 
                && triple.Value < 5)
            {
                float dx = (float)Math.Sin(triple.Angle * Mathf.Deg2Rad) * triple.Value;
                //float dy = (float)Math.Cos(triple.Angle * Mathf.Deg2Rad) * triple.Value;
                //Vector2 positionRelative = new Vector2(dx, dy);

                float dxVar = 2.5f;
                if (triple.Value > 4)
                    dxVar = 2.5f;
                else
                    dxVar = 1.8f;


                if (dx < dxVar && dx > -dxVar)
                {
                    if (dx < 0) // obstacle to the left
                    {
                        if(dx_left_min < dx)
                            dx_left_min = dx;
                    }
                    else // obstacle to the right
                    {
                        if (dx_right_min > dx)
                            dx_right_min = dx;
                    }
                }
            }
        }

        if(dx_right_min != float.MaxValue && dx_left_min != float.MinValue) // obstacle to the left and to the right
        {
            if (dx_right_min- dx_left_min < 4)
            {
                obstacleMultiple = true;
                return 1;
            }
            return 0;
        }
        else if(dx_left_min != float.MinValue) // obstacle to the left
        {
            if (obstacleMultiple)
            {
                return 1;
            }
            return -1;
        }
        else if (dx_right_min != float.MaxValue) // obstacle to the right
        {
            if (obstacleMultiple)
            {
                return 1;
            }
            return 1;
        }
        return 0;
    }

    Waste wasteDetection(List<Triple> data)
    {
        foreach (Triple triple in data)
        {
            if (triple.Tag == "Metal" || triple.Tag == "Glass" || triple.Tag == "Plastic")
            {
                // local coordinate rotation around the Y axis to the given angle
                Quaternion rotation = Quaternion.AngleAxis(triple.Angle, transform.up);
                // add the desired distance to the direction
                Vector3 addDistanceToDirection = rotation * transform.forward * triple.Value;
                // add the distance and direction to the current position to get the final destination
                Vector3 position = transform.position + addDistanceToDirection;

                Waste w = new Waste(triple.Tag, position, Time.time);
                if (Util.findWasteInList(w, wasteList) == null)
                {
                    wasteList.Add(w);
                }
            }
        }

        // calculate distances
        foreach (Waste w in wasteList)
        {
            w.Distance = Vector3.Distance(w.Position, transform.position);
        }

        // sort the list
        wasteList.Sort(
            delegate (Waste w1, Waste w2)
            {
                if (w1.Distance < w2.Distance)
                {
                    return -1;
                }
                else if (w1.Distance > w2.Distance)
                {
                    return 1;
                }
                return 0;
            }
        );

        // select waste to pick up
        foreach (Waste w in wasteList)
        {
            if (canBePickedUp(w.Tag) && w.Pickedup == false && 
                w.Position.x > zoneLimitMin.x && w.Position.x < zoneLimitMax.x &&
                w.Position.z > zoneLimitMin.y && w.Position.z < zoneLimitMax.y)
            {
                return w;
            }
            
        }
        return null;
    }

    float goTo(Vector3 position)
    {
        Vector3 targetDir = position - transform.position;
        targetDir = targetDir.normalized;

        Vector3 normalizedDirection = position - transform.position;
        float whichWay = Vector3.Cross(transform.forward, normalizedDirection).y;

        float angle = Vector3.Angle(transform.forward, targetDir);
        angle *= (whichWay > 0) ? 1 : ((whichWay < 0) ? -1 : 0);

        double distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(position.x, position.z));

        float var = 2;
        if (distance > 20)
            var = 3;
        else if (distance > 15)
            var = 2;
        else if (distance > 10)
            var = 1;
        else
            var = 0.5f;

        if (whichWay >= -var && whichWay <= var)
            return 0;
        else if (whichWay > 0)
            return 1;
        else
            return -1;
        return whichWay;
    }


}
