using UnityEngine;
using System.Collections;

public class DroneMovementScript : MonoBehaviour
{
    //Public
    public Camera cam;
    [Range(0, 45)]
    public float maxTiltAngle = 30.0f;
    [Range(0, 45)]
    public float yawAngleSpeed = 3.0f;
    [Range(0, 10)]
    public float motorPower = 1.4f;

    public bool stabilized = true;
    public bool followingCamera = false;
    public bool isAutonomousNavigation = false;

    //Private
    private Rigidbody drone;
    private static float gravityForce = 9.81f;
    private Vector3 prevPosition;
    private bool inCollision = false;
    private float pitch = 0.0f,
                  roll = 0.0f,
                  yaw = 0.0f,
                  throttle = 0.0f;
    private float speedMS, meters = 0.0f;

    private float wantedYaw;
    private float currentRoll = 0.0f;
    private float rollVelocity;
    private float upForce;
    private float throttleVelocity;
    private float currentPitch = 0.0f;
    private float pitchVelocity;
    private Vector3 velocityToSmoothDampToZero;
    [HideInInspector] public float currentYaw;
    private float yawVelocity;


    void Awake()
    {
        drone = GetComponent<Rigidbody>();
        currentYaw = transform.localRotation.eulerAngles.y;
        wantedYaw = currentYaw;
        prevPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

    public bool IsAutonomous
    {
        get { return isAutonomousNavigation; }
        set { isAutonomousNavigation = value; }
    }

    public bool InCollision
    {
        get { return inCollision; }
    }

    // Next setters has value [-1;1]
    public float Pitch
    {
        get { return pitch; }
        set { pitch = value; }
    }

    public float Roll
    {
        get { return roll; }
        set { roll = value; }
    }

    public float Yaw
    {
        get { return yaw; }
        set { yaw = value; }
    }

    public float Throttle
    {
        get { return throttle; }
        set { throttle = value; }
    }

    private void OnCollisionEnter(Collision collision)
    {
        inCollision = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        inCollision = false;
    }

    public float GetPitchInfo()
    {
        return -currentPitch / 45.0f;
    }

    public float GetRollInfo()
    {
        return currentRoll / 45.0f;
    }

    public float SpeedMS
    {
        get { return speedMS; }
    }

    public float Meters
    {
        get { return meters; }
    }

    void FixedUpdate()
    {
        //Speed measurement
        float dt = Time.deltaTime;
        Vector3 currPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        float m = (currPosition - prevPosition).magnitude;
        speedMS = m / dt;
        meters += m;
        prevPosition = currPosition;

        PitchUpdate();
        RollUpdate();
        YawUpdate(); //Rotation
        MovementUpDownUpdate();

        drone.rotation = Quaternion.Euler(
            new Vector3(currentPitch, currentYaw, currentRoll)
        );

        ClampingSpeedValues();

        if (followingCamera)
        {
            Quaternion camYaw = Quaternion.AngleAxis(currentYaw + 180.0f, Vector3.up);
            cam.transform.position = drone.transform.position + new Vector3(4.5f * drone.transform.forward.x, 0.5f, 4.5f * drone.transform.forward.z);
            cam.transform.rotation = camYaw;
        }
        Vector3 v = drone.velocity;
        v.x = Mathf.Sign(v.x) * Mathf.Min(Mathf.Abs(v.x), 3);
        v.z = Mathf.Sign(v.z) * Mathf.Min(Mathf.Abs(v.z), 3);

        drone.velocity = v;
    }

    void MovementUpDownUpdate()
    {
        if (!isAutonomousNavigation)
        {
            throttle = 0.0f;
            float verticalAxis = Input.GetAxis("Vertical");
            if (Mathf.Abs(verticalAxis) > 0.2f)
            {
                throttle += verticalAxis;
            }
        }

        if (Mathf.Abs(pitch) > 0.2f || Mathf.Abs(roll) > 0.2f)
        {
            float pctLean = Mathf.Max(Mathf.Abs(currentRoll), Mathf.Abs(currentPitch)) / 45.0f; //[0;1]
            if (stabilized)
                upForce = gravityForce * (1.0f + (pctLean * pctLean) * 0.6f + throttle * motorPower / 5.0f);
            else
                upForce = gravityForce * (1.0f + (pctLean * pctLean) * 0.5f + throttle * motorPower / 5.0f);
        }
        else
        {
            upForce = gravityForce * (1.0f + throttle * motorPower / 5.0f);
        }

        if (isAutonomousNavigation || (!isAutonomousNavigation && Mathf.Abs(throttle) > 0.2f))
        {
            float mp2 = motorPower * 5.0f;
            if (drone.velocity.y > mp2)
                drone.velocity = new Vector3(drone.velocity.x, mp2, drone.velocity.z);
            else if (drone.velocity.y < -mp2)
                drone.velocity = new Vector3(drone.velocity.x, -mp2, drone.velocity.z);
        }
        else
        {
            if (stabilized)
                drone.velocity = new Vector3(drone.velocity.x, Mathf.SmoothDamp(drone.velocity.y, 0.0f, ref throttleVelocity, 1.0f), drone.velocity.z);
            else
                drone.velocity = new Vector3(drone.velocity.x, Mathf.SmoothDamp(drone.velocity.y, 0.0f, ref throttleVelocity, drone.velocity.y), drone.velocity.z);
        }
        drone.AddRelativeForce(Vector3.up * upForce);
    }

    void PitchUpdate()
    {
        if (!isAutonomousNavigation)
        {
            pitch = 0.0f;
            float input = -Input.GetAxis("RightV");

            if (Mathf.Abs(input) > 0.2f)
                pitch += input;
        }

        if (isAutonomousNavigation || (!isAutonomousNavigation && Mathf.Abs(pitch) > 0.2f))
        {
            drone.AddRelativeForce(Vector3.forward * pitch * (Mathf.Abs(currentPitch) / maxTiltAngle) * motorPower);
            currentPitch = Mathf.SmoothDamp(currentPitch, maxTiltAngle * pitch, ref pitchVelocity, 0.2f);
        }
        else
        {
            currentPitch = Mathf.SmoothDamp(currentPitch, 0.0f, ref pitchVelocity, 0.2f);
        }
    }

    void RollUpdate()
    {
        if (!isAutonomousNavigation)
        {
            roll = 0.0f;
            float input = Input.GetAxis("RightH");

            if (Mathf.Abs(input) > 0.2f)
                roll += input;
        }

        if (isAutonomousNavigation || (!isAutonomousNavigation && Mathf.Abs(roll) > 0.2f))
        {
            drone.AddRelativeForce(Vector3.left * roll * (Mathf.Abs(currentRoll) / maxTiltAngle) * motorPower);
            currentRoll = Mathf.SmoothDamp(currentRoll, maxTiltAngle * roll, ref rollVelocity, 0.2f);
        }
        else
        {
            currentRoll = Mathf.SmoothDamp(currentRoll, 0.0f, ref rollVelocity, 0.2f);
        }
    }

    public float WantedYaw
    {
        get { return wantedYaw; }
        set { wantedYaw = value; }
    }


    void YawUpdate()
    {
        if (!isAutonomousNavigation)
        {
            yaw = 0.0f;
            float input = Input.GetAxis("Horizontal");
            if (Mathf.Abs(input) > 0.2f)
            {
                wantedYaw += input * yawAngleSpeed;
            }
        }
        wantedYaw += yaw * yawAngleSpeed;
        currentYaw = Mathf.SmoothDamp(currentYaw, wantedYaw, ref yawVelocity, 0.25f);
    }

    void ClampingSpeedValues()
    {
        float absPitch = Mathf.Abs(pitch);
        float absRoll = Mathf.Abs(roll);
        if ((isAutonomousNavigation && absPitch > 0.0f && absRoll > 0.0f) || (absPitch > 0.2f && absRoll > 0.2f))
        {
            drone.velocity = Vector3.ClampMagnitude(
                drone.velocity,
                Mathf.Lerp(drone.velocity.magnitude, Mathf.Max(absRoll, absPitch) / maxTiltAngle * motorPower * motorPower, Time.deltaTime / motorPower/*increase for longer*/)
            );
        }
        else if ((isAutonomousNavigation && absPitch > 0.0f && absRoll == 0.0f) || (absPitch > 0.2f && absRoll < 0.2f))
        {
            drone.velocity = Vector3.ClampMagnitude(
                drone.velocity,
                Mathf.Lerp(drone.velocity.magnitude, absPitch / maxTiltAngle * motorPower * motorPower, Time.deltaTime / motorPower/*increase for longer*/)
            );
        }
        else if ((isAutonomousNavigation && absPitch == 0.0f && absRoll > 0.0f) || (absPitch < 0.2f && absRoll > 0.2f))
        {
            drone.velocity = Vector3.ClampMagnitude(
                drone.velocity,
                Mathf.Lerp(drone.velocity.magnitude, absRoll / maxTiltAngle * motorPower * motorPower, Time.deltaTime / motorPower/*increase for longer*/)
            );
        }
        else if ((isAutonomousNavigation && absPitch == 0.0f && absRoll == 0.0f && throttle == 0.0f) || (absPitch < 0.2f && absRoll < 0.2f && Mathf.Abs(throttle) < 0.2f))
        {
            if (stabilized)
            {
                drone.velocity = Vector3.SmoothDamp(
                    drone.velocity,
                    Vector3.zero,
                    ref velocityToSmoothDampToZero,
                    0.75f //increase to longer stop
                );
            }
            else
            {
                drone.velocity = Vector3.SmoothDamp(
                    drone.velocity,
                    Vector3.zero,
                    ref velocityToSmoothDampToZero,
                    drone.velocity.magnitude / 2.0f //increase to longer stop
                );
            }
        }
    }

    
    public void GoToPoint(Vector3 point)
    {
        Vector3 dronePos = drone.transform.position;
        Vector3 direction = (point - dronePos);
        float distToPoint = direction.magnitude;
        float deniv = direction.y;

        if (Mathf.Abs(deniv) > 1.0f)
            deniv /= Mathf.Abs(deniv);
        direction.Normalize();
        if (distToPoint < 10f)
        {
            direction *= distToPoint / 10.0f;
            deniv /= 2f;
        }

        Throttle = deniv;

        Vector2 droneForward = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 droneToDirection = new Vector2(point.x, point.z) - new Vector2(dronePos.x, dronePos.z);
        float angleDest = Vector2.SignedAngle(new Vector2(1, 0), droneToDirection);
        float angleRobot = Vector2.SignedAngle(new Vector2(1, 0), new Vector2(this.transform.forward.x, this.transform.forward.z));
        float angle = Mathf.DeltaAngle(angleRobot, angleDest);

        if (Mathf.Abs(angle) > 5)
        {
            Yaw = -Mathf.Sign(angle) * 0.4f;
            Pitch = 0;
            Roll = 0;
        }
        else if (!WaypointReached(point))
        {
            Yaw = 0;
            Roll = 0;
            Pitch = distToPoint < 10f ? distToPoint / 10f + 0.01f: 1 ;
        }
        else
        {
            Roll = 0f;
            Pitch = 0f;
            Yaw = 0f;
        }
    }

    bool WaypointReached(Vector3 point)
    {
        Vector3 dronePos = drone.transform.position;
        return (dronePos - point).magnitude < 0.15f;
    }

}
