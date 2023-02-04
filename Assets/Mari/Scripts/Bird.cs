using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField]
    private float speed = 5;
    [SerializeField]
    private Transform initialPostion;
    [SerializeField]
    private Transform worm;

    AttackEventManager attackEventManagerinstance;

    private void Start()
    {
        attackEventManagerinstance = AttackEventManager.Instance;
        attackEventManagerinstance.onExtraDangerousAreaEntered += IncreaseSpeed;
        attackEventManagerinstance.onExtraDangerousAreaExited += DecreaseSpeed;
    }

    private void OnDestroy()
    {
        attackEventManagerinstance.onExtraDangerousAreaEntered -= IncreaseSpeed;
        attackEventManagerinstance.onExtraDangerousAreaExited -= DecreaseSpeed;
    }

    private void IncreaseSpeed()
    {
        speed = 20;
    }

    private void DecreaseSpeed()
    {
        speed = 5;
    }

    private void Update()
    {

        if (attackEventManagerinstance.shouldBirdAttack)
            transform.position = Vector3.MoveTowards(transform.position, worm.position, Time.deltaTime * speed);
        else
            transform.position = Vector3.MoveTowards(transform.position, initialPostion.position, Time.deltaTime * speed);
    }

    //private void GoBack()
    //{
    //    Debug.Log("Goback from Bird");
    //    //if (_isBirdGoingBack) return;
    //    //if(_isMoving)
    //        CancelBirdAttack();
    //    //StartCoroutine(MoveTo(initialPostion));
    //    //transform.Translate( wormPosition,Space.World, speed);

    //    //var toTranslate = new Vector3(relativeTo.forward.x, 0, relativeTo.forward.z);
    //    //toMove.Translate(toTranslate * quantificator, space);
    //    //return toMove.position;
    //}

    //private void

    //[ContextMenu("TestMovement")]
    //public void TestMovement()
    //{
    //    _movementCanceled = false;
    //    StartCoroutine(MoveTo(testPostion.position, speed));
    //}

    //[ContextMenu("CancelMovement")]
    //public void CancelBirdAttack()
    //{
    //   _attackCanceled = true;
    //}


    //private IEnumerator MoveTo(Transform target)
    //{

    //    var currentPos = transform.position;
    //    var distance = Vector3.Distance(currentPos, target.position);
    //    // TODO: make sure speed is always > 0
    //    var duration = distance / speed;

    //    var timePassed = 0f;

    //    while (timePassed < duration && !_attackCanceled)
    //    {
    //        // always a factor between 0 and 1
    //        var factor = timePassed / duration;

    //        transform.position = Vector3.Lerp(currentPos, target.position, factor);

    //        // increase timePassed with Mathf.Min to avoid overshooting
    //        timePassed += Math.Min(Time.deltaTime, duration - timePassed);

    //        // "Pause" the routine here, render this frame and continue
    //        // from here in the next frame
    //        yield return null;
    //    }



    //    if (!_attackCanceled)
    //    {
    //        transform.position = target.position;
    //        _isMoving = false;
    //    }

    //    // Something you want to do when moving is done
    //}

    //private void Update()
    //{
    //    transform.position = Vector3.Lerp(transform.position, testPostion.position, 0.01f);
    //}

}
