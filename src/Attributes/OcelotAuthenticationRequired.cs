using System;
namespace Ocelot.ConfigBuilder.Attributes
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
  public class OcelotAuthenticationRequiredAttribute : Attribute
  {
    public OcelotAuthenticationRequiredAttribute(string providerKey)
    {
      this.ProviderKey = providerKey;
    }

    public string ProviderKey { get; }
  }

}
