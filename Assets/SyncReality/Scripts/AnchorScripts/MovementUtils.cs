using UnityEngine;

public static class MovementUtils
{
    public static Vector3 TranslateForward(Transform toMove, Transform relativeTo, float quantificator, Space space)
    {
        var toTranslate = new Vector3(relativeTo.forward.x, 0,relativeTo.forward.z);
        toMove.Translate(toTranslate * quantificator, space);
        return toMove.position;
    }

    public static Vector3 TranslateBackwords(Transform toMove, Transform relativeTo, float quantificator, Space space)
    {
        var toTranslate = new Vector3(relativeTo.forward.x, 0, relativeTo.forward.z);
        toMove.Translate(-toTranslate * quantificator, space);
        return toMove.position;
    }

    public static Vector3 TranslateRight(Transform toMove, Transform relativeTo, float quantificator, Space space)
    {
        var toTranslate = new Vector3(relativeTo.right.x, 0, relativeTo.right.z);
        toMove.Translate(toTranslate * quantificator, space);
        return toMove.position;
    }

    public static Vector3 TranslateLeft(Transform toMove, Transform relativeTo, float quantificator, Space space)
    {
        var toTranslate = new Vector3(relativeTo.right.x, 0, relativeTo.right.z);
        toMove.Translate(-toTranslate * quantificator, space);
        return toMove.position;
    }

    public static Vector3 TranslateUp(Transform toMove, float quantificator, Space space)
    {
        toMove.Translate(Vector3.up * quantificator,space);
        return toMove.position;
    }

    public static Vector3 TranslateDown(Transform toMove, float quantificator, Space space)
    {
        toMove.Translate(-Vector3.up * quantificator, space);
        return toMove.position;
    }

    public static Quaternion RotateRight(Transform toMove, float quantificator, Space space)
    {
        toMove.Rotate(Vector3.up * quantificator, space);
        return toMove.rotation;
    }

    public static Quaternion RotateLeft(Transform toMove, float quantificator, Space space)
    {
        toMove.Rotate(-Vector3.up * quantificator, space);
        return toMove.rotation;
    }
}
