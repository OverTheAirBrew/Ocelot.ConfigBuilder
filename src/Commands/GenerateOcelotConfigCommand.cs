using System.Threading.Tasks.Dataflow;
using System.IO;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Oakton.AspNetCore;
using Oakton;
using Ocelot.ConfigBuilder.Attributes;
using System.Collections.Generic;
using Newtonsoft.Json;
using Ocelot.ConfigBuilder;
using Ocelot.ConfigBuilder.Models;
using Ocelot.ConfigBuilder.Kubernetes;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.AspNetCore.Routing;

[assembly: Oakton.OaktonCommandAssembly]

namespace TheNerdyBrewingCo.Api.Commands
{

  [Description("This will generate the ocelot config from the current api")]
  public class GenerateOcelotConfigCommand : OaktonCommand<NetCoreInput>
  {
    private readonly IDictionary<string, Type> _constraintMap;

    public GenerateOcelotConfigCommand()
    {
      var routeOptions = new RouteOptions();
      _constraintMap = routeOptions.ConstraintMap;
    }

    public override bool Execute(NetCoreInput input)
    {
      var host = input.BuildHost();
      var serviceType = typeof(IApiDescriptionGroupCollectionProvider);

      var ocelotBaseConfig = host.Services.GetService(typeof(IOcelotConfigBuilderConfiguration)) as IOcelotConfigBuilderConfiguration;

      if (!(host.Services.GetService(serviceType) is IApiDescriptionGroupCollectionProvider explorer))
        return true;

      var results = explorer
        .ApiDescriptionGroups
        .Items
        .SelectMany(x => x.Items);

      var grouped = results.GroupBy(r => r.RelativePath);

      var config = new OcelotConfig()
      {
      };

      foreach (var group in grouped)
      {
        var ocelotRoutes = group.Select(x => x.ActionDescriptor.EndpointMetadata.Where(y => y.GetType() == typeof(OcelotRoute))).FirstOrDefault();

        var ocelotRoute = GetRoute(ocelotRoutes, group.Key);

        if (ocelotRoute == null) continue;

        var claimsToHeaders = group.Select(x => x.ActionDescriptor.EndpointMetadata.Where(y => y.GetType() == typeof(OcelotClaimToHeaderAttribute))).FirstOrDefault();

        var downStreamRoute = group.Key;
        var upstreamMethods = group.Select(x => x.HttpMethod).ToArray();

        // var upstreamPath = upstreamUrl != null ? formatUrl(upstreamUrl.Url, ocelotBaseConfig.PrefixToRemove) : formatUrl(downStreamRoute, ocelotBaseConfig.PrefixToRemove);

        var route = new OcelotConfigRoute
        {
          DownstreamPathTemplate = formatUrl(downStreamRoute),
          DownstreamHostAndPorts = new List<OcelotConfigBuilderDownstreamHost> { ocelotBaseConfig.DownstreamHost },
          UpstreamPathTemplate = formatUrl(ocelotRoute.OcelotTemplate),
          UpstreamHttpMethod = upstreamMethods,
        };

        foreach (var claim in claimsToHeaders)
        {
          var castClaim = claim as OcelotClaimToHeaderAttribute;

          route.AddHeadersToRequest.Add(castClaim.HeaderName, $"Claims[{castClaim.Claim}]>{castClaim.ValuePath}".Trim());
        }

        if (ocelotRoute.AuthenticationProvider != null)
        {
          route.AuthenticationOptions = new OcelotConfigRouteAuthOptions
          {
            AuthenticationProviderKey = ocelotRoute.AuthenticationProvider
          };
        }

        config.Routes.Add(route);
      }

      config.GlobalConfiguration.BaseUrl = ocelotBaseConfig.BaseUrl;

      var data = JsonConvert.SerializeObject(config, new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.Indented
      });

      Console.WriteLine(data);

      var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
      var configmap = new Configmap
      {
        Metadata = new ConfigmapMetadata
        {
          Name = ocelotBaseConfig.Kubernetes.Name,
          Namespace = ocelotBaseConfig.Kubernetes.Namespace
        },
      };

      configmap.Data.Add("ocelot.json", data);

      var output = serializer.Serialize(configmap);

      File.WriteAllText(ocelotBaseConfig.OutputFileName, output);

      return true;
    }

    private string formatUrl(string url)
    {
      if (url.Substring(0, 1) == "/") return url;
      return $"/{url}";
    }

    public OcelotRoute GetRoute(IEnumerable<object> ocelotRoutes, string routeTemplate)
    {
      foreach (var route in ocelotRoutes.OrderByDescending(x => (x as OcelotRoute).OcelotTemplate.Length))
      {
        var template = (route as OcelotRoute).Template;
        var formattedTemplate = formatRouteTemplate(template);

        var contains = routeTemplate.Contains(formattedTemplate);
        if (contains)
        {
          return (route as OcelotRoute);
        }
      }

      var defaultRoute = ocelotRoutes.First();
      return (defaultRoute as OcelotRoute);
    }

    private string formatRouteTemplate(string routeTemplate)
    {
      foreach (var constraint in _constraintMap)
      {
        var key = $":{constraint.Key}";
        routeTemplate = routeTemplate.Replace(key, "");
      }

      return routeTemplate;
    }
  }
}
