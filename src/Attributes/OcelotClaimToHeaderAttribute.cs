using System;
namespace Ocelot.ConfigBuilder.Attributes
{
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
  public class OcelotClaimToHeaderAttribute : Attribute
  {
    public OcelotClaimToHeaderAttribute(string headerName, string claim, string valuePath)
    {
      this.HeaderName = headerName;
      this.Claim = claim;
      this.ValuePath = valuePath;
    }

    public string HeaderName { get; }
    public string Claim { get; }
    public string ValuePath { get; }
  }

}
