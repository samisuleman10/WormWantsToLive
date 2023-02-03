using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnablerRunChecker : MonoBehaviour
{


    private void Awake()
    {
        RunEnabler.ChangeReplacerRunStatusSubscription += ChangeRunButton;
    }
    private void OnDestroy()
    {
        RunEnabler.ChangeReplacerRunStatusSubscription -= ChangeRunButton;
    }

    private void ChangeRunButton(bool newStatus)
    {
        this.gameObject.SetActive(newStatus);
    }
    
    void Start()
    {
        RunEnabler.UpdateReplacerStatus();
    }


}
