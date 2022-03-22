# A quicker way to initialize objects and call methods

The reflection API is the default way to dynamically initialize objects and call their methods at runtime. However, it is quite a bit slower than the equivalent static code. The goal of this library is to speed up such processes by utilizing dynamic code generation. It is inspired by the [FastMember](https://github.com/mgravell/fast-member) library.

You can find more information about this library in [this blog post](https://dsext001-eu1-215dsi0708-ifwe.3dexperience.3ds.com///#app:X3DMCTY_AP/content:url=https%3A%2F%2Fdsext001-eu1-215dsi0708-3dswym%2e3dexperience%2e3ds%2ecom&community=k-qK1CFpS2qhQzhlwrAHIw&contentType=post&contentId=ALBm1I_8RXaejD35x2K9Jw).

## Menu

- [The example class](#the-example-class)
- [Initialize objects of a class](#initialize-objects-of-a-class)
- [Call instance methods](#call-instance-methods)
    - [Call methods that return void](#call-methods-that-return-void)
    - [Call methods that return results](#call-methods-that-return-results)
- [Call static methods](#call-static-methods)
- [Call generic methods](#call-generic-methods)
- [Benchmark results](#benchmark-results)
- [Change cache settings](#change-cache-settings)
- [License](#license)

## The example class

We will use these classes in our examples.
```
public class Person
{
    private string _name;
    private int _age;

    public Person()
    {
    }

    public Person(string name, int age)
    {
        _name = name;
        _age = age;
    }
    
    public void PrintInfo() => Console.WriteLine($"{name}:{age}");

    public void SetInfo(string, int age)
    {
        _name = name;
        _age = age;
    }

    public int ReturnOne() => 1;

    public int PlusOne(int i) => i + 1;
    
    public static void SetInfoStatic(string, int age)
    {
        _name = name;
        _age = age;
    }

    public static int PlusOneStatic(int i) => i + 1;

    public T1 GenericFunc(string s, T2 input)
        where T1: ISkill, new()
    {
        var rs = new T1();
        foreach (var skill in skills)
        {
            rs.Skills.add(skill);
        }
    }
}

public interface ISkill
{
    public List<string> Skills { get; set; }
}

public class Skill : ISkill
{
    public List<string> Skills { get; set; } = new List<string>();
}
```

## Initialize objects of a class

**Note**: the target class must have at least one public constructor in order to use this library.

The simplest use case is to create an instance with a parameterless public constructor.
```
var typeName = typeof(Person).AssemblyQualifiedName; // Must use assembly qualified name here
Func<object[], object> initializer = Initializer.Create(typeName);
object instance = initializer(null); // Because we are calling a parameterless constructor, we need to pass null here
```

To call a constructor which has parameter(s), we must supply a list of types. The arguments can then be passed as an array of objects.
```
var parameterTypes = new[] { typeof(string), typeof(string) };
Func<object[], object> initializer = Initializer.Create(typeName, parameterTypes);

var arguments = new object[] { "Duong", "32" };
object instance = initializer(arguments);
```

## Call instance methods

**Note**: this library can only call public methods.

### Call methods that return void

We use `Invocator.CreateAction` to create the necessary delegate. We can omit the list of argument types when calling a parameterless action.
```
var typeName = typeof(Person).AssemblyQualifiedName;
var methodName = nameof(Person.PrintInfo);
Action<object, object[]> invocator = Invocator.CreateAction(typeName, methodName);
```

Because we are calling an instance method, we need an instance of type `Person`. This instance can be created by `Initializer` class, or by static code. Then we can invoke the delegate created above.
```
var target = new Person();
invocator(target, null); // Because PrintInfo takes no arguments, we pass null here
```

Similar to `Initializer` class, to pass arguments to the called methods, we need to provide a list of argument types when creating the delegate; and the arguments can be passed as an object array.
```
var typeName = typeof(Person).AssemblyQualifiedName;
var methodName = nameof(Person.SetName);
var argumentTypes = new[] { typeof(string), typeof(int) };
Action<object, object[]> invocator = Invocator.CreateAction(typeName, methodName, argumentTypes);

var target = new Person();
var arguments = new object[] { "Duong", 32 };
invocator(target, arguments);
```

### Call methods that return results

In this case, we need to use the `Invocator.CreateFunc` method. This is how we call a parameterless method.
```
var typeName = typeof(Person).AssemblyQualifiedName;
var methodName = nameof(Person.ReturnOne);
Func<object, object[], object> invocator = Invocator.CreateAction(typeName, methodName);
// Or to be explicit: Func<object, object[], object> invocator = Invocator.CreateAction(typeName, methodName, Type.EmptyTypes);
object rs = invocator(target, null);
```

And this is how we call a method that has parameter(s).
```
var typeName = typeof(Person).AssemblyQualifiedName;
var methodName = nameof(Person.PlusOne);
var argumentTypes = new[] { typeof(int) };
Func<object, object[], object> invocator = Invocator.CreateAction(typeName, methodName, argumentTypes);
var arguments = new object[] { 17 };
object rs = invocator(target, arguments);
```

## Call static methods

Calling a static method is almost identical to calling an instance method. The only difference is that we don't need a `target` instance.
```
var typeName = typeof(Person).AssemblyQualifiedName;
var staticActionName = nameof(Person.SetInfoStatic);
var staticFuncName = nameof(Person.PlusOneStatic);

var actionInvocator = Invocator.CreateAction(typeName, methodName, new[] { typeof(string), typeof(int) });
actionInvocator(null, new object[] { "Duong", 32 });

var funcInvocator = Invocator.CreateFunc(typeName, methodName, new[] { typeof(int) });
var rs = funcInvocator(null, new object[] { 17 });
```

Notice that we are using `null` as the target of our delegate. When calling a parameterless static method, even the argument array is not needed.
```
var rs = invocator(null, null);
```

## Call generic methods

Both `Invocator.CreateAction` and `Invocator.CreateFunc` accept an optional argument of type [GenericInfo](/FastAndFaster/Helpers/GenericInfo.cs). This argument enables the invocation of generic methods. We use dynamic code generation to execute the code below.
```
Person person = new Person();
Skill rs = person.GenericFunc<Skill, bool>("James Bond", true);
```

The arguments of `GenericFunc` is `(string s, T2 input)`. Because we only have one generic parameter at index 1 (the second parameter), we set `GenericInfo.GenericTypeIndex = new[] { 1 }`. And because we use `T1 == Skill, T2 == bool`, we set `GenericInfo.GenericTypeIndex == new[] { typeof(Skill), typeof(bool) }`. The static code above is equivalent to:
```
var typeName = typeof(Person).AssemblyQualifiedName;
var methodName = nameof(Person.GenericFunc);
var argumentTypes = new[] { typeof(string), typeof(bool) };
var arguments = new object[] { "James Bond", true };
var genericInfo = new GenericInfo
{
    GenericTypeIndex = new[] { 1 },
    GenericTypeIndex == new[] { typeof(Skill), typeof(bool) }
};

var invocator = Invocator.CreateFunc(typeName, methodName, argumentTypes, genericInfo);
var target = new Person();

var rs = invocator(target, arguments);
```

The code to call a generic method that returns void is almost identical, we only need to change `Invocator.CreateFunc` to `Invocator.CreateAction`.

## Change cache settings

To improve performance, all delegates created by `Initializer` and `Invocator` are cached. By default, a cache entry will expire 12 hours after the last time it was used. You can check and modify these settings separately for each class.
```
var initCacheSettings = Initializer.SlidingExpirationInSecs; // The default value is 43,200
Initializer.SlidingExpirationInSecs = 3600 // Delegates will now expire 1 hour after their last use
var invocCacheSettings = Invocator.SlidingExpirationInSecs; // The default value is 43,200
Invocator.SlidingExpirationInSecs = 600 // Delegates will now expire 10 minutes after their last use
```

## Benchmark results

This library comes with a benchmark project, which can be found [here](/FastAndFaster.Benchmark).

Below are some results for the reflection API, static C# code, and this library.

Object creation.
|                          Method |      Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------- |----------:|----------:|----------:|-------:|------:|------:|----------:|
|       Reflection_CreateInstance | 33.835 ns | 0.6646 ns | 0.5892 ns | 0.0063 |     - |     - |      40 B |
| DynamicGenerator_CreateInstance |  4.348 ns | 0.0885 ns | 0.0785 ns | 0.0064 |     - |     - |      40 B |
|           Static_CreateInstance |  2.737 ns | 0.0813 ns | 0.0721 ns | 0.0064 |     - |     - |      40 B |

Call an instance method that returns `void`.
|                      Method |       Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |-----------:|----------:|----------:|-------:|------:|------:|----------:|
|       Reflection_CallAction | 164.920 ns | 2.7548 ns | 3.3832 ns | 0.0062 |     - |     - |      40 B |
| DynamicGenerator_CallAction |   4.707 ns | 0.0988 ns | 0.0924 ns |      - |     - |     - |         - |
|           Static_CallAction |   1.900 ns | 0.0386 ns | 0.0342 ns |      - |     - |     - |         - |

Call an instance method that returns results.
|                    Method |       Mean |     Error |    StdDev |     Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------- |-----------:|----------:|----------:|-----------:|-------:|------:|------:|----------:|
|       Reflection_CallFunc | 195.186 ns | 2.5206 ns | 2.3578 ns | 194.786 ns | 0.0100 |     - |     - |      64 B |
| DynamicGenerator_CallFunc |   5.863 ns | 0.1851 ns | 0.1545 ns |   5.790 ns | 0.0038 |     - |     - |      24 B |
|           Static_CallFunc |   2.163 ns | 0.1300 ns | 0.3602 ns |   2.046 ns | 0.0038 |     - |     - |      24 B |


Call a static method that returns `void`.
|                            Method |        Mean |     Error |    StdDev |      Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |------------:|----------:|----------:|------------:|-------:|------:|------:|----------:|
|       Reflection_CallStaticAction | 162.2037 ns | 3.1451 ns | 5.1675 ns | 161.1659 ns | 0.0062 |     - |     - |      40 B |
| DynamicGenerator_CallStaticAction |   3.5516 ns | 0.1019 ns | 0.1133 ns |   3.5288 ns |      - |     - |     - |         - |
|           Static_CallStaticAction |   0.0100 ns | 0.0127 ns | 0.0119 ns |   0.0055 ns |      - |     - |     - |         - |

Call a static method that returns results.
|                          Method |       Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------- |-----------:|----------:|----------:|-------:|------:|------:|----------:|
|       Reflection_CallStaticFunc | 188.508 ns | 3.2940 ns | 3.0812 ns | 0.0100 |     - |     - |      64 B |
| DynamicGenerator_CallStaticFunc |   4.472 ns | 0.1700 ns | 0.1590 ns | 0.0038 |     - |     - |      24 B |
|           Static_CallStaticFunc |   2.081 ns | 0.0760 ns | 0.0711 ns | 0.0038 |     - |     - |      24 B |

We can see that in all cases, the performance of our library is much closer to static C# code than to the reflection API.

## License

MIT License

https://opensource.org/licenses/MIT
