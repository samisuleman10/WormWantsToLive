using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Syncreality.Playfield
{
    [ExecuteAlways]
    public class PlayfieldInteraction : MonoBehaviour
    {
        public Canvas canvas;

        public UnityEvent<Vector3> tapEvent = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> dragEvent = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> releaseEvent = new UnityEvent<Vector3>();

        // Update is called once per frame
        
        void Update()
        {
#if UNITY_EDITOR
            canvas.transform.localScale = new Vector3(1f/(transform.localScale.x * 100f), 1f / (transform.localScale.z * 100f), canvas.transform.localScale.z);
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(transform.localScale.x * 100, transform.localScale.z * 100);
#endif
        }

        private void Start()
        {
            canvas.transform.localScale = new Vector3(1f/(transform.localScale.x * 100f), 1f / (transform.localScale.z * 100f), canvas.transform.localScale.z);
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(transform.localScale.x * 100, transform.localScale.z * 100);
        }
    }
}
