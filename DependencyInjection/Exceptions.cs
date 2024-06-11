using System;

namespace Assets.Scripts.DependencyInjection
{
    public class NotInjectableException : Exception
    {
        public NotInjectableException(string field, string reason) : base($"{field} is not injectable , {reason}") { }
    }
    public class ServiceScopeNotFoundException : Exception
    {
        public ServiceScopeNotFoundException(string scopeName) : base($"service scope {scopeName} not found") { }
    }
    public class ServiceScopeAlreadyExistException : Exception
    {
        public ServiceScopeAlreadyExistException(string scopeName) : base($"service scope {scopeName} already exist") { }
    }
    public class PropertyReadonlyNotInjectableException : NotInjectableException
    {
        public PropertyReadonlyNotInjectableException(string field) : base(field,"it's readonly") { }
    }
}
