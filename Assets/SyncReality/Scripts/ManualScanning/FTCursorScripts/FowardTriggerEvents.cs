using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FowardTriggerEvents : MonoBehaviour
{

    public FingerTapActionType ThisAction;
    public Material ThisActionMaterial; 

    [HideInInspector]
    public UnityEvent<Collider, FingerTapAction> OnTriggerEnterFoward;
    [HideInInspector]
    public UnityEvent<Collider, FingerTapAction> OnTriggerExitFoward;
    [HideInInspector]
    public UnityEvent<Collider, FingerTapAction> OnTriggerStayFoward;

    private FingerTapAction _lastActivatedTrigger = new FingerTapAction{  Type = FingerTapActionType.None } ;

    private bool _hasEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_hasEntered)
        {
            _hasEntered = true; 
            var _activationObj = other.gameObject.GetComponent<FowardTriggerEvents>();
            if (_activationObj != null)
                _lastActivatedTrigger = new FingerTapAction { Type = _activationObj.ThisAction, TypeLook = ThisActionMaterial };
            else
                _lastActivatedTrigger = FingerTapAction.Default;

            OnTriggerEnterFoward.Invoke(other, _lastActivatedTrigger);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _hasEntered = false ;
        OnTriggerExitFoward.Invoke(other, _lastActivatedTrigger);
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerStayFoward.Invoke(other, _lastActivatedTrigger);
    }


}
