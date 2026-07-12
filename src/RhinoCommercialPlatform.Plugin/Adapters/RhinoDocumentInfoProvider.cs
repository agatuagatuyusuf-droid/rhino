using System;

namespace RhinoCommercialPlatform.Plugin.Adapters;

/// <summary>
/// Provides information about the current Rhino document.
/// All methods are safe to call even when no document is open.
/// </summary>
public sealed class RhinoDocumentInfoProvider
{
    public bool HasDocument => Rhino.RhinoDoc.ActiveDoc != null;

    public string? DocumentName
    {
        get
        {
            try
            {
                return Rhino.RhinoDoc.ActiveDoc?.Name;
            }
            catch
            {
                return null;
            }
        }
    }

    public string? DocumentPath
    {
        get
        {
            try
            {
                return Rhino.RhinoDoc.ActiveDoc?.Path;
            }
            catch
            {
                return null;
            }
        }
    }

    public int? ObjectCount
    {
        get
        {
            try
            {
                return Rhino.RhinoDoc.ActiveDoc?.Objects?.Count;
            }
            catch
            {
                return null;
            }
        }
    }

    public string? RhinoVersion
    {
        get
        {
            try
            {
                return Rhino.RhinoApp.ExeVersion.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
