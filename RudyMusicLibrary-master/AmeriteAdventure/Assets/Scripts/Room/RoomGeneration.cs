﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

public class RoomGeneration : MonoBehaviour
{
    private static RoomGeneration instance;
    public static RoomGeneration Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(RoomGeneration)) as RoomGeneration;
        }
        return instance;
    }

    public GameObject PlayerObject, pfRoomGroup, pfFloorPlane, pfWallCube, pfDoorCube, pfEnemy, pfHB;

    private bool created;
    private Vector3Int gridSize;

    //Variables assigned at constructor
    private Room[,] rooms;
    private Vector3Int startPos, totalRoomNum, roomSize;
    private Grid gridInstance;   
    private int numRooms;
    private Tile floor, wall, door, hallway;
    private List<Vector3Int> initRooms;

    public void RoomGenerationData(Room[,] allRooms, Vector3Int[] importVect3Ints, Grid hasTileMaps, int numberOfRooms, Tile[] allTiles, List<Vector3Int> initRoomPos)
    {
        rooms = allRooms;

        numRooms = numberOfRooms;

        startPos = importVect3Ints[0];
        totalRoomNum = importVect3Ints[1];
        roomSize = importVect3Ints[2];

        gridInstance = hasTileMaps;

        floor = allTiles[0];
        wall = allTiles[1];
        door = allTiles[2];
        hallway = allTiles[3];

        initRooms = initRoomPos;
    }

    public void Init()
    {
        if (totalRoomNum == null) { totalRoomNum = new Vector3Int(4, 4, 0); }
        if (roomSize == null) { roomSize = new Vector3Int(16, 16, 0); }
        gridSize = new Vector3Int(totalRoomNum.x * roomSize.x, totalRoomNum.y * roomSize.y, 0);
    }

    public void Create(bool playerIsInhere)
    {
        if(!created)
        {
            Pathing();
            CreateRooms();
            ConnectDoorWays();
            if (playerIsInhere) { PutPlayerInStartingRoom(); }
        }
        created = true;
    }


    public void Pathing()
    {
        for(int x = 0; x < totalRoomNum.x; ++x)
        {
            for (int y = 0; y < totalRoomNum.y; ++y)
            {
                if (rooms[x, y] != null)
                {
                    if (y < totalRoomNum.y - 1) // UP
                    {
                        if (rooms[x, y + 1] != null)
                        {
                            rooms[x, y].Opening |= Direction.Up;
                        }
                    }
                    if (x < totalRoomNum.x - 1) // RIGHT
                    {
                        if (rooms[x + 1, y] != null)
                        {
                            rooms[x, y].Opening |= Direction.Right;
                        }
                    }
                    if (y >= 1) // DOWN
                    {
                        if (rooms[x, y - 1] != null)
                        {
                            rooms[x, y].Opening |= Direction.Down;
                        }
                    }
                    if (x >= 1) // LEFT
                    {
                        if (rooms[x - 1, y] != null)
                        {
                            rooms[x, y].Opening |= Direction.Left;
                        }
                    }
                }
            }
        }
    }

    public void CreateRooms()
    {
        foreach (Vector3Int v3i in initRooms)
        {
            GameObject RoomGrouping = Instantiate(pfRoomGroup, gridInstance.transform);
            RoomGrouping.name = "Room (" + v3i.x + ", " + v3i.y + ")";
            bool isRectRoom = true;
            DoorList doorsAndMoreToAdd = new DoorList();
            //Square vs Procedurally Generated Rooms
            if (isRectRoom) { doorsAndMoreToAdd.AddDoorListRange(rooms[v3i.x, v3i.y].CreateSquareRoom(roomSize)); }
            else
            {
                doorsAndMoreToAdd.floor.AddRange(rooms[v3i.x, v3i.y].CreateFloors(roomSize));
                doorsAndMoreToAdd.wall.AddRange(rooms[v3i.x, v3i.y].CreateWalls(1 == 1));
            }
            //List<Vector3Int> floorsToAdd = rooms[v3i.x, v3i.y].CreateFloors(roomSize);
            //List<Vector3Int> wallsToAdd = rooms[v3i.x, v3i.y].CreateWalls(1==1);
            doorsAndMoreToAdd.AddDoorListRange(rooms[v3i.x, v3i.y].CreateAllDoors());
            foreach (Vector3Int f in doorsAndMoreToAdd.floor) { if (doorsAndMoreToAdd.wall.Contains(f)) { doorsAndMoreToAdd.wall.Remove(f); } }
            foreach (Vector3Int d in doorsAndMoreToAdd.door) { if (doorsAndMoreToAdd.wall.Contains(d)) { doorsAndMoreToAdd.wall.Remove(d); } }

            if (!rooms[v3i.x, v3i.y].Opening.Equals(Direction.None))
            {
                if (rooms[v3i.x, v3i.y].Opening.HasFlag(Direction.Up))
                {
                    GameObject tempUpDoor = MakeObj(pfDoorCube, v3i, rooms[v3i.x, v3i.y].doors[0].SouthWestPos, RoomGrouping.transform);
                    tempUpDoor.name = "Up";
                }
                if (rooms[v3i.x, v3i.y].Opening.HasFlag(Direction.Right))
                {
                    GameObject tempRightDoor = MakeObj(pfDoorCube, v3i, rooms[v3i.x, v3i.y].doors[1].SouthWestPos, RoomGrouping.transform);
                    tempRightDoor.transform.position =
                        new Vector3(tempRightDoor.transform.position.x - (0.5f), tempRightDoor.transform.position.y, tempRightDoor.transform.position.z + (0.5f));
                    tempRightDoor.transform.Rotate(new Vector3(0f, 90f, 0f));
                    tempRightDoor.name = "Right";
                }
                if (rooms[v3i.x, v3i.y].Opening.HasFlag(Direction.Down))
                {
                    GameObject tempDownDoor = MakeObj(pfDoorCube, v3i, rooms[v3i.x, v3i.y].doors[2].SouthWestPos, RoomGrouping.transform);
                    tempDownDoor.name = "Down";
                }
                if (rooms[v3i.x, v3i.y].Opening.HasFlag(Direction.Left))
                {
                    GameObject tempLeftDoor = MakeObj(pfDoorCube, v3i, rooms[v3i.x, v3i.y].doors[3].SouthWestPos, RoomGrouping.transform);
                    tempLeftDoor.transform.position = 
                        new Vector3(tempLeftDoor.transform.position.x - (0.5f) , tempLeftDoor.transform.position.y, tempLeftDoor.transform.position.z + (0.5f));
                    tempLeftDoor.transform.Rotate(new Vector3(0f, 90f, 0f));
                    tempLeftDoor.name = "Left";
                }
            }
            doorsAndMoreToAdd.wall.AddRange(rooms[v3i.x, v3i.y].FillWalls());
            foreach (Vector3Int wta in doorsAndMoreToAdd.wall) { GameObject wallObj = MakeObj(pfWallCube, v3i, wta, RoomGrouping.transform); }
            //foreach (Vector3Int w in doorsAndMoreToAdd.wall) { GameObject wallObj = MakeObj(pfWallCube, v3i, w, RoomGrouping.transform); }
            
            GameObject RoomFloor = Instantiate(pfFloorPlane, RoomGrouping.transform);
            RoomFloor.transform.localScale = new Vector3(roomSize.x / 10f, 1f, roomSize.y / 10f);
            RoomFloor.transform.position = new Vector3((v3i.x * roomSize.x) + (roomSize.x / 2), 0f, (v3i.y * roomSize.y) + (roomSize.y / 2));
            RoomFloor.GetComponent<NavMeshSurface>().BuildNavMesh();
            rooms[v3i.x, v3i.y].roomInGame = RoomGrouping;
        }
    }

    public GameObject MakeObj(GameObject prefabObj, Vector3Int roomIndex, Vector3Int tileIndex, Transform parentTransform)
    {
        GameObject cloneObj = Instantiate(prefabObj, parentTransform);
        cloneObj.transform.localPosition = new Vector3(((roomIndex.x * roomSize.x) + tileIndex.x + (cloneObj.transform.localScale.x / 2)), 0.99f, ((roomIndex.y * roomSize.y) + tileIndex.y + (cloneObj.transform.localScale.z / 2)));
        return cloneObj;
    }

    public void ConnectDoorWays()
    {
        foreach (Vector3Int riVector in initRooms)
        {
            if (!rooms[riVector.x, riVector.y].Opening.Equals(Direction.None))
            {
                int doorCount = 0;
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Up))
                {
                    int teleportDoorIndex = 0;
                    for (int objNum = 0; objNum < 4; ++objNum)
                    {
                        if (rooms[riVector.x, riVector.y + 1].roomInGame.transform.GetChild(objNum).gameObject.name == "Down")
                        {
                            teleportDoorIndex = objNum;
                        }
                    }
                    rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(doorCount).GetComponent<TeleportDoor>()
                        .Init(riVector.x, riVector.y, riVector.x, riVector.y + 1, rooms[riVector.x, riVector.y + 1].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(0f, 0f, 2f));
                    ++doorCount;
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Right))
                {
                    int teleportDoorIndex = 0;
                    for (int objNum = 0; objNum < 4; ++objNum)
                    {
                        if (rooms[riVector.x + 1, riVector.y].roomInGame.transform.GetChild(objNum).gameObject.name == "Left")
                        {
                            teleportDoorIndex = objNum;
                        }
                    }
                    rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(doorCount).GetComponent<TeleportDoor>()
                        .Init(riVector.x, riVector.y, riVector.x + 1, riVector.y, rooms[riVector.x + 1, riVector.y].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(2f, 0f, 0f));
                    ++doorCount;
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Down))
                {
                    int teleportDoorIndex = 0;
                    for (int objNum = 0; objNum < 4; ++objNum)
                    {
                        if (rooms[riVector.x, riVector.y - 1].roomInGame.transform.GetChild(objNum).gameObject.name == "Up")
                        {
                            teleportDoorIndex = objNum;
                        }
                    }
                    rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(doorCount).GetComponent<TeleportDoor>()
                        .Init(riVector.x, riVector.y, riVector.x, riVector.y - 1, rooms[riVector.x, riVector.y - 1].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(0f, 0f, -2f));
                    ++doorCount;
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Left))
                {
                    int teleportDoorIndex = 0;
                    for (int objNum = 0; objNum < 4; ++objNum)
                    {
                        if (rooms[riVector.x - 1, riVector.y].roomInGame.transform.GetChild(objNum).gameObject.name == "Right")
                        {
                            teleportDoorIndex = objNum;
                        }
                    }
                    rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(doorCount).GetComponent<TeleportDoor>()
                        .Init(riVector.x, riVector.y, riVector.x - 1, riVector.y, rooms[riVector.x - 1, riVector.y].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(-2f, 0f, 0f));
                    ++doorCount;
                }
            }
        }
    }

    public void PutPlayerInStartingRoom()
    {
        bool PlayerIsLost = true;
        while ( PlayerIsLost )
        {
            int xCord = Mathf.FloorToInt(Random.Range(0f, totalRoomNum.x)), zCord = Mathf.FloorToInt(Random.Range(0f, totalRoomNum.y));
            if (initRooms.Contains(new Vector3Int(xCord, zCord, 0)))
            {
                if (rooms[xCord, zCord].CountNumRoomsConnected() < 4) //is an edge room
                {
                    Vector3 roomTilePos = rooms[xCord, zCord].FindFloor();
                    PlayerObject.transform.position =
                        new Vector3(((xCord * roomSize.x) + roomTilePos.x + (PlayerObject.transform.localScale.x / 2)), 0.99f, ((zCord * roomSize.y) + roomTilePos.z + (PlayerObject.transform.localScale.z / 2)));
                    rooms[xCord, zCord].roomInGame.transform.GetChild(rooms[xCord, zCord].roomInGame.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
                    PlayerIsLost = false;
                }
            }
        }
    }

    public void EnableRooms(int RoomX, int RoomY, bool enable)
    {
        if (enable)
        {
            GameObject[] tempEnemies = new GameObject[5];
            for (int i = 0; i < 5; ++i)
            {
                Vector3 enemyStartPos = rooms[RoomX, RoomY].FindFloor();
                //Debug.Log("Enemy Start Pos: " + enemyStartPos);
                enemyStartPos.x += (RoomX * roomSize.x);
                enemyStartPos.z += (RoomY * roomSize.y);
                tempEnemies[i] = Instantiate(pfEnemy, enemyStartPos, Quaternion.identity, rooms[RoomX, RoomY].roomInGame.transform);
                //Debug.Log("Actual Enemy Start Pos: " + tempEnemies[i].transform.position);
                tempEnemies[i].GetComponent<EnemyMovement>().Player = PlayerObject;
                tempEnemies[i].GetComponent<EnemyData>().myX = RoomX;
                tempEnemies[i].GetComponent<EnemyData>().myY = RoomY;
                tempEnemies[i].GetComponent<EnemyData>().EnemyHB_Instance = Instantiate(pfHB, FindObjectOfType<Canvas>().transform).GetComponent<HealthBar>();
                tempEnemies[i].SetActive(false);
            }
            rooms[RoomX, RoomY].enableRoom(true, tempEnemies);
        }
        else { rooms[RoomX, RoomY].enableRoom(false); }
    }

    public void EnemyDied(int xIndex, int yIndex)
    {
        --rooms[xIndex, yIndex].numEnemies;
        if (rooms[xIndex, yIndex].numEnemies == 0)
        {
            rooms[xIndex, yIndex].myState = RoomState.Cleared;
            rooms[xIndex, yIndex].UnLockDoors(true, rooms[xIndex, yIndex].Opening);
        }
    }

    public List<Vector3Int> RoomScale(List<Vector3Int> vects, Vector3Int scale)
    {
        List<Vector3Int> scaledVects = new List<Vector3Int>(); 
        foreach (Vector3Int v in vects)
        {
            scaledVects.Add(new Vector3Int(((scale.x * roomSize.x) + v.x), ((scale.y * roomSize.y) + v.y), 0));
        }
        return scaledVects;
    }
}


public class Walker
{
    public Vector3Int pos, prevPos, lowerBounds, upperBounds;

    public Walker()
    {
        pos = new Vector3Int(0, 0, 0);
        lowerBounds = new Vector3Int(0, 0, 0);
        upperBounds = new Vector3Int(8, 8, 0);
        prevPos = pos;
    }

    public Walker(Vector3Int sp, Vector3Int l, Vector3Int u)
    {
        pos = sp;
        lowerBounds = l;
        upperBounds = u;
        prevPos = pos;
    }

    public Walker(int x, int y, int lowX, int lowY, int upX, int upY)
    {
        pos = new Vector3Int(x, y, 0);
        lowerBounds = new Vector3Int(lowX, lowY, 0);
        upperBounds = new Vector3Int(upX, upY, 0);
        prevPos = pos;
    }

    public Vector3Int Step()
    {
        prevPos = pos;
        int option = Random.Range(0, 4);
        switch (option)
        {
            case 0: // up
                ++pos.y;
                if (pos.y >= upperBounds.y) // if pos is out of bounds
                {
                    --pos.y; // reset pos
                    pos = Step(); // try again
                }
                break;
            case 1: // right
                ++pos.x;
                if (pos.x >= upperBounds.x) // if pos is out of bounds
                {
                    --pos.x; // reset pos
                    pos = Step(); // try again
                }
                break;
            case 2: // down
                --pos.y;
                if (pos.y < lowerBounds.y) // if pos is out of bounds
                {
                    ++pos.y; // reset pos
                    pos = Step(); // try again
                }
                break;
            case 3: // left
                --pos.x;
                if (pos.x < lowerBounds.x) // if pos is out of bounds
                {
                    ++pos.x; // reset pos
                    pos = Step(); // try again
                }
                break;
            default: //invalid option
                break;
        }
        return pos;
    }

    public Vector3Int ControlledStep(int option)
    {
        Vector3Int temp = prevPos;
        prevPos = pos;
        switch (option)
        {
            case 0: // up
                ++pos.y;
                if (pos.y >= upperBounds.y) // if pos is out of bounds
                {
                    --pos.y; // reset pos
                    prevPos = temp;
                    return temp = new Vector3Int(-1, -1, -1);
                }
                break;
            case 1: // right
                ++pos.x;
                if (pos.x >= upperBounds.x) // if pos is out of bounds
                {
                    --pos.x; // reset pos
                    prevPos = temp;
                    return temp = new Vector3Int(-1, -1, -1);
                }
                break;
            case 2: // down
                --pos.y;
                if (pos.y < lowerBounds.y) // if pos is out of bounds
                {
                    ++pos.y; // reset pos
                    prevPos = temp;
                    return temp = new Vector3Int(-1, -1, -1);
                }
                break;
            case 3: // left
                --pos.x;
                if (pos.x < lowerBounds.x) // if pos is out of bounds
                {
                    ++pos.x; // reset pos
                    prevPos = temp;
                    return temp = new Vector3Int(-1, -1, -1);
                }
                break;
            default: //invalid option
                Debug.Log("Invalid Option for Controlled Step");
                return temp = new Vector3Int(-1, -1, -1);
        }
        return pos;
    }

    public string PrintWalker() { return "Pos: " + pos + ", Lower Bounds: " + lowerBounds + ", Upper Bounds:" + upperBounds + ", Previous Positon: " + prevPos; }
}