using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#if UNITY_EDITOR
public class RoomGenerator : MonoBehaviour
{
    public LayoutArea layoutArea;
    public List<PlacementRule> placementRules = new List<PlacementRule>();


    public void GenerateRoom()
    {
        foreach (var rule in placementRules)
            rule.Init();



        var placedMocksForRule = new Dictionary<PlacementRule, List<GameObject>>();
        List<PlacementRule> appliedPlacementRules = new List<PlacementRule>();


        while(getCurrentlyPlaceableRules(appliedPlacementRules).Count > 0)
        {
            var placements = getCurrentlyPlaceableRules(appliedPlacementRules);

            var ruleToPlace = placements[Random.Range(0, placements.Count)];
            if(ruleToPlace.canBeFlipped)
            {
                ruleToPlace.maxSize = ruleToPlace.maxSize.flip();
                ruleToPlace.minSize = ruleToPlace.minSize.flip();
            }

            var scale = new Vector3(
                    Random.Range(ruleToPlace.minSize.x, ruleToPlace.maxSize.x),
                    Random.Range(ruleToPlace.minSize.y, ruleToPlace.maxSize.y),
                    Random.Range(ruleToPlace.minSize.z, ruleToPlace.maxSize.z)
                    );
            var mock = spawnMockForRule(ruleToPlace, scale, placedMocksForRule);

            if (mock == null)
            {
                ruleToPlace.leftAmountToSet--;
                continue;
            }


            if (!placedMocksForRule.ContainsKey(ruleToPlace))
                placedMocksForRule.Add(ruleToPlace, new List<GameObject>());
            placedMocksForRule[ruleToPlace].Add(mock);


            appliedPlacementRules.Add(ruleToPlace);
            ruleToPlace.leftAmountToSet--;

        }

    }

    GameObject spawnMockForRule(PlacementRule rule, Vector3 scale, Dictionary<PlacementRule, List<GameObject>> mocksForRule)
    {
        var pos = Vector3.zero;
        var normal = Vector3.zero;
        int maxTries = 100;

        var rot = Quaternion.Euler(0, rule.randomRotation ? Random.Range(0, 360) : 0, 0);
        do
        {
            var r = Random.Range(0f, rule.likelinessToBePlacedOnWalls + rule.likelinessToBePlacedWithoutWalls);

            if (r >= rule.likelinessToBePlacedOnWalls)
            {
                pos = layoutArea.getRandomBoxSpaceInPolygon(rule.stacking, new Bounds(Vector3.zero, scale), rot);

                if (pos.y < 0)
                {
                    return null;
                }
            }
            else
            {
                rot = Quaternion.identity;
                var e = layoutArea.getRandomBoxSpaceOnWall(rule.stacking, scale, rule.alignedToWall);
                pos = e.pos;
                if (pos.y < 0)
                    return null;
                normal = e.normal;
            }
            maxTries--;
        } while (rule.allowedConnectionsTo.Count != 0 && !getIfPositionIsRuleValid(pos, rule, mocksForRule) && maxTries > 0);

        if (maxTries == 0)
            return null;

        var mock = layoutArea.SpawnMock(
            pos,
            rule.classification,
            scale);

        mock.transform.rotation = rot;

        if(rule.alignedToWall)
        {
//            Debug.Log(normal);
            mock.transform.rotation = Quaternion.Euler(0, Vector2.SignedAngle(normal.flatten(), Vector2.up), 0);
        }
        return mock;
    }

    bool getIfPositionIsRuleValid(Vector3 pos, PlacementRule rule, Dictionary<PlacementRule, List<GameObject>> mocksForRule)
    {
        bool isAllowed = true;
        if (pos.y < 0)
            return false;
        foreach (var relation in rule.allowedConnectionsTo)
        {
            if (!mocksForRule.ContainsKey(relation.placementRule))
                continue;
            else
            {
                isAllowed = false;
                foreach(var mock in mocksForRule[relation.placementRule])
                {
                    Bounds b = new Bounds(mock.transform.position, mock.transform.localScale);
                    b.Expand(Vector3.up * 10f);

                    var rotPos = RotatePointAroundPivot(pos, mock.transform.position, mock.transform.rotation);

                    if (relation.onTop)
                    {
                        if (b.Contains(rotPos))
                            isAllowed = true;
                    }
                    if (relation.onSide)
                    {
                        if (!b.Contains(rotPos) && Mathf.Sqrt(b.SqrDistance(rotPos)) < 3)
                            isAllowed = true;
                    }
                }
            }
        }
        return isAllowed;
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rot)
    {
        return rot * (point - pivot) + pivot;
    }

    List<PlacementRule> getCurrentlyPlaceableRules(List<PlacementRule> appliedPlacements)
    {
        return placementRules
            .Where(r => r.leftAmountToSet > 0)
            .Where(r => r.allowedConnectionsTo.Count == 0 || r.allowedConnectionsTo.Where(or => appliedPlacements.Contains(or.placementRule)).ToList().Count > 0)
            .ToList();
    }
}
#endif
