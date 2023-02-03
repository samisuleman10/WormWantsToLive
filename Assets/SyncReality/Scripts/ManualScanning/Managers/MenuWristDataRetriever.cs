using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MenuWristDataRetriever : MonoBehaviour
{
    
    private static GameObject _lastActiveMenu;

    public GameObject MenuToTrack;

    void Start()
    {
        if (MenuToTrack != null)
            _lastActiveMenu = MenuToTrack;
    }

    private void Update()
    {
        if (MenuToTrack != null && MenuToTrack.activeSelf)
            _lastActiveMenu = MenuToTrack;
    }

    public static GameObject GetLastActiveMenuObj()
    {
        return _lastActiveMenu;
    }

}
