using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerController : MonoBehaviour
{
    public bool isPoison;
    public AudioSource aS;
    public AudioClip goodEatingSound, badEatingSound;
    public ParticleSystem goodParticles, badParticles;

    public void OnTriggerEnter(Collider col)
    {

        if (isPoison)
        {
            PoisonManager.addPoison();
            aS.Stop();
            aS.PlayOneShot(goodEatingSound);
            badParticles.Play();
            Invoke("Despawn", 1f);

        }
        else
        {
            FoodManager.addFood();
            aS.Stop();
            aS.PlayOneShot(badEatingSound);
            goodParticles.Play();
            Invoke("Despawn", 1f);
        }

    }

    void Despawn()
    {
        Destroy(gameObject);
    }
}
