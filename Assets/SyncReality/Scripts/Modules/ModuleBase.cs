using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ModuleBase<IN, OUT> : MonoBehaviour, IModule<IN, OUT>
{
    public abstract OUT Execute(IN input);

}
