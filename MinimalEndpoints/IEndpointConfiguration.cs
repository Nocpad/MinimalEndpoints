using Microsoft.AspNetCore.Builder;

namespace Nocpad.AspNetCore.MinimalEndpoints;

public interface IEndpointConfiguration : IEndpoint
{
    abstract static void Configure(RouteHandlerBuilder builder);
}


public interface IEndpointConfiguration<TEndpointGroup> : IEndpointConfiguration where TEndpointGroup : IEndpointGroup { }