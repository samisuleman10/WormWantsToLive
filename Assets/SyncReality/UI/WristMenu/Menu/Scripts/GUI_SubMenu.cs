using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace utils.GUI
{
    public class GUI_SubMenu : MonoBehaviour
    {
        //TODO: move variable to GUI_MenuManager
        public float slideDuration = 0.25f;
        [HideInInspector] public Transform spawnPoint;
        [HideInInspector] public Transform exitPoint;
        //TODO: find a better name
        [HideInInspector] public string message = "";
        List<GUI_PhysicalButton<System.Enum>> buttons = new List<GUI_PhysicalButton<System.Enum>>();
        Transform parentCanvas;
        bool sliding;
        private void Awake()
        {
            RefreshButtons();
            Canvas canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                parentCanvas = canvas.transform;

            }
        }

        void OnValidate()
        {
            RefreshButtons();
        }
        public void Init(Transform spawnPoint, Transform exitPoint, string message = "")
        {
            this.spawnPoint = spawnPoint;
            this.exitPoint = exitPoint;
            if (message.Length != 0)
                this.message = message;
        }



        public void Show()
        {
            StartCoroutine(Slide(true));
        }

        public void Hide()
        {
            StartCoroutine(Slide(false));
        }
        //Later on, buttons will only be added dynamically
        void RefreshButtons()
        {
            buttons.Clear();
            GUI_PhysicalButton<System.Enum>[] buttonsArray = gameObject.GetComponentsInChildren<GUI_PhysicalButton<System.Enum>>();
            buttons.AddRange(buttonsArray);
        }
        //TODO: SubMenu shouldn't know about sliding, make a dedicated script
        IEnumerator Slide(bool slidingIn)
        {
            sliding = true;

            Transform startingPoint = slidingIn ? spawnPoint : parentCanvas;
            Transform finalPoint = slidingIn ? parentCanvas : exitPoint;

            float timeElapsed = 0;
            while (timeElapsed < slideDuration)
            {
                float alpha = timeElapsed / slideDuration;
                transform.position = Vector3.Lerp(startingPoint.position, finalPoint.position, alpha);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (!slidingIn)
                Destroy(gameObject);
            sliding = false;
        }
    }
}

