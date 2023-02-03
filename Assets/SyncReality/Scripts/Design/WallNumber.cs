using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
 
[System.Serializable]
public class WallNumber : MonoBehaviour
{
 [HideInInspector] public Compass compass;
  public TextMeshProUGUI numberText;

  

  public int GetWallNumberInt()
  { 
    return int.Parse(gameObject.name.Substring(10, 1));
  }
  

  public void HighlightWallNumber()
  {
    numberText.color = compass.colorHighlight;
    // when compass is selected
  }
  public void SelectWallNumber()
  {
    numberText.color = compass.colorSelected;
    // when specific wallnr is selected
  }
  public void DelightWallNumber()
  {
    numberText.color = compass.colorDefault;
  }
  
 
 
}
