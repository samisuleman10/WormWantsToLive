using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Creates virtual objects based on SyncInstances and SyncRelations
/// </summary>
///
///


public class GamifierModule : ModuleBase<(List<GameObject> syncs, List<Sync> virtualObjcets), List<GameObject>>
{

    /// <summary>
    /// Creates virtual objects
    /// </summary>
    /// <param name="input">All SyncInstances and virtualObjects</param>
    /// <returns>All Objects</returns>
    public override List<GameObject> Execute((List<GameObject> syncs, List<Sync> virtualObjcets) input)
    {

        //     Random.seed = 0;

        Random.InitState(1231);
//        input.syncs.ForEach(t => Debug.Log(t.name));

        var nonPlacedHeros = FindObjectOfType<DesignAccess>().GetSyncs().Where(
                s => s.quality == SyncQuality.Hero
                ).ToList();
        nonPlacedHeros = nonPlacedHeros.Where(s => input.syncs.Where(t => { return t.name.Contains(s.name); }).Count() == 0).ToList();
         
        #if UNITY_EDITOR
        if(!Application.isPlaying)
        {
            nonPlacedHeros = FindObjectOfType<DesignAccess>().GetSyncs().Where(
                    s => s.quality == SyncQuality.Hero && 
                         ToolWindowSettings.Instance.objectsToDrawInPreview.Where(os => os.objectToDraw != null && os.objectToDraw.GetComponent<Sync>() && os.objectToDraw.GetComponent<Sync>() == s).Count() == 0)
                .ToList();
        }
#endif

   //     Debug.Log(nonPlacedHeros.Count);
        var combinedSyncs = input.virtualObjcets;
        combinedSyncs.AddRange(nonPlacedHeros);

        var allMatrices = new List<(Matrix4x4 mat, GameObject obj)>();
        foreach (var sync in combinedSyncs.OrderBy(s=>s.quality == SyncQuality.Virtual).ThenBy(s => s.hasFreeArea))
        {
            
                int id = 0;
                var matrixToAdd = Matrix4x4.Scale(Vector3.one * 10f) * Matrix4x4.Translate(new Vector3(1, 10, 1));

                //            Debug.Log("Gamifier");


                var matrices = new List<Matrix4x4>();
                if (sync.GetSyncLinksFromComponents().Count > 0)
                {
                foreach (var link in sync.syncLinks)
                {
                    if (!Application.isPlaying)
                    {
#if UNITY_EDITOR
                        
                        var otherObjs = ToolWindowSettings.Instance.objectsToDrawInPreview
                            .Where((o) => {

                                if (o.objectToDraw == link.linkedObject)
                                    return true;
                                if ((o.objectToDraw?.GetComponent<Sync>() && link.linkedObject?.GetComponent<Sync>()))
                                return link.linkedObject?.GetComponent<Sync>().ID == (o.objectToDraw?.GetComponent<Sync>().ID);

                                if (link.linkedObject?.GetComponent<SurroundSync>() && (o.objectToDraw?.GetComponentInParent<SurroundSync>()))
                                    return link.linkedObject?.GetComponent<SurroundSync>()?.ID == (o.objectToDraw?.GetComponentInParent<SurroundSync>()?.ID);


                                return false; }
                        ); //Name is compared for SurroundSyncs

                        if (otherObjs == null || otherObjs.Count() == 0)
                            continue;

                        foreach (var otherObj in otherObjs)
                        {
                            matrixToAdd =
                            Matrix4x4.Translate(otherObj.transform.ExtractPosition()) *
                            Matrix4x4.Rotate((otherObj.objectToDraw.GetComponent<Sync>() ? otherObj.transform.rotation : Quaternion.Euler(0, -180, 0) * otherObj.objectToDraw.transform.rotation)) *
                            Matrix4x4.Translate(link.anchorVector.ScaleBy(otherObj.transform.ExtractScale().ScaleBy(otherObj.objectToDraw.GetComponent<Sync>() ? otherObj.objectToDraw.GetComponent<Sync>().bounds.max : Vector3.one) * 1f)) *

                            Matrix4x4.Translate(link.offsetVector) *

                            Matrix4x4.Rotate(Quaternion.Inverse((link.linkedObject.transform.rotation)))*
                            Matrix4x4.Scale((link.scaleWithSync ? otherObj.transform.lossyScale : Vector3.one)) *
                                (otherObj.objectToDraw.GetComponent<Sync>() ? Matrix4x4.identity : Matrix4x4.identity) //*
                                                                                                                                                   //(otherObj.objectToDraw.GetComponent<Sync>() ? Matrix4x4.identity : Matrix4x4.Translate(new Vector3(otherObj.objectToDraw.combinedMesh().extents.x, 0, otherObj.objectToDraw.combinedMesh().extents.z)))
                            ;
                            matrices.Add(matrixToAdd);
                            if (!link.onEveryClutter)
                                break;
                        }
#endif
                    }

                    else
                    {
                        var otherObjs = input.syncs.Where(
                            o =>
                            {

                                if (o == link.linkedObject)
                                    return true;
                                if ((o?.GetComponent<Sync>() && link.linkedObject?.GetComponent<Sync>()))
                                    return link.linkedObject?.GetComponent<Sync>().ID == (o?.GetComponent<Sync>().ID);

                                if (link.linkedObject?.GetComponent<SurroundSync>() && (o?.GetComponentInParent<SurroundSync>()))
                                    return link.linkedObject?.GetComponent<SurroundSync>()?.ID == (o?.GetComponentInParent<SurroundSync>()?.ID);


                                return false;
                            });
                        /*if (otherObj == null)
                        { 
                            var foundObject = FindObjectsOfType<SurroundSync>().Where
                            (o =>
                            { 
                                return o.name.Contains(link.linkedObject.name);
                            }).FirstOrDefault();
                            otherObj = foundObject?.transform.GetChild(0).gameObject;

                        }*/
                        if (otherObjs == null || otherObjs.Count() == 0)
                            otherObjs = FindObjectsOfType<SurroundSync>().Where(o => o.GetComponentInParent<SurroundSync>() && o.GetComponentInParent<SurroundSync>()?.ID == link.linkedObject?.GetComponent<SurroundSync>()?.ID).Select(t => t.transform.GetChild(0).gameObject);

                        if (otherObjs == null || otherObjs.Count() == 0)
                            continue;

                        foreach (var otherObj in otherObjs)
                        {

                            if (!otherObj.GetComponent<Sync>())
                                Debug.Log(otherObj.name);
                            matrixToAdd =
                                Matrix4x4.Translate(otherObj.transform.position) *
                                Matrix4x4.Rotate(otherObj.transform.rotation) *
                                Matrix4x4.Translate(link.anchorVector.ScaleBy(otherObj.transform.localScale.ScaleBy(otherObj.GetComponent<Sync>() ? otherObj.GetComponent<Sync>().bounds.max : Vector3.one) * 1f)) *
                                Matrix4x4.Translate(link.offsetVector) *
                                

                                Matrix4x4.Rotate(Quaternion.Inverse((link.linkedObject.transform.rotation))) *
                                Matrix4x4.Scale((link.scaleWithSync ? otherObj.transform.localScale : Vector3.one))
                                ;
                            matrices.Add(matrixToAdd);
                            allMatrices.Add((matrixToAdd, otherObj));
                        }
                    }
                }
                }
                else
                {
                    var mocks = FindObjectOfType<ModuleManager>().currentMocks;
                    matrixToAdd = Matrix4x4.TRS(
                        FindObjectOfType<LayoutArea>().getRandomBoxSpaceInPolygon(false, sync.hasFreeArea ? new Bounds(sync.freeArea.position, new Vector3(sync.freeArea.width, 1, sync.freeArea.height) * 1f) : sync.bounds, sync.transform.rotation, 
                        allMatrices.Select(m => {
                            var s = m.obj.GetComponent<Sync>();
                            if(s == null)
                            {
                                return new Bounds(m.mat.ExtractPosition(), m.mat.ExtractScale());
                            }
                            return new Bounds(m.mat.ExtractPosition(), s.hasFreeArea ? new Vector3(s.freeArea.width, 1, s.freeArea.height) : m.mat.ExtractScale().ScaleBy(s.bounds.size * 2f));
                            })), 
                        sync.transform.rotation, Vector3.one);
                    matrices.Add(matrixToAdd);
                    allMatrices.Add((matrixToAdd, sync.gameObject));
                }

                foreach(var matrix in matrices)
                {

                if (Application.isPlaying)
                {
                    //                Debug.Log("Gamified: " + sync.name);
                    var o = sync.gameObject;
                    o.transform.position = matrix.ExtractPosition();
                    o.transform.rotation = matrix.ExtractRotation() * sync.transform.rotation;
                    o.transform.localScale = sync.transform.localScale.ScaleBy(matrix.lossyScale);
                    o.transform.parent = FindObjectOfType<StoryTeller>().syncsParent;
                }

#if UNITY_EDITOR


                ToolWindowSettings.Instance.objectsToDrawInPreview.Add(
                    new ToolWindowSettings.ObjectInfo(id,
                    sync.gameObject,
                    matrix * Matrix4x4.Rotate(sync.transform.rotation),
                    sync.transform,
                    null,
                    WallFace.East
                    ));
                    //ToolWindowSettings.Instance.pru.DrawMesh(mf.sharedMesh, Matrix4x4.TRS(scan.position, scan.transform.rotation, scan.scale.ScaleBy(sync.bounds.size.invert())) * transformMatrix * Matrix4x4.Translate(-sync.transform.TransformVector(sync.bounds.center)), mr.sharedMaterial, 0);


#endif
                }
            
        }

        Random.InitState(System.Environment.TickCount);
        if (!Application.isPlaying)
            return input.syncs;
        else
        {
            var st = FindObjectOfType<StoryTeller>();
            var retList = new List<GameObject>();
            foreach (Transform child in st.syncsParent)
                retList.Add(child.gameObject);
            foreach (Transform child in st.surroundSyncsParent)
                retList.Add(child.gameObject);
            return retList;
        }
    }
    
    
    // virtual syncs:
    // If there is a Synclink with just one, Always relate the virtual to the nonvirtual as secondary
    // If there isa  SyncLink with two Virtuals, just use the primary and secondary objects
    
    
    
    
}
