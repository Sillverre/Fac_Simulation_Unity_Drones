using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverMovementScript : MonoBehaviour {

    public enum MOTOR { backLeft = 0, backRight, frontLeft, frontRight };

    public List<GameObject> motors;

    public float motorPower = 100.0f;

    private float powerWheelLeft = 0.0f;
    private float powerWheelRight = 0.0f;


    public void ChangeMotorSpeed (float powerWheelLeft, float powerWheelRight)
    {
        if (powerWheelLeft > 1)
        {
            powerWheelLeft = 1;
        }
        this.powerWheelLeft = powerWheelLeft;
        this.powerWheelRight = powerWheelRight;
    }

    
    public void Start()
    {
        motors = new List<GameObject>();
        motors.Add(this.transform.Find("backLeft").gameObject);
        motors.Add(this.transform.Find("backRight").gameObject);
        motors.Add(this.transform.Find("frontLeft").gameObject);
        motors.Add(this.transform.Find("frontRight").gameObject);
    }


    public void FixedUpdate()
    {
        setMotor();
    }

    float oldPowerLeft=0, oldPowerRight=0;
    private void setMotor()
    {

        if (oldPowerLeft != powerWheelLeft || oldPowerRight != powerWheelRight)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            oldPowerLeft = powerWheelLeft;
            oldPowerRight = powerWheelRight;
        }

        getCollider(MOTOR.backLeft).motorTorque = powerWheelLeft * motorPower;
        getCollider(MOTOR.frontLeft).motorTorque = powerWheelLeft * motorPower;

        getCollider(MOTOR.backRight).motorTorque = powerWheelRight * motorPower;
        getCollider(MOTOR.frontRight).motorTorque = powerWheelRight * motorPower;

        Vector3 velocity = GetComponent<Rigidbody>().velocity;
        if (Mathf.Abs(velocity.x) > 1)
            velocity.x = Mathf.Sign(velocity.x);
        if (Mathf.Abs(velocity.z) > 1)
            velocity.z = Mathf.Sign(velocity.z);
        GetComponent<Rigidbody>().velocity = velocity;
    }


    private WheelCollider getCollider(MOTOR m)
    {
        return motors[(int)m].GetComponent<WheelCollider>();
    }


}
