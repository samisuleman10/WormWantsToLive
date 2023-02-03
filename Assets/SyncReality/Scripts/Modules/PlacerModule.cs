using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
//using static ToolWindowSettings;

/// <summary>
/// Creates SyncInstances out of Map Dictionary
/// </summary>
public class PlacerModule : ModuleBase<Dictionary<ScanVolume, Sync>, List<GameObject>>
{
    public bool drawSyncsInScene = true;

    /// <summary>
    /// Creates SyncInstances
    /// </summary>
    /// <param name="input">Map Dictionary ScanVolumes -> Sync from Matcher</param>
    /// <returns>List of Instantiated SyncInstances</returns>

    public override List<GameObject> Execute(Dictionary<ScanVolume, Sync> input)
    {
        
        List<GameObject> results = new List<GameObject>();



        //Interate through every mapping and spawn SyncInstance accordingly
        foreach (var scan in input.Keys)
        {
            var sync = input[scan];
            if(Application.isPlaying)
            {
                StoryTeller storyTeller = FindObjectOfType<StoryTeller>();
                Sync syncInstance = null;
                if (sync.quality == SyncQuality.Clutter)
                    syncInstance = Instantiate(sync, scan.position, scan.rotation, storyTeller.syncsParent);
                else
                {
                    syncInstance = sync;
                    sync.transform.position = scan.position;
                    sync.transform.rotation = scan.rotation;
                    sync.transform.parent = storyTeller.syncsParent;
                }
                var invertScale = sync.transform.localScale.invert();
                syncInstance.transform.localScale = scan.scale.ScaleBy(syncInstance.bounds.size.invert()).ScaleBy(sync.transform.localScale);
                syncInstance.transform.position -= syncInstance.transform.TransformVector(syncInstance.bounds.center).ScaleBy(invertScale);
                results.Add(syncInstance.gameObject);
            } 
#if UNITY_EDITOR
            else
            {
                int id = 0;

                //if (ToolWindowSettings.Instance.pru != null)
                {
                    var invertScale = sync.transform.localScale.invert();
                    //                    Debug.Log("NOT NULL");
                    ToolWindowSettings.Instance.objectsToDrawInPreview.Add(
                            new ToolWindowSettings.ObjectInfo(id,
                            sync.gameObject, 
                            Matrix4x4.TRS(scan.position, scan.transform.rotation, scan.scale.ScaleBy(sync.bounds.size.invert())) * Matrix4x4.Translate(-sync.transform.TransformVector(sync.bounds.center).ScaleBy(invertScale)),
                            sync.transform,
                            scan.transform,
                            WallFace.East
                            ));
                        //ToolWindowSettings.Instance.pru.DrawMesh(mf.sharedMesh, Matrix4x4.TRS(scan.position, scan.transform.rotation, scan.scale.ScaleBy(sync.bounds.size.invert())) * transformMatrix * Matrix4x4.Translate(-sync.transform.TransformVector(sync.bounds.center)), mr.sharedMaterial, 0);
                        
                    }
                    id++;
                
            }
            #endif

        }

        return results;
    }
}
