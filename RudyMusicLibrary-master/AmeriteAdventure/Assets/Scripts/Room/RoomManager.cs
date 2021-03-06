﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    //variables modified in inspector
    public GameObject Player, pfRoomGen, IceBoss;
    public Vector3Int numRoomsInDimension, roomSize;
    public int numRoomsInLevel;
    public Tile[] tiles;
    public RoomGeneration[] generation_Instances;
    public bool isBossRoom;

    public List<string> songTitles;

    //variables not accessible in inspector
    private List<Room[,]> allLevelRooms;
    //private List<RoomNode> RoomGraph;
    private Vector3Int startRoomPos;

    private void Start()
    {
        if (isBossRoom)
        {
            BossGen();
        }
        else
        {
            allLevelRooms = new List<Room[,]>();
            generation_Instances = new RoomGeneration[3];
            List<Vector3Int> initRoomPositionsOf0 = new List<Vector3Int>();
            Direction tempDir = Direction.Left | Direction.Right;
            string IDof0 = "Room_Generation_Instance#" + 0;

            //Init Start of Level
            generation_Instances[0] = Instantiate(pfRoomGen, this.transform).GetComponent<RoomGeneration>();
            allLevelRooms.Add(new Room[1, 1]);
            allLevelRooms[0][0, 0] = new Room(8, roomSize, tempDir);
            allLevelRooms[0][0, 0].myState = RoomState.Cleared;
            startRoomPos = new Vector3Int(0, 0, 0);
            initRoomPositionsOf0.Add(startRoomPos);
            Vector3Int[] vectsToExportof0 = { startRoomPos, new Vector3Int(1, 1, 0), roomSize };
            generation_Instances[0].myRGID = IDof0;
            generation_Instances[0].songNameTitles = songTitles;
            generation_Instances[0].startRoomVec = startRoomPos;
            generation_Instances[0].rgShift = startRoomPos;
            generation_Instances[0].RoomGenerationData(allLevelRooms[0], vectsToExportof0, generation_Instances[0].GetComponent<Grid>(), numRoomsInLevel, tiles, initRoomPositionsOf0, IDof0);
            generation_Instances[0].PlayerObject = Player;
            generation_Instances[0].Init();
            generation_Instances[0].StartCreate();

            for (int numInstance = 1; numInstance < generation_Instances.Length; ++numInstance)
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

                for (int x = 0; x < numRoomsInDimension.x; ++x)
                {
                    for (int y = 0; y < numRoomsInDimension.y; ++y)
                    {
                        if (initRoomPositions.Contains(new Vector3Int(x, y, 0)))
                        {
                            int count = 0;
                            if (initRoomPositions.Contains(new Vector3Int(x + 1, y, 0))) { ++count; }
                            if (initRoomPositions.Contains(new Vector3Int(x - 1, y, 0))) { ++count; }
                            if (initRoomPositions.Contains(new Vector3Int(x, y + 1, 0))) { ++count; }
                            if (initRoomPositions.Contains(new Vector3Int(x, y - 1, 0))) { ++count; }
                            if (count != 4 && count != 0) { generation_Instances[numInstance].startRoomVec = new Vector3Int(x, y, 0); }
                        }
                    }
                }

                for (int x2 = numRoomsInDimension.x - 1; x2 >= 0; --x2)
                {
                    for (int y2 = numRoomsInDimension.y - 1; y2 >= 0; --y2)
                    {
                        if (initRoomPositions.Contains(new Vector3Int(x2, y2, 0)))
                        {
                            int count = 0;
                            if (initRoomPositions.Contains(new Vector3Int(x2 + 1, y2, 0))) { ++count; }
                            if (initRoomPositions.Contains(new Vector3Int(x2 - 1, y2, 0))) { ++count; }
                            if (initRoomPositions.Contains(new Vector3Int(x2, y2 + 1, 0))) { ++count; }
                            if (initRoomPositions.Contains(new Vector3Int(x2, y2 - 1, 0))) { ++count; }
                            if (count != 4 && count != 0)
                            {
                                if (generation_Instances[numInstance].startRoomVec != new Vector3Int(x2, y2, 0)) { generation_Instances[numInstance].endRoomVec = new Vector3Int(x2, y2, 0); }
                            }
                        }
                    }
                }

                Vector3Int[] vectsToExport = { startRoomPos, numRoomsInDimension, roomSize };
                generation_Instances[numInstance].myRGID = ID;
                
                generation_Instances[numInstance].RoomGenerationData(allLevelRooms[numInstance], vectsToExport, generation_Instances[numInstance].GetComponent<Grid>(), numRoomsInLevel, tiles, initRoomPositions, ID);
                generation_Instances[numInstance].PlayerObject = Player;
                generation_Instances[numInstance].roomToConnectTo = allLevelRooms[0][0, 0];
                generation_Instances[numInstance].Init();
                generation_Instances[numInstance].Create(false);
                if (numInstance == 1)
                {
                    generation_Instances[numInstance].gameObject.transform.position = new Vector3(-500f, 0f, 500f);
                    generation_Instances[numInstance].rgShift = new Vector3(-500f, 0f, 500f);
                }
                else
                {
                    generation_Instances[numInstance].gameObject.transform.position = new Vector3(500f, 0f, 500f);
                    generation_Instances[numInstance].rgShift = new Vector3(500f, 0f, 500f);
                }
            }
            generation_Instances[0].FinishCreate(true);
        }
    }

    private void BossGen()
    {
        allLevelRooms = new List<Room[,]>();
        generation_Instances = new RoomGeneration[1];
        List<Vector3Int> initRoomPositionsBoss = new List<Vector3Int>();
        string IDofBoss = "Room_Generation_Instance#Boss";
        numRoomsInLevel = 2;
        allLevelRooms.Add(new Room[1, 2]);
        allLevelRooms[0][0, 0] = new Room(16, roomSize, Direction.Up); //spawn first room
        allLevelRooms[0][0, 0].myState = RoomState.Cleared;
        allLevelRooms[0][0, 1] = new Room(2, roomSize, Direction.Down); //spawn boss room
        allLevelRooms[0][0, 1].isABossRoom = true;
        generation_Instances[0] = Instantiate(pfRoomGen, this.transform).GetComponent<RoomGeneration>();
        startRoomPos = new Vector3Int(0, 0, 0);
        initRoomPositionsBoss.Add(startRoomPos);
        initRoomPositionsBoss.Add(new Vector3Int(0,1,0));
        Vector3Int[] vectsToExportof0 = { startRoomPos, new Vector3Int(1, 1, 0), roomSize };
        generation_Instances[0].myRGID = IDofBoss;
        generation_Instances[0].pfBoss = IceBoss;
        generation_Instances[0].startRoomVec = startRoomPos;
        generation_Instances[0].rgShift = startRoomPos;
        generation_Instances[0].RoomGenerationData(allLevelRooms[0], vectsToExportof0, generation_Instances[0].GetComponent<Grid>(), numRoomsInLevel, tiles, initRoomPositionsBoss, IDofBoss);
        generation_Instances[0].PlayerObject = Player;
        generation_Instances[0].Init();
        generation_Instances[0].Create(true);
    }
}

/*

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
*/
