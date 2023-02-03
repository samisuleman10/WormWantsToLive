using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SpatialAnchorAudioHandler : BasicEventManager<SpatialAnchorAudioHandler>
{
    [Header("Audio Settings")]
    [SerializeField]
    private AudioClip placeAnchor;
    [SerializeField]
    private AudioClip deleteAnchor;
    private AudioSource _UISource;

    private void Start()
    {
        _UISource = GetComponent<AudioSource>();
    }

    public void PlayPlacementSound()
    {
        if (_UISource.isPlaying)
        {
            _UISource.Stop();
        }
        _UISource.clip = placeAnchor;
        _UISource.Play();
    }

    public void PlayDeleteSound()
    {
        if (_UISource.isPlaying)
        {
            _UISource.Stop();
        }
        _UISource.clip = deleteAnchor;
        _UISource.Play();
    }

}
