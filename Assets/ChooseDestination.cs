using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChooseDestination : MonoBehaviour
{
    private  float _xAxis;
    private  float _yAxis;
    private  float _zAxis;
    private Vector3 Min;
    private Vector3 Max;
    private Vector3 _randomDestination;
    private Vector3 _landingDestination;

    // Variable de comportement
    private  int flightMode=0;

    private DroneMovementScript movement;

    //Choix de l'altitude max
    public float yMax;

    private GameObject GameArea;

    //Drone que l'on suit en flightMode 2
    private GameObject leader;

    //Drone qui peuvent nous suivre en flightMode 0
    private GameObject droneLeft;
    private GameObject droneRight;

    void Start()
    {
        var droneRenderer = transform.Find("Quadcopter").GetComponent<Renderer>();
        droneRenderer.material.SetColor("_Color", Color.blue);

        movement = GetComponent<DroneMovementScript>();

        SetRanges();

        _xAxis = UnityEngine.Random.Range(Min.x, Max.x);
        _yAxis = UnityEngine.Random.Range(Min.y, Max.y);
        _zAxis = UnityEngine.Random.Range(Min.z, Max.z);

        _randomDestination = new Vector3(_xAxis, _yAxis, _zAxis );

        leader= null ;
        droneLeft= null ;
        droneRight= null ;
        

    }

    void Update()
    {   
        // 0 : Vol indépendant/Leader
        if(flightMode == 0){
            movement.GoToPoint( _randomDestination);

            if (WaypointReached(_randomDestination)){
                SetRanges();

                _xAxis = UnityEngine.Random.Range(Min.x, Max.x);
                _yAxis = UnityEngine.Random.Range(Min.y, Max.y);
                _zAxis = UnityEngine.Random.Range(Min.z, Max.z);

                _randomDestination = new Vector3(_xAxis, _yAxis, _zAxis );
            }
        }

        // 1 : Détection détectée, on s'écrase.
        if(flightMode == 1){

            movement.GoToPoint( _landingDestination);
        }

        // 2 : Suivi du leader.
        if(flightMode == 2){

            Vector3 nextPosition = leader.GetComponent<ChooseDestination>().askLeaderPosition(gameObject);
            movement.GoToPoint( nextPosition );
        }



    }

    private void SetRanges()
     {
        GameArea = GameObject.Find("Plane");
        Mesh AreaMesh = GameArea.GetComponent<MeshFilter>().mesh;
        Bounds bounds = AreaMesh.bounds;
        float boundsX = GameArea.transform.localScale.x * bounds.size.x;
        float boundsZ = GameArea.transform.localScale.z * bounds.size.z;
         Min = new Vector3(-boundsX/2, 2, -boundsZ/2);
         Max = new Vector3(boundsX/2, yMax, boundsZ/2);
     }

    private bool WaypointReached(Vector3 point)
    {
        Vector3 dronePos = transform.position;
        return (dronePos - point).magnitude < 0.30f;
    }



    //Detection de collision (Désactivée en attendant un déplacement avec évitement)
    void OnCollisionEnter(Collision collision)
    {
        /*
        flightMode=1;
        _landingDestination = new Vector3( transform.position.x , 0, transform.position.z);

        var droneRenderer = transform.Find("Quadcopter").GetComponent<Renderer>();
        droneRenderer.material.SetColor("_Color", Color.red); */
    }

    //Detection d'un autre drone
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hello: " + other.gameObject.name);

        if(flightMode == 1)
        {
            return;
        }
        if(other.gameObject.name == "drone(Clone)" && 
        leader == null && 
        !isMyChild(other.gameObject) &&
        other.GetComponent<ChooseDestination>().GetFlightMode() != 1 ) 
        {

                flightMode=2;
                
                var droneRenderer = transform.Find("Quadcopter").GetComponent<Renderer>();
                droneRenderer.material.SetColor("_Color", Color.yellow);

                leader = other.gameObject;
        

        }
    }

    //Getter pour les autres drones
    private int GetFlightMode()
    {
        return flightMode;
    }

    // Vérifie si le drone potentialChild ne fait pas partie des suiveurs indirects
    private bool isMyChild(GameObject potentialChild)
    {
        if (droneLeft == null && droneRight == null){
            return false;
        }
        if (droneLeft == potentialChild)
        {
            return true;
        }
        else
        {
        if (droneRight == potentialChild)
        {
            return true;
        }
        else
        {
            return (droneLeft.GetComponent<ChooseDestination>().isMyChild(potentialChild) && 
            droneRight.GetComponent<ChooseDestination>().isMyChild(potentialChild));
        }
        }
        
    }

    // Cherche récursivement sa place dans la formation
    private Vector3 askLeaderPosition(GameObject asker)
    {
        if (droneLeft == null){
            droneLeft = asker;
        }
        if ( asker == droneLeft){
            return transform.position - (transform.forward + transform.right)*5;
        }
        if (droneRight == null){
            droneRight = asker;
        }
        if ( asker == droneRight){
            return transform.position - (transform.forward + -transform.right)*5;
        }
        else
        {
            return droneLeft.GetComponent<ChooseDestination>().askLeaderPosition(asker);
        }
    }
}
