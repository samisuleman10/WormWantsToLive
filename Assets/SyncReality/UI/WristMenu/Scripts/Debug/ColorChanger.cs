using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ColorChanger : MonoBehaviour
{
    Renderer thisRenderer;

    void Awake()
    {
        thisRenderer = gameObject.GetComponent<Renderer>();
    }

    public void changeColor()
    {
        thisRenderer.material.color = Random.ColorHSV();
    }
}
