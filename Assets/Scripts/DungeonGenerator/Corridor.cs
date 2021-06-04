using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{

    public Vector3Int startPoint, endPoint;
    public HashSet<Vector3Int> floor;


    public int Width
    {
        get
        {
            return _internalWitdh;
        }
    }

    private int _internalWitdh;


    public Corridor(Vector3Int start, Vector3Int end, int width)
    {
        startPoint = start;
        endPoint = end;
        _internalWitdh = width;

        floor = new HashSet<Vector3Int>();

        Vector3Int position = start;

        do
        {
            
            if (end.z > position.z)
                position += new Vector3Int(0, 0, 1);
            else if (end.z < position.z)
                position += new Vector3Int(0, 0, -1);

            for (int i = position.x - _internalWitdh / 2; i <= position.x + _internalWitdh / 2; ++i)
                floor.Add(new Vector3Int(i, 0, position.z));
        }
        while (position.z != end.z);

        do
        {
            
            if (end.x > position.x)
                position += Vector3Int.right;
            else if (end.x < position.x)
                position += Vector3Int.left;

            for (int i = position.z - _internalWitdh / 2; i <= position.z + _internalWitdh / 2; ++i)
                floor.Add(new Vector3Int(position.x, 0, i));
        } while (position.x != end.x) ;


    }

}
