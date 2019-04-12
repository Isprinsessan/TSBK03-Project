using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour {

    public Grid GridScript;
    //public LayerMask unwalkableMask;
    int maxDist;
    public LayerMask unwalkableMask;
    MovementAi moveScript;
    Vector3 angleStep;
    Vector3 rayDir;

    public RaycastHit[] hit;
    int raysNr;
    // Use this for initialization
    float angle;
    float z;
    float x;

    public InfluenceMap iMap;

    public GameObject target;

	void Start () {
        maxDist = 6;
        raysNr = 20;
        angleStep = new Vector3(0.1f, 0, 0f);
        rayDir = new Vector3(1.0f, 0, 0.0f);
        hit = new RaycastHit[raysNr];
        z = 0;
        x = 0;
        angle = -Mathf.PI/2;
        moveScript = gameObject.GetComponent<MovementAi>();

	}



	// Update is called once per frame
	void Update () {
        rayDir = new Vector3(-1.0f, 0, 1.0f);
        angle = -(19*Mathf.PI)/40;
        for (int i = 0; i < raysNr; i++)
        {

            z = Mathf.Cos(angle);
            x = Mathf.Sin(angle);

             rayDir = new Vector3( x, 0, z);
            //print("z: " + z + "x: " + x);
            angle += Mathf.PI/20;

            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(rayDir), out hit[i], maxDist))
            {
                if (hit[i].collider.gameObject.name == target.name)
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(rayDir).normalized * hit[i].distance, Color.red);

                    iMap.SetPoint(hit[i].point);
                }
                else if(hit[i].collider.gameObject.tag == "Player"){
                    //annan Ai
                }
                else if (hit[i].collider.gameObject.tag == "LowBox")
                {
                    //add climbingstuff
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(rayDir).normalized * hit[i].distance, Color.yellow);
                    Node n = GridScript.GetNodeFromWorldPos(hit[i].point);
                    if (n.walkable)
                    {
                        GridScript.DisableNode(n); //n.walkable=false;
                    }
                }

            }
           
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(rayDir).normalized * maxDist, Color.white);

            }
        }
	}

    public int GetRaysNr
    {
        get
        {
            return raysNr;
        }
    }

    public int GetMaxDist
    {
        get
        {
            return maxDist;
        }
    }

    public float GetAngle
    {
        get
        {
            return -(19 * Mathf.PI) / 40;
        }
    }
    
}
