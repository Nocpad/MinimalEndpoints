using Microsoft.AspNetCore.Routing;

namespace Nocpad.AspNetCore.MinimalEndpoints;

public interface IEndpointGroupConfiguration : IEndpointGroup
{
    abstract static void Configure(RouteGroupBuilder group);
}
