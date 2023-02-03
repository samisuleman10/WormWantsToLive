using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DesignArea's special FloorPlane whose bounds and content can be modified, to be loaded in by the Surrounder
[ExecuteInEditMode][SelectionBase]
public class FlexFloor : MonoBehaviour
{
 
#if UNITY_EDITOR
    public DesignArea designArea; 
      
      
      private float _prevScaleX;
      private float _prevScaleZ;
      private Material _prevMaterial;

      private void Update()
      {
        if (Application.isPlaying)
            return;
        if (ToolWindowSettings.Instance.toolWindowBuilt == false)
            return;
            //todo centralize this
            if (_prevScaleX != transform.localScale.x)
            {
                  _prevScaleX = transform.localScale.x;
                  designArea.FloorSizeChanged();
            }
            else   if (_prevScaleZ != transform.localScale.z)
            {
                  _prevScaleZ = transform.localScale.z;
                  designArea.FloorSizeChanged();
            }
      }
      
      #region CalledFromDesignArea
      public void ResetToDefaultScale()
      {
         //   transform.localScale = new Vector3(designArea.defaultFloorWidth, designArea.defaultFloorHeight, designArea.defaultFloorHeight);
      }
      public void RedrawFlexFloor()
      {
    
      }
    
      public void QueryStatus()
      {
          
      }

     
      public void InitializeFlexFloor()
      {
         // GetComponent<MeshRenderer>().material = designArea.GetFirstActivePresentSyncSetup().floorMaterial;
      }
      public void ResetFlexFloor(DesignArea designArea)
      {  
          /*floorMaterial = designArea.defaultFloorMaterial;
          ceilingMaterial = designArea.defaultCeilingMaterial;
          floorMaterialTwo = designArea.defaultFloorMaterial;
          ceilingMaterialTwo = designArea.defaultCeilingMaterial;
          floorMaterialThree = designArea.defaultFloorMaterial;
          ceilingMaterialThree = designArea.defaultCeilingMaterial;
          GetComponent<MeshRenderer>().material = floorMaterial; */
      }
    #endregion
#endif

}