using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerSpawner : MonoBehaviour
{
    public GameObject barrierPrefab;
    public MeshRenderer groundPlane;

    public int evilFlowerCount = 10;
    public int goodFlowerCount = 10;
    public float groundheight = .5f;
    public GameObject[] flowerSprites;
    public GameObject[] rootSprites;

    public GameObject EvilFlower;

    public GameObject FlowerContainer;

    // Start is called before the first frame update
    public GameObject spawnFlower()
    {
      groundPlane = FindObjectOfType<DesignAccess>().spawnedGroundPlane.GetComponent<MeshRenderer>();
      Destroy(groundPlane.GetComponent<MeshCollider>());
        GameObject flowerContainer = new GameObject();

        for (int i = 0; i < evilFlowerCount; i++)
        {
            bool validSpot = false;

            Vector3 pos = new Vector3();
            Quaternion rot = new Quaternion();
            int escapeCount = 0;
            while (!validSpot)
            {
                escapeCount++;
                pos = new Vector3(Random.Range(-groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x, groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x), groundheight, Random.Range(-groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z, groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z));

                if (Physics.CheckSphere(pos, .12f))
                {

                    rot = Quaternion.EulerAngles(0, Random.Range(0, 180), 0);
                    validSpot = true;
                }

                if (escapeCount == 100)
                    break;
            }

            GameObject Container = Instantiate(FlowerContainer, pos, rot, flowerContainer.transform);
            Instantiate(EvilFlower, pos, rot, Container.transform);
            Instantiate(rootSprites[Random.Range(0, rootSprites.Length-1)], pos, rot, Container.transform);

            Container.GetComponent<FlowerController>().isPoison = true;
        }

        for (int i = 0; i < goodFlowerCount; i++)
        {
            bool validSpot = false;

            Vector3 pos = new Vector3();
            Quaternion rot = new Quaternion();
            int escapeCount = 0;
            while (!validSpot)
            {
                escapeCount++;
                pos = new Vector3(Random.Range(-groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x, groundPlane.bounds.size.x / 2 + groundPlane.transform.position.x), groundheight, Random.Range(-groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z, groundPlane.bounds.size.z / 2 + groundPlane.transform.position.z));

                if (Physics.CheckSphere(pos, .12f) == false)
                {
                    rot = Quaternion.EulerAngles(0, Random.Range(0, 180), 0);
                    validSpot = true;
                }
                if (escapeCount == 100)
                    break;
            }

            GameObject Container = Instantiate(FlowerContainer, pos, rot, flowerContainer.transform);
            Instantiate(flowerSprites[Random.Range(0, flowerSprites.Length - 1)], pos, rot, Container.transform);
            Instantiate(rootSprites[Random.Range(0, rootSprites.Length - 1)], pos, rot, Container.transform);

            Container.GetComponent<FlowerController>().isPoison = false;
        }

        return flowerContainer;
    }

    public GameObject spawnBarrier()
    {
        GameObject barrierObj = Instantiate(barrierPrefab, transform); 
        barrierObj.transform.localScale = groundPlane.transform.localScale * 2;
        barrierObj.transform.position = groundPlane.transform.position + new Vector3(0,0.5f,0);
        barrierObj.transform.rotation = groundPlane.transform.rotation;
        return barrierObj;
    }
}
