using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FoodManager : MonoBehaviour
{

    static int foodCount = 0;

    public TextMeshPro counter;
    static TextMeshPro staticCounter;
    public static void resetFood()
    {
        foodCount = 0;
    }


    public static void addFood()
    {
        foodCount++;
        staticCounter.text = foodCount + "/5";
        if(foodCount >= 5)
        {
            GameStateManager.winGame();
        }
    }




    // Start is called before the first frame update
    void Start()
    {
        staticCounter = counter;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
