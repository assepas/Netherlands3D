﻿using Netherlands3D.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCameraInputProvider : MonoBehaviour
{
    [Header("Interface input ignore")]
    public bool ignoreInputWhenHoveringInterface = true;

    [HideInInspector]
    public bool ingoringInput = false;
    [HideInInspector]
    public bool lockDraggingInput = false;
    [HideInInspector]
    public bool isDragging = false;

    [Header("Invoke events")]
    public FloatEvent horizontalInput;
    public FloatEvent verticalInput;
    public Vector3Event lookInput;
    public Vector3Event flyInput;
    public Vector3Event rotateInput;

    public FloatEvent zoomInput;
    public FloatEvent upDownInput;
    public Vector3Event pointerPosition;

    public BoolEvent draggingModifier;
    public BoolEvent rotateModifier;
    public BoolEvent firstPersonModifier;
}