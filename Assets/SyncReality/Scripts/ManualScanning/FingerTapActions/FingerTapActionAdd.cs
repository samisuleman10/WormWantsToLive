using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTapActionAdd : FingerTapActionBase
{
    
    private bool _isHolding = false;
    private Transform _lastCreatedTransform;

    public override void OnHold(GameObject target, GameObject cursor)
    {
        if(_isHolding)
            _lastCreatedTransform.position = cursor.transform.position;
    }

    public override void OnRelease(GameObject target, GameObject cursorPosition)
    {
        _isHolding = false;
    }

    public override void OnTap(GameObject target, GameObject cursor)
    {
        Debug.LogWarning("On Tap Added Mesh");
        var RootSpam = RoomSelector.GetCurrentSelectedRoom();
        if (RootSpam == null)
            return;

        _lastCreatedTransform = Instantiate( target, target.transform.parent).transform;
        _lastCreatedTransform.SetParent(RootSpam.transform, true);

        _isHolding = true;
    }
}
