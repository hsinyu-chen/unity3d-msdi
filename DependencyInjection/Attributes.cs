using System;

namespace Assets.Scripts.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DIConfigurationAttribute : System.Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class InjectAttribute : System.Attribute
    {
        public bool IsRequired { get; set; } = true;
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InjectScopeAttribute : System.Attribute
    {
        public InjectScopeAttribute(string scopeName)
        {
            ScopeName = scopeName;
        }

        public string ScopeName { get; }
    }
}
