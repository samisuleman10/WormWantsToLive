using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BackdropCoordinatesManager : MonoBehaviour
{
    DistributeObjectOverPoly distributer;
    SurrounderModule surrounder;
   
    void Start()
    {
        distributer = GetComponent<DistributeObjectOverPoly>();
        surrounder = FindObjectOfType<SurrounderModule>();
    }

     
}
