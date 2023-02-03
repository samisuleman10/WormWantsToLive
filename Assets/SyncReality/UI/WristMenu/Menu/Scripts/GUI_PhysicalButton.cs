using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace utils.GUI
{
    public abstract class GUI_PhysicalButton<T> : MonoBehaviour where T : System.Enum
    {
        public T menuToOpen;
        public string message = "";
        //Todo: Inject parent?
        GUI_MenuManager<T> manager;
        void Awake()
        {
            manager = GetComponentInParent<GUI_MenuManager<T>>();
        }
        public void OpenMenu()
        {
            manager.LoadMenu(menuToOpen, message);
        }
    }
}

