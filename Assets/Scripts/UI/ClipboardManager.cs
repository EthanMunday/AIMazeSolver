using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipboardManager : MonoBehaviour
{
    const string GPT_PROMPT_COPY = "dawdadwad";
    public void CopyToClipboard()
    {
        GUIUtility.systemCopyBuffer = GPT_PROMPT_COPY;
    }

    public void PasteClipboard()
    {
        if (!AIPlayerController.inst.isRunning) AIPlayerController.inst.StartRunning();
        ParseOutput newParse = AIOutputInterpreter.ParseOutput(GUIUtility.systemCopyBuffer);
        if (newParse.condition != ParseCondition.Success)
        {
            Debug.Log("Clipboard Parse Faliure");
        }
        Debug.Log(newParse.instructions.Count);
        AIPlayerController.inst.RunInstructions(newParse);
    }
}
