using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Syncreality.Playfield
{
    public class PlayfieldDragable : MonoBehaviour 
    {
        public PlayfieldInteraction playfield;
        bool isMoving;
        Vector3 goToPos;
        public float interactionRadius = 1f;
        // Start is called before the first frame update
        void Start()
        {
            playfield.tapEvent.AddListener(pos =>
            {
                if(Vector3.Distance(transform.position, pos) <= interactionRadius)
                    isMoving = true;
                goToPos = pos;
            });
            playfield.dragEvent.AddListener(pos =>
            {
                goToPos = pos;
            });
            playfield.releaseEvent.AddListener(pos =>
            {
                isMoving = false;
            });
        }

        // Update is called once per frame
        void Update()
        {
            if(isMoving)
            {
                transform.position = Vector3.Lerp(transform.position, goToPos, 0.5f * Time.deltaTime * 60f);
            }
        }

        private void OnDrawGizmosSelected()
        {

            Gizmos.color = new Color(0,0,1,0.3f);
            Gizmos.DrawSphere(transform.position, interactionRadius);
        }
    }
}
