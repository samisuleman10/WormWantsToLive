using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

// walls always have the same number. Faces can map and rotate
// interaction:
    // - first wall number click selects compass
         // compass selected adds highlight to wallnumbers
    // - (any) wall number click WHEN COMPASS SELECTED (so when highlighted) selects wallnumber
[Serializable]
public enum WallFace
{
    North,
    East,
    South,
    West
}

public enum NorthProperty
{
    LongestWall,
    MostFreeSpace
}

[SelectionBase][Serializable]
public class Compass : MonoBehaviour
{
    [Header("Assigns")]
    public DesignArea designArea;
    public Backdropper backdropper;
    public GameObject wallNumberPrefab;
    public Transform wallNumberParent; 
    [Header("Settings")] 
    public bool reverseFaceOrder;
    public bool turnTowardsCamera;
    public bool alwaysTurnTowardsCamera;
    public float bodyHeight = 15f;
    public float wallNumberScale = 1f;
    public float wallNumberHeight = 3;
    public float wallNumberOffset = 0.5f;
    
  
    public Color colorDefault;
    public Color colorHighlight;
    public Color colorSelected;

    [HideInInspector]   public WallNumber _wallNumber1;
    [HideInInspector]    public WallNumber _wallNumber2;
    [HideInInspector]    public WallNumber _wallNumber3;
    [HideInInspector]    public WallNumber _wallNumber4;
    
     public NorthProperty northProperty;
    
    private List<WallNumber> _wallNumbers;
    private Dictionary<int, WallFace> _numberFacePairings;

    
  
    
    
 

#if UNITY_EDITOR
    public WallNumber GetWallNumberForFace(WallFace face)
    {
        int keynr = 0;
        if (_numberFacePairings == null)
            ResetDictionary();
        foreach (KeyValuePair<int, WallFace> pair in _numberFacePairings)
        {
            if (pair.Value == face)
                keynr =  pair.Key;
        }
        if (keynr == 0)
            Debug.LogWarning("Tried to sort WallFaces with no North present!");
        if (keynr == 1)
            return _wallNumber1;
        if (keynr == 2)
            return _wallNumber2;
        if (keynr == 3)
            return _wallNumber3;
        if (keynr == 4)
            return _wallNumber4;
        return null;
    }
    public int GetNumberForFace(WallFace face)
    {
        int keynr = 0;
        if (_numberFacePairings == null)
            ResetDictionary();
        foreach (KeyValuePair<int, WallFace> pair in _numberFacePairings)
        {
            if (pair.Value == face)
                keynr =  pair.Key;
        }
        if (keynr == 0)
            Debug.LogWarning("Tried to sort WallFaces with no North present!");
        if (keynr == 1)
            return 1;
        if (keynr == 2)
            return 2;
        if (keynr == 3)
            return 3;
        if (keynr == 4)
            return 4;
        return 0;
    }
    public WallNumber GetWallNumberForNumber(int number)
    {
        if (_numberFacePairings == null)
            ResetDictionary();
        if (number == 1)
            return _wallNumber1;
        if (number == 2)
            return _wallNumber2;
        if (number == 3)
            return _wallNumber3;
        if (number == 4)
            return _wallNumber4;
        Debug.LogWarning("Did not find WallNumber for: " + number);
        return null;
    }
    public WallFace GetWallFaceForNumber(int number)
    { 
        if (_numberFacePairings == null)
            ResetDictionary();
        return _numberFacePairings[number];
    }





    private void ResetDictionary()
    {
        _numberFacePairings = new Dictionary<int, WallFace>
        {
            {1, WallFace.North},
            {2, WallFace.East},
            {3, WallFace.South},
            {4, WallFace.West}
        }; 
    }

    public void ResetCompass()
    {
        ResetDictionary(); 
        RedrawCompass();
    }
    public void RedrawCompass()
    { 
        gameObject.SetActive(true);

        // redraw should not reset wall face pairings
        DestroyWallNumbers();
        AutoFillFaces();
        SpawnWallNumbers();
        PositionCompass();
        
        if (turnTowardsCamera)
            TurnTowardsCamera();
          
        // click one of four directions for a selected wallnr? with in the middle button to rotate and inndr button to reverse order
    }

    private void PositionCompass()
    {
            transform.position = new Vector3(GetWallNumberForFace(WallFace.North).transform.position.x, bodyHeight, GetWallNumberForFace(WallFace.North).transform.position.z);
    }

    public void Highlight()
    {
        if (_wallNumbers == null)
            RedrawCompass();
        foreach (var wallNumber in _wallNumbers)
            wallNumber.HighlightWallNumber();
        GetComponent<Image>().color = colorSelected;
        //todo highlight compass body 
    }

    public void Delight() // DeHighlight 
    {
        if (_wallNumbers == null)
            RedrawCompass();
        foreach (var wallNumber in _wallNumbers)
            wallNumber.DelightWallNumber();
        GetComponent<Image>().color = colorDefault;
    }
 
    private void SpawnWallNumbers()
    {
        _wallNumbers = new List<WallNumber>();
        _wallNumber1 = Instantiate(wallNumberPrefab).GetComponent<WallNumber>();
        _wallNumber2 = Instantiate(wallNumberPrefab).GetComponent<WallNumber>();
        _wallNumber3 = Instantiate(wallNumberPrefab).GetComponent<WallNumber>();
        _wallNumber4 = Instantiate(wallNumberPrefab).GetComponent<WallNumber>();
        _wallNumbers.Add(_wallNumber1);
        _wallNumbers.Add(_wallNumber2);
        _wallNumbers.Add(_wallNumber3);
        _wallNumbers.Add(_wallNumber4); 
        for (int i = 1; i < _wallNumbers.Count+1; i++)
        {
            _wallNumbers[i-1].transform.SetParent(wallNumberParent);
            _wallNumbers[i-1].compass = this;
            _wallNumbers[i-1].transform.localScale = new Vector3(wallNumberScale, wallNumberScale, wallNumberScale);
            _wallNumbers[i-1].gameObject.name = "WallNumber" + i;
            _wallNumbers[i-1].numberText.text =    _numberFacePairings[i].ToString().Substring(0,1); 
        }
        // set in middle of wall

        var localScale = designArea.FlexFloorProp.transform.localScale;
        float xMin = localScale.x  * designArea.transform.localScale.x * -5f+ designArea.FlexFloorProp.transform.position.x;  
        float xMax = localScale.x * designArea.transform.localScale.x * 5f  + designArea.FlexFloorProp.transform.position.x;
        float zMin = localScale.z * designArea.transform.localScale.z * -5f+ designArea.FlexFloorProp.transform.position.z;
        float zMax = localScale.z * designArea.transform.localScale.z *  5f + designArea.FlexFloorProp.transform.position.z;
 
        _wallNumber1.transform.position = new Vector3( (xMin + xMax) / 2,  wallNumberHeight, zMin + wallNumberOffset);
        _wallNumber2.transform.position = new Vector3(  xMin + wallNumberOffset ,  wallNumberHeight, (zMin + zMax) /2);
        _wallNumber3.transform.position = new Vector3( (xMin + xMax) / 2,  wallNumberHeight, zMax - wallNumberOffset);
        _wallNumber4.transform.position = new Vector3(  xMax - wallNumberOffset ,  wallNumberHeight, (zMin + zMax) /2);
        _wallNumber2.transform.rotation = Quaternion.Euler(0, 270, 0);
        _wallNumber4.transform.rotation = Quaternion.Euler(0, 90, 0);
 
        _wallNumber1.numberText.gameObject.hideFlags = HideFlags.HideInHierarchy;
        _wallNumber2.numberText.gameObject.hideFlags = HideFlags.HideInHierarchy;
        _wallNumber3.numberText.gameObject.hideFlags = HideFlags.HideInHierarchy;
        _wallNumber4.numberText.gameObject.hideFlags = HideFlags.HideInHierarchy;

        UnderlineAnchoredWallNumbers();

    }

    public void UnderlineAnchoredWallNumbers()
    {
        if (designArea.GetBackdropForWallFace(GetWallFaceForNumber(1)).anchored)
            _wallNumber1.numberText.fontStyle = FontStyles.Underline; 
        else
            _wallNumber1.numberText.fontStyle = FontStyles.Normal; 
        if (designArea.GetBackdropForWallFace(GetWallFaceForNumber(2)).anchored)
            _wallNumber2.numberText.fontStyle = FontStyles.Underline; 
        else
            _wallNumber2.numberText.fontStyle = FontStyles.Normal; 
        if (designArea.GetBackdropForWallFace(GetWallFaceForNumber(3)).anchored)
            _wallNumber3.numberText.fontStyle = FontStyles.Underline; 
        else
            _wallNumber3.numberText.fontStyle = FontStyles.Normal; 
        if (designArea.GetBackdropForWallFace(GetWallFaceForNumber(4)).anchored)
            _wallNumber4.numberText.fontStyle = FontStyles.Underline; 
        else
            _wallNumber4.numberText.fontStyle = FontStyles.Normal; 
    }
    public void TurnTowardsCamera()
    {
        if (SceneView.lastActiveSceneView == null)
            return;
        if (SceneView.lastActiveSceneView.camera != null)
        {
            var cameraTransform = SceneView.lastActiveSceneView.camera.transform;

            Vector3 distance = cameraTransform.position - transform.position;
            distance.x = distance.z = 0.0f;
            transform.LookAt( cameraTransform.position - distance ); 
            transform.Rotate(0,180,0);
        
            if (_wallNumbers == null) return;
            if (_wallNumbers.Count == 0) return;
            foreach (var wallNumber in _wallNumbers)
                if (wallNumber != null)
                {
                    distance = cameraTransform.position - wallNumber.transform.position;
                    distance.x = distance.z = 0.0f;
                    wallNumber.transform.LookAt( cameraTransform.position - distance ); 
                    wallNumber.transform.Rotate(0,180,0);
                }
        } 
    }
    private void DestroyWallNumbers()
    {
       if (_wallNumber1 != null)
           if (_wallNumber1.gameObject != null)
               DestroyImmediate(_wallNumber1.gameObject);
       if (_wallNumber2 != null)
           if (_wallNumber2.gameObject != null)
               DestroyImmediate(_wallNumber2.gameObject);
       if (_wallNumber3 != null)
           if (_wallNumber3.gameObject != null)
               DestroyImmediate(_wallNumber3.gameObject);
       if (_wallNumber4 != null) 
               if (_wallNumber4.gameObject != null)
                   DestroyImmediate(_wallNumber4.gameObject);
    }

   
     
    public void ClickMiddle()
    {
        Debug.Log("click middle");
    }

    public void MakeNorth(int wallNumberInt)
    {
        _numberFacePairings = new Dictionary<int, WallFace>();
        _numberFacePairings.Add(wallNumberInt, WallFace.North);
        AutoFillFaces();
        designArea.InitializeDesignArea();
    }

    private void AutoFillFaces()
    {
        // requires a North face in dict
        int northKey = 0;
        foreach (KeyValuePair<int, WallFace> pair in _numberFacePairings)
        {
            if (pair.Value == WallFace.North)
                northKey = pair.Key;
        }
        if (northKey == 0 )
            Debug.LogWarning("Tried to sort WallFaces with no North present!");

        _numberFacePairings = new Dictionary<int, WallFace>();

        if (reverseFaceOrder == false)
        {
            if (northKey == 1)
            {
                _numberFacePairings.Add(1, WallFace.North);
                _numberFacePairings.Add(2, WallFace.East);
                _numberFacePairings.Add(3, WallFace.South);
                _numberFacePairings.Add(4, WallFace.West);
            }
            else   if (northKey == 2)
            {
                _numberFacePairings.Add(2, WallFace.North);
                _numberFacePairings.Add(3, WallFace.East);
                _numberFacePairings.Add(4, WallFace.South);
                _numberFacePairings.Add(1, WallFace.West);
            }
            else   if (northKey == 3)
            {
                _numberFacePairings.Add(3, WallFace.North);
                _numberFacePairings.Add(4, WallFace.East);
                _numberFacePairings.Add(1, WallFace.South);
                _numberFacePairings.Add(2, WallFace.West);
            }
            else   if (northKey == 4)
            {
                _numberFacePairings.Add(4, WallFace.North);
                _numberFacePairings.Add(1, WallFace.East);
                _numberFacePairings.Add(2, WallFace.South);
                _numberFacePairings.Add(3, WallFace.West);
            }
        }
        else if (reverseFaceOrder)
        {
            if (northKey == 1)
            {
                _numberFacePairings.Add(1, WallFace.North);
                _numberFacePairings.Add(4, WallFace.East);
                _numberFacePairings.Add(3, WallFace.South);
                _numberFacePairings.Add(2, WallFace.West);
            }
            else   if (northKey == 2)
            {
                _numberFacePairings.Add(2, WallFace.North);
                _numberFacePairings.Add(1, WallFace.East);
                _numberFacePairings.Add(4, WallFace.South);
                _numberFacePairings.Add(3, WallFace.West);
            }
            else   if (northKey == 3)
            {
                _numberFacePairings.Add(3, WallFace.North);
                _numberFacePairings.Add(2, WallFace.East);
                _numberFacePairings.Add(1, WallFace.South);
                _numberFacePairings.Add(4, WallFace.West);
            }
            else   if (northKey == 4)
            {
                _numberFacePairings.Add(4, WallFace.North);
                _numberFacePairings.Add(3, WallFace.East);
                _numberFacePairings.Add(2, WallFace.South);
                _numberFacePairings.Add(1, WallFace.West);
            }
        }
     
       
    }

    public void HideCompass()
    {
       gameObject.SetActive(false);
       _wallNumber1.gameObject.SetActive(false);
       _wallNumber2.gameObject.SetActive(false);
       _wallNumber3.gameObject.SetActive(false);
       _wallNumber4.gameObject.SetActive(false);
    }

  
#endif
}
