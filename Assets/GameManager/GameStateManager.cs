using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{

    static bool isPlaying = false;


    static GameObject winNaratorInstance;
    static GameObject loseNaratorInstance;
    


    static public GameObject staticWinNarator, staticLoseNarator;

    public GrassSpawner grass;
    public FlowerSpawner flower;

    static GrassSpawner staticGrass;
    static FlowerSpawner staticFlower;

    static GameObject grassContainer;
    static GameObject flowerContainer;


    private void Start()
    {
        staticGrass = grass;
        staticFlower = flower;
    }


    public static void startGame()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            winNaratorInstance = null;
            loseNaratorInstance = null;

            grassContainer = staticGrass.spawnGrass();
            flowerContainer = staticFlower.spawnFlower();

            
        }
    }




    public static void winGame()
    {
        winNaratorInstance = Instantiate(staticWinNarator, new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), Quaternion.identity);
        PoisonManager.resetPoison();

    }

    public static void loseGame()
    {
        loseNaratorInstance = Instantiate(staticLoseNarator, new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), Quaternion.identity);
        PoisonManager.resetPoison();

    }


   
}
