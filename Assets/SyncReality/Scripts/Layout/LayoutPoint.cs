using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutPoint : MonoBehaviour
{
    public Vector3 absolutePosition;

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
