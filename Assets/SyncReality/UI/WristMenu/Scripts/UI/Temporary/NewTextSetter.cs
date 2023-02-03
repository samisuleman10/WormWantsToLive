using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[ExecuteInEditMode]
public class NewTextSetter : MonoBehaviour
{
    public TextMeshPro origin;
    public TextMeshProUGUI target;
    void Awake()
    {

        target.text = origin.text;
    }

    void OnValidate()
    {

        target.text = origin.text;
    }
}
