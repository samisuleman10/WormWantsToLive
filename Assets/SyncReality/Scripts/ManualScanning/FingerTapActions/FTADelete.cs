using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTADelete : FingerTapActionBase
{
    private Transform _lastSelected;

    public Color DeleteRegular;
    public Color DeleteHighlight;
    

    public override void OnSelect(GameObject target, bool toEdit)
    {
        base.OnSelect(target);

        FTCursorsEventManager.ChangeFTCActionType(FingerTapActionType.DeleteMesh, DeleteHighlight);        
        
        _lastSelected = target.transform;
    }

    public override void OnDeselect(GameObject target)
    {
        base.OnDeselect(target);

        FTCursorsEventManager.ChangeFTCActionType(FingerTapActionType.DeleteMesh, DeleteRegular);
        _lastSelected = null;
    }



    public override void OnTap(GameObject target, GameObject cursorPosition)
    {
        Debug.LogWarning("On Tap Move Mesh");

        if (_lastSelected == null)
            return;

        Destroy(_lastSelected.gameObject);
        _lastSelected = null;

        FTCursorsEventManager.ChangeFTCActionType(FingerTapActionType.DeleteMesh, DeleteRegular);        

    }

}
