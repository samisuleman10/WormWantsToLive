using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    static float Timer = 0;

    static bool isPlaying = false;


    public GameObject winNarrator, loseNarrator;

    static GameObject winNarratorInstance;
    static GameObject loseNarratorInstance;
 

    static public GameObject staticWinNarrator, staticLoseNarrator;

    public GrassSpawner grass;
    public FlowerSpawner flower;

    static GrassSpawner staticGrass;
    static FlowerSpawner staticFlower;

    static GameObject grassContainer;
    static GameObject flowerContainer;
    static GameObject barrierObject;

    public GameObject UiBlend;
    public AudioSource blendSound;

    private bool _startEnvironmentLoaded = false;

    private void Start()
    { 
        staticGrass = grass;
        staticFlower = flower;

        winNarratorInstance = winNarrator;
        loseNarratorInstance = loseNarrator;
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
            StoryTeller.LoadSyncSetup("StartEnvo");
        if (Input.GetKeyUp(KeyCode.B))
            startActualGame();
        
        
        if (_startEnvironmentLoaded == false)
            return;
    
        if (!isPlaying)
        {
            if (Timer <= 0 && Camera.main.transform.position.y <.4f)
            {
                startActualGame();
                enableBlend();
            }
            else
            {
                Timer -= Time.deltaTime;
            }
        }
    }

    public void startOverallGame()
    {
        Debug.Log("PIPELINE FINISHED");
        _startEnvironmentLoaded = true;
    }
    public void startActualGame()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            winNarratorInstance = null;
            loseNarratorInstance = null;

            StoryTeller.LoadSyncSetup("RocksInPot");
            
            grassContainer = staticGrass.spawnGrass();
            flowerContainer = staticFlower.spawnFlower();
            barrierObject = staticFlower.spawnBarrier();
        }
    }



    public static void winGame()
    {
        winNarratorInstance = Instantiate(staticWinNarrator, new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), Quaternion.identity);
        PoisonManager.resetPoison();
        isPlaying = false;
        Timer = 10f;
    }

    public static void loseGame()
    {
        loseNarratorInstance = Instantiate(staticLoseNarrator, new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z), Quaternion.identity);
        PoisonManager.resetPoison();
        isPlaying = false;
        Timer = 10f;
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

    private StoryTeller _storyTeller;
    private StoryTeller StoryTeller
    {
        get
        {
            if (_storyTeller == null)
                _storyTeller = FindObjectOfType<StoryTeller>();
            return _storyTeller;
        }
    }
}
