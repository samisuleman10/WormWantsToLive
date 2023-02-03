using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableSimplifyNudgers : MonoBehaviour
{

    public List<ObjectTranslationByControllers> Nudgers = new List<ObjectTranslationByControllers> ();

    public List<GameObject> ControllerMenus = new List<GameObject> ();

    public void ChangeSimplifyNudger(bool newStatus)
    {
        foreach (var controller in Nudgers)
        {
            controller.SimplifiedNudger = newStatus;
        }

        foreach (var menu in ControllerMenus)
        {
            menu.SetActive(newStatus);
        }

    }

}
