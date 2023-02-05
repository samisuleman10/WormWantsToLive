using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointGravitySwitcher : MonoBehaviour
{
    bool isAbove = false;

    public float barierHeight = .5f;
    private void Start()
    {
        if(transform.position.y > barierHeight)
        {
            isAbove = true;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(transform.position.y > barierHeight && !isAbove)
        {
            isAbove = true;
            GetComponent<Rigidbody>().useGravity = true;
        }
        if (transform.position.y < barierHeight && isAbove)
        {
            isAbove = false;
            GetComponent<Rigidbody>().useGravity = false;
        }
    }
}
