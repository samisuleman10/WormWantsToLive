using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

 



public class MessMaker : MonoBehaviour
{
    
    #if UNITY_EDITOR
    public void CreateMessModule()
    {
        MessModule messModule = ScriptableObject.CreateInstance<MessModule>();
        int randomInt =  Random.Range (0, 999);
        VerifyMessModuleFolder();
        string path = "Assets/Resources/MessModules/MessModule"+randomInt+".asset";
        messModule.moduleID = "MM" + randomInt;
        AssetDatabase.CreateAsset(messModule, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
     //   Debug.Log("done creating messmodule");
    }

    private void VerifyMessModuleFolder()
    {
        if ( AssetDatabase.IsValidFolder("Assets/Resources/MessModules"   ) == false )
            AssetDatabase.CreateFolder("Assets/Resources", "MessModules");
    }

    public void DeleteMessModule(MessModule messModule)
    {
        string guid;
        long file;
        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(messModule, out guid, out file))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(path);
        }
    }
    #endif
    
    public enum SurfaceType
    {
        Ceiling,
        WallFace,
        Backdrop,
        Floor,
        Sync,
        SurroundSync
    }


    public List<MessModule> GetAllMessModules()
    {
        List<MessModule> returnList = new List<MessModule>(); 
        returnList =   Resources.LoadAll<MessModule> ("MessModules/").ToList(); 
        return returnList; 
     
    }
  
    public List<MessModule> GetAnchoredMessModules()
    {
        DesignArea designArea = FindObjectOfType<DesignArea>();
        
        List<MessModule> returnList = new List<MessModule>(); 
        List<MessModule> potentialsList =  GetAllMessModules();

        foreach (var messModule in potentialsList)
            if (designArea.GetMessModulesFromAnchors().Contains(messModule))
               returnList.Add(messModule);  
        return returnList;
    }
    public void CreateMessFromMessModules()
    {
        if ( GetComponentInChildren<DistributeObjectOverPoly>() != null)
             GetComponentInChildren<DistributeObjectOverPoly>().Execute();
    }

    public MessModule GetMessModuleByID(string id)
    {
        foreach (var messmodule in GetAllMessModules())
            if (messmodule.moduleID == id)
                return messmodule;
        Debug.LogWarning("Did not find a MessModule for ID: " + id);
        return null;
    }

    

}
