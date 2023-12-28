using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    Camera currentCamera;
    private void Start()
    {
        currentCamera = FindFirstObjectByType<Camera>();
    }

    private void OnMouseDrag()
    {
        Vector2 ray = currentCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f);
        transform.position = hit.point;
    }
}
