using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace Ocelot.ConfigBuilder.Attributes
{
  public class OcelotRoute : RouteAttribute
  {
    public OcelotRoute(string template, string ocelotTemplate, string authenticationProvider)
      : base(template)
    {
      this.OcelotTemplate = ocelotTemplate;
      this.AuthenticationProvider = authenticationProvider;
    }

    public string OcelotTemplate { get; }
    public string AuthenticationProvider { get; }
  }
}
