
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IsFingerPinching : MonoBehaviour
{

    public OVRHand HandRef;
    public UnityEvent EventToTrigger;

    private bool _previousStatus = false;
    private bool _currentStatus = false;


    private void Update()
    {
        if(HandRef != null)
        {
            _currentStatus = HandRef.GetFingerIsPinching(OVRHand.HandFinger.Index);
            if(_currentStatus && !_previousStatus)
                EventToTrigger?.Invoke();
            _previousStatus = _currentStatus;
        }
    }

}
