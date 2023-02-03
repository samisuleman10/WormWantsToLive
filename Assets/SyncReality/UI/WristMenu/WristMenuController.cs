using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction.Input;


 
    public class WristMenuController : MonoBehaviour
    {
        public HandRef hand;

        public float OpenMenuMin = 0.4f;
        public float OpenMenuMax = 0.9f;

        
        public float MenuXOffset = 0;
        public float MenuYOffset = 0;
        public float MenuZOffset = 0;
        
        public GameObject CurrentMenu;




        Pose wristPose;

        bool isActive;

        public Transform CanvasPos; 


        private float _closeTimeGap = 0.7f;
        private Coroutine _closeRoutine;

        void Start()
        {
            CheckForMenu();

            //open();
            close();        
            
            
        }

        void CheckForMenu()
        {
            if (CurrentMenu == null)
            {
                Debug.LogWarning("WristMenuController requires MenuObject in CurrentMenu Variable");
            }

        }

        // Update is called once per frame
        void Update()
        {
            /*
            float wristDot;
            if (hand.GetJointPose(HandJointId.HandMiddle1, out wristPose))
            {
                wristDot = Vector3.Dot(wristPose.rotation * Vector3.up, Vector3.up);
            }
            else return;


            if (isActive)
            {

                if (wristDot > OpenMenuMax || wristDot < OpenMenuMin) close();

                transform.position = wristPose.position + new Vector3(MenuXOffset, MenuYOffset, MenuZOffset) ;
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - CanvasPos.position);
            }
            else
            {
                if (wristDot < OpenMenuMax && wristDot > OpenMenuMin) open();
            }

            Debug.Log(wristDot);
            
            */
        }

        public void open()
        {
            if (CurrentMenu != null)
            {
                if(_closeRoutine != null)
                {
                    StopCoroutine(_closeRoutine);
                    _closeRoutine = null;

                }

                CurrentMenu.SetActive(true);
                isActive = true;
            }
        }

    public void InstaClose()
    {
        if (CurrentMenu != null)
        {
            CurrentMenu.SetActive(false);
            isActive = false;
        }
    }

    public void close()
        {
            if(CurrentMenu!=null)
            {
                _closeRoutine = StartCoroutine(CloseMenu());
            }
        }

    private IEnumerator CloseMenu()
    {
        yield return new WaitForSeconds(_closeTimeGap);
        CurrentMenu.SetActive(false);
        isActive = false;
    }


}
 