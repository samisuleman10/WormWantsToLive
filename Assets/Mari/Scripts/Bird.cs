using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private Transform initialPostion;

    private Transform camera;
    private bool _isActive;
    private bool _isWormFound;

    [SerializeField]
    private TextMesh text;

    [SerializeField]
    private AudioSource attackAudioSource;


    private void Start()
    {
        camera = Camera.main.transform;
        Invoke(nameof(StartBird), 5);//todo in the future start this from StartGame
    }

    public void IncreaseSpeed(float factor)
    {
        speed = speed*factor;
    }

    private void StartBird()
    {
        SetActive(true);
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        if(active == false)
            transform.position = initialPostion.transform.position;
    }

    public void PauseBird()
    {
        SetActive(false);
    }

    private void Reset()
    {
        transform.position = initialPostion.transform.position;
        _isWormFound = false;
    }

    private void Restart()
    {
        if (text != null)
            text.text = "Restart";
        Reset();
        SetActive(true);
    }

    private void Update()
    {
        if (!_isActive || _isWormFound) return;

        if (camera.position.y > 1)
        {
            if (!attackAudioSource.isPlaying && !_isWormFound)
                attackAudioSource.Play();
            transform.position = Vector3.MoveTowards(transform.position, camera.position, Time.deltaTime * 2 * speed);
        }
        if(camera.position.y < 1 && camera.position.y > 0.6)
        {
            if(!attackAudioSource.isPlaying && !_isWormFound)
                attackAudioSource.Play();
            transform.position = Vector3.MoveTowards(transform.position, camera.position, Time.deltaTime * speed);
        }
        if(camera.position.y < 0.6)
        {
            if (attackAudioSource.isPlaying)
                attackAudioSource.Stop();
            transform.position = Vector3.MoveTowards(transform.position, initialPostion.position, Time.deltaTime * speed);
        }

        if(transform.position == camera.position)
        {
            _isWormFound = true;
            attackAudioSource.Stop();
            if(text!=null)
                text.text = "Game over";
            Invoke(nameof(Restart), 5);
            SetActive(false);
        }// transform.LookAt(target);
    }
}

//Invoke(nameof(ForceSetInactive), 20);
//text.text = "ForceSetActive";

//private void ForceSetInactive()//reset
//{
//    SetActive(false);
//    Invoke(nameof(ForceSetActive), 20);

//    transform.position = initialPostion.transform.position;
//    _isWormFound = false;
//    //text.text = "ForceSetInactive";
//}