using System.Collections;
using System.Collections.Generic;
using utils.GUI;
using TMPro;
using UnityEngine;
/*Temp class, will have something better later*/
public class MessageRelayer : MonoBehaviour
{

    void Awake()
    {
        GUI_PhysicalButton<MenuName> button = GetComponent<GUI_PhysicalButton<MenuName>>();
        string buttonText = GetComponentInChildren<TextMeshProUGUI>().text;
        button.message = buttonText;
    }
}
