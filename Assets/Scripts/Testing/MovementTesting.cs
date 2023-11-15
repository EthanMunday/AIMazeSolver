using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class MovementTesting : MonoBehaviour
{
    AIMovementController controller;
    private void Start()
    {
        controller = new AIMovementController(this.gameObject, 3.0f);
    }

    private void Update()
    {
        Vector2 input;
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        if (input.magnitude > 0f)
        {
            controller.AddMovementInput(input);
            controller.CalculateInput();
        }
    }
}
