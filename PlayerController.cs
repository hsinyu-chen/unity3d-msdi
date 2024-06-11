using Assets.Scripts;
using Assets.Scripts.DependencyInjection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Inject]
    IFooService fooService;
    private void Awake()
    {
        //use specified scope
        //DependencyInjectionCore.Inject(this, ServiceScope1.Name);
        
        //use root scope
        DependencyInjectionCore.Inject(this);
    }
    void Start()
    {
        InvokeRepeating(nameof(Test), 0, 3);
    }
    void Test()
    {
        fooService.Bar();
    }
}
