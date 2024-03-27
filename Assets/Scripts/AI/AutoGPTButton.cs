using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AutoGPTButton : MonoBehaviour
{
    private OpenAIClient openAI;
    private AutoGPTStatus status;
    private ChatResponse data = null;
    int moveLimit = 0;
    int maxMoveLimit = 10;
    int runLimit = 0;
    int maxRunLimit = 100;

    private void Start()
    {
        openAI = new OpenAIClient();
        status = AutoGPTStatus.Default;
    }

    private void Update()
    {
        switch (status)
        {
            case AutoGPTStatus.Starting: 
                StartRunning();
                break;
            case AutoGPTStatus.Generating:
                GeneratePrompt();
                status = AutoGPTStatus.WaitingForGeneration;
                break;
            case AutoGPTStatus.WaitingForGeneration:
                WaitingForGeneration();
                break;
            case AutoGPTStatus.Moving:
                Moving();
                break;
            case AutoGPTStatus.WaitingForMoving:
                WaitingForMoving();
                break;

        }
    }

    public void SetToRunning()
    {
        if (status == AutoGPTStatus.Default) status = AutoGPTStatus.Starting;
    }

    void StartRunning()
    {
        if (!AIPlayerController.inst.isRunning)
        {
            moveLimit = 0;
            AIPlayerController.inst.StartRunning();
            status = AutoGPTStatus.Generating;
        }
    }

    async void GeneratePrompt()
    {
        var messages = new List<Message>
        {
            new Message(Role.System, "1) You are a video game AI controller, navingating a 2D plane, to reach a goal position. \n 2) You will be given your position, the goals position and the positions of a walls start and ends and will calculate the path to the target. \n 3) Going through walls is impossible so use intersection maths to plan your route around the walls to reach the goal. \n 4) After explaining your proccess, you must write '===' to start inputing followed by a list of Vector3's where x and y is the speed in units per second you want to travel and z is the duration in seconds you wish to move for. \n 5) You can only perform up to 5 inputs per message"),
            new Message(Role.User, "As a test, move up for 5 seconds, then diagonally down-right for 2.3 seconds, then left and slightly up for 3.1 seconds"),
            new Message(Role.Assistant, "[Thinking] The up direction is positive Y and right direction is positive X, so I should go in the direction (0.0, 1.0) for 5 seconds for the first command, then (1.0, -1.0) for 2.3 seconds for the second command, then (-1.0, 0.3) for 3.1 seconds for the third command \n === \n (0.0, 1.0, 5.0) \n (1.0, -1.0, 2.3) \n (-1.0, 0.3, 3.1)"),
            new Message(Role.User, AIPlayerController.inst.GeneratePromptText()),
        };
        var chatRequest = new ChatRequest(messages);
        await Task.Delay(1000);
        data = await openAI.ChatEndpoint.GetCompletionAsync(chatRequest);
    }

    private void WaitingForGeneration()
    {
        if (data != null) status = AutoGPTStatus.Moving;
    }


    void Moving()
    {
        Debug.Log("yeet");
        ParseOutput newParse = AIOutputInterpreter.ParseOutput(data);
        AIPlayerController.inst.RunInstructions(newParse);
        moveLimit++;
        data = null;
        if (newParse.condition != ParseCondition.Success)
        {
            Debug.Log("Clipboard Parse Faliure");
            status = AutoGPTStatus.Generating;
            return;
        }
        status = AutoGPTStatus.WaitingForMoving;
    }

    void WaitingForMoving()
    {
        if (!AIPlayerController.inst.isRunning)
        {
            status = AutoGPTStatus.Default;
            return;
        }

        if (AIPlayerController.inst.currentInstructionIndex == AIPlayerController.inst.instructions.Count &&
            AIPlayerController.inst.currentInstruction == null)
        {
            if (moveLimit >= maxMoveLimit)
            {
                AIPlayerController.inst.StopRunning();
                status = AutoGPTStatus.Default;
                return;
            }

            status = AutoGPTStatus.Generating;
            return;
        }
    }
}

public enum AutoGPTStatus
{ 
    Default,
    Starting,
    Generating,
    WaitingForGeneration,
    Moving,
    WaitingForMoving,
    Ended
}