using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO;
using System.IO;
using UnityEngine.Windows;
using File = System.IO.File;
using System.Globalization;

public static class AIResultOutputer
{
    const string FOLDER_PATH = "/Scripts/Output/Results/";

    public static void SaveResults()
    {
        string fileName = System.DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HH'-'mm'-'ss");
        string filePath = Application.dataPath + FOLDER_PATH + fileName + ".txt";

        if (File.Exists(filePath)) return;

        File.WriteAllText(filePath, fileName + "\n \n");

        File.AppendAllText(filePath, "Player Start: ");
        File.AppendAllText(filePath, AIPlayerController.inst.startPosition.ToString() + "\n");

        Vector3 playerPosition = AIPlayerController.inst.transform.position;
        Vector3 targetPosition = AITarget.inst.transform.position;

        File.AppendAllText(filePath, "Player End: ");
        File.AppendAllText(filePath, playerPosition.ToString() + "\n\n");

        File.AppendAllText(filePath, "Target: ");
        File.AppendAllText(filePath, targetPosition.ToString() + "\n");

        File.AppendAllText(filePath, "Target Reached: ");
        if ((playerPosition - targetPosition).magnitude < 1.0f)
        {
            File.AppendAllText(filePath, "Success\n\n");
        }

        else if(AIPlayerController.inst.forcedExit)
        {
            File.AppendAllText(filePath, "Manual Exit\n\n");
        }

        else
        {
            File.AppendAllText(filePath, "Faliure\n\n");
        }

        File.AppendAllText(filePath, "Instructions:\n\n");

        List<ParseOutput> instructionsRan = AIPlayerController.inst.allOutputs;
        int outputIndex = 1;
        int successfulInstructions = 0;
        int inputsRan = 0;
        
        foreach(ParseOutput instr in instructionsRan)
        {
            File.AppendAllText(filePath, "Instruction " + outputIndex.ToString() + ":\n");
            outputIndex++;

            if (instr.condition != ParseCondition.Success)
            {
                File.AppendAllText(filePath, "Failed\n\n");
                continue;
            }
            
            foreach(AIInstruction input in instr.instructions)
            {
                File.AppendAllText(filePath, input.movementVector.ToString() + " Time: " + input.time.ToString() + "\n");
                inputsRan++;
            }

            successfulInstructions++;
            File.AppendAllText(filePath, "\n");
        }

        File.AppendAllText(filePath, "Inputs Ran: ");
        File.AppendAllText(filePath, inputsRan.ToString() + "\n\n");

        float successRatio = 100 * ((float)successfulInstructions / instructionsRan.Count);

        File.AppendAllText(filePath, "Successful Instruction Rate: ");
        File.AppendAllText(filePath, successRatio.ToString() + "%");
    }
}
