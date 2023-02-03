using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelectedObj : MonoBehaviour
{

    public GameObject ObjectToDestroy;

    public void DestroyObject()
    {
        Destroy(ObjectToDestroy);

    }



}
