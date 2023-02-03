using UnityEngine;

public class ReferenceObjectTranslationByControllers : MonoBehaviour
{
    [SerializeField]
    private Transform _objectToTranslate;

    private void Start()
    {
        ObjectTranslationByControllers.Instance.SetObjectToTranslate(_objectToTranslate);
    }
}
