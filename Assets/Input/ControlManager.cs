using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlManager : MonoBehaviour
{
    public static ControlBindings inputs;
    private void Awake()
    {
        inputs = new ControlBindings();
    }
}
