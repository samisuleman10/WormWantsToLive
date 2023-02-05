using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public MeshRenderer groundPlane;
    public GameObject container;

    public int maxGrasssprites = 100;
    public float groundheight = .5f;
    public GameObject[] grassSprites;
    
    
    

    // Start is called before the first frame update
    public GameObject spawnGrass()
    {
         groundPlane = FindObjectOfType<DesignAccess>().spawnedGroundPlane.GetComponent<MeshRenderer>();
         Destroy(groundPlane.GetComponent<MeshCollider>()); 
       
        for (int i = 0; i < maxGrasssprites; i++)
        {
            Vector3 pos =groundPlane.transform.position + new Vector3(Random.Range(-groundPlane.bounds.size.x/2, groundPlane.bounds.size.x / 2), groundheight, Random.Range(-groundPlane.bounds.size.z / 2, groundPlane.bounds.size.z / 2));
            Quaternion rot = Quaternion.EulerAngles(0, Random.Range(0,180), 0);
 
            Instantiate(grassSprites[Random.Range(0, grassSprites.Length-1)], pos, rot, container.transform);
        }
        return container;
    }
}
