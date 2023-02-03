using System;
using UnityEngine;

public class StatusManager : BasicEventManager<StatusManager>
{
    public Action<string, Color> onStatusAdded;

    public void OnStatusAdded(string status, Color color)
    {
        onStatusAdded?.Invoke(status,color);
    }
}
