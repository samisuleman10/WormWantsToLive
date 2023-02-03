using System;

[Serializable]
public class AnchorIdentifier
{
    public string AppIdentifier;
    public string AnchorId;

    public AnchorIdentifier(string appIdentifier, string anchorId)
    {
        AppIdentifier = appIdentifier;
        AnchorId = anchorId;
    }
}
