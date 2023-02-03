using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOpenFile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ModalViewManager.GetFilepathLoaded += ListButtonReturn;
        //StartCoroutine(OpenMenu());
    }
    private void OnDisable()
    {
        ModalViewManager.GetFilepathLoaded -= ListButtonReturn;
    }

    private IEnumerator OpenMenu()
    {
        yield return new WaitForSeconds(12);
        ModalViewManager.GetFileFromFolder("");
    }

    public void ListButtonReturn(string x)
    {
       
    }

}
