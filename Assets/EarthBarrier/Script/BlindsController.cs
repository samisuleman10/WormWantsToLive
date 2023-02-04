using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindsController : MonoBehaviour
{
    public float barrierHeight = .5f;

    bool isAbove;

    float FogBlendProcess;

    public Color underGroundFog;
    public float undergroundFogDistance;
    public Color skyFog;
    public float overgroundFogDistance;

    public AudioClip earthBreakSound;

    public AudioSource aS;
    public ParticleSystem particles;

    public AudioSource UndergroundMusic;
    public AudioSource OvergroundMusic;

    public AudioSource MovementAS;
    public AudioClip movementSound;
    Vector3 lastSoundPos;


    private void Start()
    {
        if(transform.position.y > barrierHeight)
        {
            isAbove = true;
            FogBlendProcess = 1;
            UndergroundMusic.volume = 0;
            OvergroundMusic.volume = 1;
        }
        else
        {
            isAbove = false;
            FogBlendProcess = 0;
            UndergroundMusic.volume = 1;
            OvergroundMusic.volume = 0;
        }

    }

    // Update is called once per frame
    void Update()
    {

        transform.position = Camera.main.transform.position;


        if(transform.position.y > barrierHeight && isAbove == false)
        {
            isAbove = true;
            aS.PlayOneShot(earthBreakSound);
            particles.Play();
        }
        if(transform.position.y < barrierHeight && isAbove == true)
        {
            isAbove = false;
        }


        if((FogBlendProcess < 1 && isAbove) || (FogBlendProcess > 0 && !isAbove))
        {
            if(isAbove)
            {
                FogBlendProcess += Time.deltaTime*2;
                if (FogBlendProcess > 1) FogBlendProcess = 1;
                RenderSettings.fogColor = Color.Lerp(underGroundFog, skyFog , FogBlendProcess);
                RenderSettings.fogEndDistance = Mathf.Lerp(undergroundFogDistance, overgroundFogDistance, FogBlendProcess);
                OvergroundMusic.volume = FogBlendProcess;
                UndergroundMusic.volume = 1 - FogBlendProcess;
            }
            else
            {
                FogBlendProcess -= Time.deltaTime*2;
                if (FogBlendProcess < 0) FogBlendProcess = 0;
                RenderSettings.fogColor = Color.Lerp(underGroundFog, skyFog, FogBlendProcess);
                RenderSettings.fogEndDistance = Mathf.Lerp(undergroundFogDistance, overgroundFogDistance, FogBlendProcess);
                OvergroundMusic.volume =  FogBlendProcess;
                UndergroundMusic.volume = 1- FogBlendProcess;
            }
        }

        Debug.Log(FogBlendProcess);

        if (!isAbove)
        {
            if(Vector3.Distance(transform.position, lastSoundPos) > .5f)
            {
                lastSoundPos = transform.position;
                MovementAS.PlayOneShot(movementSound);
            }
        }


    }




}
