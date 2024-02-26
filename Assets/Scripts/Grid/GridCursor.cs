using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;

public class GridCursor : MonoBehaviour
{
    public static int gridXSize = 50;
    public static int gridYSize = 25;
    public static string saveLoad;
    public static bool canPlace = true;
    WallGrid wallGrid;
    Camera cameraComponent;

    const string LOL = "=== (1.0,0,3.23)";

    ControlBindings bindings;
    InputAction lmb;
    InputAction rmb;
    Vector2 input;

    void Start()
    {
        //ParseOutput newOutput = AIOutputInterpreter.ParseOutput(LOL);
        //if (newOutput.condition == ParseCondition.Success)
        //{
        //    foreach (AIInstruction instruction in newOutput.instructions)
        //    {
        //        Debug.Log(instruction.movementVector + " " + instruction.time);
        //    }
        //}
        
        cameraComponent = FindFirstObjectByType<Camera>();
        bindings = ControlManager.inputs;
        wallGrid = new WallGrid
            (gridXSize,
            gridYSize,
            FindFirstObjectByType<Grid>(),
            Resources.Load("WallSpriteObj").GameObject(),
            this.gameObject);


        AddInput(lmb, bindings.Player.LeftClick);
        AddInput(rmb, bindings.Player.RightClick);
    }

    private void Update()
    {
        if (input.sqrMagnitude == 0) return;
        Vector2 ray = cameraComponent.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero, 100f);
        if (hit.collider == null) return;
        Vector3Int hitPoint = wallGrid.globalGrid.WorldToCell(hit.point);
        hitPoint.x = Mathf.Clamp(hitPoint.x, 0, gridXSize);
        hitPoint.y = Mathf.Clamp(hitPoint.y, 0, gridYSize);
        if (!canPlace) return;
        if (input.x == 1.0f && !wallGrid.values[hitPoint.x, hitPoint.y])
        {
            if (hitPoint.x < gridXSize + 1 && hitPoint.y < gridYSize + 1)
            {
                wallGrid.UpdateGrid(hitPoint, true);
            }
        }

        else if (input.y == 1.0f && wallGrid.values[hitPoint.x, hitPoint.y])
        {
            if (hitPoint.x < gridXSize + 1 && hitPoint.y < gridYSize + 1)
            {
                wallGrid.UpdateGrid(hitPoint, false);
            }
        }
    }

    private void AddInput(InputAction newInput, InputAction correspondingButton)
    {
        newInput = correspondingButton;
        newInput.Enable();
        newInput.performed += OnInput;
        newInput.canceled += OnInput;
    }

    void OnInput(InputAction.CallbackContext button)
    {
        switch (button.action.name)
        {
            case "LeftClick":
                if (button.performed) input.x = 1;
                else if (button.canceled) input.x = 0;
                break;
            case "RightClick":
                if (button.performed) input.y = 1;
                else if (button.canceled) input.y = 0;
                break;
            default: break;
        }
    }

    public void ChangeSaveFile(string name)
    {
        saveLoad = name;
    }

    public void SaveGame()
    {
        GridSaveLoader.SaveToFile(saveLoad, wallGrid.values);
    }

    public void LoadGame()
    {
        bool[,] newGrid = GridSaveLoader.LoadFromFile(saveLoad);
        wallGrid.SetGrid(newGrid);
    }
}
