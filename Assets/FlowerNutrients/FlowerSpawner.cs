using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{


    public MeshRenderer groundPlane;

    public int evilFlowerCount = 10;
    public int goodFlowerCount = 10;
    public float groundheight = .5f;
    public GameObject[] flowerSprites;
    public GameObject[] rootSprites;

    public GameObject EvilFlower;

    public GameObject FlowerContainer;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < evilFlowerCount; i++)
        {
            bool validSpot = false;

            Vector3 pos = new Vector3();
            Quaternion rot = new Quaternion();
            while (!validSpot)
            {
                pos = new Vector3(Random.Range(-groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x, groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x), groundheight, Random.Range(-groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z, groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z));

                if (Physics.CheckSphere(pos, .12f))
                {

                    rot = Quaternion.EulerAngles(0, Random.Range(0, 180), 0);
                    validSpot = true;
                }
            }

            GameObject Container = Instantiate(FlowerContainer, pos, rot);
            Instantiate(EvilFlower, pos, rot, Container.transform);
            Instantiate(rootSprites[Random.Range(0, rootSprites.Length-1)], pos, rot, Container.transform);

            Container.GetComponent<FlowerController>().isPoison = true;
        }

        for (int i = 0; i < goodFlowerCount; i++)
        {
            bool validSpot = false;

            Vector3 pos = new Vector3();
            Quaternion rot = new Quaternion();
            while (!validSpot)
            {
                pos = new Vector3(Random.Range(-groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x, groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x), groundheight, Random.Range(-groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z, groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z));

                if (Physics.CheckSphere(pos, .12f))
                {

                    rot = Quaternion.EulerAngles(0, Random.Range(0, 180), 0);
                    validSpot = true;
                }
            }

            GameObject Container = Instantiate(FlowerContainer, pos, rot);
            Instantiate(flowerSprites[Random.Range(0, flowerSprites.Length - 1)], pos, rot, Container.transform);
            Instantiate(rootSprites[Random.Range(0, rootSprites.Length - 1)], pos, rot, Container.transform);

            Container.GetComponent<FlowerController>().isPoison = false;
        }
    }
}
