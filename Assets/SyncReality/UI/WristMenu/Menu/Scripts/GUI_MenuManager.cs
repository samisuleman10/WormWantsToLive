using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace utils.GUI
{
    //TODO: replace with a scriptable object
    /*
    public enum MenuName
    {
        Default,
        SpaceSetup,
        FAQ,
        RoomType,
        BoundsSetup,
        ObjectSetup,
        AdvancedObjectSetup,
        ObjectDetailSetup,
        SpaceProfile,
        RoomOptions,
        AnchoringOptions,
        ConnectedRoomOptions
    }
    */


    public abstract class GUI_MenuManager<T> : MonoBehaviour where T : System.Enum
    {

        [System.Serializable]
        public class Menu
        {
            public GameObject menuPrefab;
            public T menuName;
        }

        public List<Menu> subMenus = new List<Menu>();
        public Dictionary<T, GameObject> menuDictionary = new Dictionary<T, GameObject>();

        public Transform spawnPoint;
        public Transform exitPoint;

        GUI_SubMenu activeSubMenu = null;
        Canvas thisCanvas;

        protected void Awake()
        {
            thisCanvas = GetComponent<Canvas>();
            GameObject menu = GameObject.Instantiate(subMenus[0].menuPrefab, transform.position, transform.rotation, thisCanvas.transform);
            activeSubMenu = menu.GetComponent<GUI_SubMenu>();
            activeSubMenu.Init(spawnPoint, exitPoint, "");

            menuDictionary.Clear();
            foreach (Menu item in subMenus)
            {
                menuDictionary.Add(item.menuName, item.menuPrefab);
            }
            SetExitPoint();
        }
        protected void OnValidate()
        {
            menuDictionary.Clear();
            foreach (Menu item in subMenus)
            {
                menuDictionary.Add(item.menuName, item.menuPrefab);
            }
            SetExitPoint();
        }
        //move it to a dedicated script?
        public void SetExitPoint()
        {
            if (spawnPoint == null || exitPoint == null) return;
            exitPoint.localPosition = Vector3.Scale(spawnPoint.localPosition, new Vector3(-1, 1, 1));
        }

        // I should probably make this inheritable in practical scenarios so I can have this as generic as possible
        public void LoadMenu(T menuName, string message = "")
        {
            //This line checks if the value given is the default value by converting it to an int
            if ((int)System.Enum.Parse(menuName.GetType(), menuName.ToString()) == 0) return;
            GameObject menu = GameObject.Instantiate(menuDictionary[menuName], spawnPoint.position, transform.rotation, thisCanvas.transform);
            GUI_SubMenu spawningMenu = menu.GetComponent<GUI_SubMenu>();
            spawningMenu.Init(spawnPoint, exitPoint, message);
            spawningMenu.Show();
            activeSubMenu.Hide();
            activeSubMenu = spawningMenu;
        }
    }
}