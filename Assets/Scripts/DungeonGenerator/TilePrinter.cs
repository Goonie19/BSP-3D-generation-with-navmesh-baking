using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePrinter : MonoBehaviour
{
    public Tilemap FloorMap;
    public Tilemap WallTilemap;
    public List<TileBase> _floorTile;
    [SerializeField] 
    private TileBase _wallTop, _wallSideRight, _wallSideLeft, _wallBottom, _wallFull, _wallExternalCornerDownLeft, _wallExternalCornerDownRight, _wallExternalCornerUpLeft, _wallExternalCornerUpRight,
        _wallDiagonalCornerDownRight, _wallDiagonalCornerDownLeft, _wallDiagonalCornerUpRight, _wallDiagonalCornerUpLeft;

    public void PaintFloorTiles(IEnumerable<Vector3Int> floorPositions)
    {
        PaintTiles(floorPositions, FloorMap, _floorTile);
    }


    void PaintTiles(IEnumerable<Vector3Int> floorPositions, Tilemap tilemap, List<TileBase> tile)
    {
        foreach (Vector3Int pos in floorPositions)
        {
            int index = UnityEngine.Random.Range(0, tile.Count);
            PaintSingleTile(pos, tilemap, tile[index]);
        }
    }

    public void PaintSingleBasicWall(Vector3Int position, WallType type)
    {
        TileBase tile = null;
        switch (type)
        {
            case WallType.Up:
                tile = _wallTop;
                break;
            case WallType.UpRight:
                tile = _wallDiagonalCornerUpRight;
                break;
            case WallType.Right:
                tile = _wallSideRight;
                break;
            case WallType.DownRight:
                tile = _wallDiagonalCornerDownRight;
                break;
            case WallType.Down:
                tile = _wallBottom;
                break;
            case WallType.DownLeft:
                tile = _wallDiagonalCornerDownLeft;
                break;
            case WallType.Left:
                tile = _wallSideLeft;
                break;
            case WallType.UpLeft:
                tile = _wallDiagonalCornerUpLeft;
                break;
            case WallType.ExternalCornerDownLeft:
                tile = _wallExternalCornerDownLeft;
                break;
            case WallType.ExternalCornerDownRight:
                tile = _wallExternalCornerDownRight;
                break;
            case WallType.ExternalCornerUpLeft:
                tile = _wallExternalCornerUpLeft;
                break;
            case WallType.ExternalCornerUpRight:
                tile = _wallExternalCornerUpRight;
                break;
            default:
                tile = _wallFull;
                break;
        }
        PaintSingleTile(position, WallTilemap, tile);
    }

    private void PaintSingleTile(Vector3Int position, Tilemap tilemap, TileBase tile)
    {
        Vector3Int tilePos = tilemap.WorldToCell(position);
        tilemap.SetTile(tilePos, tile);

    }

    public void ClearTiles()
    {
        FloorMap.ClearAllTiles();
        WallTilemap.ClearAllTiles();
    }
}