namespace RhinoCommercialPlatform.Core.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
