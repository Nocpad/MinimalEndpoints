namespace Nocpad.AspNetCore.MinimalEndpoints;

public interface IEndpointGroup
{
    abstract static string Name { get; }

    abstract static string Route { get; }
}
