namespace Ocelot.ConfigBuilder.Models
{
  public class OcelotConfigBuilderDownstreamHost
  {
    public OcelotConfigBuilderDownstreamHost(string host, int port)
    {
      this.Host = host;
      this.Port = port;
    }

    public string Host { get; set; }
    public int Port { get; set; }
  }
}
