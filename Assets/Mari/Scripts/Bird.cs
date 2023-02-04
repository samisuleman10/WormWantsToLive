using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private Transform initialPostion;

    private Transform camera;

    private void Start()
    {
        camera = Camera.main.transform;
    }

    private void Update()
    {
        if (camera.position.y > 1)
        {
            transform.position = Vector3.MoveTowards(transform.position, camera.position, Time.deltaTime * 2 * speed);
        }
        if(camera.position.y < 1 && camera.position.y > 0.6)
        {
            transform.position = Vector3.MoveTowards(transform.position, camera.position, Time.deltaTime * speed);
        }
        if(camera.position.y < 0.6)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPostion.position, Time.deltaTime * speed);
        }
    }
}
