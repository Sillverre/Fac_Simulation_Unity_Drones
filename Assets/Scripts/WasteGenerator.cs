using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WasteGenerator : MonoBehaviour {

    public int nbSart = 5;
    public int nbMax = 20;
    public int delay = 5;

    private float lastUpdate = 0;
    private int nbWaste = 3;

    public Vector2 rangeX = new Vector2(5f, 295f);
    public Vector2 rangeZ = new Vector2(5f, 295f);

    // Use this for initialization
    void Start () {
        if (nbSart < nbWaste)
            nbSart = nbWaste;
        if (nbSart > nbMax)
            nbMax = nbSart;

        if (nbSart > nbWaste)
        {
            for(int i=0; i<nbSart-nbWaste; i++)
            {
                generateWaste();
            }
        }
    }

	// Update is called once per frame
	void Update () {
		if(Time.time - lastUpdate > delay && nbWaste < nbMax)
        {
            generateWaste();
        }
	}

    void generateWaste()
    {
        float randomVal = Random.Range(0.0f, 30.0f);
        GameObject waste;
        if (randomVal < 10)
            waste = GameObject.Find("soda1");
        else if(randomVal < 20)
            waste = GameObject.Find("Glass Bottle1");
        else
            waste = GameObject.Find("Plastic Bottle1");


        if (waste != null)
        {
            Vector3 position = new Vector3(Random.Range(rangeX.x, rangeX.y), 0.5f, Random.Range(rangeZ.x, rangeZ.y));
            GameObject o = Instantiate(waste);
            o.transform.position = position;

            lastUpdate = Time.time;
            nbWaste++;
        }
    }
}
