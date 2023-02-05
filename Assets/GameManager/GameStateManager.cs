using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    static float Timer = 0;

    static bool isPlaying = false;


    public GameObject winNarator, loseNarator;

    static GameObject winNaratorInstance;
    static GameObject loseNaratorInstance;
    


    static public GameObject staticWinNarator, staticLoseNarator;

    public GrassSpawner grass;
    public FlowerSpawner flower;

    static GrassSpawner staticGrass;
    static FlowerSpawner staticFlower;

    static GameObject grassContainer;
    static GameObject flowerContainer;

    public GameObject UiBlend;
    public AudioSource blendSound;

    private void Start()
    {
        staticGrass = grass;
        staticFlower = flower;

        winNaratorInstance = winNarator;
        loseNaratorInstance = loseNarator;
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
        isPlaying = false;
        Timer = 10f;
    }

    public static void loseGame()
    {
        loseNaratorInstance = Instantiate(staticLoseNarator, new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), Quaternion.identity);
        PoisonManager.resetPoison();
        isPlaying = false;
        Timer = 10f;
    }

    private void Update()
    {
        if (!isPlaying)
        {
            if (Timer <= 0 && Camera.main.transform.position.y <.4f)
            {
                startGame();
                enableBlend();
            }
            else
            {
                Timer -= Time.deltaTime;
            }
        }
    }


    void enableBlend()
    {
        UiBlend.SetActive(true);
        blendSound.Stop();
        blendSound.Play();
        Invoke("disableBlend", 4f);
    }

    void disableBlend()
    {
        UiBlend.SetActive(false);
    }
}
