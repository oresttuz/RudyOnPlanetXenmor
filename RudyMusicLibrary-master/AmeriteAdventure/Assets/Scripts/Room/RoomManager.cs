using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    //variables modified in inspector
    public GameObject Player;
    public Vector3Int numRoomsInDimension, roomSize;
    public int numRoomsInLevel;
    public Grid levelGrid;
    public Tile[] tiles;

    //variables not accessible in inspector
    private RoomGeneration generation_Instance;
    private Room[,] levelRooms;
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
        levelRooms = new Room[numRoomsInDimension.x, numRoomsInDimension.y];
        startRoomPos = new Vector3Int(Random.Range(0, numRoomsInDimension.x), Random.Range(0, numRoomsInDimension.y), 0);

        levelRooms[startRoomPos.x, startRoomPos.y] = new Room(4, roomSize);

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

                levelRooms[temp.x, temp.y] = new Room(4, roomSize);
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
        generation_Instance = RoomGeneration.Instance();
        generation_Instance.RoomGenerationData(levelRooms, vectsToExport, levelGrid, numRoomsInLevel, tiles, initRoomPositions);
        generation_Instance.PlayerObject = Player;
        generation_Instance.Init();
        generation_Instance.Create();
    }

    public void DecideRoomPathway()
    {
        List<Vector2Int> edgeRoomsToChooseFrom = new List<Vector2Int>();
        for (int i = 0; i < levelRooms.GetLength(0); ++i)
        {
            for (int j = 0; j < levelRooms.GetLength(1); ++j)
            {
                if (i == 0 || i == levelRooms.GetLength(0) - 1 || j == 0 || j == levelRooms.GetLength(1) - 1)
                {
                    if (levelRooms[i, j] != null) { edgeRoomsToChooseFrom.Add(new Vector2Int(i, j)); }
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
