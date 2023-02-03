using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum RoomType
{
    Unselected = 0,
    LivingRoom = 1,
    HallWay = 2
}


public class RoomTypeGameObject : MonoBehaviour
{

    public RoomType RoomType;
}
