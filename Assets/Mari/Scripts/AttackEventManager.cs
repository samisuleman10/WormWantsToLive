using System;

public class AttackEventManager : BasicEventManager<AttackEventManager>
{
    public bool shouldBirdAttack;
    public Action onExtraDangerousAreaEntered;
    public Action onSemiDangerousAreaEntered;
    public Action onExtraDangerousAreaExited;

    public void ExtraDangerAreaEntered()
    {
        onExtraDangerousAreaEntered?.Invoke();
    }

    public void ExtraDangerAreaExited()
    {
        onExtraDangerousAreaExited?.Invoke();
    }

    //public void SemiDangerousAreaEntered()
    //{
    //    onSemiDangerousAreaEntered?.Invoke();
    //}

    //public Action onSemiDangerousAreaExited;
    //public Action onSafeAreaEntered;
    //public Action onSafeAreaExited;


    //public Action<Transform> onStartBirdAttack;
    //public Action onMakeBirdGoBack;

    //public void StartBirdAttack(Transform worm)
    //{
    //    onStartBirdAttack?.Invoke(worm);
    //}

    //public void MakeBirdGoBack()
    //{
    //    onMakeBirdGoBack?.Invoke();
    //}

    //public void SemiDangerousAreaExited()
    //{
    //    onSemiDangerousAreaExited?.Invoke();
    //}

    //public void SafeAreaEntered()
    //{
    //    onSafeAreaEntered.Invoke();
    //}

    //public void SafeAreaExited()
    //{
    //    onSafeAreaExited.Invoke();
    //}
}
