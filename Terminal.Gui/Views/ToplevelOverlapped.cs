﻿namespace Terminal.Gui;

public partial class Toplevel
{
    /// <summary>Gets or sets if this Toplevel is in overlapped mode within a Toplevel container.</summary>
    public bool IsOverlapped => Application.OverlappedTop is { } && Application.OverlappedTop != this && !Modal;

    /// <summary>Gets or sets if this Toplevel is a container for overlapped children.</summary>
    public bool IsOverlappedContainer { get; set; }
}

