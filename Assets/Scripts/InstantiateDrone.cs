using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateDrone : MonoBehaviour
{
    private  float _xAxis;
    private  float _yAxis;
    private  float _zAxis;
    private Vector3 Min;
    private Vector3 Max;
    private Vector3 _randomPosition ;

    //Choix de l'altitude max
    public float yMax;

    public int numberToInstantiate;
    public Rigidbody dronePrefab;

    private GameObject GameArea;
    // Start is called before the first frame update
    void Start()
    {
        SetRanges();

        for(int i = 0; i < numberToInstantiate; i++){
            _xAxis = UnityEngine.Random.Range(Min.x, Max.x);
            _yAxis = UnityEngine.Random.Range(Min.y, Max.y);
            _zAxis = UnityEngine.Random.Range(Min.z, Max.z);

            _randomPosition = new Vector3(_xAxis, _yAxis, _zAxis );
            InstantiateRandomObjects();
        }

    }

    private void SetRanges()
     {
         GameArea = GameObject.Find("Plane");
        Mesh AreaMesh = GameArea.GetComponent<MeshFilter>().mesh;
        Bounds bounds = AreaMesh.bounds;
        float boundsX = GameArea.transform.localScale.x * bounds.size.x;
        float boundsZ = GameArea.transform.localScale.z * bounds.size.z;
         Min = new Vector3(-boundsX/2, 0, -boundsZ/2);
         Max = new Vector3(boundsX/2, yMax, boundsZ/2);
     }

    private void InstantiateRandomObjects()
     {
             Instantiate(dronePrefab, _randomPosition , Quaternion.identity);
     }

}
