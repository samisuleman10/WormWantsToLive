using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEngine;
using static SurrounderModule;

/// <summary>
/// Matches ScanVolumes and Syncs, based on Sync Settings and SyncLinks
/// </summary>
public class MatcherModule : ModuleBase<(List<ScanVolume> scans, List<Sync> syncs, List<WallInfo> wallInfos), Dictionary<ScanVolume, Sync>>
{
    /// <summary>
    /// Creates Dictionary of matched ScanVolumes and Syncs, based on Sync Settings and SyncLinks
    /// </summary>
    /// <param name="input">ScanVolumes and Syncs</param>
    /// <returns>Dictionary of matches ScanVolumes and Syncs</returns>
    public override Dictionary<ScanVolume, Sync> Execute((List<ScanVolume> scans, List<Sync> syncs, List<WallInfo> wallInfos) input )
    {
        var scores = setupScanSyncScores(input.syncs, input.scans);


        //Preperation for Filtering
        input.syncs = input.syncs
            .OrderBy((s1) => (int)s1.quality)
            .ToList();

        input.scans = input.scans.OrderBy(s => s.position.y).ToList();

        //Dictionary Creation
        var result = new Dictionary<ScanVolume, Sync>();

        var syncQueue = input.syncs
            .Where(s => s.quality != SyncQuality.Clutter)
            .OrderBy(s => (int)s.quality)
            .ThenByDescending(s => s.hasFreeArea)
            .ThenBy(s => s.GetComponents<SyncLink>().ToList().Count)
            .ToList();



        while (syncQueue.Count > 0)
        {
            var sync = syncQueue[0];
            var scans = getScansThatFulfillSyncLinks(input.scans, sync, syncQueue);
            var choosenScan = getScanChoosen(scans.Count > 0 ? scans : input.scans, input.scans, sync, result, input.wallInfos);
            if(choosenScan != null)
                result.Add(choosenScan, sync);
            syncQueue.RemoveAt(0);

            syncQueue = syncQueue
                .OrderBy(s => (int)s.quality)
                .ThenByDescending(s => s.GetComponentsInChildren<SyncLink>().Where(sl => result.Values.Contains(s)).Count())
                .ThenBy(s => s.GetComponents<SyncLink>().ToList().Count).ToList();
        }

        foreach (var scan in input.scans)
        {
            var choosenSync = getSyncChoosen(scan, input.syncs.Where(s => s.quality == SyncQuality.Clutter).OrderBy(s => s.GetComponents<SyncLink>().ToList().Count).ToList(), result);
            if (choosenSync != null)
                result.Add(scan, choosenSync);
        }
        foreach (var t in result)
            t.Value.scan = t.Key;
        return result;
    }



    /// <summary>
    /// Chooses a Sync for a specific ScanVolume, used for clutter
    /// </summary>
    /// <param name="scan">current ScanVolume</param>
    /// <param name="leftSyncs">List of remaining possible syncs</param>
    /// <param name="appliedSyncs">Already mapped Syncs</param>
    /// <returns></returns>
    private Sync getSyncChoosen(ScanVolume scan, List<Sync> leftSyncs, Dictionary<ScanVolume, Sync> appliedSyncs)
    {



        //Filter By Classification
        var filterList = leftSyncs
            .Where(sync => sync.classificationList.Contains(scan.classification))
            .Where(sync =>
            {
                var syncSizeMin = sync.minSize.ScaleBy(sync.bounds.size);
                var syncSizeMax = sync.maxSize.ScaleBy(sync.bounds.size);
                var scanSize = scan.bounds.size;
                return !sync.useSizes ||
                (syncSizeMin.x <= scanSize.x && syncSizeMin.y <= scanSize.y && syncSizeMin.z <= scanSize.z &&
                syncSizeMax.x >= scanSize.x && syncSizeMax.y >= scanSize.y && syncSizeMax.z >= scanSize.z);
            }).ToList();

        var newFilterList = filterSyncsBySyncLink(filterList, appliedSyncs, scan);


        if (newFilterList.Count() != 0)
            filterList = newFilterList;
        else
        {
            filterList = filterList.Where(f => f.GetComponents<SyncLink>().ToList().Count == 0).ToList();
        }
        //Filter out clutter fillers, if there still is other stuff to place
        if(filterList.Where(f=>f.quality != SyncQuality.Clutter).ToList().Count > 0)
        {
            filterList = filterList.Where(f => f.quality != SyncQuality.Clutter).ToList();
        }

        //Filter out only syncs with lowest dependencies
        for (int i = 0; i < filterList.Count; i++)
        {
            Sync sync = filterList[i];
            foreach(var syncLink in sync.GetComponents<SyncLink>())
                if(appliedSyncs.Values.Where(appliedSync => appliedSync == syncLink.linkedObject.GetComponentInParent<Sync>()).ToList().Count == 0)
                {
                    //Debug.Log("Removed for ");
                    filterList.Remove(sync);
                    i--;
                    break;
                } else
                {
                  //  Debug.Log("Hold");
                }
        }
        


        //Order by volume;
        filterList = filterList.OrderBy(sync => Mathf.Abs(sync.bounds.Volume() - scan.bounds.Volume())).ToList();
        //Choose best half
        filterList.RemoveRange((int)(filterList.Count * 0.5f), (int)(filterList.Count * 0.5f));

        //Order by aspect Ratio
        filterList = filterList.OrderBy(sync => Mathf.Abs((sync.bounds.size.x / sync.bounds.Volume() - scan.bounds.size.x / scan.bounds.Volume()) + (sync.bounds.size.y / sync.bounds.Volume() - scan.bounds.size.y / scan.bounds.Volume()) + (sync.bounds.size.z / sync.bounds.Volume() - scan.bounds.size.z / scan.bounds.Volume()))).ToList();
        //Choose best half
        filterList.RemoveRange((int)(filterList.Count * 0.5f), (int)(filterList.Count * 0.5f));

        Sync chosenSync = filterList.FirstOrDefault();

        //Remove Sync from List if it is not a clutter Sync
        if (chosenSync != null && chosenSync.quality != SyncQuality.Clutter)
            leftSyncs.Remove(chosenSync);

        return chosenSync;
    }

    [Range(0, 10)]
    public float volumeRelevance = 1;
    [Range(0, 10)]
    public float aspectRatioRelevance = 1;


    
    /// <summary>
    /// Get best fitting scan for a specified sync, used for non clutter
    /// </summary>
    /// <param name="scansToCheck">All scans left in consideration</param>
    /// <param name="sync">Sync to choose a scan for</param>
    /// <returns>Choosen ScanVolume</returns>
    private ScanVolume getScanChoosen(List<ScanVolume> scansToCheck, List<ScanVolume> leftScans, Sync sync, Dictionary<ScanVolume, Sync> appliedSyncs, List<WallInfo> wallInfos)
    {
        var allMocks = FindObjectsOfType<MockPhysical>(true).ToList();
        ScanVolume result = null;
        var filteredScanList = scansToCheck
            .Where(scan =>
            {
                if(!sync.hasFreeArea)
                    return true;
                var r = sync.freeArea;
                var scaleFactor = scan.transform.lossyScale.ScaleBy(sync.gameObject.combinedMesh().size.invert());
                var p = scan.transform.position + scan.transform.rotation * (new Vector3(r.x, 0, r.y).ScaleBy(scaleFactor));

                var size = new Vector3(r.width, 0, r.height).ScaleBy(scaleFactor);
                var p1 = p + scan.transform.rotation * size * 0.5f;
                var p2 = p - scan.transform.rotation * size * 0.5f;
                var p3 = p + scan.transform.rotation * new Vector3(size.x, 0, -size.y) * 0.5f;
                var p4 = p - scan.transform.rotation * new Vector3(size.x, 0, -size.y) * 0.5f;

                //ToolWindowSettings.Instance.objectsToDrawInPreview.Add(new ToolWindowSettings.ObjectInfo(0, debugSphere, Matrix4x4.TRS(p, Quaternion.identity, size * 0.5f), null, null, WallFace.North));
                //ToolWindowSettings.Instance.objectsToDrawInPreview.Add(new ToolWindowSettings.ObjectInfo(0, debugSphere, Matrix4x4.TRS(p + size * 0.5f, Quaternion.identity, Vector3.one), null, null, WallFace.North));


                if(!GeometryUtils.PointInPolygon(p1, FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3()) ||
                    !GeometryUtils.PointInPolygon(p2, FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3()) ||
                    !GeometryUtils.PointInPolygon(p3, FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3()) ||
                    !GeometryUtils.PointInPolygon(p4, FindObjectOfType<LayoutArea>().GetLayoutPointsAsVector3()))
                {
                    return false;
                }

                if(Application.isPlaying)
                    allMocks.ForEach(m => m.gameObject.SetActive(true));
                var allOverlappingColliders = 
                Physics.OverlapBox(
                    p,
                    size * 0.5f, 
                    scan.transform.rotation).ToList();
                if (Application.isPlaying)
                    allMocks.ForEach(m => m.gameObject.SetActive(false));
                allOverlappingColliders = allOverlappingColliders.Where(c => c.gameObject != scan.transform.gameObject && c.gameObject.GetComponent<MockPhysical>()).ToList();
            //    foreach (var c in allOverlappingColliders)
            //        Debug.Log("From" + scan.transform.gameObject.name + " - C: " + c.gameObject.name + " " + (allOverlappingColliders.Count == 0));
                bool useIt = allOverlappingColliders.Count == 0;
                return useIt;
                
            })
            .Where(scan => sync.classificationList.Contains(scan.classification))
            .Where(scan =>
            {
                var syncSizeMin = sync.minSize.ScaleBy(sync.bounds.size);
                var syncSizeMax = sync.maxSize.ScaleBy(sync.bounds.size);
                var scanSize = scan.bounds.size;
                return !sync.useSizes || 
                (syncSizeMin.x <= scanSize.x && syncSizeMin.y <= scanSize.y && syncSizeMin.z <= scanSize.z &&
                syncSizeMax.x >= scanSize.x && syncSizeMax.y >= scanSize.y && syncSizeMax.z >= scanSize.z);
            }).ToList();

        

        //Order by Volume and aspect ratio
        filteredScanList = filteredScanList
            .OrderBy(scan => 
            //Volume
        Mathf.Abs(scan.bounds.Volume() - sync.bounds.Volume()) * volumeRelevance * 0.1f +
        //Aspect Ratio
        Mathf.Abs(
            (sync.bounds.size.x / sync.bounds.size.x - scan.bounds.size.x / scan.bounds.size.x) + 
            (sync.bounds.size.y / sync.bounds.size.x - scan.bounds.size.y / scan.bounds.size.x) + 
            (sync.bounds.size.z / sync.bounds.size.x - scan.bounds.size.z / scan.bounds.size.x)) * aspectRatioRelevance).ToList();

        filteredScanList = filterScansBySyncLink(filteredScanList, appliedSyncs, sync, wallInfos);

        result = filteredScanList.FirstOrDefault();
        if(result != null)
            leftScans.Remove(result);
        return result;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

# if UNITY_EDITOR
    private void OnValidate()
    {
  //      FindObjectOfType<ToolWindowSettings>().SendPipelineUpdateSignal();
//        FindObjectOfType<ModuleManager>().UpdatePipeline();
    }
#endif
    List<ScanVolume> getScansThatFulfillSyncLinks(List<ScanVolume> filteredScanList, Sync sync, List<Sync> leftSyncs)
    {
        List<(int score, ScanVolume scan)> result = new List<(int score, ScanVolume scan)>();
        var dependentSyncs = leftSyncs.Where(ls => ls.GetComponent<SyncLink>() != null && ls.GetComponent<SyncLink>().linkedObject.GetComponent<Sync>() == sync);
     //   Debug.Log("Check Dependent Syncs");
     //   int bestScore = 0;
        if (dependentSyncs.Count() > 0)
        {
            //TODO: CONSIDER PAIRS/TRIPLES/...
         //   Debug.Log("There are Dependent Syncs " + (String.Concat(dependentSyncs.Select(s => s.gameObject.name))));
            /*foreach(var comb in combs)
            {
                Debug.Log("New Comb");
                Debug.Log(String.Join(", ", comb.Select(c => c.transform.gameObject.name)));
            }*/
            //foreach(var dependentSync in dependentSyncs)
            //{


            int initalCombLength = dependentSyncs.ToList().Count;
            var links = dependentSyncs.Select(s => s.GetComponent<SyncLink>());
            bool addIt = false;
            
            while (!addIt && initalCombLength > 0)
            {
                var allScanCombinations = filteredScanList.Combinations(initalCombLength);
                foreach (var scanCombinations in allScanCombinations)
                {
                    foreach (var scan2 in filteredScanList)
                    {
                    if (scanCombinations.Contains(scan2))
                        continue;

                    addIt = links.All(link =>
                    {
                        if (link.topOf || link.bottomOf)
                        {
                            if (topOfBottomOfCondition(scanCombinations, scan2.transform.gameObject, link))
                                return true;
                        }
                        if (link.maxDistance > 0)
                        {
                            if (maxDistanceCondition(scanCombinations, scan2.transform.gameObject, link, false, Vector2.zero))
                                return true;
                        }
                        if (link.minDistance > 0)
                        {
                            if (minDistanceCondition(scanCombinations, scan2.transform.gameObject, link, false, Vector2.zero))
                                return true;
                        }
                        return false;

                    });
                    if (addIt)
                        {

                            result.Add((initalCombLength, scan2));
                        }

                }
            }
                initalCombLength--;
            }
            
            //}
            return result.Where(r => r.score == result.Max(g => g.score)).Select(t => t.scan).ToList();
        }
        return filteredScanList;
    }

    /// <summary>
    /// Applies the SyncLinks that might be attached to a sync
    /// </summary>
    /// <param name="filteredScanList">The Left Scans in the List</param>
    /// <param name="appliedSyncs">The already applied syncs, which might be referenced by the syncLink</param>
    /// <param name="sync">The current Sync</param>
    /// <returns></returns>
    List<ScanVolume> filterScansBySyncLink(List<ScanVolume> filteredScanList, Dictionary<ScanVolume, Sync> appliedSyncs, Sync sync, List<WallInfo> wallInfos)
    {
        if (sync.GetComponent<SyncLink>())
        {
            var link = sync.GetComponent<SyncLink>();
            GameObject refScan = null;

            bool isSurroundSync = false;
            Vector3 surroundSyncRefPos = Vector3.zero;
            if (link.linkedObject.GetComponent<Sync>())
            {

                var LinkedScans = appliedSyncs.Where(x => x.Value == link.linkedObject.GetComponent<Sync>()).Select(x => { (ScanVolume scan, Sync sync) res = (x.Key, x.Value); return res; }).ToList();
                refScan = LinkedScans.FirstOrDefault().scan?.transform.gameObject;
                if (refScan == null)
                    return filteredScanList;
            }
            if(link.linkedObject.GetComponent<SurroundSync>())
            {
                isSurroundSync = true;
                var surroundSync = link.linkedObject.GetComponent<SurroundSync>(); 
                var wallTile = wallInfos.Where(wi => wi.dir == surroundSync.face && surroundSync.mapToIndex == wi.faceIndex).FirstOrDefault(); 
                surroundSyncRefPos = wallTile.currentPos;
                
            }
            if ((link.topOf || link.bottomOf) && !isSurroundSync)
            {
                filteredScanList = filteredScanList.Where(scan =>
                {
                    return topOfBottomOfCondition(new[] { scan }, refScan, link);
                }).ToList();

            }

            if(link.minDistance > 0)
            {
                filteredScanList = filteredScanList.Where(scan =>
                {
                    return minDistanceCondition(new[] { scan }, refScan, link, isSurroundSync, surroundSyncRefPos);
                }).ToList();
            }

            if (link.maxDistance > 0)
            {
                filteredScanList = filteredScanList.Where(scan =>
                {
                    return maxDistanceCondition(new[] { scan }, refScan, link, isSurroundSync, surroundSyncRefPos);
                }).ToList();
            }

            if (link.preferredDistance > 0)
            {
                filteredScanList = filteredScanList.OrderBy(scan =>
                {
                    return Mathf.Abs(Vector3.Distance(scan.transform.position, isSurroundSync ? surroundSyncRefPos : refScan.transform.position) - link.preferredDistance);
                }).ToList();
            }

            if (link.rotationConstraint)
            {
                filteredScanList = filteredScanList.Where(scan =>
                {
                    var angle = Vector3.Angle(scan.transform.forward, refScan.transform.forward);
                    bool inside = (angle <= link.maxAngle && angle >= link.minAngle);
                    return link.rotationOutside ? !inside : inside;
                }).ToList();
            }
        }
        return filteredScanList;
    }

    bool topOfBottomOfCondition(IEnumerable<ScanVolume> scans, GameObject refScan, SyncLink link)
    {
        List<bool> allConditionsFullfilled = new List<bool>();
        foreach (var scan in scans)
        {
            float scanBottomY = scan.transform.TransformPoint(link.topOf ? Vector3.down * 0.5f : Vector3.up * 0.5f).y;
            float refScanTopY = refScan.transform.TransformPoint(Vector3.up * 0.5f).y;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one + Vector3.up * 100f);
            var pointInLocalRefSpace = refScan.transform.InverseTransformPoint(scan.transform.position);
            allConditionsFullfilled.Add((link.topOf ? refScanTopY - 0.1f <= scanBottomY : refScanTopY >= scanBottomY)
            && bounds.Contains(pointInLocalRefSpace));
        }
        return allConditionsFullfilled.All(c => c);
    }

    bool maxDistanceCondition(IEnumerable<ScanVolume> scans, GameObject refScan, SyncLink link, bool isSurroundSync, Vector3 surroundSyncRefPos)
    {
        return scans.All(scan => Vector3.Distance(scan.transform.position, isSurroundSync ? surroundSyncRefPos : refScan.transform.position) <= link.maxDistance);
    }
    bool minDistanceCondition(IEnumerable<ScanVolume> scans, GameObject refScan, SyncLink link, bool isSurroundSync, Vector3 surroundSyncRefPos)
    {
        return scans.All(scan => Vector3.Distance(scan.transform.position, isSurroundSync ? surroundSyncRefPos : refScan.transform.position) >= link.minDistance);
    }
    

   List<Sync> filterSyncsBySyncLink(List<Sync> filteredSyncList, Dictionary<ScanVolume, Sync> appliedSyncs, ScanVolume scan)
    {
        foreach(var sync in filteredSyncList)
        if (sync.GetComponent<SyncLink>())
        {
            var link = sync.GetComponent<SyncLink>();
            var LinkedScans = appliedSyncs.Where(x => x.Value == link.linkedObject.GetComponent<Sync>()).Select(x => { (ScanVolume scan, Sync sync) res = (x.Key, x.Value); return res; }).ToList();
            var refScan = LinkedScans.FirstOrDefault().scan;
            if (refScan == null)
                continue;
            if (link.topOf || link.bottomOf)
            {
                filteredSyncList = filteredSyncList.Where(sync =>
                {
                    float scanBottomY = scan.transform.TransformPoint(link.topOf ? Vector3.down * 0.5f : Vector3.up * 0.5f).y;
                    float refScanTopY = refScan.transform.TransformPoint(Vector3.up * 0.5f).y;
                    Bounds bounds = new Bounds(Vector3.zero, Vector3.one + Vector3.up * 100f);
                    var pointInLocalRefSpace = refScan.transform.InverseTransformPoint(scan.transform.position);
                    return (link.topOf ? refScanTopY <= scanBottomY : refScanTopY >= scanBottomY)
                    && bounds.Contains(pointInLocalRefSpace);
                }).ToList();

            }

            if (link.minDistance > 0)
            {
                    filteredSyncList = filteredSyncList.Where(sync =>
                {
                    return minDistanceCondition(new[] { scan }, refScan.transform.gameObject, link, false, Vector3.zero);
                }).ToList();
            }

            if (link.maxDistance > 0)
            {
                    filteredSyncList = filteredSyncList.Where(sync =>
                {
                    return maxDistanceCondition(new[] { scan }, refScan.transform.gameObject, link, false, Vector3.zero);
                }).ToList();
            }

            if (link.preferredDistance > 0)
            {
                    filteredSyncList = filteredSyncList.OrderBy(sync =>
                {
                    return Mathf.Abs(Vector3.Distance(scan.transform.position, refScan.transform.position) - link.preferredDistance);
                }).ToList();
            }

            if (link.rotationConstraint)
            {
                    filteredSyncList = filteredSyncList.Where(sync =>
                {
                    var angle = Vector3.Angle(scan.transform.forward, refScan.transform.forward);
                    bool inside = (angle <= link.maxAngle && angle >= link.minAngle);
                    return link.rotationOutside ? !inside : inside;
                }).ToList();
            }
        }
        return filteredSyncList;
    }

    /// <summary>
    /// Setsup a score list for each possible Sync and scan relation 
    /// </summary>
    /// <param name="syncs">List of syncs</param>
    /// <param name="scans">List of scans</param>
    /// <returns>Dictionary of Scan-Sync pair and corresponding score</returns>
    private Dictionary<(ScanVolume scan, Sync sync), float> setupScanSyncScores(List<Sync> syncs, List<ScanVolume> scans)
    {

        Dictionary<(ScanVolume scan, Sync sync), float> scanSyncMappingScore = new Dictionary<(ScanVolume scan, Sync sync), float>();

        foreach(var scan in scans)
        {
            foreach (var sync in syncs)
            {
                float score = -1;

                if(sync.classificationList.Contains(scan.classification))
                {
                    score = -0;
                    float scaleScore = Mathf.Abs(sync.bounds.Volume() - scan.bounds.Volume());
                    score += scaleScore;
                    float ratioScore = Mathf.Abs((sync.bounds.size.x / sync.bounds.Volume() - scan.bounds.size.x / scan.bounds.Volume()) + (sync.bounds.size.y / sync.bounds.Volume() - scan.bounds.size.y / scan.bounds.Volume()) + (sync.bounds.size.z / sync.bounds.Volume() - scan.bounds.size.z / scan.bounds.Volume()));
                    score += ratioScore;
                }

                scanSyncMappingScore[(scan, sync)] = score;
            }
        }

        return scanSyncMappingScore;
    }
}






/// <summary>
/// Includes Helper Methods for Bounds
/// </summary>
public static class BoundsExtention
{
    /// <summary>
    /// Gets the Volume of the bounds
    /// </summary>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Volume(this Bounds b)
    {
        return b.size.x * b.size.y * b.size.z;
    }

    public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
    {
        return (k == 0 ? new[] { new T[0] } :
          elements.SelectMany((e, i) =>
            elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c))));
    }
}