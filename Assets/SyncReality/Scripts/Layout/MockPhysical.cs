using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




 

[ExecuteAlways]
[System.Serializable]
public class MockPhysical : MonoBehaviour
{

    public Classification Classification;
    [HideInInspector] public bool HasBeenFilled = false;


    public static MockPhysical GetMockPhysicalFromMockData (LayoutArea layoutArea, MockData mockData)
    {

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.SetParent(layoutArea.mockPhysicalsParent.transform);
        MockPhysical mock = obj.AddComponent<MockPhysical>();
        mock.transform.localScale = mockData.TransformationMatrix4X4.ExtractScale();
        mock.transform.rotation = mockData.TransformationMatrix4X4.ExtractRotation();
        mock.transform.position = mockData.TransformationMatrix4X4.ExtractPosition();
        mock.gameObject.name = mockData.GameObjectName;
        mock.Classification = mockData.Classification; 
      //  mock.ResetColor();
        return mock;

    }


    private void Start()
    {
        var surrounder = FindObjectOfType<SurrounderModule>();
        var m =
                (surrounder.getGroundBox()[0] +
                surrounder.getGroundBox()[1] +
                surrounder.getGroundBox()[2] +
                surrounder.getGroundBox()[3]) / 4f;
        flipMockSizeCorrectly(m);
    }

    public void flipMockSizeCorrectly(Vector3 m)
    {

        if (transform.localScale.x < transform.localScale.z)
        {
            transform.localScale = transform.localScale.flip();
            transform.rotation = transform.rotation * Quaternion.Euler(0, 90, 0);
        }
        if (Vector2.Dot(transform.forward.flatten(), (m - transform.position).flatten()) < 0)
        {
            transform.rotation = transform.rotation * Quaternion.Euler(0, 180, 0);
        }
    }

    public MockData  GetMockData()
    {
        return new MockData(this);
    }
#if UNITY_EDITOR
    public void Initialize()
    {
    //    this.OnVariableChange += VariableChangeHandler;
    ResetColor();

    }


    private void Update()
    {
        if (transform.hasChanged)
        {
            FindObjectOfType<ModuleManager>().UpdatePipeline();
            transform.hasChanged = false;

        }
    }

    public void ResetColor(bool hovered = false)
    { 
        /*if (hovered)
           GetComponent<MeshRenderer>().material = FindObjectOfType<LayoutArea>().GetMockMaterial(Classification.Ceiling);
        else*/
            GetComponent<MeshRenderer>().material = FindObjectOfType<LayoutArea>().GetMockMaterial(Classification); 

    }
#endif

    public void ActivateMock()
    {
//        ResetColor(true); // tricky to 'highlight' on mock click from non-mock-selection so just ignore this for now
      //  GetComponent<MeshRenderer>().material = _layoutArea.GetMockMaterial(Classification.Window);
    }
    public void DeactiveMock()
    {
     //   ResetColor();
        //GetComponent<MeshRenderer>().material = _layoutArea.GetMockMaterial(Classification); 
    }



}
