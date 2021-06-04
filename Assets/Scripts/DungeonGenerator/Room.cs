using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[Serializable]
public class Room
{
    public Vector3Int centerPos;

    public Vector3Int size;

    public HashSet<Vector3Int> floor;


    public Room(Vector3Int center, Vector3Int size)
    {
        centerPos = center;
        this.size = size;
        floor = new HashSet<Vector3Int>();

        for (int i = center.x - size.x/2; i <= center.x + size.x/2; ++i)
            for (int j = center.z - size.z/2; j <= center.z + size.z/2; ++j)
                floor.Add(new Vector3Int(i, 0, j));


        
    }
}
