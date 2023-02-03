using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
public class WallPiece : MonoBehaviour
{
 [HideInInspector] public WallFace face;
 [HideInInspector] public int index;
// [HideInInspector] public SurroundSync anchoredSurroundSync = null;
 [HideInInspector] public List<SurroundSync> anchoredSurroundSyncs;
 [HideInInspector] public GameObject anchoredWallMini = null;

 public GameObject creationModel;


 


}
#endif