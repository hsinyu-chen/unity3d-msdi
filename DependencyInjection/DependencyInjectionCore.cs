using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq.Expressions;
using System;
using UnityEngine;
using System.Threading;
using System.Collections.Concurrent;
using Unity.VisualScripting;

namespace Assets.Scripts.DependencyInjection
{
    public class DependencyInjectionCore
    {
        readonly static ConcurrentDictionary<Type, object> injectors = new();
        readonly static ConcurrentDictionary<string, IServiceScope> scopes = new();
        static IServiceProvider serviceProvider;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitDependencyInjection()
        {
            Debug.Log("Init Dependency Injection");
            var services = new ServiceCollection();
            var types = Assembly.GetExecutingAssembly().GetTypes();
            //find and excute all service configuration methods
            foreach (var configMethod in types.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(x => x.IsDefined(typeof(DIConfigurationAttribute), false)))
            {
                configMethod.Invoke(null, new object[] { services });
            }
            serviceProvider = services.BuildServiceProvider();
            //build all injectors
            var createInjector = typeof(DependencyInjectionCore).GetMethod(nameof(CreateInjector), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var type in types)
            {
                var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (members.Any(x => x.IsDefined(typeof(InjectAttribute), false)))
                {
                    injectors.TryAdd(type, createInjector.MakeGenericMethod(type).Invoke(null, Array.Empty<object>()));
                }
            }
        }
        public static IServiceScope CreateScope<T>(T instance, string scopeName) where T : MonoBehaviour
        {
            var scope = serviceProvider.CreateScope();
            if (scopes.ContainsKey(scopeName))
            {
                throw new ServiceScopeAlreadyExistException(scopeName);
            }
            instance.destroyCancellationToken.Register(() =>
            {
                if (scopes.TryRemove(scopeName, out var _scope))
                {
                    _scope.Dispose();
                }
            });
            scopes.TryAdd(scopeName, scope);
            return scope;
        }
        /// <summary>
        /// inject services to MonoBehaviour instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IServiceScope Inject<T>(T instance, string scopeName = default) where T : MonoBehaviour
        {
            IServiceProvider provider;
            if (scopeName == default)
            {
                provider = serviceProvider;
            }
            else
            {
                if (scopes.TryGetValue(scopeName, out var _scope))
                {
                    provider = _scope.ServiceProvider;
                }
                else
                {
                    throw new ServiceScopeNotFoundException(scopeName);
                }
            }
            var scope = provider.CreateScope();
            instance.destroyCancellationToken.Register(() =>
            {
                scope.Dispose();
            });
            ((Action<T, IServiceProvider>)injectors.GetOrAdd(typeof(T), type => CreateInjector<T>()))(instance, scope.ServiceProvider);
            return scope;
        }
        static readonly MethodInfo getService = typeof(ServiceProviderServiceExtensions)
               .GetMethod(nameof(ServiceProviderServiceExtensions.GetService), 1,
               BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(IServiceProvider) }, null);
        static readonly MethodInfo getRequiredService = typeof(ServiceProviderServiceExtensions)
             .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), 1,
               BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(IServiceProvider) }, null);
        /// <summary>
        /// Create Compiled Expression for MonoBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="PropertyReadonlyNotInjectableException">throw when intent to inject service to readonly property</exception>
        public static Action<T, IServiceProvider> CreateInjector<T>() where T : MonoBehaviour
        {
            var body = new List<Expression>();
            var instance = Expression.Parameter(typeof(T), "instance");
            var serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            foreach (var member in typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.IsDefined(typeof(InjectAttribute), true)))
            {
                var isRequired = member.GetCustomAttribute<InjectAttribute>().IsRequired;
                var getServiceMethod = isRequired ? getRequiredService : getService;

                if (member.MemberType == MemberTypes.Property && member is PropertyInfo property)
                {
                    if (property.CanWrite)
                    {
                        var closedGetServiceMethod = getServiceMethod.MakeGenericMethod(property.PropertyType);
                        body.Add(Expression.Assign(Expression.Property(instance, property), Expression.Call(closedGetServiceMethod, serviceProvider)));
                    }
                    else
                    {
                        throw new PropertyReadonlyNotInjectableException($"{typeof(T).FullName}.{property.Name}");
                    }
                }
                else if (member.MemberType == MemberTypes.Field && member is FieldInfo field)
                {
                    if (field.IsInitOnly)
                    {
                        throw new PropertyReadonlyNotInjectableException($"{typeof(T).FullName}.{field.Name}");
                    }
                    else
                    {
                        var closedGetServiceMethod = getServiceMethod.MakeGenericMethod(field.FieldType);
                        body.Add(Expression.Assign(Expression.Field(instance, field), Expression.Call(closedGetServiceMethod, serviceProvider)));
                    }
                }
            }
            return LambdaExpression.Lambda<Action<T, IServiceProvider>>(Expression.Block(body), instance, serviceProvider).Compile();
        }
    }
}
