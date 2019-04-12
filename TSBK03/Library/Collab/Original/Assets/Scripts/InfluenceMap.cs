using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InfluenceMap : MonoBehaviour {

    public Transform PlayerPos;
    public static int ImapSize = 50;
    float[,] Imap = new float[ImapSize, ImapSize];
    int[,] InSight = new int[ImapSize, ImapSize];

    //En minut att gå från vitt till svart i 60 fps = 0.99994f, bra värde: 0.999f
    float InfluenceDelay = 0.99f;
    //Hörselavstånd
    float HearingDistance = 2.0f;
    //Hur mycker längre man ser än Raycasten
    float ExtraViewingDistance = 4.0f;
    //Avstånd mellan position i Imapen och Spelaren
    float Dist = 0.0f;
    int X, Y;

    public Image Im;
    
    Grid grid;

    Raycast RayC;
    RaycastHit hit;
    //*******************************************************
    //Göra så att man ser pathen för alla karaktärer, inte bara en
    //Göra så man kan klättra över väggar (Inte jätteprioriterat)
    //Gör så de flyr när de ser sin fiende
    //Skapa en mask som i Aoe2, fog of war för influence

    //Problemet verkar vara att när en har hittat en annan och den andra springer ifrån den så vet den inte vad den ska göra. Den är fast i "hittad"-loopen
    //*******************************************************

    void Awake () {
        //Hämtar griden
        grid = GetComponent<Grid>();

        //Hämtar canvas-bilden, gör det där uppe istället.
        //Im = GameObject.FindWithTag("ImapImage").GetComponent<Image>();

        //Sätter alla värden i Imap till 0
        InitMap();

    }

    void Update () {
        //Uppdatera influencemappen
        CreateInfluenceMap();
        //Uppdatera bilden
        CreateImage();
    }

    //Initialisera influencemappen så den har värden
    void InitMap()
    {
        for(int i = 0; i < ImapSize; i++)
        {
            for (int j = 0; j < ImapSize; j++)
            {
                Imap[i, j] = 0.0f;
            }
        }
    }

    //Skapar influence mapen med avseende på AI:ns position
    void CreateInfluenceMap()
    {
        for (int x = -ImapSize / 2; x < ImapSize / 2; x++)
        {

            //Byter koordinatsystem
            X = World2Indices(x);

            for (int y = -ImapSize / 2; y < ImapSize / 2; y++)
            {
                //Position för AI:n
                Vector2 Ppos = new Vector2(PlayerPos.position.x, PlayerPos.position.z);

                //Position från influencemappen i världskoordinater
                Vector2 Ipos = new Vector2(x, y);

                //Vektor för spelarens riktining i lokalt koordinatsystem
                Vector2 FacingDir = new Vector2(PlayerPos.transform.forward.x + PlayerPos.transform.position.x, PlayerPos.transform.forward.z + PlayerPos.transform.position.z);

                //Skapar vektorer av positionerna för att kunna ränka ut vinkel och avstånd
                Vector2 VectorToImapPoint = Ipos - Ppos;
                Vector2 FacingDirectionVector = FacingDir - Ppos;

                //Avstånd mellan punkten och AI:n
                Dist = Mathf.Sqrt(Mathf.Pow(VectorToImapPoint.x, 2) + Mathf.Pow(VectorToImapPoint.y, 2));

                //Byter koordinatsystem
                Y = World2Indices(y);

                //Hämtar vinkeln och gör om till grader
                float rayAngle = PlayerPos.GetComponent<Raycast>().GetAngle * Mathf.Rad2Deg;

                //Hämta maxlängden raycasten ska gå
                float MaxDist = PlayerPos.GetComponent<Raycast>().GetMaxDist;

                //Gör om vektorerna till vector3 för att slippa y-värdet
                Vector3 Vector3PlayerPos = new Vector3(Ppos.x, 0.0f, Ppos.y);
                Vector3 Vector3ToImapPoint = new Vector3(VectorToImapPoint.x, 0.0f, VectorToImapPoint.y);

                //Om vinkeln och avståndet båda är inom synfältet, sätt färg. Samma sak om punkten är inom hörselavstånd 
                if ((Vector2.Angle(FacingDirectionVector, VectorToImapPoint) < rayAngle && Dist < MaxDist + ExtraViewingDistance) || Dist - HearingDistance < HearingDistance)
                {
                    //Om raycasten kolliderar med ett objekt som inte är målet
                    if (Physics.Raycast(Vector3PlayerPos, Vector3ToImapPoint, out hit, MaxDist + ExtraViewingDistance))
                    {
                        //Om avståndet till punkten är mindre än avståndet till intersecten, sätt den till vit annars låt den gå mot svart
                        if (Dist < hit.distance)
                        {
                            Imap[X, Y] = 1.0f - 0.05f * Dist;
                            InSight[X, Y] = 1;
                        }
                        else
                        {
                            Imap[X, Y] = Imap[X, Y] * InfluenceDelay;
                            InSight[X, Y] = 0;
                        }
                    }
                    else
                    {
                        Imap[X, Y] = 1.0f - 0.05f * Dist;
                        InSight[X, Y] = 1;
                    }

                }
                else //Om den är utanför synfältet, låt färgen gå mot svart
                {
                    Imap[X, Y] = Imap[X, Y] * InfluenceDelay;
                    InSight[X, Y] = 0;
                }
            }
        }
    }

    //Skapar influencemap bilden
    void CreateImage()
    {
        Texture2D texture = new Texture2D(ImapSize, ImapSize);
        Sprite sprite;

        for(int i = 0; i < ImapSize; i++)
        {
            for(int j = 0; j < ImapSize; j++)
            {
                Color color = new Color(0.0f, 0.0f, 0.0f, 0.0f);

                //Sätter fiendens färg till rött och målets färg till grön i influence mappen om den är i synfältet
                if (PlayerPos.GetComponent<MovementAi>().targetIM.Imap[i, j] > 0.0f && InSight[i, j] == 1)
                {
                    color = new Color(0.0f, Imap[i, j], 0.0f);
                }
                else if (PlayerPos.GetComponent<MovementAi>().enemyIM.Imap[i, j] > 0.0f && InSight[i, j] == 1)
                {
                    color = new Color(Imap[i, j], 0.0f, 0.0f);
                }
                else
                {
                    color = new Color(Imap[i, j], Imap[i, j], Imap[i, j]);
                }
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
        Vector2 PivotPoint = new Vector2(0.5f, 0.5f); 
        sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, ImapSize, ImapSize), PivotPoint);
        Im.sprite = sprite;
    }

    //Byter från världskoordinater till influencemap position
    int World2Indices(int WorldPos)
    {
        int IndicePos = WorldPos + 25;
        return IndicePos;
    }

    //Byter från position i influence map till världsposition
    int Indices2World(int IndicePos)
    {
        int WorldPos = IndicePos - 25;
        return WorldPos;
    }

    public float GetIMvalue(Vector3 WorldPos){

        int x = (int)Mathf.Round(WorldPos.x);
        int y = (int)Mathf.Round(WorldPos.z);
          
        return Imap[World2Indices(x), World2Indices(y)];

    }

    public float[] GetHighestSmell(Vector3 WorldPos, Grid gridSc){
        int x = (int)Mathf.Round(WorldPos.x);
        int y = (int)Mathf.Round(WorldPos.z);

        x = World2Indices(x);
        y = World2Indices(y);

        int radius = 3;
        float maxValue = 0;
        int xLow = 0;
        int xMax = 0;
        int yLow = 0;
        int yMax = 0;

        Vector3 maxPos = new Vector3(0,0,0);

        xLow = Mathf.Clamp(x - radius, 0, 50);
        xMax = Mathf.Clamp(x + radius, 0, 50);
        yLow = Mathf.Clamp(y - radius, 0, 50);
        yMax = Mathf.Clamp(y + radius, 0, 50);

        //Vector2 playerPos = new Vector2(x, y);
        //Vector2 oldPos = new Vector2(0,0);
        for (int xStep =xLow; xStep < xMax; xStep++)
        {
            for (int yStep = yLow; yStep < yMax; yStep++)
            { 
                Node n = gridSc.GetNodeFromWorldPos(new Vector3(Indices2World(xStep), 0, Indices2World(yStep)));
                if (Vector3.Distance(WorldPos, new Vector3(Indices2World(xStep), 0, Indices2World(yStep))) < 1.4)
                {
                }
              else if(Imap[xStep,yStep] > maxValue && n.walkable){
                    maxValue = Imap[xStep, yStep];
                    maxPos = new Vector3((float)Indices2World(xStep), 0, (float)Indices2World(yStep));
                   // oldPos = new Vector2(xStep, yStep);

                }
                /*else if((Imap[xStep,yStep] - maxValue) <0.01){
                    if(Vector2.Distance(playerPos,oldPos) < Vector2.Distance(playerPos,new Vector2(xStep,yStep))){
                        maxValue = Imap[xStep, yStep];
                        maxPos = new Vector3(Indices2World(xStep), 0, Indices2World(yStep));
                        oldPos = new Vector2(xStep, yStep);
                    }
                }*/

            
            }
        }
        float[] test = new float[3];
        test[0] = maxValue;
        test[1] = maxPos.x;
        test[2] = maxPos.z;
        return test;
    }
    public float[] GetLowestSmell(Vector3 WorldPos, Grid gridSc)
    {
        int x = (int)Mathf.Round(WorldPos.x);
        int y = (int)Mathf.Round(WorldPos.z);

        x = World2Indices(x);
        y = World2Indices(y);

        int radius = 3;
        float minValue = 10;
        int xLow = 0;
        int xMax = 0;
        int yLow = 0;
        int yMax = 0;

        float pointValue = Imap[x, y];
        Vector3 minPos = new Vector3(0, 0, 0);

        xLow = Mathf.Clamp(x - radius, 0, 50);
        xMax = Mathf.Clamp(x + radius, 0, 50);
        yLow = Mathf.Clamp(y - radius, 0, 50);
        yMax = Mathf.Clamp(y + radius, 0, 50);

        //Vector2 playerPos = new Vector2(x, y);
        //Vector2 oldPos = new Vector2(0,0);
        for (int xStep = xLow; xStep < xMax; xStep++)
        {
            for (int yStep = yLow; yStep < yMax; yStep++)
            {
                Node n = gridSc.GetNodeFromWorldPos(new Vector3(Indices2World(xStep), 0, Indices2World(yStep)));
                if (Vector3.Distance(WorldPos, new Vector3(Indices2World(xStep), 0, Indices2World(yStep))) < 1.4)
                {
                }
                else if (pointValue > 0.15f) {
                    if (Imap[xStep, yStep] > 0)
                    {

                        if (Imap[xStep, yStep] < minValue && n.walkable)
                        {
                            minValue = Imap[xStep, yStep];
                            minPos = new Vector3(Indices2World(xStep), 0, Indices2World(yStep));
                            // oldPos = new Vector2(xStep, yStep);

                        }
                    }

                }
                else
                {
                    
                    if (Imap[xStep, yStep] < minValue && n.walkable)
                    {
                        minValue = Imap[xStep, yStep];
                        minPos = new Vector3(Indices2World(xStep), 0, Indices2World(yStep));
                        // oldPos = new Vector2(xStep, yStep);

                    }
                        
                }




            }
        }
        float[] retVal = new float[3];
        retVal[0] = minValue;
        retVal[1] = minPos.x;
        retVal[2] = minPos.z;
        return retVal;
    }


    //Hitta lägsta värdet i Imap, Den här borde nog egentligen ligga i pathfinder
    public int[] FindMinPoint()
    {
        //Array att lagra alla minsta värden i, 0 = x värde 1 = y värde
        float[,] PointArr = new float[2500, 2];
        float MinValue = 1.0f;
        int counter = 0;

        for (int x = 0; x < ImapSize; x++)
        {
            for(int y = 0; y < ImapSize; y++)
            {
                //Om två värden är lika låga, spara båda i en array förutsatt att dom är walkable
                Vector3 tempVec = new Vector3(x, y, 1.0f);
                if(Imap[x, y] - MinValue < 0.001f && grid.GetComponent<Grid>().GetNodeFromWorldPos(tempVec).walkable)
                {
                    PointArr[counter, 0] = x;
                    PointArr[counter, 1] = y;
                    counter++;
                }
                else if (Imap[x, y] < MinValue)
                {
                    MinValue = Imap[x, y];

                    //Rensa arrayen om vi hittar ett mindre värde
                    counter = 0;
                    Array.Clear(PointArr, 0, PointArr.Length);
                    
                    //Sätt första punkten till det nya värdet
                    PointArr[counter , 0] = x;
                    PointArr[counter , 1] = y;
                    counter++;
                }   
            }
        }

        //Randoma plats i arrayen
        int randomPos = Mathf.RoundToInt(UnityEngine.Random.Range(0.0f, counter - 1.0f));
        //Sätt x och y värde till den randomade punktens koordinater
        int FinalX = Mathf.RoundToInt(PointArr[randomPos, 0]);
        int FinalY = Mathf.RoundToInt(PointArr[randomPos, 1]);

        int[] FinalPoint = new int[2];
        FinalPoint[0] = Indices2World(FinalX);
        FinalPoint[1] = Indices2World(FinalY);

        return FinalPoint;
    }

    //Hitta största värdet i imappen, används för att jaga spelaren
    public float[] FindMaxPoint()
    {
        float[] MaxPoint = new float[3];
        float MaxValue = 1.05f;

        for(int x = 0; x < ImapSize; x++)
        {
            for(int y = 0; y < ImapSize; y++)
            {
                if(Imap[x, y] > MaxValue)
                {
                    MaxValue = Imap[x, y];
                    MaxPoint[0] = MaxValue;
                    MaxPoint[1] = (float) Indices2World(x);
                    MaxPoint[2] = (float) Indices2World(y);
                }
            }
        }
        return MaxPoint;
    }

    //Sätt en punkt i influencemappen till ett satt värde
    public void SetPoint(Vector3 objectPos)
    {
        int[] ImapPos = new int[2];
        ImapPos[0] = World2Indices(Mathf.RoundToInt(objectPos.x));
        ImapPos[1] = World2Indices(Mathf.RoundToInt(objectPos.z));
        //print("x " + ImapPos[0]);
        //print("y " + ImapPos[1]);
        Imap[ImapPos[0], ImapPos[1]] = 2.0f;
    }

    public void SetPointOne(Vector3 objectPos)
    {
        int[] ImapPos = new int[2];
        ImapPos[0] = World2Indices(Mathf.RoundToInt(objectPos.x));
        ImapPos[1] = World2Indices(Mathf.RoundToInt(objectPos.z));
        Imap[ImapPos[0], ImapPos[1]] = 1.0f;
    }
}

