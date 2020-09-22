using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    //variables modified in inspector
    public GameObject Player, pfRoomGen;
    public Vector3Int numRoomsInDimension, roomSize;
    public int numRoomsInLevel;
    //public Grid levelGrid;
    public Tile[] tiles;

    //variables not accessible in inspector
    private RoomGeneration[] generation_Instances;
    private List<Room[,]> allLevelRooms;
    //private Room[,] levelRooms;
    private List<RoomNode> RoomGraph;
    private Vector3Int startRoomPos;
    
    /* create the floor plan for the level like in rg
     * assign starting point
     * create graph from the level layout
     * determine pathways
     * assign doorways
     * assign endroom
     * assign all other rooms
     * 
     * generate prefab rooms
     * assign values for half-fab rooms
     * generate half-fab rooms
     * assign values for proc-gen rooms
     * generate proc-gen rooms
     */

    private void Awake()
    {
        allLevelRooms = new List<Room[,]>();
        generation_Instances = new RoomGeneration[2];
        for (int numInstance = 0; numInstance < generation_Instances.Length; ++numInstance)
        {
            string ID = "Room_Generation_Instance#" + numInstance;
            generation_Instances[numInstance] = Instantiate(pfRoomGen, this.transform).GetComponent<RoomGeneration>();

            allLevelRooms.Add(new Room[numRoomsInDimension.x, numRoomsInDimension.y]);
            startRoomPos = new Vector3Int(Random.Range(0, numRoomsInDimension.x), Random.Range(0, numRoomsInDimension.y), 0);

            allLevelRooms[numInstance][startRoomPos.x, startRoomPos.y] = new Room(4, roomSize);

            if (numRoomsInLevel > (numRoomsInDimension.x * numRoomsInDimension.y))
            {
                numRoomsInLevel = (numRoomsInDimension.x * numRoomsInDimension.y);
            }

            List<Walker> RoomPosInitWalkers = new List<Walker>();
            List<Vector3Int> initRoomPositions = new List<Vector3Int>();
            RoomPosInitWalkers.Add(new Walker(startRoomPos, new Vector3Int(0, 0, 0), numRoomsInDimension));

            initRoomPositions.Add(startRoomPos);
            int currRooms = 1;

            while (currRooms < numRoomsInLevel)
            {
                Vector3Int temp;
                foreach (Walker w in RoomPosInitWalkers)
                {
                    if (currRooms >= numRoomsInLevel) break;

                    temp = w.Step();

                    if (temp == null) break;
                    if (initRoomPositions.Contains(temp)) break;

                    allLevelRooms[numInstance][temp.x, temp.y] = new Room(4, roomSize);
                    initRoomPositions.Add(temp);
                    currRooms++;
                }
                bool ChanceToAddWalker = (Random.Range(0.0f, 1.0f) >= 0.5f);
                if (ChanceToAddWalker)
                {
                    RoomPosInitWalkers.Add(new Walker(startRoomPos, new Vector3Int(0, 0, 0), numRoomsInDimension));
                }
            }

            Vector3Int[] vectsToExport = { startRoomPos, numRoomsInDimension, roomSize };
            generation_Instances[numInstance].RoomGenerationData(allLevelRooms[numInstance], vectsToExport, generation_Instances[numInstance].GetComponent<Grid>(), numRoomsInLevel, tiles, initRoomPositions, ID);
            generation_Instances[numInstance].PlayerObject = Player;
            generation_Instances[numInstance].Init();
            if (numInstance > 0)
            {
                generation_Instances[numInstance].Create(false);
                generation_Instances[numInstance].gameObject.transform.position = new Vector3(1000f, 0f, 1000f);
            }
            else
            {
                generation_Instances[numInstance].Create(true);
            }
        }

        
        /*
        generation_Instances = new RoomGeneration[2];
        for (int numInstance = 0; numInstance < generation_Instances.Length; ++numInstance)
        {
            string ID = "Room_Generation_Instance#" + numInstance;
            generation_Instances[numInstance] = Instantiate(pfRoomGen, this.transform).GetComponent<RoomGeneration>();
            Vector3Int[] vectsToExport = { startRoomPos, numRoomsInDimension, roomSize };
            generation_Instances[numInstance].RoomGenerationData(levelRooms, vectsToExport, generation_Instances[numInstance].GetComponent<Grid>(), numRoomsInLevel, tiles, initRoomPositions, ID);
            generation_Instances[numInstance].PlayerObject = Player;
            generation_Instances[numInstance].Init();
            if (numInstance > 0)
            {
                generation_Instances[numInstance].Create(false);
                generation_Instances[numInstance].gameObject.transform.position = new Vector3(1000f, 0f, 1000f);
            }
            else
            {
                generation_Instances[numInstance].Create(true);
            }
        }
        */

    }

    public void DecideRoomPathway(int numIns)
    {
        List<Vector2Int> edgeRoomsToChooseFrom = new List<Vector2Int>();
        for (int i = 0; i < allLevelRooms[numIns].GetLength(0); ++i)
        {
            for (int j = 0; j < allLevelRooms[numIns].GetLength(1); ++j)
            {
                if (i == 0 || i == allLevelRooms[numIns].GetLength(0) - 1 || j == 0 || j == allLevelRooms[numIns].GetLength(1) - 1)
                {
                    if (allLevelRooms[numIns][i, j] != null) { edgeRoomsToChooseFrom.Add(new Vector2Int(i, j)); }
                }
            }
        }
        int randIndex1, randIndex2;
        bool findingIndexes = true;
        while (findingIndexes)
        {
            if (edgeRoomsToChooseFrom.Count == 0)
            {
                Debug.LogError("No edge rooms to choose from");
                break;
            }
            randIndex1 = Random.Range(0, edgeRoomsToChooseFrom.Count);
            randIndex2 = Random.Range(0, edgeRoomsToChooseFrom.Count);
            if (randIndex1 != randIndex2) { findingIndexes = false; }
        }
        //continue from here 9/17/20
    }
}

public class RoomNode
{
    Room myRoom;
    List<RoomNode> Neighbors;
    bool isStartRoom, isEndRoom;
    private int Step { get; set; }

    public RoomNode() //default don't use
    {
        isStartRoom = false;
        isEndRoom = false;
        Step = -1;
    }

    public RoomNode(Room r, bool start, bool end)
    {
        myRoom = r;
        isStartRoom = start;
        isEndRoom = end;
        Step = 0;
    }

    public void AddNextRoomNode(RoomNode next)
    {
        ++next.Step;
        Neighbors.Add(next);
    }
}
