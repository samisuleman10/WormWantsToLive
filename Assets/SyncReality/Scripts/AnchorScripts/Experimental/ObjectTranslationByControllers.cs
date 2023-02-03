using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Object = UnityEngine.Object;

/// <summary>
/// Component moves any transform by controller's stickers. If you want to translate
/// a object, call ObjectTranslationByControllers.SetObjectToTranslate(obj) and when finished 
/// call ObjectTranslationByControllers.SetObjectToTranslate(null) or press Trigger button.
/// The intention is to reuse this script for any object to translate.
/// To see "ReferenceObjectTranslationByControllers" if an Editor reference is needed.
/// </summary>

public class ObjectTranslationByControllers : BasicEventManager<ObjectTranslationByControllers>
{
    public bool IsControllerUsedForMenu     = false;
    public bool SimplifiedNudger            = true;

    [SerializeField]
    private Transform _objectToTranslate;
    private Transform _rightHandAnchor;
    private Transform _leftHandAnchor;

    public Action onTranslationFinished;

    private void Start()
    {
        if (ControllersInputHandler.Instance == null)
        {
            var controllerInput = this.gameObject.AddComponent<ControllersInputHandler>();
            controllerInput.SetGetAreControllersUsedByMenu(IsControllerUsedForMenu);
        }

        SetupControllers();
    }
   
    private void ForceMoveItem(Transform item)
    {
        Debug.Log("ForceMoveItem");
        SetObjectToTranslate(item);
    }

    private void SetupControllers()
    {
        var controllers = Object.FindObjectsOfType<OVRControllerHelper>();
        if (controllers == null)
        {
            Debug.LogError("From ObjectTranslationByControllers, controllers not found.");
            return;
        }

        var leftController = controllers.First(x => x.m_controller == OVRInput.Controller.LTouch);
        if (leftController == null)
        {
            Debug.LogError("From ObjectTranslationByControllers, left controller not found.");
            return;
        }
        _leftHandAnchor = leftController.transform;

        var rightController = controllers.First(x => x.m_controller == OVRInput.Controller.RTouch);
        if (rightController == null)
        {
            Debug.LogError("From ObjectTranslationByControllers, right controller not found.");
            return;
        }
        _rightHandAnchor = rightController.transform;
    }

    private void Update()
    {
        var areControllersUsedByMenu = ControllersInputHandler.Instance.GetAreControllersUsedByMenu;
        if (areControllersUsedByMenu || _objectToTranslate == null || _rightHandAnchor == null || _leftHandAnchor == null)
            return;

        MoveObjectByClickedButton();

    }

    private void MoveObjectByClickedButton()
    {
        bool thumbstick2Up_RightHand = OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp);
        bool thumbstick2Down_RightHand = OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown);
        bool thumbstick2Left_RightHand = OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft);
        bool thumbstick2Right_RightHand = OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight);

        bool thumbstick2Up_LeftHand = OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp);
        bool thumbstick2Down_LeftHand = OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown);
        bool thumbstick2Left_LeftHand = OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft);
        bool thumbstick2Right_LeftHand = OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight);

        if (thumbstick2Up_RightHand)
            MovementUtils.TranslateForward(_objectToTranslate, _rightHandAnchor, 0.01f, Space.World);

        if (thumbstick2Down_RightHand)
            MovementUtils.TranslateBackwords(_objectToTranslate, _rightHandAnchor, 0.01f, Space.World);

        if (thumbstick2Left_RightHand)
            MovementUtils.TranslateLeft(_objectToTranslate, _rightHandAnchor, 0.01f, Space.World);

        if (thumbstick2Right_RightHand)
            MovementUtils.TranslateRight(_objectToTranslate, _rightHandAnchor, 0.01f, Space.World);

        if (thumbstick2Up_LeftHand)
            MovementUtils.TranslateForward(_objectToTranslate, _leftHandAnchor, 0.001f, Space.World);

        if (thumbstick2Down_LeftHand)
            MovementUtils.TranslateBackwords(_objectToTranslate, _leftHandAnchor, 0.001f, Space.World);

        if (thumbstick2Left_LeftHand)
            MovementUtils.TranslateLeft(_objectToTranslate, _leftHandAnchor, 0.001f, Space.World);

        if (thumbstick2Right_LeftHand)
            MovementUtils.TranslateRight(_objectToTranslate, _leftHandAnchor, 0.001f, Space.World);

        if (!SimplifiedNudger)
        {
            bool trigger1Pressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
            bool trigger2Pressed = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

            bool pressedDownRight = OVRInput.Get(OVRInput.Button.One)
                && CheckForControllers(InputDeviceCharacteristics.Right);

            bool pressedUpRight = OVRInput.Get(OVRInput.Button.Two)
                && CheckForControllers(InputDeviceCharacteristics.Right);

            bool pressedDownLeft = OVRInput.Get(OVRInput.Button.Three)
                && CheckForControllers(InputDeviceCharacteristics.Left);

            bool pressedUpLeft = OVRInput.Get(OVRInput.Button.Four)
                && CheckForControllers(InputDeviceCharacteristics.Left);

            if (pressedUpRight)
                MovementUtils.TranslateUp(_objectToTranslate, 0.001f, Space.World);

            if (pressedDownRight)
                MovementUtils.TranslateDown(_objectToTranslate, 0.001f, Space.World);

            if (pressedUpLeft)
                MovementUtils.RotateRight(_objectToTranslate, 0.3f, Space.World);

            if (pressedDownLeft)
                MovementUtils.RotateLeft(_objectToTranslate, 0.3f, Space.World);

            if (trigger1Pressed || trigger2Pressed) TranslationFinished();
        }
    }

    public bool CheckForControllers(InputDeviceCharacteristics controller)
    {
        List<InputDevice> leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controller, leftHandDevices);


        return leftHandDevices.Count > 0;
    }

    public void SetObjectToTranslate(Transform toTranslate)
    {
        _objectToTranslate = toTranslate;
        var newObjectToTranslateIsValid = _objectToTranslate != null;
    }

    private void TranslationFinished()
    {
        onTranslationFinished?.Invoke();
        //_objectToTranslate = null;
    }
}
