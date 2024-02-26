using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayerController : MonoBehaviour
{
    public static AIPlayerController inst;
    public Vector3 startPosition;
    public bool isBeingDragged = false;
    public bool isRunning = false;
    public bool forcedExit = false;
    public List<ParseOutput> allOutputs = new List<ParseOutput>();
    public List<AIInstruction> instructions = new List<AIInstruction>();
    public int currentInstructionIndex = 0;
    public AIInstruction currentInstruction;
    public Vector2 currentMovement;
    public float currentTime;

    private void Start()
    {
        inst = this;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            forcedExit = true;
            StopRunning();
            forcedExit = false;
        }
        if (!isRunning) NonPlayUpdate();
        else PlayUpdate();
    }

    private void NonPlayUpdate()
    {
        if (!isBeingDragged) return;
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
        startPosition = transform.position;
    }

    public void PlayUpdate()
    {
        if (currentInstruction == null)
        {
            if (currentInstructionIndex == instructions.Count) return;
            currentInstruction = instructions[currentInstructionIndex];
            currentMovement = currentInstruction.movementVector.normalized;
            currentTime = instructions[currentInstructionIndex].time;
        }
        Debug.Log(currentMovement.x + " " + currentMovement.y);
        Vector3 positionOffset = new Vector3(currentMovement.x, currentMovement.y);
        transform.position += positionOffset * Time.deltaTime;

        Vector3 targetPosition = AITarget.inst.transform.position;
        if ((transform.position - targetPosition).magnitude < 1.0f)
        {
            StopRunning();
            return;
        }

        currentTime -= Time.deltaTime;
        if (currentTime > 0.0f) return;
        currentInstruction = null;
        currentInstructionIndex++;
        
    }

    public bool StartRunning()
    {
        if (isRunning)
        {
            Debug.LogWarning("Error: Already Running");
            return false;
        }
        isRunning = true;
        GridCursor.canPlace = false;
        currentInstruction = null;
        return true;
    }

    public void RunInstructions(ParseOutput givenInstructions)
    {
        if (!isRunning) return;
        allOutputs.Add(givenInstructions);
        if (givenInstructions.condition != ParseCondition.Success) return;
        instructions.AddRange(givenInstructions.instructions);
    }

    public void StopRunning()
    {
        currentInstructionIndex = 0;
        currentInstruction = null;
        isRunning = false;
        GridCursor.canPlace = true;
        AIResultOutputer.SaveResults();
        transform.position = startPosition;
        instructions.Clear();
    }


    private void OnMouseDown()
    {
        if (isRunning) return;
        isBeingDragged = true;
        GridCursor.canPlace = false;
    }

    private void OnMouseUp()
    {
        if (isRunning) return;
        isBeingDragged = false;
        GridCursor.canPlace = true;
    }
}
