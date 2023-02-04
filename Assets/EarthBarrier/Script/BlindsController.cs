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

    private void Start()
    {
        if(transform.position.y > barrierHeight)
        {
            isAbove = true;
        }
        else
        {
            isAbove = false;
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
                RenderSettings.fogColor = Color.Lerp(underGroundFog, skyFog , FogBlendProcess);
                RenderSettings.fogEndDistance = Mathf.Lerp(undergroundFogDistance, overgroundFogDistance, FogBlendProcess);
            }
            else
            {
                FogBlendProcess -= Time.deltaTime*2;
                RenderSettings.fogColor = Color.Lerp(underGroundFog, skyFog, FogBlendProcess);
                RenderSettings.fogEndDistance = Mathf.Lerp(undergroundFogDistance, overgroundFogDistance, FogBlendProcess);
            }
        }


    }




}
