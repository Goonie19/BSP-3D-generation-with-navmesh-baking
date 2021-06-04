using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public static DungeonGenerator Instance;

    [Header("Generation Parameters")]
    public Vector2Int MapSize;
    public Vector2Int minSizeOfRoom;

    [Range(1,4)]
    public int corridorWidth;

    [Range(1f, 10f)]
    public int offset;
    public float IterationsForBSP;

    [Header("Dungeon Printer")]
    public TilePrinter tilePrinter = null;
    public Material transparentMaterial;

    private GameObject Plane;
    private NavMeshSurface surface;

    private BoundsInt _treeRoot;
    private BinaryTree<BoundsInt> _tree;
    private HashSet<Corridor> _corridors;
    private GameObject colliders;
    private HashSet<Room> _rooms;
    private HashSet<Vector3Int> _dungeonFloor;
    private HashSet<Vector3Int> _wallPositions;

    private void Awake()
    {
        Instance = this;
    }

    #region DIRECTIONS

    private List<Vector3Int> _cardinalDirectionsList = new List<Vector3Int>
    {
        new Vector3Int(0, 0, 1), //UP
        new Vector3Int(1 ,0, 0), //RIGHT
        new Vector3Int(0, 0, -1), // DOWN
        new Vector3Int(-1, 0, 0) //LEFT
    };

    private List<Vector3Int> _diagonalDirectionsList = new List<Vector3Int>
    {
        new Vector3Int(1,0, 1), //UP-RIGHT
        new Vector3Int(1,0, -1), //RIGHT-DOWN
        new Vector3Int(-1, 0, -1), // DOWN-LEFT
        new Vector3Int(-1, 0, 1) //LEFT-UP
    };

    private List<Vector3Int> _eightDirectionsList = new List<Vector3Int>
    {
        new Vector3Int(0,0,1), //UP
        new Vector3Int(1,0,1), //UP-RIGHT
        new Vector3Int(1,0,0), //RIGHT
        new Vector3Int(1,0,-1), //RIGHT-DOWN
        new Vector3Int(0, 0, -1), // DOWN
        new Vector3Int(-1, 0,-1), // DOWN-LEFT
        new Vector3Int(-1, 0,0), //LEFT
        new Vector3Int(-1, 0,1) //LEFT-UP

    };

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
        _dungeonFloor = new HashSet<Vector3Int>();

    }

    public void GenerateDungeon()
    {
        if (colliders != null)
            DestroyImmediate(colliders);

        colliders = new GameObject();
        colliders.name = "Wall Colliders";
        
        _treeRoot = new BoundsInt(Vector3Int.zero, new Vector3Int(MapSize.x * (int)tilePrinter.WallTilemap.layoutGrid.cellSize.x, 0, MapSize.y * (int)tilePrinter.WallTilemap.layoutGrid.cellSize.y));
        _tree = BSP(new BinaryTree<BoundsInt>(_treeRoot), 0);

        CreateRooms();
        CreateWalls(_dungeonFloor);

        CreatePlaneNavmesh();
    }

    


    #region BSP

    public BinaryTree<BoundsInt> BSP(BinaryTree<BoundsInt> tree, int iterations)
    {
        BinaryTree<BoundsInt> leftChild = null;
        BinaryTree<BoundsInt> righChild = null;

        int cutx; 
        int cuty;

        if (tree == null || NoSpaceForRoom(tree))
            return null;
        else
        {
            if (!CanDivide(tree, iterations))
                return tree;
            else
            {
                #region DIVIDING ZONE
                if( tree.Root.size.x >= tree.Root.size.z)
                {
                    cutx = SplitVertically(tree.Root);

                    leftChild = new BinaryTree<BoundsInt>( new BoundsInt(
                        tree.Root.min, 
                        new Vector3Int(cutx, tree.Root.size.y, tree.Root.size.z) 
                        ) );

                    righChild = new BinaryTree<BoundsInt>(new BoundsInt(
                        new Vector3Int(tree.Root.min.x + cutx, tree.Root.min.y, tree.Root.min.z),
                        new Vector3Int(tree.Root.size.x - cutx, tree.Root.size.y, tree.Root.size.z) 
                        ) );

                    

                } else
                {
                    cuty = SplitHorizontally(tree.Root);

                    leftChild = new BinaryTree<BoundsInt>(new BoundsInt(
                        tree.Root.min,
                        new Vector3Int(tree.Root.size.x, tree.Root.size.y, cuty))
                        );

                    righChild = new BinaryTree<BoundsInt>(new BoundsInt( 
                        new Vector3Int(tree.Root.min.x, tree.Root.min.y, tree.Root.min.z + cuty ), 
                        new Vector3Int(tree.Root.size.x, tree.Root.size.y, tree.Root.size.z - cuty) )
                        );
                }
                 
                #endregion

                return new BinaryTree<BoundsInt>(tree.Root,
                    BSP(leftChild, iterations + 1),
                    BSP(righChild, iterations + 1));
            }
        }
    }

    #endregion

    #region CREATE ROOMS & CORRIDORS

    void CreateRooms()
    {
        
        _rooms = createSimpleRooms(_tree);
        _corridors = GenerateCorridors(_tree);

        List<Vector3Int> roomCenters = new List<Vector3Int>();

        if (_dungeonFloor != null)
        {
            _dungeonFloor.Clear();
            tilePrinter.ClearTiles();
        }

        foreach(Room room in _rooms)
        {
            roomCenters.Add(room.centerPos);
            _dungeonFloor = Concat<Vector3Int>(room.floor, _dungeonFloor);
        }
        foreach (Corridor corridor in _corridors)
        {
            _dungeonFloor = Concat<Vector3Int>(corridor.floor, _dungeonFloor);
        }

        tilePrinter.PaintFloorTiles(_dungeonFloor);


    }

    private HashSet<Room> createSimpleRooms(BinaryTree<BoundsInt> tree)
    {
        HashSet<Room> rooms = new HashSet<Room>();

        if (tree != null)
        {
            if (tree.isAleaf())
            {  //The node has no childs i.e., it is a final room

                Vector3Int sizeOfRoom = new Vector3Int(UnityEngine.Random.Range(minSizeOfRoom.x, tree.Root.size.x - offset), 0, UnityEngine.Random.Range(minSizeOfRoom.y, tree.Root.size.z - offset));

                Vector3Int centerPos = new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Root.center).y);

                rooms.Add(new Room(
                            centerPos,
                            sizeOfRoom
                                )
                        );
            }

            Debug.Log(tree.Root.center);
            rooms = Concat<Room>(rooms, createSimpleRooms(tree.Left));
            rooms = Concat<Room>(rooms, createSimpleRooms(tree.Right));
        }
        return rooms;
    }

    private HashSet<Corridor> GenerateCorridors(BinaryTree<BoundsInt> tree)
    {
        HashSet<Corridor> corridorList = new HashSet<Corridor>();
        if (tree != null)
        {

            corridorList = Concat<Corridor>(corridorList, GenerateCorridors(tree.Left));
            corridorList = Concat<Corridor>(corridorList, GenerateCorridors(tree.Right));

            if (tree.hasTwoChilds())
            {
                corridorList.Add(new Corridor(
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Left.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Left.Root.center).y),
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Root.center).y),
                    corridorWidth
                    ));
                corridorList.Add(new Corridor(
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Root.center).y),
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Right.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Right.Root.center).y),
                    corridorWidth
                    ));
            } else if(tree.hasLeftChild())
            {
                corridorList.Add(new Corridor(
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Left.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Left.Root.center).y),
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Root.center).y),
                    corridorWidth
                    ));
            } else if(tree.hasRightChild())
            {
                corridorList.Add(new Corridor(
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Right.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Right.Root.center).y),
                    new Vector3Int(tilePrinter.FloorMap.WorldToCell(tree.Root.center).x, 0, tilePrinter.FloorMap.WorldToCell(tree.Root.center).y),
                    corridorWidth
                    ));
            }
            
        }

        return corridorList;
    }

    #endregion

    #region CREATE WALLS

    void CreateWalls(HashSet<Vector3Int> floorPositions)
    {
        _wallPositions = new HashSet<Vector3Int>();
        HashSet<Vector3Int> wallCardinalPositions = FindWalls(floorPositions, _cardinalDirectionsList);
        createBasicWalls(wallCardinalPositions);
        _wallPositions = wallCardinalPositions;
        HashSet<Vector3Int> wallDiagonalPositions = FindWalls(floorPositions, _eightDirectionsList);
        createCornerWalls(wallDiagonalPositions);
        _wallPositions = Concat<Vector3Int>(_wallPositions, wallDiagonalPositions);

        for (int i = _treeRoot.min.x; i <= _treeRoot.size.x; ++i)
        {

            for (int j = _treeRoot.min.y; j <= _treeRoot.size.z; ++j)
            {
                Vector3Int pos = new Vector3Int(i, 0, j);
                if (!_dungeonFloor.Contains(pos) && !_wallPositions.Contains(pos))
                    tilePrinter.PaintSingleBasicWall(pos, WallType.None);
            }
        }
        

    }

    void createBasicWalls(HashSet<Vector3Int> wallPositions)
    {
        WallType type;

        foreach (Vector3Int pos in wallPositions)
        {
            CreateWallCollider(pos);

            type = CheckWallTypeCardinal(pos);

            tilePrinter.PaintSingleBasicWall(pos, type);
        }

      
    }

    void createCornerWalls(HashSet<Vector3Int> wallPositions)
    {
        WallType type;

        foreach (Vector3Int pos in wallPositions)
        {
            CreateWallCollider(pos);


            type = CheckWallTypeCorner(pos);

            tilePrinter.PaintSingleBasicWall(pos, type);
        }


    }

    #endregion

    #region COMPROBATIONS

    public bool NoSpaceForRoom(BinaryTree<BoundsInt> tree)
    {
        BoundsInt root = tree.Root;
        Vector3Int size = root.size;
        return ((root.size.x - offset < minSizeOfRoom.x) || //If no single room can be adjusted 
                (root.size.z - offset < minSizeOfRoom.y));
    }

    public bool CanDivide(BinaryTree<BoundsInt> tree, int iterations)
    {
        BoundsInt root = tree.Root;
        Vector3Int size = root.size;
        //If two rooms can still be added; otherwise it finishes 
        return (
                (
                  ((root.size.x >= 2 * (minSizeOfRoom.x + offset)) && (root.size.z >= (minSizeOfRoom.y + offset))) //Two room fits
                   ||
                  ((root.size.x >= (minSizeOfRoom.x + offset)) && (root.size.z >= 2 * (minSizeOfRoom.y + offset)))
                )
                &&
                (iterations < IterationsForBSP)
                );
    }

    private WallType CheckWallTypeCorner(Vector3Int pos)
    {
        string neighbourBinType = "";
        foreach (Vector3Int dir in _eightDirectionsList)
        {
            if (_dungeonFloor.Contains(pos + dir))
                neighbourBinType += "1";
            else
                neighbourBinType += "0";
        }

        int typeAsInt = Convert.ToInt32(neighbourBinType, 2);
        WallType type = WallType.None;

        if (WallTypesCheck.wallDiagonalCornerDownLeft.Contains(typeAsInt))
            type = WallType.DownLeft;
        else if (WallTypesCheck.wallDiagonalCornerDownRight.Contains(typeAsInt))
            type = WallType.DownRight;
        else if (WallTypesCheck.wallDiagonalCornerUpLeft.Contains(typeAsInt))
            type = WallType.UpLeft;
        else if (WallTypesCheck.wallDiagonalCornerUpRight.Contains(typeAsInt))
            type = WallType.UpRight;
        else if (WallTypesCheck.wallFullEightDirections.Contains(typeAsInt))
            type = WallType.None;
        else if (WallTypesCheck.wallBottmEightDirections.Contains(typeAsInt))
            type = WallType.Down;

        return type;
    }

    private WallType CheckWallTypeCardinal(Vector3Int pos)
    {
        string neighbourBinType = "";
        foreach (Vector3Int dir in _cardinalDirectionsList)
        {
            if (_dungeonFloor.Contains(pos + dir))
                neighbourBinType += "1";
            else
                neighbourBinType += "0";
        }

        int typeAsInt = Convert.ToInt32(neighbourBinType, 2);
        WallType type = WallType.None;

        if (WallTypesCheck.wallTop.Contains(typeAsInt))
            type = WallType.Up;
        else if (WallTypesCheck.wallSideRight.Contains(typeAsInt))
            type = WallType.Right;
        else if (WallTypesCheck.wallBottom.Contains(typeAsInt))
            type = WallType.Down;
        else if (WallTypesCheck.wallSideLeft.Contains(typeAsInt))
            type = WallType.Left;
        else if (WallTypesCheck.wallExternalCornerDownLeft.Contains(typeAsInt))
            type = WallType.ExternalCornerDownLeft;
        else if (WallTypesCheck.wallExternalCornerDownRight.Contains(typeAsInt))
            type = WallType.ExternalCornerDownRight;
        else if (WallTypesCheck.wallExternalCornerUpLeft.Contains(typeAsInt))
            type = WallType.ExternalCornerUpLeft;
        else if (WallTypesCheck.wallExternalCornerUpRight.Contains(typeAsInt))
            type = WallType.ExternalCornerUpRight;

        return type;



    }

    #endregion

    #region OPERATIONS

    private void CreateWallCollider(Vector3Int pos)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.transform.position = pos + new Vector3(tilePrinter.WallTilemap.tileAnchor.x, 0, tilePrinter.WallTilemap.tileAnchor.z);
        g.transform.parent = colliders.transform;
        g.transform.localScale = new Vector3(g.transform.localScale.x, 2, g.transform.localScale.z);
        g.GetComponent<MeshRenderer>().material = transparentMaterial;
        g.isStatic = true;
    }

    private void CreatePlaneNavmesh()
    {
        if(Plane != null)
        {
            Destroy(Plane);
        }

        Plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

        GameObject surfacePlane = Plane;

        if (surface != null)
        {
            Destroy(surface);
        }

        surface = surfacePlane.AddComponent<NavMeshSurface>();


        surface.transform.position = new Vector3(_treeRoot.center.x, -0.2f, _treeRoot.center.z);

        float NewScaleX = (MapSize.x / 2) / surface.GetComponent<MeshFilter>().mesh.vertices[10].x;
        float NewScaleY = (MapSize.y / 2) / surface.GetComponent<MeshFilter>().mesh.vertices[10].z;

        surface.transform.localScale = new Vector3(NewScaleX, surface.transform.localScale.y, NewScaleY);

        surface.BuildNavMesh();
    }

    HashSet<Vector3Int> FindWalls(HashSet<Vector3Int> floorPositions, List<Vector3Int> directionList)
    {
        HashSet<Vector3Int> wallPositions = new HashSet<Vector3Int>();

        foreach (Vector3Int pos in floorPositions)
        {
            foreach (Vector3Int dir in directionList)
            {
                Vector3Int neighbourPos = pos + dir;
                if (!floorPositions.Contains(neighbourPos) && !_wallPositions.Contains(neighbourPos))
                    wallPositions.Add(neighbourPos);

            }
        }

        return wallPositions;
    }

    private int SplitVertically(BoundsInt root)
    {
        return UnityEngine.Random.Range(1, root.size.x);
    }

    private int SplitHorizontally(BoundsInt root)
    {
        return UnityEngine.Random.Range(1, root.size.z);
    }

    private HashSet<T> Concat<T>(HashSet<T> lista1, HashSet<T> lista2)
    {
        foreach (T t in lista2)
            lista1.Add(t);

        return lista1;
    }

    public Vector3Int GetRandomFloor()
    {
        int index_room = UnityEngine.Random.Range(0, _rooms.Count);
        Vector3Int RandomPos = new Vector3Int(0,0,0);

        int i = 0;
        foreach(Room r in _rooms)
        {
            if(i == index_room)
            {
                int x_var = UnityEngine.Random.Range(-r.size.x / 2, r.size.x / 2);
                int z_var = UnityEngine.Random.Range(-r.size.z / 2, r.size.z / 2);

                RandomPos = r.centerPos + new Vector3Int(x_var, 0, z_var);
            }
            i++;
        }
        return RandomPos;
    }

    #endregion

}
