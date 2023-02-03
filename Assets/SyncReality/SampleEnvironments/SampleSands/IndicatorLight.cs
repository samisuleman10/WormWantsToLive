using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorLight : MonoBehaviour
{
    public enum Status
    {
        Green,
        Orange,
        Red
    }

    public Material greenMaterial;
    public Material orangeMaterial;
    public Material redMaterial;
    
    public MeshRenderer glassRenderer;

    public Status _status = Status.Green;

    private void Awake()
    {
        SetColor();
    }

    public void SetStatus (Status status)
    {
        _status = status;
        SetColor();
    }

    private void SetColor()
    {
        if (_status == Status.Green)
            glassRenderer.material = greenMaterial;
        if (_status == Status.Orange)
            glassRenderer.material = orangeMaterial;
        if (_status == Status.Red)
            glassRenderer.material = redMaterial;
    }
}