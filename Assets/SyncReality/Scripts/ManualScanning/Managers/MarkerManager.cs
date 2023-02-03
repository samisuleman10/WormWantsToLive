using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarkerManager 
{

    public static readonly string MarkerLayer = "Marker";
    public static readonly string MarkerTag = "Marker";

    private static List<GameObject> _createdMarkers = new List<GameObject> ();



    public static bool HasMarkerLayer()
    {
        int layer = LayerMask.NameToLayer(MarkerLayer);
        return layer > -1;
    }

    public static void AddMarker(GameObject marker)
    {
        _createdMarkers.Add(marker);
    }

    public static bool HasMarkers()
    {
        return _createdMarkers.Count < 1?false: _createdMarkers.Exists( m => m != null)  ;
    }

    public static void ClearNullMarkers()
    {
        _createdMarkers = _createdMarkers.Where(m => m != null).ToList();
    }

}
