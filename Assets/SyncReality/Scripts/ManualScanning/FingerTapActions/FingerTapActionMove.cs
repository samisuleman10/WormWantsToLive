using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTapActionMove : FingerTapActionBase
{
    
    private bool _isHolding = false;

    private Transform _oldParent;

    private Transform _lastSelected;

    public override void OnSelect(GameObject target, bool toEdit)
    {
        base.OnSelect(target );

        if(!_isHolding)
            _lastSelected = target.transform;
    }



    public override void OnTap(GameObject target, GameObject cursorPosition)
    {
        Debug.LogWarning("On Tap Move Mesh");

        if (_lastSelected == null)
            return;

        if(!_isHolding)
        {
            _oldParent = _lastSelected.transform.parent;
            _lastSelected.transform.SetParent(cursorPosition.transform,true);
            _isHolding=true;
        }
        else
        {
            _lastSelected.transform.SetParent(_oldParent, true);
            _oldParent = null;
            _isHolding = false;
        }
    }
}
