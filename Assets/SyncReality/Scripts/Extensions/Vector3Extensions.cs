using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 ScaleBy(this Vector3 str, Vector3 scaledBy)
    {
        return new Vector3(str.x * scaledBy.x, str.y * scaledBy.y, str.z * scaledBy.z);
    }

    public static Vector3 invert(this Vector3 str)
    {
        return new Vector3(1f/str.x, 1f/str.y, 1f/str.z);
    }
    
    public static float[] ToFloatArray(this Vector3[] array) {
        var floats = new float[array.Length * 3];

        for (var i = 0; i < floats.Length; i += 3) {
            var vec = array[i / 3];

            floats[i] = vec.x;
            floats[i + 1] = vec.y;
            floats[i + 2] = vec.z;
        }
        return floats;
    }
    public static Vector3[] ToVector3Array(this float[] array) {
        var vectors = new Vector3[array.Length / 3];

        for (var i = 0; i < array.Length; i += 3) {
            var x = array[i];
            var y = array[i + 1];
            var z = array[i + 2];

            vectors[i / 3].Set(x, y, z);
        }

        return vectors;
    }
    public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2)
    {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++)
        {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }

    public static Vector2 flatten(this Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }


    public static Vector3 flip(this Vector3 str)
    {
        return new Vector3(str.z, str.y, str.x);
    }

    public static Bounds combinedMesh(this GameObject go1)
    {
        var pos = go1.transform.position;
        var rot = go1.transform.rotation;
        Matrix4x4 mat = Matrix4x4.Scale(go1.transform.localScale);

        var allMF = go1.GetComponentsInChildren<MeshRenderer>(false);
        //        Debug.Log("Meshes Founds: " + allMF.Length);

        var newMid = mat.inverse.MultiplyPoint3x4(new Vector3(
            allMF.Max(m => (m.bounds.max).x) + (allMF.Min(m => (m.bounds.min).x)),
            allMF.Max(m => (m.bounds.max).y) + (allMF.Min(m => (m.bounds.min).y)),
            allMF.Max(m => (m.bounds.max).z) + (allMF.Min(m => (m.bounds.min).z))
            ) * 0.5f).ScaleBy(go1.transform.localScale);

        var newSize = mat.inverse.MultiplyPoint3x4(new Vector3(
            allMF.Max(m => ((m.bounds.max)).x) -
            (
            allMF.Min(m => ((m.bounds.min)).x)),
            allMF.Max(m => ((m.bounds.max)).y) -
            (
            allMF.Min(m => ((m.bounds.min)).y)),
            allMF.Max(m => ((m.bounds.max)).z) -
            (
            allMF.Min(m => ((m.bounds.min)).z)))).ScaleBy(go1.transform.localScale);


        var b = new Bounds(
            newMid,
            newSize);
        return b;
    }

}