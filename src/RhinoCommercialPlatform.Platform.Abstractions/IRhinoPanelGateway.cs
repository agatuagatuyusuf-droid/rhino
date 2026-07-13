using System;

namespace RhinoCommercialPlatform.Platform.Abstractions;

public interface IRhinoPanelGateway
{
    bool RegisterPanel(object pluginInstance, Type panelType, string panelName, object? icon);
    bool OpenPanel(Type panelHostType, bool makeSelectedPanel);
    void ClosePanel(Guid panelId);
    bool IsPanelVisible(Type panelHostType);
    object? GetPanel(Guid panelId);
}
