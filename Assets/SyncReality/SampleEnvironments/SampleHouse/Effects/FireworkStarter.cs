using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkStarter : MonoBehaviour
{
    public void startThem()
    {
        foreach(var fws in FindObjectsOfType<FireworkSpawnerController>())
            fws.InstantiateIt(); 
    }
}
