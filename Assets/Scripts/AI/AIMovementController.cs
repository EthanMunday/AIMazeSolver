using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovementController
{
    public float movementSpeed;
    Vector3 movementVector;
    GameObject pawn;

    public void AddMovementInput(Vector2 input)
    {
        movementVector += new Vector3(input.x, input.y);
    }

    public void CalculateInput()
    {
        pawn.transform.position += movementVector.normalized * Time.deltaTime * movementSpeed;
        movementVector = Vector3.zero;
    }

    public AIMovementController(GameObject _pawn, float _movementSpeed)
    {
        movementVector = Vector2.zero;
        pawn = _pawn;
    }
}
