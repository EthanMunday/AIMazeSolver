using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class AIOutputInterpreter
{
    const string INITIALISE_INPUT_STRING = "===";
    const char INPUT_START = '(';
    const char INPUT_BREAK = ',';
    const char INPUT_END = ')';

    public static ParseOutput ParseOutput(string input)
    {
        List<AIInstruction> rv = new List<AIInstruction>();
        int currentIndex = input.IndexOf(INITIALISE_INPUT_STRING);
        int safety = 0;
        if (currentIndex == -1) return new ParseOutput(rv, ParseCondition.LocateInputError);
        while (currentIndex < input.Length && currentIndex != -1 && safety < 100000)
        {
            safety++;
            currentIndex = input.IndexOf(INPUT_START, currentIndex);
            AIInstruction newInput = FindInstruction(input, ref currentIndex);
            if (newInput.movementVector.magnitude == 0.0f) continue;
            if (newInput.time == 0.0f) continue;
            rv.Add(newInput);
        }
        if (safety == 100000) return new ParseOutput(rv, ParseCondition.InputFormattingError);
        if (rv.Count == 0) return new ParseOutput(rv, ParseCondition.NoInputsFound);
        return new ParseOutput(rv, ParseCondition.Success);
        
    }

    static AIInstruction FindInstruction(string input, ref int index)
    {
        AIInstruction rv = new AIInstruction(new Vector2(), 0.0f);
        rv.movementVector.x = FindFloat(input, ref index);
        rv.movementVector.y = FindFloat(input, ref index);
        rv.time = FindFloat(input, ref index);
        return rv;
    }

    static float FindFloat(string input, ref int index)
    {
        float rv = 0.0f;
        if (index == -1) return rv;
        for (int i = index; i < input.Length; i++)
        {
            if (IsNumericalValue(input[i]))
            {
                break;
            }
            index++;
        }

        StringBuilder parseRv = new StringBuilder();
        for (int i = index; i < input.Length; i++)
        {
            if (!IsNumericalValue(input[i]))
            {
                float.TryParse(parseRv.ToString(), out rv);
                return rv;
            }
            parseRv.Append(input[i]);
            index++;
        }
        
        index = -1;
        return rv;
    }

    static bool IsNumericalValue(char currentChar)
    { 
        if (char.IsDigit(currentChar)) return true;
        if (currentChar == '-') return true;
        if (currentChar == '.') return true;
        return false;
    }
}

[Serializable]
public struct ParseOutput
{
    public List<AIInstruction> instructions;
    public ParseCondition condition;
    public ParseOutput(List<AIInstruction> _instructions, ParseCondition _condition)
    {
        instructions = _instructions;
        condition = _condition;
    }
}

[Serializable]
public enum ParseCondition
{
    Success,
    LocateInputError,
    NoInputsFound,
    InputFormattingError
}

[Serializable]
public class AIInstruction
{
    public Vector2 movementVector;
    public float time;
    public AIInstruction(Vector2 _movementVector, float _time)
    {
        movementVector = _movementVector;
        time = _time;
    }

}