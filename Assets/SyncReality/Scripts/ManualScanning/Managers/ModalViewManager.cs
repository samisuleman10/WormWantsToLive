using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static FTCursorsEventManager;

public static class ModalViewManager 
{

    public delegate void SendFilePath(string newVector);
    public static SendFilePath GetFilepathLoaded;

    //private static string PFSaveRoomListPath = "LoadRoomRayList";
    private static string PFSaveRoomListPath = "LoadRoomPokeKeypad";
    
    private static GameObject _currentModal;

    public static void GetFileFromFolder(string filepath)
    {
        if (_currentModal != null)
            return;


        var filechooser = GameObject.Instantiate(Resources.Load(PFSaveRoomListPath) as GameObject);
        var lastMenu = MenuWristDataRetriever.GetLastActiveMenuObj();

        if (lastMenu != null)
        {
            filechooser.transform.SetParent(lastMenu.transform.parent, false);
            filechooser.transform.position = lastMenu.transform.position;
            filechooser.transform.rotation = lastMenu.transform.rotation;

        }

        //filechooser.transform.SetParent(parent?.transform);
        filechooser.name = "ModalFileOpener";
        var btnTemplateRef = filechooser.GetComponent<HolderButtonTemplate>();
        if(btnTemplateRef == null)
        {
            Debug.LogError("No Button template at ModalViewManager_GetFileFromFolder");
            return;
        }

        if(btnTemplateRef.ButtonListTemplate != null)
            btnTemplateRef.ButtonListTemplate.gameObject.SetActive(false);

        bool useList = false;
        if (btnTemplateRef.ButtonList != null && btnTemplateRef.ButtonList.Count > 0)
        {
            foreach (var button in btnTemplateRef.ButtonList)
                button.gameObject.SetActive(false);
            useList = true;
        }

        if (Directory.Exists(Application.persistentDataPath+"/"+filepath))
        {
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/" + filepath);
            var files =  dir.GetFiles("*.json");
            int currentListButton = 0;
            foreach(var file in files)
            {
                GameObject btn = null;
                if (!useList)
                {
                    btn = GameObject.Instantiate(btnTemplateRef.ButtonListTemplate, btnTemplateRef.ButtonListTemplate.transform.parent);
                    btn.SetActive(true);

                    var btnComp = btn.GetComponent<Button>();
                    var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnComp == null || btnText == null)
                    {
                        Debug.LogError("No Button or text in the btn template");
                        continue;
                    }

                    btnText.text = file.Name.Replace(file.Extension, "");
                    btnComp.onClick.AddListener(() =>
                    {
                        GetFilepathLoaded?.Invoke(file.Name);
                        GameObject.Destroy(btnTemplateRef.gameObject);
                    });
                }
                else
                {
                    if (currentListButton < btnTemplateRef.ButtonList.Count)
                        btn = btnTemplateRef.ButtonList[currentListButton];
                    currentListButton++;

                    btn.SetActive(true);

                    var btnComp = btn.GetComponent<InteractableUnityEventWrapper>();
                    var btnText = btn.GetComponentInChildren<TextMesh>();
                    if (btnComp == null || btnText == null)
                    {
                        Debug.LogError("No Button or text in the btn template");
                        continue;
                    }

                    btnText.text = file.Name.Replace(file.Extension, "");
                    btnComp.WhenSelect.AddListener(() =>
                    {
                        GetFilepathLoaded?.Invoke(file.Name);
                        GameObject.Destroy(btnTemplateRef.gameObject);
                    });
                }
            }
        }
    }
}
