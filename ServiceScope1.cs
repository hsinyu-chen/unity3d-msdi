using Assets.Scripts.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-10000)]
public class ServiceScope1 : MonoBehaviour
{
    public const string Name = nameof(ServiceScope1);
    private void Awake()
    {
        DependencyInjectionCore.CreateScope(this, Name);
    }
}
