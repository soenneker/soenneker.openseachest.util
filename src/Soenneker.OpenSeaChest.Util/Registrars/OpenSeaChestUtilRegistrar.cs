using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.OpenSeaChest.Util.Abstract;
using Soenneker.OpenSeaChest.Util.Dtos;
using Soenneker.Utils.Process.Registrars;
using Soenneker.Utils.File.Registrars;
using Soenneker.Utils.Paths.Resources.Registrars;
namespace Soenneker.OpenSeaChest.Util.Registrars;

/// <summary>Registers openSeaChest and its process dependencies.</summary>
public static class OpenSeaChestUtilRegistrar
{
    /// <summary>Adds the utility as a singleton with optional execution settings.</summary>
    public static IServiceCollection AddOpenSeaChestUtilAsSingleton(this IServiceCollection services, Action<OpenSeaChestOptions>? configure = null)
    {
        services.AddProcessUtilAsSingleton().AddFileUtilAsSingleton().AddResourcesPathUtilAsSingleton();
        services.AddOptions<OpenSeaChestOptions>();
        if (configure is not null)
            services.Configure(configure);
        services.TryAddSingleton<IOpenSeaChestUtil, OpenSeaChestUtil>();
        return services;
    }

    /// <summary>Adds the utility as a scoped service with optional execution settings.</summary>
    public static IServiceCollection AddOpenSeaChestUtilAsScoped(this IServiceCollection services, Action<OpenSeaChestOptions>? configure = null)
    {
        services.AddProcessUtilAsScoped().AddFileUtilAsScoped().AddResourcesPathUtilAsScoped();
        services.AddOptions<OpenSeaChestOptions>();
        if (configure is not null)
            services.Configure(configure);
        services.TryAddScoped<IOpenSeaChestUtil, OpenSeaChestUtil>();
        return services;
    }
}
