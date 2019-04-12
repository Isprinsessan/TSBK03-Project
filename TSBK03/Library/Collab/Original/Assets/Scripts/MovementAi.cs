using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAi : MonoBehaviour {


    public float Speed = 1f;
    public float GroundDistance = 0.2f;
    public LayerMask Ground;

    private Rigidbody _body;
    private Vector3 _inputs = Vector3.zero;
  
    public int state;
    public Pathfinding pathScript;
    public Transform targetObj;
    int rot;
    float turnspeed;
    public InfluenceMap iMap;
    public Grid GridScript;

    float[] maxIM;
    int[] minIM;
    public bool targetVisible;
    Node seekNode;
    public InfluenceMap targetIM;
    public InfluenceMap enemyIM;


    void Start()
    {
        state = 1; //state 0=still, 1 = move, 2 = hunt
        _body = GetComponent<Rigidbody>();
        StartCoroutine(FSMChose());
        _inputs = Vector3.zero;
        rot = 1;
        turnspeed = 20.0f;
        targetVisible = false;
        maxIM = new float[3];
        minIM = iMap.FindMinPoint();
        seekNode = new Node(true, new Vector3(0, 0, 0), 0, 0);


    }

    void Update()
    {
        // _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);

        maxIM = iMap.FindMaxPoint();
       if (maxIM[0] > 1.0f)
        {
            pathScript.target = new Vector3(maxIM[1], 0.5f, maxIM[2]);
        //  print("X: " + maxIM[1] + "Y: " + maxIM[2]);

            if(Vector3.Distance(transform.position, pathScript.target) < 1.1f){
                //iMap.SetPointOne(pathScript.target);
                print(gameObject.name + ": FIIIIND IIIT!!");
            }
        }
        else if(targetIM.GetIMvalue(transform.position) > 0){
            float[] ret = targetIM.GetHighestSmell(transform.position,GridScript);
            pathScript.target = new Vector3(ret[1], 0.5f, ret[2]);
            Node n = GridScript.GetNodeFromWorldPos(new Vector3(ret[1], 0.5f, ret[2]));
            print(gameObject.name + ": Following smell" + ret[0] + "PosSmell: " + targetIM.GetIMvalue(transform.position) + " walkable: " + n.walkable);
            print(gameObject.name + ": target poing: " + new Vector3(ret[1], 0.5f, ret[2]) + " stand point: " + transform.position );


        }
        else if (enemyIM.GetIMvalue(transform.position) > 0.1)
        {
            float[] returLow = enemyIM.GetLowestSmell(transform.position,GridScript);


            pathScript.target = new Vector3(returLow[1], 0.5f, returLow[2]);
            Node nA = GridScript.GetNodeFromWorldPos(new Vector3(returLow[1], 0.5f, returLow[2]));

            print(gameObject.name + ":Springer från lukt" + returLow[0] + " PosSmell: " + enemyIM.GetIMvalue(transform.position)+ " walkable: " + nA.walkable);


        }
        else
        {
            if (state == 1)   //move around
            {
                //print(Vector3.Distance(transform.position, pathScript.target));
                seekNode = GridScript.GetNodeFromWorldPos(pathScript.target);

                if ((Vector3.Distance(transform.position, pathScript.target) < 1.1f) || !seekNode.walkable)
                {
                    minIM = iMap.FindMinPoint();

                }

                pathScript.target = new Vector3(minIM[0], 0.5f, minIM[1]);
                pathScript.AllowMove = true;

            }
            else if (state == 0) //state 0, stay
            {

                pathScript.AllowMove = false;
                transform.RotateAround(transform.position, Vector3.up, rot * turnspeed * Time.deltaTime);


            }
        }


    }

    /*
    void FixedUpdate()
    {
        if (state == 1 || state == 2)
        {
            _body.MovePosition(_body.position + _inputs * Speed * Time.fixedDeltaTime);
        }

    }*/


    IEnumerator  FSMChose()
    {
        float sec = 0.0f;
        while (state != 2)
        {
            
            sec = Random.Range(2.0f, 4.0f);
           // print("Sec: " + sec);
            yield return new WaitForSeconds(sec);
            state = Random.Range(0,2);
            if(state ==0){
                if(rot==1){
                    rot = -1;
                }
                else{
                    rot = 1;
                }
            }
            //print("state: " + state);

        }
    }
}
