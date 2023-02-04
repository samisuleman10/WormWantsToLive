using UnityEngine;

public class Worm : MonoBehaviour
{
    AttackEventManager attackManagerInstance;
    private void Start()
    {
        attackManagerInstance = AttackEventManager.Instance;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("SafeArea"))
        {
            attackManagerInstance.shouldBirdAttack = false;
        }

        if (other.CompareTag("ExtraDangerousArea"))
        {
            attackManagerInstance.shouldBirdAttack = true;
            attackManagerInstance.ExtraDangerAreaEntered();
        }

        if (other.CompareTag("SemiDangerousArea"))
        {
            attackManagerInstance.shouldBirdAttack = true;
            attackManagerInstance.ExtraDangerAreaExited();
        }
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    if(other.CompareTag("SafeArea"))
    //    {
            
    //        Debug.Log("In safe area ");
    //    }

    //    if (other.CompareTag("ExtraDangerousArea"))
    //    {
    //        Debug.Log("In extra dangerous area ");

    //        //SetMiddleSpeedForBird();

    //    }

    //    if (other.CompareTag("SemiDangerousArea"))
    //    {
    //        Debug.Log("In semi dangerous area ");

    //       // AttackEventManager.Instance.StartBirdAttack(transform);

    //        //SetExtraSpeedForBird();
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("SafeArea"))
    //    {
    //        Debug.Log("Outside safe area ");

    //        //StartBirdAtack();
    //    }

    //    if (other.CompareTag("ExtraDangerousArea"))
    //    {
    //        Debug.Log("Outside dangerous area ");
    //    }

    //    if (other.CompareTag("SemiDangerousArea"))
    //    {
    //        Debug.Log("Outside dangerous area ");
    //    }
    //}
}
