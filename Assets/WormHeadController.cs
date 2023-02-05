using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHeadController : MonoBehaviour
{
   
    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Camera.main.transform.position;
    }
}
