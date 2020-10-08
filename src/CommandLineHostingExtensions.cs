using System.Threading.Tasks;
using Oakton.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.ConfigBuilder.Models;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Ocelot.ConfigBuilder
{
  public static class UseConfigBuilder
  {
#if NETSTANDARD2_0
    public static Task<int> UseOcelotConfig(this IWebHostBuilder builder, string[] args)
    {
      return builder.RunOaktonCommands(args);
    }
#else
    public static Task<int> UseOcelotConfig(this IHostBuilder builder, string[] args)
    {
        return builder.RunOaktonCommands(args);
    }  
#endif

    public static void UseOcelotConfigBuilder(this IServiceCollection collection, OcelotConfigBuilderConfiguration options)
    {
      collection.AddSingleton<IOcelotConfigBuilderConfiguration>(options);
    }
  }

  public class OcelotConfigBuilderConfiguration : IOcelotConfigBuilderConfiguration
  {
    public string BaseUrl { get; set; }
    public string OutputFileName { get; set; }
    public string PrefixToRemove { get; set; }
    public OcelotConfigBuilderDownstreamHost DownstreamHost { get; set; }
    public KubernetesGeneration Kubernetes { get; set; }
  }

  public interface IOcelotConfigBuilderConfiguration
  {
    string BaseUrl { get; set; }
    string OutputFileName { get; set; }
    string PrefixToRemove { get; set; }
    OcelotConfigBuilderDownstreamHost DownstreamHost { get; set; }
    KubernetesGeneration Kubernetes { get; set; }
  }

  public class KubernetesGeneration
  {
    public string Name { get; set; }
    public string Namespace { get; set; }
  }
}
