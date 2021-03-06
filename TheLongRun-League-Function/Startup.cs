﻿using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using TheLongRun.Common;

[assembly: WebJobsStartup(typeof(TheLongRunLeaguesFunction.TheLongRunLeaguesStartup),
    "The Long Run - Leagues Domain")]
namespace TheLongRunLeaguesFunction
{
    /// <summary>
    /// Start-up class to load all the dependency injection and startup configuration code
    /// </summary>
    public class TheLongRunLeaguesStartup
        : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {

            // Initialise any common services
            TheLongRun.Common.CQRSAzureBindings.InitializeServices(builder.Services);

            // Add the standard (built-in) bindings 
            builder.AddBuiltInBindings();

            //Register any extensions for bindings
            builder.AddExtension<InjectConfiguration>();

            

        }
    }

    /// <summary>
    /// Dependency injection configuration for this Azure functions app
    /// </summary>
    public class InjectConfiguration 
        : IExtensionConfigProvider
    {
        private IServiceProvider _serviceProvider;

        public InjectConfiguration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            // Add any custom bindings for this domain
            // and then add any core ones from TheLongRun.Common
            CQRSAzureBindings.InitializeInjectionConfiguration(context);
            
           
        }
    }
}
