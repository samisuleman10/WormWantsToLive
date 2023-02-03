using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoVolumeManager : MonoBehaviour
{
    
    public void UndoLastVolume()
    {

        FTCursorsEventManager.SendBasicCursorEvent(FingerTapEvent.Undo);
    }
    /*
    public void UndoAllBounds()
    {
        
        if(ScanVolumes.Count>0){

            foreach (GameObject G in ScanVolumes)
            {
                GameObject.Destroy(G);

            }
            
            ScanVolumes.Clear();
        }
        
    }
    
    public void UndoAllObjects()
    {
        
        if(ScanVolumes.Count>0){

            foreach (GameObject G in ScanVolumes)
            {
                GameObject.Destroy(G);

            }
            
            ScanVolumes.Clear();
        }
        
    }*/
    
}
