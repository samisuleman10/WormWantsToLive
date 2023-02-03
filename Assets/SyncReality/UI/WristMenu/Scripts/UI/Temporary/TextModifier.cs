using System.Collections;
using System.Collections.Generic;
using utils.GUI;
using TMPro;
using UnityEngine;
/*Temp class, will have something better later*/
public class TextModifier : MonoBehaviour
{
    GUI_SubMenu parentMenu;
    TMP_Text text;
    void Awake()
    {
        parentMenu = gameObject.GetComponentInParent<GUI_SubMenu>();
        text = GetComponent<TMP_Text>();
    }

    void Start()
    {
        Invoke("SetText", 0);
    }


    void SetText()
    {
        string newText = text.text.Replace("#", parentMenu.message);
        text.SetText(newText);
        //Debug.LogWarning($" current text is: {currentText}, message is {parentMenu.message}");
    }
}
