using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace utils.GUI
{
    [ExecuteAlways]
    public class GUI_PhysicalScaler : MonoBehaviour
    {
        public Transform targetChild;
        RectTransform parentCanvas;

        private void Awake()
        {
            SetButtonSize(GetButtonBoundaries());
        }
        protected void OnRectTransformDimensionsChange()
        {
            SetButtonSize(GetButtonBoundaries());

        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            SetButtonSize(GetButtonBoundaries());
        }
#endif


        /*Get the desired button width and height (scaled according to parent canvas)*/
        Vector2 GetButtonBoundaries()
        {
            RectTransform thisRectTransform = GetComponent<RectTransform>();
            if (thisRectTransform == null)
            {
                Debug.LogWarning($"RectTransform of{gameObject.name} is null");
                //I really want a clear sign that the button scaling isn't working
                return Vector2.zero;
            }
            float width = thisRectTransform.rect.width;
            float height = thisRectTransform.rect.height;
            //maybe cache it?
            Transform canvas = gameObject.GetComponentInParent<Canvas>()?.transform;
            if (canvas != null)
            {
                width *= canvas.localScale.x;
                height *= canvas.localScale.y;
            }

            return new Vector2(width, height);
        }

        /*Set the physical button size to match a certain width and height*/
        void SetButtonSize(Vector2 dimensions)
        {
            if (targetChild == null) return;
            float zScale = targetChild.localScale.z;
            targetChild.localScale = Vector3.one;
            targetChild.localScale = new Vector3(dimensions.x / targetChild.lossyScale.x, dimensions.y / targetChild.lossyScale.y, zScale);
        }
    }
}

