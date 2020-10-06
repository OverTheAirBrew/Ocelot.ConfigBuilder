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

[assembly: Oakton.OaktonCommandAssembly]

namespace TheNerdyBrewingCo.Api.Commands
{

  [Description("This will generate the ocelot config from the current api")]
  public class GenerateOcelotConfigCommand : OaktonCommand<NetCoreInput>
  {
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
        var authRequired = group.Select(x => x.ActionDescriptor.EndpointMetadata.Where(y => y.GetType() == typeof(OcelotAuthenticationRequiredAttribute))).FirstOrDefault().FirstOrDefault() as OcelotAuthenticationRequiredAttribute;
        var claimsToHeaders = group.Select(x => x.ActionDescriptor.EndpointMetadata.Where(y => y.GetType() == typeof(OcelotClaimToHeaderAttribute))).FirstOrDefault();

        var downStreamRoute = group.Key;
        var upstreamMethods = group.Select(x => x.HttpMethod).ToArray();

        var route = new OcelotConfigRoute
        {
          DownstreamPathTemplate = downStreamRoute,
          DownstreamHostAndPorts = new List<OcelotConfigBuilderDownstreamHost> { ocelotBaseConfig.DownstreamHost },
          UpstreamPathTemplate = downStreamRoute,
          UpstreamHttpMethod = upstreamMethods,
        };

        foreach (var claim in claimsToHeaders)
        {
          var castClaim = claim as OcelotClaimToHeaderAttribute;

          route.AddHeadersToRequest.Add(castClaim.HeaderName, $"Claims[{castClaim.Claim}] > {castClaim.ValuePath}");
        }

        if (authRequired != null)
        {
          route.AuthenticationOptions = new OcelotConfigRouteAuthOptions
          {
            AuthenticationProviderKey = authRequired.ProviderKey
          };
        }

        config.Routes.Add(route);
      }

      config.GlobalConfiguration.BaseUrl = ocelotBaseConfig.BaseUrl;

      Console.WriteLine(JsonConvert.SerializeObject(config, new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore
      }));

      return true;
    }
  }
}
