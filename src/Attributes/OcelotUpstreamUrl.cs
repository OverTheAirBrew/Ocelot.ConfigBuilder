using System;
namespace Ocelot.ConfigBuilder.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class OcelotUpstreamUrl : Attribute
    {
        public OcelotUpstreamUrl(string url)
        {
            this.Url = url;
        }

        public string Url { get; }
    }

}
