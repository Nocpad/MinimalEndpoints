namespace Nocpad.AspNetCore.MinimalEndpoints;

public interface IEndpoint { }


public interface IEndpoint<TEndpointGroup> : IEndpoint where TEndpointGroup : IEndpointGroup { }
