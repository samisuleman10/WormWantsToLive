using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProfileData
{
    [SerializeField]
    public int LastIDCount = -1;
    [SerializeField]
    public List<RoomData> roomDatas = new List<RoomData>();
}