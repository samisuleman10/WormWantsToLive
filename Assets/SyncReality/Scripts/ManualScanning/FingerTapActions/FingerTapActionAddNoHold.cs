using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTapActionAddNoHold : FingerTapActionBase
{
    
    private Transform _lastCreatedTransform;

    private float _thisCreatingDelay = 0.5f;




    public override void OnHold(GameObject target, GameObject cursor)
    {
    }
    public override void OnRelease(GameObject target, GameObject cursorPosition)
    {
        StartCoroutine(WaitCreationDelay(_thisCreatingDelay));
    }

    public override void OnTap(GameObject target, GameObject cursor)
    {
        Debug.LogWarning("On Tap Added Mesh");
        var RootSpam = RoomSelector.GetCurrentSelectedRoom();
        if (RootSpam == null)
            return;

        if (!_isWaitingCreatingDelay && target != null)
        {
            _lastCreatedTransform = Instantiate(target, target.transform.parent).transform;
            _lastCreatedTransform.SetParent(RootSpam.transform, true);
        }
    }



}
