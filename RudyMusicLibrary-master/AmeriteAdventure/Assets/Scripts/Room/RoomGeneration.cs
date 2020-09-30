using System.Collections;
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

    public GameObject PlayerObject, pfRoomGroup, pfFloorPlane, pfWallCube, pfDoorCube, pfHB, pfExit, pfLight, pfBook;
    public GameObject[] pfEnemies;
    public string myRGID;
    public List<string> songNameTitles;
    public Vector3Int startRoomVec = new Vector3Int(-1,-1,-1), endRoomVec = new Vector3Int(-1, -1, -1);
    public Room roomToConnectTo;
    public Vector3 rgShift;

    public Material[] materials;
    public Sprite[] bookSprites;

    private bool created;
    private Vector3Int gridSize;

    //Variables assigned at constructor
    private Room[,] rooms;
    private Vector3Int startPos, totalRoomNum, roomSize;
    private Grid gridInstance;   
    private int numRooms;
    private Tile floor, wall, door, hallway;
    private List<Vector3Int> initRooms;

    public void RoomGenerationData(Room[,] allRooms, Vector3Int[] importVect3Ints, Grid hasTileMaps, int numberOfRooms, Tile[] allTiles, List<Vector3Int> initRoomPos, string name)
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
        myRGID = name;
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
            CreateRooms(false);
            ConnectDoorWays();
            if (playerIsInhere) { PutPlayerInStartingRoom(); }
        }
        created = true;
    }

    public void StartCreate()
    {
        if (!created)
        {
            Pathing();
            CreateRooms(true);
        }
        created = true;
    }

    public void FinishCreate(bool playerIsInhere)
    {
        if (created)
        {
            int yes = 0;
            ConnectDoorWays(yes);
            if (playerIsInhere) { PutPlayerInStartingRoom(); }
        }
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
                    if (startRoomVec.z != -1 && totalRoomNum.x != 1 && totalRoomNum.y != 1)
                    {
                        if (x == startRoomVec.x && y == startRoomVec.y) //START ROOM; needs to an edge room
                        {
                            if (!rooms[x, y].Opening.HasFlag(Direction.Right))
                            {
                                rooms[x, y].Opening |= Direction.Right;
                                rooms[x, y].DirectionToStart = Direction.Right;
                            }
                            else if (!rooms[x, y].Opening.HasFlag(Direction.Down))
                            {
                                rooms[x, y].Opening |= Direction.Down;
                                rooms[x, y].DirectionToStart = Direction.Down;
                            }
                            else if (!rooms[x, y].Opening.HasFlag(Direction.Left))
                            {
                                rooms[x, y].Opening |= Direction.Left;
                                rooms[x, y].DirectionToStart = Direction.Left;
                            }
                            else if (!rooms[x, y].Opening.HasFlag(Direction.Up))
                            {
                                rooms[x, y].Opening |= Direction.Up;
                                rooms[x, y].DirectionToStart = Direction.Up;
                            }
                        }
                    }
                }
            }
        }
    }

    public void CreateRooms(bool isRectRoom)
    {
        foreach (Vector3Int v3i in initRooms)
        {
            GameObject RoomGrouping = Instantiate(pfRoomGroup, gridInstance.transform);
            RoomGrouping.name = "Room (" + v3i.x + ", " + v3i.y + ")";
            DoorList doorsAndMoreToAdd = new DoorList();
            //Square vs Procedurally Generated Rooms
            if (isRectRoom) { doorsAndMoreToAdd.AddDoorListRange(rooms[v3i.x, v3i.y].CreateSquareRoom(roomSize)); }
            else
            {
                doorsAndMoreToAdd.floor.AddRange(rooms[v3i.x, v3i.y].CreateFloors(roomSize));
                doorsAndMoreToAdd.wall.AddRange(rooms[v3i.x, v3i.y].CreateWalls(1 == 1));
            }
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
                        new Vector3(tempRightDoor.transform.position.x - (0.5f * 2f), tempRightDoor.transform.position.y, tempRightDoor.transform.position.z + (0.5f * 2f));
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
                        new Vector3(tempLeftDoor.transform.position.x - (0.5f * 2f) , tempLeftDoor.transform.position.y, tempLeftDoor.transform.position.z + (0.5f * 2f));
                    tempLeftDoor.transform.Rotate(new Vector3(0f, 90f, 0f));
                    tempLeftDoor.name = "Left";
                }
            }
            List<Vector3Int> fillWalls = rooms[v3i.x, v3i.y].FillWalls();
            foreach (Vector3Int wta in doorsAndMoreToAdd.wall)
            {
                GameObject wallObj = MakeObj(pfWallCube, v3i, wta, RoomGrouping.transform);
                if (Random.Range(0, 1f) < 0.1f)
                {
                    GameObject tempLight = Instantiate(pfLight, wallObj.transform);
                    tempLight.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                }
            }
            foreach (Vector3Int fwta in fillWalls)
            {
                GameObject wallObj = MakeObj(pfWallCube, v3i, fwta, RoomGrouping.transform);
                wallObj.GetComponent<MeshRenderer>().material = materials[0];
            }
            GameObject RoomFloor = Instantiate(pfFloorPlane, RoomGrouping.transform);
            RoomFloor.transform.localScale = new Vector3(roomSize.x / (10f), 1f, roomSize.y / 10f);
            RoomFloor.transform.position = new Vector3(((v3i.x * roomSize.x) + (roomSize.x / 2)) * 2f, 0f, ((v3i.y * roomSize.y) + (roomSize.y / 2)) * 2f);
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
                int doorCount = 0, thisDoorWasMade = -1;
                if (riVector.Equals(startRoomVec))
                {
                    if (myRGID == "Room_Generation_Instance#1")
                    {
                        int teleportDoorIndex = 0;
                        string directionString = rooms[riVector.x, riVector.y].DirectionToString(rooms[riVector.x, riVector.y].DirectionToStart);
                        for (int objNum = 0; objNum < 4; ++objNum)
                        {
                            if (rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(objNum).gameObject.name == directionString) { teleportDoorIndex = objNum; }
                        }
                        rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>()
                        .Init(this, riVector.x, riVector.y, 0, 0, roomToConnectTo.roomInGame.transform.GetChild(0).gameObject, new Vector3(-2f, 0f, 0f));
                        rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>().myOtherParent = GetComponentInParent<RoomManager>().generation_Instances[0];
                        rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>().startDoor = 3;
                        rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>().themeToPlay = "MainMenu Theme";
                        thisDoorWasMade = rooms[riVector.x, riVector.y].DirectionToInt(rooms[riVector.x, riVector.y].DirectionToStart);
                        Debug.Log(myRGID + ", this door: " + thisDoorWasMade);
                    }
                    if (myRGID == "Room_Generation_Instance#2")
                    {
                        int teleportDoorIndex = 0;
                        string directionString = rooms[riVector.x, riVector.y].DirectionToString(rooms[riVector.x, riVector.y].DirectionToStart);
                        for (int objNum = 0; objNum < 4; ++objNum)
                        {
                            if (rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(objNum).gameObject.name == directionString) { teleportDoorIndex = objNum; }
                        }
                        rooms[riVector.x, riVector.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>()
                        .Init(this, riVector.x, riVector.y, 0, 0, roomToConnectTo.roomInGame.transform.GetChild(1).gameObject, new Vector3(2f, 0f, 0f));
                        rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>().myOtherParent = GetComponentInParent<RoomManager>().generation_Instances[0];
                        rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>().startDoor = 4;
                        rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(teleportDoorIndex).GetComponent<TeleportDoor>().themeToPlay = "MainMenu Theme";
                        thisDoorWasMade = rooms[riVector.x, riVector.y].DirectionToInt(rooms[riVector.x, riVector.y].DirectionToStart);
                        Debug.Log(myRGID + ", this door: " + thisDoorWasMade);
                    }
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Up))
                {
                    if (thisDoorWasMade != 0)
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
                            .Init(this, riVector.x, riVector.y, riVector.x, riVector.y + 1, rooms[riVector.x, riVector.y + 1].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(0f, 0f, 2f));
                    }
                    ++doorCount;
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Right))
                {
                    if (thisDoorWasMade != 1)
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
                            .Init(this, riVector.x, riVector.y, riVector.x + 1, riVector.y, rooms[riVector.x + 1, riVector.y].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(2f, 0f, 0f));
                    }
                    ++doorCount;
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Down))
                {
                    if (thisDoorWasMade != 3)
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
                            .Init(this, riVector.x, riVector.y, riVector.x, riVector.y - 1, rooms[riVector.x, riVector.y - 1].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(0f, 0f, -2f));
                    }                    
                    ++doorCount;
                }
                if (rooms[riVector.x, riVector.y].Opening.HasFlag(Direction.Left))
                {
                    if (thisDoorWasMade != 7)
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
                            .Init(this, riVector.x, riVector.y, riVector.x - 1, riVector.y, rooms[riVector.x - 1, riVector.y].roomInGame.transform.GetChild(teleportDoorIndex).gameObject, new Vector3(-2f, 0f, 0f));
                    }
                    ++doorCount;
                }
            }
        }
    }

    public void ConnectDoorWays(int startYesOrNo)
    {
        if (myRGID == "Room_Generation_Instance#0")
        {
            int firstShiftId, secondShiftId;
            Vector3Int first, second;
            Vector3 firstShift, secondShift;
            string firstStr, secondStr;
            int firstDoor = -1, secondDoor = -1;
            //First
            first = GetComponentInParent<RoomManager>().generation_Instances[1].startRoomVec;
            firstShiftId = GetComponentInParent<RoomManager>().generation_Instances[1].rooms[first.x, first.y].DirectionToInt(GetComponentInParent<RoomManager>().generation_Instances[1].rooms[first.x, first.y].DirectionToStart);
            switch (firstShiftId)
            {
                case 0: //UP
                    firstShift = new Vector3(0f, 0f, -2f);
                    firstStr = "Up";
                    break;
                case 1: //RIGHT
                    firstShift = new Vector3(-2f, 0f, 0f);
                    firstStr = "Right";
                    break;
                case 3: //DOWN
                    firstShift = new Vector3(0f, 0f, 2f);
                    firstStr = "Down";
                    break;
                case 7: //LEFT
                    firstShift = new Vector3(2f, 0f, 0f);
                    firstStr = "Left";
                    break;
                default: //NONE
                    firstShift = new Vector3(-1f, -1f, -1f);
                    firstStr = "None";
                    break;
            }
            for (int i = 0; i < 4; ++i)
            {
                if (GetComponentInParent<RoomManager>().generation_Instances[1].rooms[first.x, first.y].roomInGame.transform.GetChild(i).name == firstStr)
                {
                    firstDoor = i;
                }
            }
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(1).GetComponent<TeleportDoor>()
                .Init(this, startRoomVec.x, startRoomVec.y, first.x, first.y, GetComponentInParent<RoomManager>().generation_Instances[1].rooms[first.x, first.y].roomInGame.transform.GetChild(firstDoor).gameObject, firstShift);
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(1).GetComponent<TeleportDoor>().myOtherParent = GetComponentInParent<RoomManager>().generation_Instances[1];
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(1).GetComponent<TeleportDoor>().startDoor = 1;
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(1).GetComponent<TeleportDoor>().themeToPlay = songNameTitles[1];
            //Second
            second = GetComponentInParent<RoomManager>().generation_Instances[2].startRoomVec;
            secondShiftId = GetComponentInParent<RoomManager>().generation_Instances[2].rooms[second.x, second.y].DirectionToInt(GetComponentInParent<RoomManager>().generation_Instances[2].rooms[second.x, second.y].DirectionToStart);
            switch (secondShiftId)
            {
                case 0: //UP
                    secondShift = new Vector3(0f, 0f, -2f);
                    secondStr = "Up";
                    break;
                case 1: //RIGHT
                    secondShift = new Vector3(-2f, 0f, 0f);
                    secondStr = "Right";
                    break;
                case 3: //DOWN
                    secondShift = new Vector3(0f, 0f, 2f);
                    secondStr = "Down";
                    break;
                case 7: //LEFT
                    secondShift = new Vector3(2f, 0f, 0f);
                    secondStr = "Left";
                    break;
                default: //NONE
                    secondShift = new Vector3(-1f, -1f, -1f);
                    secondStr = "None";
                    break;
            }
            for (int i = 0; i < 4; ++i)
            {
                if (GetComponentInParent<RoomManager>().generation_Instances[2].rooms[second.x, second.y].roomInGame.transform.GetChild(i).name == secondStr)
                {
                    secondDoor = i;
                }
            }
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(0).GetComponent<TeleportDoor>()
                .Init(this, startRoomVec.x, startRoomVec.y, second.x, second.y, GetComponentInParent<RoomManager>().generation_Instances[2].rooms[second.x, second.y].roomInGame.transform.GetChild(secondDoor).gameObject, secondShift);
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(0).GetComponent<TeleportDoor>().myOtherParent = GetComponentInParent<RoomManager>().generation_Instances[2];
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(0).GetComponent<TeleportDoor>().startDoor = 2;
            rooms[startRoomVec.x, startRoomVec.y].roomInGame.transform.GetChild(0).GetComponent<TeleportDoor>().themeToPlay = songNameTitles[0];
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
                        new Vector3(((xCord * roomSize.x) + roomTilePos.x + (PlayerObject.transform.localScale.x / 2)) * 2f, 0.99f, ((zCord * roomSize.y) + roomTilePos.z + (PlayerObject.transform.localScale.z / 2)) * 2f);
                    rooms[xCord, zCord].roomInGame.transform.GetChild(rooms[xCord, zCord].roomInGame.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
                    PlayerIsLost = false;
                    rooms[xCord, zCord].myState = RoomState.Cleared;
                }
            }
        }
    }

    public void EnableRooms(int RoomX, int RoomY, bool enable)
    {
        if (enable)
        {
            Debug.Log(myRGID);
            Debug.Log(RoomX + ", " + RoomY);
            Debug.Log(rooms[RoomX, RoomY]);
            if (rooms[RoomX, RoomY].roomInGame.transform.GetChild(rooms[RoomX, RoomY].roomInGame.transform.childCount - 1).childCount > 0)
            {
                if (rooms[RoomX, RoomY].roomInGame.transform.GetChild(rooms[RoomX, RoomY].roomInGame.transform.childCount - 1).GetChild(0).gameObject.name == "FogOfWar")
                {
                    Destroy(rooms[RoomX, RoomY].roomInGame.transform.GetChild(rooms[RoomX, RoomY].roomInGame.transform.childCount - 1).GetChild(0).gameObject);
                }
            }
            GameObject[] tempEnemies = new GameObject[5];
            if (rooms[RoomX, RoomY].myState == RoomState.Unopened)
            {
                int numEnemiesInRoom = tempEnemies.Length, currEnemies = 0, tries = 0;
                List<Vector3> enemyIsHere = new List<Vector3>();
                while (currEnemies < numEnemiesInRoom)
                {
                    Vector3 tempVec3 = rooms[RoomX, RoomY].FindFloor();
                    int indexOfpfEnemy = Mathf.FloorToInt(Random.Range(0f, pfEnemies.Length - 0.01f));
                    if (!enemyIsHere.Contains(tempVec3))
                    {
                        Vector3 tempVec32 = new Vector3((RoomX * roomSize.x * 2f) + rgShift.x + (tempVec3.x * 2f), 0.5f, (RoomY * roomSize.y * 2f) + rgShift.z + (tempVec3.z * 2f));
                        tempEnemies[currEnemies] = Instantiate(pfEnemies[indexOfpfEnemy], tempVec32, Quaternion.identity, rooms[RoomX, RoomY].roomInGame.transform);
                        //tempEnemies[currEnemies].transform.position = tempVec32;
                        Debug.Log(tempEnemies[currEnemies].transform.position);
                        tempEnemies[currEnemies].transform.localScale 
                            = new Vector3(0.5f * tempEnemies[currEnemies].transform.localScale.x, tempEnemies[currEnemies].transform.localScale.y, 0.5f * tempEnemies[currEnemies].transform.localScale.z);
                        tempEnemies[currEnemies].GetComponent<EnemyMovement>().Player = PlayerObject;
                        tempEnemies[currEnemies].GetComponent<EnemyData>().myX = RoomX;
                        tempEnemies[currEnemies].GetComponent<EnemyData>().myY = RoomY;
                        tempEnemies[currEnemies].GetComponent<EnemyData>().EnemyHB_Instance = Instantiate(pfHB, FindObjectOfType<Canvas>().transform).GetComponent<HealthBar>();
                        tempEnemies[currEnemies].GetComponent<EnemyData>().myIns = this;
                        tempEnemies[currEnemies].SetActive(false);
                        ++currEnemies;
                        enemyIsHere.Add(tempVec3);
                        tries = 0;
                    }
                    else
                    {
                        if (tries >= 20)
                        {
                            //Debug.Log("I tried");
                            Vector3 tempVec32 = new Vector3((RoomX * roomSize.x * 2f) + rgShift.x + (tempVec3.x * 2f), 0.5f, (RoomY * roomSize.y * 2f) + rgShift.z + (tempVec3.z * 2f) );
                            tempEnemies[currEnemies] = Instantiate(pfEnemies[indexOfpfEnemy], tempVec32, Quaternion.identity, rooms[RoomX, RoomY].roomInGame.transform);
                            //tempEnemies[currEnemies].transform.position = tempVec32;
                            Debug.Log(tempEnemies[currEnemies].transform.position);
                            tempEnemies[currEnemies].transform.localScale
                                = new Vector3(0.5f * tempEnemies[currEnemies].transform.localScale.x, tempEnemies[currEnemies].transform.localScale.y, 0.5f * tempEnemies[currEnemies].transform.localScale.z);
                            tempEnemies[currEnemies].GetComponent<EnemyMovement>().Player = PlayerObject;
                            tempEnemies[currEnemies].GetComponent<EnemyData>().myX = RoomX;
                            tempEnemies[currEnemies].GetComponent<EnemyData>().myY = RoomY;
                            tempEnemies[currEnemies].GetComponent<EnemyData>().EnemyHB_Instance = Instantiate(pfHB, FindObjectOfType<Canvas>().transform).GetComponent<HealthBar>();
                            tempEnemies[currEnemies].GetComponent<EnemyData>().myIns = this;
                            tempEnemies[currEnemies].SetActive(false);
                            ++currEnemies;
                            tries = 0;
                        }
                        else { ++tries; }
                    }
                }
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

            if (rooms[xIndex, yIndex].isABossRoom || new Vector3(xIndex, yIndex, 0) == endRoomVec)
            {
                GameObject tempExitDoor = Instantiate(pfExit, rooms[xIndex, yIndex].roomInGame.transform);
                Vector3Int exitTile = rooms[xIndex, yIndex].FindFloor();
                tempExitDoor.transform.localPosition = new Vector3((xIndex * roomSize.x) + exitTile.x, 0.05f, (yIndex * roomSize.y) + exitTile.z);
                PlayerObject.transform.GetChild(3).gameObject.SetActive(true);
                PlayerObject.transform.GetComponentInChildren<PointTowardsExit>().exitDoorTransform = tempExitDoor.transform;
                PlayerObject.transform.GetComponentInChildren<PointTowardsExit>().playerTransform = PlayerObject.transform;
            }
            else { SpawnBook(xIndex, yIndex); }
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

    public void SpawnBook(int xIndex, int yIndex)
    {
        GameObject tempBook = Instantiate(pfBook, rooms[xIndex, yIndex].roomInGame.transform);
        Vector3Int bookTile = rooms[xIndex, yIndex].FindFloor();
        tempBook.transform.localPosition = new Vector3((xIndex * roomSize.x) + bookTile.x, 0f, (yIndex * roomSize.y) + bookTile.z);
        tempBook.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        int indexForBook = Mathf.FloorToInt(Random.Range(0f, bookSprites.Length - 0.01f));
        tempBook.GetComponent<Book>().UpdateBook(bookSprites[indexForBook], indexForBook);
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