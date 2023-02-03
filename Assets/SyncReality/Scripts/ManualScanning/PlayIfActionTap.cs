using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FingerTapCursor;

public class PlayIfActionTap : MonoBehaviour
{

    public AudioSource SoundSource;

    public void PlayIfTap(FingerTapEventParameters param)
    {
        if (param.EventtoActivate == FingerTapEvent.Tap && SoundSource != null)
        {
            SoundSource.PlayOneShot(SoundSource.clip);
        }
    }
}
