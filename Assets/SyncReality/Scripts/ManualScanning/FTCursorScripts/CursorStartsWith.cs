
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Syncreality
{

    public class CursorStartsWith : MonoBehaviour
    {
        
        public FingerTapActionType ThisNewFTCAction = FingerTapActionType.None;

        private void Start()
        {
            FTCursorsEventManager.ChangeFTCActionType(ThisNewFTCAction, Color.blue);
            //FTCursorsEventManager.SendBasicCursorEvent( FingerTapEvent.Confirm );
        }
    }
}
