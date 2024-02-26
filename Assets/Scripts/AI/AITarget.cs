using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITarget : MonoBehaviour
{
    public static AITarget inst;
    public bool isBeingDragged = false;

    private void Start()
    {
        inst = this;
    }

    private void Update()
    {
        NonPlayUpdate();
    }

    private void NonPlayUpdate()
    {
        if (!isBeingDragged) return;
        if (AIPlayerController.inst.isBeingDragged)
        {
            isBeingDragged = false;
            return;
        }
        Vector2 ray = FindFirstObjectByType<Camera>().ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f);
        transform.position = hit.point;
        if (hit.point.x < 0.5f | hit.point.x > 49.5f | hit.point.y < 0.5f | hit.point.y > 24.5f)
        {
            Vector3 newPosition = transform.position;
            newPosition.x = Mathf.Clamp(hit.point.x, 0.5f, 49.5f);
            transform.position = newPosition;
            isBeingDragged = false;
        }
        if (hit.point.y < 0.5f | hit.point.y > 24.5f)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = Mathf.Clamp(hit.point.y, 0.5f, 24.5f);
            transform.position = newPosition;
            isBeingDragged = false;
        }
    }

    private void OnMouseDown()
    {
        if (AIPlayerController.inst.isRunning) return;
        isBeingDragged = true;
        GridCursor.canPlace = false;
    }

    private void OnMouseUp()
    {
        if (AIPlayerController.inst.isRunning) return;
        isBeingDragged = false;
        GridCursor.canPlace = true;
    }
}
