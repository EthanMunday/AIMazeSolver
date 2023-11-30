using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovementController
{
    public float movementSpeed;
    Vector2 movementVector;
    GameObject pawn;

    public void AddMovementInput(Vector2 input)
    {
        movementVector += input;
    }

    public void CalculateInput()
    {
        pawn.GetComponent<Rigidbody2D>().position += movementVector.normalized * Time.deltaTime * movementSpeed;
        movementVector = Vector3.zero;
    }

    public AIMovementController(GameObject _pawn, float _movementSpeed)
    {
        movementVector = Vector2.zero;
        pawn = _pawn;
        movementSpeed = _movementSpeed;
    }
}
