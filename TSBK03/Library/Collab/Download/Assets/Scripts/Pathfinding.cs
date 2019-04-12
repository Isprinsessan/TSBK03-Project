using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    public Transform seeker;
    public bool move = false;
    public float moveSpeed = 0.05f;
    float turnSpeed;
    float TargetDist = 1.1f;
    public Vector3 target;
    public Animator anim;
    Grid grid;
    public bool AllowMove;
    void Awake()
    {
        grid = GetComponent<Grid>();
    }

    //För rörelse
    void Start()
    {
        AllowMove = false;
        target = new Vector3(0, 0, 0);
        FindPath(seeker.position, target);
        turnSpeed = 5.0f;
    }

    void Update()
    {
        if (AllowMove)
        {
            if (Vector3.Distance(seeker.position, target) < TargetDist)
            {
                move = false;
            }
            else
            {
                move = true;
                if ((Mathf.Abs((seeker.position.x - grid.path[0].worldPosition.x)) < 0.05) && (Mathf.Abs((seeker.position.z - grid.path[0].worldPosition.z)) < 0.05))
                {
                    FindPath(seeker.position, target);
                    //grid.path[0] = grid.path[1];
                }
                anim.SetBool("is_running", true);


                Vector3 TargetNodePos = new Vector3(grid.path[0].worldPosition.x, seeker.position.y, grid.path[0].worldPosition.z);

                seeker.transform.forward = Vector3.RotateTowards(seeker.transform.forward, TargetNodePos - seeker.position, turnSpeed * Time.deltaTime, 0.0f);
                seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, TargetNodePos, moveSpeed);
            }
        }
        else{
            anim.SetBool("is_running", false);

        }
        
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.GetNodeFromWorldPos(startPos);
        Node targetNode = grid.GetNodeFromWorldPos(targetPos);
        
        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while(openSet.Count > 0)
        {
            Node currentNode = openSet.RemoveFirst();
            /*
            for(int i = 1; i < openSet.Count; i++)
            {
                if(openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            openSet.Remove(currentNode);
            */
            closedSet.Add(currentNode);

            if(currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }
            
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if(!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }
            
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else
        {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }


    //Rörelse
    void MovePath()
    {
        move = false;
        Vector3 currentPos = seeker.position;
        if (grid.path != null)
        {
            //StartCoroutine(UpdatePosition(currentPos, grid.path[0], 0));
            Moving(currentPos, grid.path[0], 0);
        }
    }
    void Moving(Vector3 currentPos, Node node, int index)
    {
        float t = 0.0f;
        Vector3 TargetNodePos = new Vector3(grid.path[0].worldPosition.x, seeker.position.y, grid.path[0].worldPosition.z);

        while (seeker.position != TargetNodePos)
        {
            seeker.transform.forward = Vector3.RotateTowards(seeker.transform.forward, TargetNodePos - seeker.position, turnSpeed * Time.deltaTime, 0.0f);
            seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, TargetNodePos, 0.1f);
            //t += movespeed;
            //seeker.position = Vector3.Lerp(currentPos, TargetNodePos, t);
           
        }

        seeker.position = TargetNodePos;
        currentPos = TargetNodePos;

        index++;
    }

    IEnumerator UpdatePosition(Vector3 currentPos, Node node, int index)
    {
        float t = 0.0f;
        Vector3 TargetNodePos = new Vector3(node.worldPosition.x, seeker.position.y, node.worldPosition.z);

        while(seeker.position != TargetNodePos)
        {
            seeker.transform.forward = Vector3.RotateTowards(seeker.transform.forward, TargetNodePos - seeker.position, turnSpeed*Time.deltaTime, 0.0f);
            seeker.transform.position = Vector3.MoveTowards(seeker.transform.position, TargetNodePos, 0.1f);
            //t += movespeed;
            //seeker.position = Vector3.Lerp(currentPos, TargetNodePos, t);
            yield return null;
        }

        seeker.position = TargetNodePos;
        currentPos = TargetNodePos;

        index++;
       /* if(index < grid.path.Count)
        {
            StartCoroutine(UpdatePosition(currentPos, grid.path[index], index));
        }*/
    }

}
