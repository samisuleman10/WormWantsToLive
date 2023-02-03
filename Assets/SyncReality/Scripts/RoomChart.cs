using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Represents: a collection of SyncLayout-RoomLayout matchings for multi-room application
// -> linking needs separate data type

public enum RoomLink
{
   Teleport, 
   Visualize
}
public struct Chart
{
   private RoomLayout _roomOne;
   private RoomLayout _roomTwo;
   private RoomLink _roomLink; 
  
}

public class RoomChart : MonoBehaviour
{
   public string ID;

//   public List<SyncLayout> syncLayouts;
//   public List<RoomLayout> roomLayouts;

   public Dictionary<RoomLayout, List<SyncLayout>> layoutMapping;


}
