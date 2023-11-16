using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSlot
{
    public bool wallData;

    public bool AddWall(int direction)
    {
        DirectionCalculator(ref direction);
        if (direction >= 8)
        {
            Debug.Log("Error: Wall Data out of index array");
            return false;
        }

        if (wallData[direction] == false)
        {
            wallData[direction] = true;
            return true;
        }
        return false;
    }

    public bool CheckWall(int direction)
    {
        DirectionCalculator(ref direction);
        if (direction >= 8)
        {
            Debug.Log("Error: Wall Data out of index array");
            return false;
        }
        return wallData[direction];
    }

    private int DirectionCalculator(ref int direction) 
    {
        if (direction <= 7) return direction;
        else direction = (direction / 45) % 8;
        return Mathf.Abs(direction);
    }

    public GridSlot()
    {
        wallData = new bool[3];
        wallObjects = new GameObject[3];
    }
}
