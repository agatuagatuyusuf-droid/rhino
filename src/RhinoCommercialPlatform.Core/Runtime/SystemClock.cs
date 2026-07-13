using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Core.Runtime;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
