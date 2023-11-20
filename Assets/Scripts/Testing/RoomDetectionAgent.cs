using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomDetectionAgent
{
    const int xMin = 0;
    const int xMax = 32;
    const int yMin = 0;
    const int yMax = 16;
    public void CheckRoom(Vector2Int roomPos, ref List<Vector2Int> checkedList, ref List<Vector2Int> roomList, ref bool[,] wallData)
    {
        checkedList.Remove(roomPos);
        if (CheckValidBool(roomPos)) roomList.Add(roomPos);
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                Vector2Int addVector = new Vector2Int(x, y);
                if (addVector.sqrMagnitude != 1) continue;
                Vector2Int newVector = roomPos + addVector;
                if (!checkedList.Contains(newVector)) continue;
                if (!CheckValidBool(newVector)) CheckRoom(newVector, ref checkedList, ref roomList, ref wallData);
                else if (!wallData[newVector.x,newVector.y]) CheckRoom(newVector, ref checkedList, ref roomList, ref wallData);
            }
        }
    }

    bool CheckValidBool(Vector2 _roomPos)
    {
        if (Mathf.Clamp(_roomPos.x, xMin, xMax) != _roomPos.x) return false;
        if (Mathf.Clamp(_roomPos.y, yMin, yMax) != _roomPos.y) return false;
        return true;
    }
}
