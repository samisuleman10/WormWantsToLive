using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchAction : MonoBehaviour
{
    public UnityEvent tapAction;

    private void OnCollisionEnter(Collision collision)
    {
        tapAction.Invoke();
    }
}
