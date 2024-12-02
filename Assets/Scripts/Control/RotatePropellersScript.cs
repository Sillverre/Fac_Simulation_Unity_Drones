using UnityEngine;
using System.Collections;

public class RotatePropellersScript : MonoBehaviour
{
    //Public
    public float timeInterval;
    public float angle;

    //Private
    private DroneMovementScript droneMovementScript;

    private float period = 0.0f;

    // Use this for initialization
    void Start()
    {
        droneMovementScript = transform.parent.GetComponent<DroneMovementScript>();
    }

    void FixedUpdate()
    {
        if (period > timeInterval)
        {
            float verticalAxis = droneMovementScript.Throttle;
            float pitchAxis = droneMovementScript.Pitch;
            float rollAxis = droneMovementScript.Roll;

            float finalAngle;
            if (verticalAxis > 0.2f || pitchAxis > 0.2f || rollAxis > 0.2f)
            {
                finalAngle = 1.7f * angle;
            }
            else if (verticalAxis < -0.2f)
            {
                finalAngle = 0.7f * angle;
            }
            else
            {
                finalAngle = angle;
            }

            transform.Rotate(new Vector3(0.0f, finalAngle, 0.0f));
            period = 0.0f;
        }
        period += UnityEngine.Time.deltaTime;
    }
}
