using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Communication {

    private Wifi wifi;
    private float wastePositionSentAt = 0;
    private float delay = 5;
    public Communication(Wifi wifi)
    {
        this.wifi = wifi;
    }


    byte[] encodeWaste(Waste w)
    {
        byte[] bytesTag = System.Text.Encoding.ASCII.GetBytes(w.Tag);
        byte[] bytesX = BitConverter.GetBytes(w.Position.x);
        byte[] bytesY = BitConverter.GetBytes(w.Position.y);
        byte[] bytesZ = BitConverter.GetBytes(w.Position.z);
        byte[] bytesAt = BitConverter.GetBytes(w.At);
        byte[] msg = new byte[bytesTag.Length + bytesX.Length + bytesY.Length + bytesZ.Length + bytesAt.Length + 2];

        int msgIndex = 1;
        System.Array.ConstrainedCopy(bytesX, 0, msg, msgIndex, bytesX.Length);
        msgIndex += bytesX.Length;
        System.Array.ConstrainedCopy(bytesY, 0, msg, msgIndex, bytesY.Length);
        msgIndex += bytesY.Length;
        System.Array.ConstrainedCopy(bytesZ, 0, msg, msgIndex, bytesZ.Length);
        msgIndex += bytesZ.Length;
        System.Array.ConstrainedCopy(bytesAt, 0, msg, msgIndex, bytesAt.Length);
        msgIndex += bytesAt.Length;
        System.Array.ConstrainedCopy(bytesTag, 0, msg, msgIndex, bytesTag.Length);
        msgIndex += bytesTag.Length;
        msg[0] = 1;
        msg[msgIndex] = (byte)(w.Pickedup ? 1 : 0);

        return msg;
    }

    byte[] encodeAlive(int id)
    {
        byte[] msg = new byte[2];
        msg[0] = 2;
        msg[1] = (byte)id;

        return msg;
    }

    public void sendData(List<Waste> wasteList)
    {
        // send wastes position to others
        if (Time.time - wastePositionSentAt > delay && wifi.getRobotAround().Count > 0)
        {
            foreach (Waste w in wasteList)
            {
                wifi.SendAll(encodeWaste(w));
            }
            wastePositionSentAt = Time.time;
        }
    }

    public void sendAlive(int id)
    {
        List<GameObject> robots = wifi.getRobotAround();
        foreach (GameObject robot in robots)
        {
            DroneIA drone = (DroneIA)robot.GetComponent("DroneIA");
            if (drone != null && drone.id == id + 1)
            {
                wifi.Send(robot, encodeAlive(id));
            }
        }
    }

    



}
