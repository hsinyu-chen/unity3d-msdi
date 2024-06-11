# unity3d-msdi
Microsoft.Extensions.DependencyInjection intergration for unity 3d

this just a proof-of-concept repo so use at your own risk

all the reflection stuff do at `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`
and compiled to function to minimize performance overhead

## Installation

 intell nuget package `Microsoft.Extensions.DependencyInjection` (I use [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity))

 copy `DependencyInjection` folder into your script folder

## How to use

### Config services

add a C# class with a static method to your script folder

and add `[DIConfiguration]` attribute to your method , you can have multiple configuration method , all of those will be excute at game startup

```
public class Startup
{
    [DIConfiguration]
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddUnityLogging();
        services.AddTransient<IFooService, FooService>();

    }
}
```
(see [Startup.cs](Startup.cs) )

### Inject(resolve) services in MonoBehaviour

when you need service , you can do Property/Field inject by using `[Inject]` Attribute

you also need to call `DependencyInjectionCore.Inject` to 

```
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
```
(see [PlayerController.cs](PlayerController.cs))


### LifeCycle of ServiceScope

when you call `Inject` it will create new scope for `MonoBehaviour` and dispose when MonoBehaviour destory , so only call once at `Awake`

if you want create scope by your self and get benefits from the msdi scope system, you can create a `MonoBehaviour` and call ` DependencyInjectionCore.CreateScope` on `Awake`, this method will create scope , and dispose when that `MonoBehaviour` destory, then add this `MonoBehaviour` to any `GameObject`
(see [ServiceScope1.cs](ServiceScope1.cs))

then when you inject services into the `MonoBehaviour` , you can use `DependencyInjectionCore.Inject(this,"scope name")` to use specified scope as parent scope of your `MonoBehaviour` service scope
(see [PlayerController.cs](PlayerController.cs))


