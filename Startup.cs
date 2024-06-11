using Assets.Scripts.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Startup
    {
        [DIConfiguration]
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddUnityLogging();
            services.AddTransient<IFooService, FooService>();

        }
    }



    // for testing


    public interface IFooService
    {
        void Bar();
    }

    public class FooService : IFooService
    {
        private readonly ILogger<FooService> logger;

        public FooService(ILogger<FooService> logger)
        {
            this.logger = logger;
        }
        public void Bar()
        {
            logger.LogDebug("bar called(Ilogger)");
        }
    }
}
