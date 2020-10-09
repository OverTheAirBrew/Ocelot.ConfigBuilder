# Ocelot.ConfigBuilder

This app uses attributes to create a ocelot config, atm it only creates a kubernetes configmap

## Available Attributes

### Ocelot Route
This is a wrapper around the standard `Route` attribute, but allows you to pass an ocelot upstream route and also the auth provider if there is one

### OcelotClaimToHeader
This allows you to add the `ClaimsToHeader` section of your ocelot configiration

## Usage
```csharp
[ApiExplorerSettings(IgnoreApi = false)]
[OcelotRoute("api/downstreamroute", "downstreamroute", "authprovider")]
[OcelotClaimToHeader("UserId", "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "value")]
public class ExampleController : ControllerBase
```

You need to make sure that the `ApiExplorerSettings` attribute is provided so the underlying api can pick up the routes

In your `Program.cs` replace `build()` with ` .UseOcelotConfig(args);`
In your `Startup.cs` include the followin in your `ConfigureService`

```csharp
services.UseOcelotConfigBuilder(new OcelotConfigBuilderConfiguration
    {
    BaseUrl = "{your gateway url}",
    OutputFileName = "{your output location of the file},
    DownstreamHost = new OcelotConfigBuilderDownstreamHost("{your downstream host}", 80),
    Kubernetes = new KubernetesGeneration
    {
        Name = "{kubernetes configmap name}",
        Namespace = "{kubernetes namespace where the gateway is installed}"
    }
    });
```

You will ofcourse need some method of reloading the ocelot configuration on the fly inside the kube cluster. As the default .net file provider cannot reload this due to symlinks i would suggest looking at https://github.com/fbeltrao/ConfigMapFileProvider