﻿using System;
using Terminal.Gui;

namespace UICatalog.Scenarios;

[ScenarioMetadata ("View Experiments", "v2 View Experiments")]
[ScenarioCategory ("Controls")]
[ScenarioCategory ("Borders")]
[ScenarioCategory ("Layout")]
[ScenarioCategory ("Proof of Concept")]
public class ViewExperiments : Scenario
{
    public override void Main ()
    {
        Application.Init ();

        Window app = new ()
        {
            Title = GetQuitKeyAndName (),
            TabStop = TabBehavior.TabGroup
        };

        var editor = new AdornmentsEditor
        {
            X = 0,
            Y = 0,
            AutoSelectViewToEdit = true,
            TabStop = TabBehavior.NoStop
        };
        app.Add (editor);

        FrameView testFrame = new ()
        {
            Title = "_1 Test Frame",
            X = Pos.Right (editor),
            Width = Dim.Fill (),
            Height = Dim.Fill (),
        };

        app.Add (testFrame);

        Button button = new ()
        {
            X = 0,
            Y = 0,
            Title = $"TopButton _{GetNextHotKey ()}",
        };

        testFrame.Add (button);

        button = new ()
        {
            X = Pos.AnchorEnd (),
            Y = Pos.AnchorEnd (),
            Title = $"TopButton _{GetNextHotKey ()}",
        };

        testFrame.Add (button);
        Application.MouseEvent += ApplicationOnMouseEvent;
        Application.Navigation.FocusedChanged += NavigationOnFocusedChanged;


        Application.Run (app);
        app.Dispose ();

        Application.Shutdown ();

        return;


        void NavigationOnFocusedChanged (object sender, EventArgs e)
        {
            if (!ApplicationNavigation.IsInHierarchy (testFrame, Application.Navigation!.GetFocused ()))
            {
                return;
            }

            editor.ViewToEdit = Application.Navigation!.GetFocused ();
        }
        void ApplicationOnMouseEvent (object sender, MouseEvent e)
        {
            if (e.Flags != MouseFlags.Button1Clicked)
            {
                return;
            }

            if (!editor.AutoSelectViewToEdit || !testFrame.FrameToScreen ().Contains (e.Position))
            {
                return;
            }

            // TODO: Add a setting (property) so only subviews of a specified view are considered.
            View view = e.View;

            if (view is { } && e.Flags == MouseFlags.Button1Clicked)
            {
                if (view is Adornment adornment)
                {
                    editor.ViewToEdit = adornment.Parent;
                }
                else
                {
                    editor.ViewToEdit = view;
                }
            }
        }
    }

    private int _hotkeyCount;

    private char GetNextHotKey ()
    {
        return (char)((int)'A' + _hotkeyCount++);
    }

    private View CreateTiledView (int id, Pos x, Pos y)
    {
        View overlapped = new View
        {
            X = x,
            Y = y,
            Height = Dim.Auto (),
            Width = Dim.Auto (),
            Title = $"Tiled{id} _{GetNextHotKey ()}",
            Id = $"Tiled{id}",
            BorderStyle = LineStyle.Single,
            CanFocus = true, // Can't drag without this? BUGBUG
            TabStop = TabBehavior.TabGroup,
            Arrangement = ViewArrangement.Fixed
        };

        Button button = new ()
        {
            Title = $"Tiled Button{id} _{GetNextHotKey ()}"
        };
        overlapped.Add (button);

        button = new ()
        {
            Y = Pos.Bottom (button),
            Title = $"Tiled Button{id} _{GetNextHotKey ()}"
        };
        overlapped.Add (button);

        return overlapped;
    }


    private View CreateOverlappedView (int id, Pos x, Pos y)
    {
        View overlapped = new View
        {
            X = x,
            Y = y,
            Height = Dim.Auto (),
            Width = Dim.Auto (),
            Title = $"Overlapped{id} _{GetNextHotKey ()}",
            ColorScheme = Colors.ColorSchemes ["Toplevel"],
            Id = $"Overlapped{id}",
            ShadowStyle = ShadowStyle.Transparent,
            BorderStyle = LineStyle.Double,
            CanFocus = true, // Can't drag without this? BUGBUG
            TabStop = TabBehavior.TabGroup,
            Arrangement = ViewArrangement.Movable | ViewArrangement.Overlapped
        };

        Button button = new ()
        {
            Title = $"Button{id} _{GetNextHotKey ()}"
        };
        overlapped.Add (button);

        button = new ()
        {
            Y = Pos.Bottom (button),
            Title = $"Button{id} _{GetNextHotKey ()}"
        };
        overlapped.Add (button);

        return overlapped;
    }
}
