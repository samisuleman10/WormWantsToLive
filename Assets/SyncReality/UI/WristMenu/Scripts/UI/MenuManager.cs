using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum MenuName
{
    Default,
    SpaceSetup,
    FAQ,
    RoomType,
    BoundsSetup,
    ObjectSetup,
    AdvancedObjectSetup,
    ObjectDetailSetup,
    SpaceProfile,
    RoomOptions,
    AnchoringOptions,
    ConnectedRoomOptions
}
public class MenuManager : utils.GUI.GUI_MenuManager<MenuName>
{

}
