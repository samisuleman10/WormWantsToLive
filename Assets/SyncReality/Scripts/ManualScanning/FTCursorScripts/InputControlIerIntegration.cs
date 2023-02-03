using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OVRInput;
using Syncreality;

public class InputControlIerIntegration : MonoBehaviour
{

    public Transform LeftControllerTapPoint;
    public Transform RightControllerTapPoint;
// 
    public WristMenuController MenuLeft;
    public WristMenuController MenuRight;

    private void Update()
    {
        #region TapEvent
        if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
        {
            this.transform.position = LeftControllerTapPoint != null? LeftControllerTapPoint.position: OVRInput.GetLocalControllerPosition(Controller.LTouch);
            FTCursorsEventManager.SetNextTapPosition(this.gameObject);
            FTCursorsEventManager.SendBasicCursorEvent(FingerTapEvent.Tap);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            this.transform.position = RightControllerTapPoint != null ? RightControllerTapPoint.position : OVRInput.GetLocalControllerPosition(Controller.RTouch);
            FTCursorsEventManager.SetNextTapPosition(this.gameObject);
            FTCursorsEventManager.SendBasicCursorEvent(FingerTapEvent.Tap);
        }
        #endregion

        #region MenuEvents

        if (OVRInput.GetDown(OVRInput.RawButton.X) && MenuLeft != null && MenuLeft.CurrentMenu !=null && !MenuLeft.CurrentMenu.activeSelf && !OVRInput.IsControllerConnected(OVRInput.Controller.Hands))
        {
            MenuLeft.open();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.A) && MenuRight != null && MenuRight.CurrentMenu != null && !MenuRight.CurrentMenu.activeSelf && !OVRInput.IsControllerConnected(OVRInput.Controller.Hands))
        {
            MenuRight.open();
        }

        if (OVRInput.GetUp(OVRInput.RawButton.X) && MenuLeft != null && MenuLeft.CurrentMenu != null  && MenuLeft.CurrentMenu.activeSelf)
        {
            MenuLeft.close();
        }
        if (OVRInput.GetUp(OVRInput.RawButton.A) && MenuRight != null && MenuRight.CurrentMenu != null  && MenuRight.CurrentMenu.activeSelf)
        {
            MenuRight.close();
        }
        #endregion

        #region CancelConfirmEvent
        if (OVRInput.GetDown(OVRInput.RawButton.B) || OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            FTCursorsEventManager.SendBasicCursorEvent(FingerTapEvent.Cancel);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger) || OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
        {
            FTCursorsEventManager.SendBasicCursorEvent(FingerTapEvent.Confirm);
        }
        #endregion
    }

}
