using Syncreality.Playfield;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayfieldAnchor : MonoBehaviour
{
    public Transform playfield;
    public Vector3 anchor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(playfield != null)
        {
            transform.position = playfield.transform.position + playfield.transform.rotation *
                    new Vector3(
                    anchor.x * playfield.transform.localScale.x * 0.5f, 
                    anchor.y * playfield.transform.localScale.y * 0.5f, 
                    anchor.z * playfield.transform.localScale.z * 0.5f);
            transform.rotation = playfield.transform.rotation;
        }
    }
}
