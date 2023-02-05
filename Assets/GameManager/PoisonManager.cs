using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonManager : MonoBehaviour
{
    public GameObject poisonVignete;
    static GameObject staicPoisonV;
    static int poisonCount = 0;

    private void Start()
    {
        staicPoisonV = poisonVignete;
    }


    public static void resetPoison()
    {
        poisonCount = 0;
        staicPoisonV.SetActive(false);
    }

    public static void addPoison()
    {
        poisonCount++;
        staicPoisonV.SetActive(false);
        if (poisonCount >= 2) GameStateManager.loseGame();
    }
}
