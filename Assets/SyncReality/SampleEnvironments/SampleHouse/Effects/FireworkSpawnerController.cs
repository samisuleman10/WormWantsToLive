using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkSpawnerController : MonoBehaviour
{
    public GameObject firework;
    public float afterTime = 0;
    
    public void InstantiateIt()
    {

        StartCoroutine(waitForIt());
    }

    IEnumerator waitForIt()
    {
        yield return new WaitForSeconds(afterTime);

        Instantiate(firework, transform.position, Quaternion.identity);
    }
}
