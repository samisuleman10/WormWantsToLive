using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEditVisualizer : MonoBehaviour
{
    public LineRenderer LR;
   
    // Start is called before the first frame update
    void Awake()
    {
        LR = gameObject.GetComponent<LineRenderer>();
    }

    public void ToggleRenderer(bool status)
    {
        LR.enabled = status;

    }
}
