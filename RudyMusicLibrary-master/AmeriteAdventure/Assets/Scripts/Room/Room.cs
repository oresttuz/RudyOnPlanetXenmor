using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

public class Room
{
    public int boundary, numEnemies;
    public bool isABossRoom;
    public Direction Opening, DirectionToStart = Direction.None, HallwayMade;
    public TileType[,] RoomGrid;
    public Door[] doors;
    public GameObject roomInGame;
    public RoomState myState = RoomState.Unopened;

    private float randomFill;

    public Room()
    {
        boundary = 0;
        RoomGrid = new TileType[8, 8];
        randomFill = 0.5f;
        Opening = Direction.None;
        HallwayMade = Direction.None;
        doors = new Door[4];
    }

    public Room(int b)
    {
        boundary = b;
        RoomGrid = new TileType[8, 8];
        randomFill = 0.5f;
        Opening = Direction.None;
        HallwayMade = Direction.None;
        doors = new Door[4];
    }

    public Room(Vector3Int dim)
    {
        boundary = 0;
        RoomGrid = new TileType[dim.x, dim.y];
        randomFill = 0.5f;
        Opening = Direction.None;
        HallwayMade = Direction.None;
        doors = new Door[4];
    }

    public Room(int b, Vector3Int dim)
    {
        boundary = b;
        RoomGrid = new TileType[dim.x, dim.y];
        randomFill = 0.5f;
        Opening = Direction.None;
        HallwayMade = Direction.None;
        doors = new Door[4];
    }

    public Room(int b, Vector3Int dim, Direction d)
    {
        boundary = b;
        RoomGrid = new TileType[dim.x, dim.y];
        randomFill = 0.5f;
        Opening = d;
        HallwayMade = Direction.None;
        doors = new Door[4];
    }

    public DoorList CreateSquareRoom(Vector3Int roomSize)
    {
        DoorList tileIndexes = new DoorList();
        for (int x = boundary; x < RoomGrid.GetLength(0) - boundary; ++x)
        {
            for (int y = boundary; y < RoomGrid.GetLength(1) - boundary; ++y)
            {
                if (x == boundary || x == RoomGrid.GetLength(0) - boundary - 1 || y == boundary || y == RoomGrid.GetLength(1) - boundary - 1)
                {
                    RoomGrid[x, y] = TileType.wall;
                    tileIndexes.wall.Add(new Vector3Int(x, y, 0));
                }
                else
                {
                    RoomGrid[x, y] = TileType.floor;
                    tileIndexes.floor.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        return tileIndexes;
    }

    public List<Vector3Int> CreateFloors(Vector3Int roomSize)
    {
        List<Walker> floorWalkers = new List<Walker>();
        List<Vector3Int> initTilePos = new List<Vector3Int>();

        Vector3Int origin = new Vector3Int(UnityEngine.Random.Range(boundary, roomSize.x - boundary), UnityEngine.Random.Range(boundary, roomSize.y - boundary), 0);

        floorWalkers.Add(new Walker(origin, new Vector3Int(boundary, boundary, 0), new Vector3Int(roomSize.x - boundary, roomSize.y - boundary, 0)));
        initTilePos.Add(origin);
        randomFill = UnityEngine.Random.Range(0.5f, 0.75f);
        int tilesToAdd = Mathf.FloorToInt(randomFill * (roomSize.x - (2 * boundary)) * (roomSize.y - (2 * boundary)));
        int currNumTiles = 1;

        while (currNumTiles < tilesToAdd)
        {
            Vector3Int temp;
            foreach (Walker w in floorWalkers)
            {
                if (currNumTiles >= tilesToAdd)
                {
                    Debug.Log("Full Room");
                    break;
                }
                temp = w.Step();
                if (temp != null)
                {
                    if (initTilePos.Contains(temp))
                    {
                        //Debug.Log("Passing by");
                    }
                    else
                    {
                        initTilePos.Add(temp);
                        RoomGrid[temp.x, temp.y] = TileType.floor;
                        currNumTiles++;
                    }
                }
            }
        }
        return initTilePos;
    }

    public List<Vector3Int> CreateWalls(bool boundaryOn)
    {
        List<Vector3Int> initWallPos = new List<Vector3Int>();
        int prevBound = boundary;
        if (boundaryOn)
        {
            if (boundary <= 0)
            {
                boundary = 1;
            }
            for (int w = boundary; w < RoomGrid.GetLength(0) - boundary; ++w)
            {
                for (int h = boundary; h < RoomGrid.GetLength(1) - boundary; ++h)
                {
                    if (RoomGrid[w, h] == TileType.floor)
                    {
                        initWallPos.AddRange(FindNeighborsToWall(w, h));
                    }
                }
            }
            if (prevBound != boundary)
            {
                boundary = prevBound;
            }
        }
        else
        {
            for (int w = boundary; w < RoomGrid.GetLength(0); ++w)
            {
                for (int h = boundary; h < RoomGrid.GetLength(1); ++h)
                {
                    if (RoomGrid[w, h] == TileType.floor)
                    {
                        initWallPos.AddRange(FindNeighborsToWall(w, h));
                    }
                }
            }
        }
        return initWallPos;
    }

    public List<Vector3Int> FillWalls()
    {
        List<Vector3Int> initWallPos = new List<Vector3Int>();
        for (int x = 0; x < RoomGrid.GetLength(0); ++x)
        {
            for (int y = 0; y < RoomGrid.GetLength(1); ++y)
            {
                if ( RoomGrid[x,y] == TileType.empty )
                {
                    RoomGrid[x, y] = TileType.wall;
                    initWallPos.Add(new Vector3Int(x, y, 0));
                }
            }
        }
        return initWallPos;
    }

    public DoorList CreateAllDoors()
    {
        DoorList dl = new DoorList();
        if (Opening.Equals(Direction.None)) { return dl; }
        if (Opening.HasFlag(Direction.Up)) { dl.AddDoorListRange(FindUpOpening()); }
        if (Opening.HasFlag(Direction.Right)) { dl.AddDoorListRange(FindRightOpening()); }
        if (Opening.HasFlag(Direction.Down)) { dl.AddDoorListRange(FindDownOpening()); }
        if (Opening.HasFlag(Direction.Left)) { dl.AddDoorListRange(FindLeftOpening()); }
        return dl;
    }

    public DoorList FindUpOpening()
    {
        DoorList dl = new DoorList();
        Vector3Int lowBound = new Vector3Int(-1, -1, 0), highBound = new Vector3Int(-1, -1, 0);

        for (int x = 0; x < RoomGrid.GetLength(0); ++x)
        {
            for (int y = RoomGrid.GetLength(1) - 1; y >= 0; --y)
            {
                if (RoomGrid[x, y] == TileType.wall && lowBound.x == -1)
                {
                    lowBound.x = x;
                    lowBound.y = y;
                }
                if (RoomGrid[RoomGrid.GetLength(0) - 1 - x, y] == TileType.wall && highBound.x == -1)
                {
                    highBound.x = RoomGrid.GetLength(0) - x;
                    highBound.y = y;
                }
            }
        }
        if ((lowBound.x + 1) < highBound.x)
        {
            bool DoorFound = false;
            Vector3Int bad = new Vector3Int(-1, -1, -1);
            int StartDoor = UnityEngine.Random.Range(lowBound.x + 1, highBound.x);

            List<Walker> wallChecks = new List<Walker>
            {
                new Walker(StartDoor, RoomGrid.GetLength(1) - 1, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1)),
                new Walker(StartDoor, RoomGrid.GetLength(1) - 1, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1))
            };

            while (!DoorFound)
            {
                int option = -1;
                foreach (Walker w in wallChecks)
                {
                    Vector3Int temp;
                    if (option < 0)
                    {
                        temp = w.ControlledStep(3);
                    }
                    else
                    {
                        temp = w.ControlledStep(1);
                    }
                    option *= -1;
                    if (temp == bad)
                    {
                        w.pos = new Vector3Int(StartDoor, (w.pos.y - 1), w.pos.z);
                        w.prevPos = w.pos;
                    }
                    else
                    {
                        if (w.pos != w.prevPos && w.pos.y == w.prevPos.y && !DoorFound)
                        {
                            if (w.pos.x >= RoomGrid.GetLength(0) || w.pos.x < 0 || w.pos.y >= RoomGrid.GetLength(1) || w.pos.y < 0)
                            {
                                Debug.LogError("W.pos was bad: " + w.pos);
                            }
                            else if (w.prevPos.x >= RoomGrid.GetLength(0) || w.prevPos.x < 0 || w.prevPos.y >= RoomGrid.GetLength(1) || w.prevPos.y < 0)
                            {
                                Debug.LogError("W.prevPos was bad: " + w.prevPos);
                            }
                            else if (RoomGrid[w.pos.x, w.pos.y] == TileType.wall && RoomGrid[w.prevPos.x, w.prevPos.y] == TileType.wall)
                            {
                                DoorFound = true;
                                if (RoomGrid[w.pos.x, w.pos.y - 1] != TileType.floor || RoomGrid[w.prevPos.x, w.prevPos.y - 1] != TileType.floor)
                                {
                                    RoomGrid[w.pos.x, w.pos.y - 1] = TileType.floor;
                                    RoomGrid[w.prevPos.x, w.prevPos.y - 1] = TileType.floor;
                                    dl.floor.Add(new Vector3Int(w.pos.x, w.pos.y - 1, w.pos.z));
                                    dl.floor.Add(new Vector3Int(w.prevPos.x, w.prevPos.y - 1, w.prevPos.z));
                                    dl.wall.AddRange(FindNeighborsToWall(w.pos.x, w.pos.y - 1));
                                    dl.wall.AddRange(FindNeighborsToWall(w.prevPos.x, w.prevPos.y - 1));
                                }
                                RoomGrid[w.pos.x, w.pos.y] = TileType.door;
                                RoomGrid[w.prevPos.x, w.prevPos.y] = TileType.door;
                                dl.door.Add(w.pos);
                                dl.door.Add(w.prevPos);
                                if (w.pos.x < w.prevPos.x)
                                {
                                    doors[0] = new Door(w.pos, w.prevPos, Direction.Up); //first is leftmost
                                }
                                else
                                {
                                    doors[0] = new Door(w.prevPos, w.pos, Direction.Up);
                                }
                            }
                        }
                    }
                }
            }
        }
        return dl;
    }
    public DoorList FindRightOpening()
    {
        DoorList dl = new DoorList();
        Vector3Int lowBound = new Vector3Int(-1, -1, 0), highBound = new Vector3Int(-1, -1, 0);
        for (int y = 0; y < RoomGrid.GetLength(1); ++y)
        {
            for (int x = RoomGrid.GetLength(0) - 1; x >= 0; --x)
            {
                if (RoomGrid[x, y] == TileType.wall && lowBound.y == -1)
                {
                    lowBound.x = x;
                    lowBound.y = y;
                }
                if (RoomGrid[x, RoomGrid.GetLength(1) - 1 - y] == TileType.wall && highBound.y == -1)
                {
                    highBound.x = x;
                    highBound.y = RoomGrid.GetLength(1) - y;
                }
            }
        }
        if ((lowBound.y + 1) < highBound.y)
        {
            bool DoorFound = false;
            Vector3Int bad = new Vector3Int(-1, -1, -1);
            int StartDoor = UnityEngine.Random.Range(lowBound.y + 1, highBound.y);

            List<Walker> wallChecks = new List<Walker>
            {
                new Walker(RoomGrid.GetLength(0) - 1, StartDoor, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1)),
                new Walker(RoomGrid.GetLength(0) - 1, StartDoor, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1))
            };

            while (!DoorFound)
            {
                int option = -1;
                foreach (Walker w in wallChecks)
                {
                    Vector3Int temp;
                    if (option < 0)
                    {
                        temp = w.ControlledStep(2);
                    }
                    else
                    {
                        temp = w.ControlledStep(0);
                    }
                    option *= -1;
                    if (temp == bad)
                    {
                        w.pos = new Vector3Int((w.pos.x - 1), StartDoor, w.pos.z);
                        w.prevPos = w.pos;
                    }
                    else
                    {
                        if (w.pos != w.prevPos && w.pos.x == w.prevPos.x && !DoorFound)
                        {
                            if (w.pos.x >= RoomGrid.GetLength(0) || w.pos.x < 0 || w.pos.y >= RoomGrid.GetLength(1) || w.pos.y < 0)
                            {
                                Debug.LogError("W.pos was bad: " + w.pos);
                            }
                            else if (w.prevPos.x >= RoomGrid.GetLength(0) || w.prevPos.x < 0 || w.prevPos.y >= RoomGrid.GetLength(1) || w.prevPos.y < 0)
                            {
                                Debug.LogError("W.prevPos was bad: " + w.prevPos);
                            }
                            else if (RoomGrid[w.pos.x, w.pos.y] == TileType.wall && RoomGrid[w.prevPos.x, w.prevPos.y] == TileType.wall)
                            {
                                DoorFound = true;
                                if (RoomGrid[w.pos.x - 1, w.pos.y] != TileType.floor || RoomGrid[w.prevPos.x - 1, w.prevPos.y] != TileType.floor)
                                {
                                    RoomGrid[w.pos.x - 1, w.pos.y] = TileType.floor;
                                    RoomGrid[w.prevPos.x - 1, w.prevPos.y] = TileType.floor;
                                    dl.floor.Add(new Vector3Int(w.pos.x - 1, w.pos.y, w.pos.z));
                                    dl.floor.Add(new Vector3Int(w.prevPos.x - 1, w.prevPos.y, w.prevPos.z));
                                    dl.wall.AddRange(FindNeighborsToWall(w.pos.x - 1, w.pos.y));
                                    dl.wall.AddRange(FindNeighborsToWall(w.prevPos.x - 1, w.prevPos.y));
                                }
                                RoomGrid[w.pos.x, w.pos.y] = TileType.door;
                                RoomGrid[w.prevPos.x, w.prevPos.y] = TileType.door;
                                dl.door.Add(w.pos);
                                dl.door.Add(w.prevPos);
                                if (w.pos.y < w.prevPos.y)
                                {
                                    doors[1] = new Door(w.pos, w.prevPos, Direction.Right); //first is bottomMost
                                }
                                else
                                {
                                    doors[1] = new Door(w.prevPos, w.pos, Direction.Right);
                                }
                            }
                        }
                    }
                }
            }
        }
        return dl;
    }
    public DoorList FindDownOpening()
    {
        DoorList dl = new DoorList();
        Vector3Int lowBound = new Vector3Int(-1, -1, 0), highBound = new Vector3Int(-1, -1, 0);
        for (int x = 0; x < RoomGrid.GetLength(0); ++x)
        {
            for (int y = 0; y < RoomGrid.GetLength(1); ++y)
            {
                if (RoomGrid[x, y] == TileType.wall && lowBound.x == -1)
                {
                    lowBound.x = x;
                    lowBound.y = y;
                }
                if (RoomGrid[RoomGrid.GetLength(0) - 1 - x, y] == TileType.wall && highBound.x == -1)
                {
                    highBound.x = RoomGrid.GetLength(0) - x;
                    highBound.y = y;
                }
            }
        }
        if ((lowBound.x + 1) < highBound.x)
        {
            bool DoorFound = false;
            Vector3Int bad = new Vector3Int(-1, -1, -1);
            int StartDoor = UnityEngine.Random.Range(lowBound.x + 1, highBound.x);

            List<Walker> wallChecks = new List<Walker>
            {
                new Walker(StartDoor, 0, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1)),
                new Walker(StartDoor, 0, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1))
            };

            while (!DoorFound)
            {
                int option = -1;
                foreach (Walker w in wallChecks)
                {
                    Vector3Int temp;
                    if (option < 0)
                    {
                        temp = w.ControlledStep(3);
                    }
                    else
                    {
                        temp = w.ControlledStep(1);
                    }
                    option *= -1;
                    if (temp == bad)
                    {
                        w.pos = new Vector3Int(StartDoor, (w.pos.y + 1), w.pos.z);
                        w.prevPos = w.pos;
                    }
                    else
                    {
                        if (w.pos != w.prevPos && w.pos.y == w.prevPos.y && !DoorFound)
                        {
                            if (w.pos.x >= RoomGrid.GetLength(0) || w.pos.x < 0 || w.pos.y >= RoomGrid.GetLength(1) || w.pos.y < 0)
                            {
                                Debug.LogError("W.pos was bad: " + w.pos);
                            }
                            else if (w.prevPos.x >= RoomGrid.GetLength(0) || w.prevPos.x < 0 || w.prevPos.y >= RoomGrid.GetLength(1) || w.prevPos.y < 0)
                            {
                                Debug.LogError("W.prevPos was bad: " + w.prevPos);
                            }
                            else if (RoomGrid[w.pos.x, w.pos.y] == TileType.wall && RoomGrid[w.prevPos.x, w.prevPos.y] == TileType.wall)
                            {
                                DoorFound = true;
                                if (RoomGrid[w.pos.x, w.pos.y + 1] != TileType.floor || RoomGrid[w.prevPos.x, w.prevPos.y + 1] != TileType.floor)
                                {
                                    RoomGrid[w.pos.x, w.pos.y + 1] = TileType.floor;
                                    RoomGrid[w.prevPos.x, w.prevPos.y + 1] = TileType.floor;
                                    dl.floor.Add(new Vector3Int(w.pos.x, w.pos.y + 1, w.pos.z));
                                    dl.floor.Add(new Vector3Int(w.prevPos.x, w.prevPos.y + 1, w.prevPos.z));
                                    dl.wall.AddRange(FindNeighborsToWall(w.pos.x, w.pos.y + 1));
                                    dl.wall.AddRange(FindNeighborsToWall(w.prevPos.x, w.prevPos.y + 1));
                                }
                                RoomGrid[w.pos.x, w.pos.y] = TileType.door;
                                RoomGrid[w.prevPos.x, w.prevPos.y] = TileType.door;
                                dl.door.Add(w.pos);
                                dl.door.Add(w.prevPos);
                                if (w.pos.x < w.prevPos.x)
                                {
                                    doors[2] = new Door(w.pos, w.prevPos, Direction.Down); //leftmost is first
                                }
                                else
                                {
                                    doors[2] = new Door(w.prevPos, w.pos, Direction.Down);
                                }
                            }
                        }
                    }
                }
            }
        }
        return dl;
    }
    public DoorList FindLeftOpening()
    {
        DoorList dl = new DoorList();
        Vector3Int lowBound = new Vector3Int(-1, -1, 0), highBound = new Vector3Int(-1, -1, 0);
        for (int y = 0; y < RoomGrid.GetLength(1); ++y)
        {
            for (int x = 0; x < RoomGrid.GetLength(0); ++x)
            {
                if (RoomGrid[x, y] == TileType.wall && lowBound.y == -1)
                {
                    lowBound.x = x;
                    lowBound.y = y;
                }
                if (RoomGrid[x, RoomGrid.GetLength(1) - 1 - y] == TileType.wall && highBound.y == -1)
                {
                    highBound.x = x;
                    highBound.y = RoomGrid.GetLength(1) - y;
                }
            }
        }
        if ((lowBound.y + 1) < highBound.y)
        {
            bool DoorFound = false;
            Vector3Int bad = new Vector3Int(-1, -1, -1);
            int StartDoor = UnityEngine.Random.Range(lowBound.y + 1, highBound.y);

            List<Walker> wallChecks = new List<Walker>
            {
                new Walker(0, StartDoor, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1)),
                new Walker(0, StartDoor, 0, 0, RoomGrid.GetLength(0), RoomGrid.GetLength(1))
            };

            while (!DoorFound)
            {
                int option = -1;
                foreach (Walker w in wallChecks)
                {
                    Vector3Int temp;
                    if (option < 0)
                    {
                        temp = w.ControlledStep(2);
                    }
                    else
                    {
                        temp = w.ControlledStep(0);
                    }
                    option *= -1;
                    if (temp == bad)
                    {
                        w.pos = new Vector3Int((w.pos.x + 1), StartDoor, w.pos.z);
                        w.prevPos = w.pos;
                    }
                    else
                    {
                        if (w.pos != w.prevPos && w.pos.x == w.prevPos.x && !DoorFound)
                        {
                            if (w.pos.x >= RoomGrid.GetLength(0) || w.pos.x < 0 || w.pos.y >= RoomGrid.GetLength(1) || w.pos.y < 0)
                            {
                                Debug.LogError("W.pos was bad: " + w.pos);
                            }
                            else if (w.prevPos.x >= RoomGrid.GetLength(0) || w.prevPos.x < 0 || w.prevPos.y >= RoomGrid.GetLength(1) || w.prevPos.y < 0)
                            {
                                Debug.LogError("W.prevPos was bad: " + w.prevPos);
                            }
                            else if (RoomGrid[w.pos.x, w.pos.y] == TileType.wall && RoomGrid[w.prevPos.x, w.prevPos.y] == TileType.wall)
                            {
                                DoorFound = true;
                                if (RoomGrid[w.pos.x + 1, w.pos.y] != TileType.floor || RoomGrid[w.prevPos.x + 1, w.prevPos.y] != TileType.floor)
                                {
                                    RoomGrid[w.pos.x + 1, w.pos.y] = TileType.floor;
                                    RoomGrid[w.prevPos.x + 1, w.prevPos.y] = TileType.floor;
                                    dl.floor.Add(new Vector3Int(w.pos.x + 1, w.pos.y, w.pos.z));
                                    dl.floor.Add(new Vector3Int(w.prevPos.x + 1, w.prevPos.y, w.prevPos.z));
                                    dl.wall.AddRange(FindNeighborsToWall(w.pos.x + 1, w.pos.y));
                                    dl.wall.AddRange(FindNeighborsToWall(w.prevPos.x + 1, w.prevPos.y));
                                }
                                RoomGrid[w.pos.x, w.pos.y] = TileType.door;
                                RoomGrid[w.prevPos.x, w.prevPos.y] = TileType.door;
                                dl.door.Add(w.pos);
                                dl.door.Add(w.prevPos);
                                if (w.pos.y < w.prevPos.y)
                                {
                                    doors[3] = new Door(w.pos, w.prevPos, Direction.Left); //bottomMost is first
                                }
                                else
                                {
                                    doors[3] = new Door(w.prevPos, w.pos, Direction.Left);
                                }
                            }
                        }
                    }
                }
            }
        }
        return dl;
    }

    public List<Vector3Int> FindNeighborsToWall(int x, int y)
    {
        List<Vector3Int> temp = new List<Vector3Int>();
        for (int n = 0; n < 8; ++n)
        {
            switch (n)
            {
                case 0: // NW
                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        if (RoomGrid[x - 1, y - 1] == TileType.empty)
                        {
                            RoomGrid[x - 1, y - 1] = TileType.wall;
                            temp.Add(new Vector3Int(x - 1, y - 1, 0));
                        }
                    }
                    break;
                case 1: // N
                    if (y - 1 >= 0)
                    {
                        if (RoomGrid[x, y - 1] == TileType.empty)
                        {
                            RoomGrid[x, y - 1] = TileType.wall;
                            temp.Add(new Vector3Int(x, y - 1, 0));
                        }
                    }
                    break;
                case 2: // NE
                    if (x + 1 < RoomGrid.GetLength(0) && y - 1 >= 0)
                    {
                        if (RoomGrid[x + 1, y - 1] == TileType.empty)
                        {
                            RoomGrid[x + 1, y - 1] = TileType.wall;
                            temp.Add(new Vector3Int(x + 1, y - 1, 0));
                        }
                    }
                    break;
                case 3: // W
                    if (x - 1 >= 0)
                    {
                        if (RoomGrid[x - 1, y] == TileType.empty)
                        {
                            RoomGrid[x - 1, y] = TileType.wall;
                            temp.Add(new Vector3Int(x - 1, y, 0));
                        }
                    }
                    break;
                case 4: // E
                    if (x + 1 < RoomGrid.GetLength(0))
                    {
                        if (RoomGrid[x + 1, y] == TileType.empty)
                        {
                            RoomGrid[x + 1, y] = TileType.wall;
                            temp.Add(new Vector3Int(x + 1, y, 0));
                        }
                    }
                    break;
                case 5: // SW
                    if (x - 1 >= 0 && y + 1 < RoomGrid.GetLength(1))
                    {
                        if (RoomGrid[x - 1, y + 1] == TileType.empty)
                        {
                            RoomGrid[x - 1, y + 1] = TileType.wall;
                            temp.Add(new Vector3Int(x - 1, y + 1, 0));
                        }
                    }
                    break;
                case 6: // S
                    if (y + 1 < RoomGrid.GetLength(1))
                    {
                        if (RoomGrid[x, y + 1] == TileType.empty)
                        {
                            RoomGrid[x, y + 1] = TileType.wall;
                            temp.Add(new Vector3Int(x, y + 1, 0));
                        }
                    }
                    break;
                default: // SE
                    if (x + 1 < RoomGrid.GetLength(0) && y + 1 < RoomGrid.GetLength(1))
                    {
                        if (RoomGrid[x + 1, y + 1] == TileType.empty)
                        {
                            RoomGrid[x + 1, y + 1] = TileType.wall;
                            temp.Add(new Vector3Int(x + 1, y + 1, 0));
                        }
                    }
                    break;
            }
        }
        return temp;

    }

    public void enableRoom(bool playerIsInRoom)
    {
        if (myState == RoomState.Cleared) { return; }
        roomInGame.SetActive(playerIsInRoom);
        if (!playerIsInRoom) { return; }
        if (myState == RoomState.Unopened)
        {
            roomInGame.transform.GetChild(roomInGame.transform.childCount - 1).GetChild(0).gameObject.SetActive(false);
            myState = RoomState.EnemiesInRoom;
            UnLockDoors(false, Opening);
        }
    }

    public void enableRoom(bool playerIsInRoom, bool EnemiesInRoom)
    {
        if (myState == RoomState.Cleared) { return; } 
        roomInGame.SetActive(playerIsInRoom);
        if (!playerIsInRoom) { return; }
        if (myState == RoomState.Unopened && EnemiesInRoom)
        {
            myState = RoomState.EnemiesInRoom;
            UnLockDoors(false, Opening);
        }
    }

    public void enableRoom(bool playerIsInRoom, GameObject[] enemyObjsToSpawn)
    {
        if (myState == RoomState.Cleared) { return; } //if room is cleared, disregard this function
        roomInGame.SetActive(playerIsInRoom);
        if (!playerIsInRoom) { return; } //if the player isn't in this room, disregard the rest of the code
        if (myState == RoomState.Unopened) 
        {
            myState = RoomState.EnemiesInRoom;
            UnLockDoors(false, Opening);
            SpawnEnemies(enemyObjsToSpawn);
        }
    }

    public void UnLockDoors(bool unlock, Direction theseDoorDirections)
    {
        Direction DoorsAltered = Direction.None;
        for (int currDoor = 0; currDoor < 4; ++currDoor)
        {
            if (roomInGame.transform.GetChild(currDoor).gameObject.name == "Wall(Clone)") { break; }
            if (theseDoorDirections.HasFlag(Direction.Up) && !DoorsAltered.HasFlag(Direction.Up))
            {
                DoorsAltered |= Direction.Up;
                if (roomInGame.transform.GetChild(currDoor).gameObject.name == "Up") { roomInGame.transform.GetChild(currDoor).GetComponent<TeleportDoor>().isLocked = !unlock; }
            }
            else if (theseDoorDirections.HasFlag(Direction.Right) && !DoorsAltered.HasFlag(Direction.Right))
            {
                DoorsAltered |= Direction.Right;
                if (roomInGame.transform.GetChild(currDoor).gameObject.name == "Right") { roomInGame.transform.GetChild(currDoor).GetComponent<TeleportDoor>().isLocked = !unlock; }
            }
            else if (theseDoorDirections.HasFlag(Direction.Down) && !DoorsAltered.HasFlag(Direction.Down))
            {
                DoorsAltered |= Direction.Down;
                if (roomInGame.transform.GetChild(currDoor).gameObject.name == "Down") { roomInGame.transform.GetChild(currDoor).GetComponent<TeleportDoor>().isLocked = !unlock; }
            }
            else if (theseDoorDirections.HasFlag(Direction.Left) && !DoorsAltered.HasFlag(Direction.Left))
            {
                DoorsAltered |= Direction.Left;
                if (roomInGame.transform.GetChild(currDoor).gameObject.name == "Left") { roomInGame.transform.GetChild(currDoor).GetComponent<TeleportDoor>().isLocked = !unlock; }
            }
        }
    }

    public void SpawnEnemies(GameObject[] enemyObjsToSpawn)
    {
        foreach (GameObject obj in enemyObjsToSpawn)
        {
            obj.SetActive(true);
            ++numEnemies;
        }
    }

    public int CountNumRoomsConnected()
    {
        int count = 0;
        if (Opening.Equals(Direction.None)) { return count; }
        if (Opening.HasFlag(Direction.Up)) { ++count; }
        if (Opening.HasFlag(Direction.Right)) { ++count; }
        if (Opening.HasFlag(Direction.Down)) { ++count; }
        if (Opening.HasFlag(Direction.Left)) { ++count; }
        return count;
    }

    public Vector3Int FindFloor()
    {
        System.Random rand = new System.Random();
        Vector3Int floorPos = new Vector3Int(-1, -1, -1);
        bool searching = true;
        while (searching)
        {
            floorPos.x = rand.Next(0, RoomGrid.GetLength(0));
            floorPos.z = rand.Next(0, RoomGrid.GetLength(1));
            if (RoomGrid[floorPos.x, floorPos.z] == TileType.floor)
            {
                if(RoomGrid[floorPos.x - 1, floorPos.z] != TileType.wall && RoomGrid[floorPos.x, floorPos.z - 1] != TileType.wall 
                    && RoomGrid[floorPos.x + 1, floorPos.z] != TileType.wall && RoomGrid[floorPos.x, floorPos.z + 1] != TileType.wall )
                return floorPos;
            }
        }
        return floorPos;
    }

    public int DirectionToInt(Direction d)
    {
        int num = -1;
        if (d.Equals(Direction.None)) { return num; }
        if (d.Equals(Direction.Up)) { num = num + 1; }
        if (d.Equals(Direction.Right)) { num = num + 2; }
        if (d.Equals(Direction.Down)) { num = num + 4; }
        if (d.Equals(Direction.Left)) { num = num + 8; }
        return num;
    }

    public string DirectionToString(Direction d)
    {
        string id = "";
        if (d.Equals(Direction.None)) { return id; }
        if (d.Equals(Direction.Up))
        {
            id = "Up";
            return id;
        }
        if (d.Equals(Direction.Right))
        {
            id = "Right";
            return id;
        }
        if (d.Equals(Direction.Down))
        {
            id = "Down";
            return id;
        }
        if (d.Equals(Direction.Left))
        {
            id = "Left";
            return id;
        }
        return id;
    }
}

public enum TileType
{
    empty,
    floor,
    wall,
    door
};

[Flags]
public enum Direction : short
{
    None = 0,
    Up = 1,
    Right = 2,
    Down = 4,
    Left = 8
};

public enum RoomState
{
    Unopened,
    EnemiesInRoom,
    Cleared
}

public class Door
{
    public Vector3Int SouthWestPos, NorthEastPos;
    public Direction Facing;

    public Door(Vector3Int w, Vector3Int e, Direction f)
    {
        SouthWestPos = w;
        NorthEastPos = e;
        Facing = f;
    }
}

public class DoorList
{
    public List<Vector3Int> floor;
    public List<Vector3Int> wall;
    public List<Vector3Int> door;

    public DoorList()
    {
        floor = new List<Vector3Int>();
        wall = new List<Vector3Int>();
        door = new List<Vector3Int>();
    }

    public void AddDoorListRange(DoorList d)
    {
        floor.AddRange(d.floor);
        wall.AddRange(d.wall);
        door.AddRange(d.door);
        return;
    }
}



