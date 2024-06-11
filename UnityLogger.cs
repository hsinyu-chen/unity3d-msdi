using UnityEngine;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Assets.Scripts.DependencyInjection
{
    public class UnityLoggerProvider : ILoggerProvider
    {
        protected class UnityLoggerScope : IDisposable
        {
            public void Dispose()
            {

            }
        }
        public class UnityLogger : Microsoft.Extensions.Logging.ILogger
        {
            private readonly string categoryName;

            public UnityLogger(string categoryName)
            {
                this.categoryName = categoryName;
            }
            public IDisposable BeginScope<TState>(TState state) where TState : notnull
            {
                return new UnityLoggerScope();
            }

            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var logString = $"{logLevel}::[{categoryName}]{formatter(state, exception)}";
                switch (logLevel)
                {
                    case LogLevel.Error:
                        Debug.LogError(logString);
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning(logString);
                        break;
                    default:
                        Debug.Log(logString);
                        break;
                }
            }
        }
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            return new UnityLogger(categoryName);
        }

        public void Dispose()
        {

        }
    }
    public static class UnityLoggerProviderExtensions
    {
        public static IServiceCollection AddUnityLogging(this IServiceCollection services, Action<ILoggingBuilder> configure = null)
        {
            services.AddLogging(_configure =>
            {
                _configure.SetMinimumLevel(Debug.isDebugBuild ? LogLevel.Trace : LogLevel.Information);
                _configure.AddProvider(new UnityLoggerProvider());
                configure?.Invoke(_configure);
            });
            return services;
        }
    }

}
