using Syncreality.Playfield;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour
{
    public PlayfieldInteraction playfield; 
    Vector3 goToPos;
    bool isDragged;
    public float DragDistance;
    // Start is called before the first frame update
    void Start()
    {
        playfield.tapEvent.AddListener(pos =>
        {
            if (Vector3.Distance(pos, transform.position) < DragDistance)
            {
                goToPos = pos;
                isDragged = true;
            }
        });
        playfield.dragEvent.AddListener(pos => goToPos = pos);
        playfield.releaseEvent.AddListener(pos => isDragged = false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isDragged)
        {
            transform.position = Vector3.Lerp(transform.position, goToPos, 0.5f * Time.deltaTime * 60f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 0, 1, 0.5f);
        Gizmos.DrawSphere(transform.position, DragDistance);
    }
}
