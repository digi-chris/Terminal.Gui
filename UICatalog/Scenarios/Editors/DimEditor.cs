﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terminal.Gui;

namespace UICatalog.Scenarios;

/// <summary>
///     Provides an editor UI for the Margin, Border, and Padding of a View.
/// </summary>
public class DimEditor : View
{
    public DimEditor ()
    {
        Title = "Dim";

        BorderStyle = LineStyle.Rounded;

        Width = Dim.Auto (DimAutoStyle.Content);
        Height = Dim.Auto (DimAutoStyle.Content);

        CanFocus = true;

        _expandButton = new ()
        {
            Orientation = Orientation.Vertical
        };


        TabStop = TabBehavior.TabStop;


        Initialized += DimEditor_Initialized;

        AddCommand (Command.Accept, () => true);
    }

    private View? _viewToEdit;

    private int _value;
    private RadioGroup? _dimRadioGroup;
    private TextField? _valueEdit;

    /// <summary>
    ///     Gets or sets whether the DimEditor should automatically select the View to edit
    ///     based on the values of <see cref="AutoSelectSuperView"/> and <see cref="AutoSelectAdornments"/>.
    /// </summary>
    public bool AutoSelectViewToEdit { get; set; }

    /// <summary>
    ///     Gets or sets the View that will scope the behavior of <see cref="AutoSelectViewToEdit"/>.
    /// </summary>
    public View? AutoSelectSuperView { get; set; }

    /// <summary>
    ///     Gets or sets whether auto select with the mouse will select Adornments or just Views.
    /// </summary>
    public bool AutoSelectAdornments { get; set; }

    public View? ViewToEdit
    {
        get => _viewToEdit;
        set
        {
            if (_viewToEdit == value)
            {
                return;
            }

            if (value is null && _viewToEdit is { })
            {
                _viewToEdit.SubviewsLaidOut -= View_LayoutComplete;
            }

            _viewToEdit = value;

            if (_viewToEdit is { })
            {
                _viewToEdit.SubviewsLaidOut += View_LayoutComplete;

                _viewToEdit.SubviewLayout += (sender, args) =>
                                             {

                                             };
            }
        }
    }

    private void View_LayoutComplete (object? sender, LayoutEventArgs args)
    {
        UpdateSettings ();
    }

    private bool _updatingSettings = false;

    private void UpdateSettings ()
    {
        if (ViewToEdit is null)
        {
            return;
        }

        _updatingSettings = true;

        Dim? dim;
        if (Dimension == Dimension.Width)
        {
            dim = ViewToEdit.Width;
        }
        else
        {
            dim = ViewToEdit.Height;
        }

        try
        {
            _dimRadioGroup!.SelectedItem = _dimNames.IndexOf (_dimNames.First (s => dim!.ToString ().StartsWith(s)));
        }
        catch (InvalidOperationException e)
        {
            // This is a hack to work around the fact that the Pos enum doesn't have an "Align" value yet
            Debug.WriteLine ($"{e}");
        }

        _valueEdit!.Enabled = false;
        switch (dim)
        {
            case DimAbsolute absolute:
                _valueEdit.Enabled = true;
                _value = absolute.Size;
                _valueEdit!.Text = _value.ToString ();
                break;
            case DimFill fill:
                var margin = fill.Margin as DimAbsolute;
                _valueEdit.Enabled = margin is {};
                _value = margin?.Size ?? 0;
                _valueEdit!.Text = _value.ToString ();
                break;
            case DimFunc func:
                _valueEdit.Enabled = true;
                _value = func.Fn ();
                _valueEdit!.Text = _value.ToString ();
                break;
            case DimPercent percent:
                _valueEdit.Enabled = true;
                _value = percent.Percentage;
                _valueEdit!.Text = _value.ToString ();
                break;
            default:
                _valueEdit!.Text = dim!.ToString ();
                break;
        }

        _updatingSettings = false;
    }

    private void NavigationOnFocusedChanged (object? sender, EventArgs e)
    {
        if (AutoSelectSuperView is null)
        {
            return;
        }

        if (ApplicationNavigation.IsInHierarchy (this, Application.Navigation!.GetFocused ()))
        {
            return;
        }

        if (!ApplicationNavigation.IsInHierarchy (AutoSelectSuperView, Application.Navigation!.GetFocused ()))
        {
            return;
        }

        ViewToEdit = Application.Navigation!.GetFocused ();
    }

    private void ApplicationOnMouseEvent (object? sender, MouseEventArgs e)
    {
        if (e.Flags != MouseFlags.Button1Clicked || !AutoSelectViewToEdit)
        {
            return;
        }

        if ((AutoSelectSuperView is { } && !AutoSelectSuperView.FrameToScreen ().Contains (e.Position))
            || FrameToScreen ().Contains (e.Position))
        {
            return;
        }

        View? view = e.View;

        if (view is null)
        {
            return;
        }

        if (view is Adornment adornment)
        {
            ViewToEdit = AutoSelectAdornments ? adornment : adornment.Parent;
        }
        else
        {
            ViewToEdit = view;
        }
    }

    /// <inheritdoc/>
    protected override void Dispose (bool disposing)
    {
        base.Dispose (disposing);
    }

    private readonly ExpanderButton? _expandButton;

    public ExpanderButton? ExpandButton => _expandButton;

    public Dimension Dimension { get; set; }

    private void DimEditor_Initialized (object? sender, EventArgs e)
    {
        Border.Add (_expandButton!);

        var label = new Label
        {
            X = 0, Y = 0,
            Text = $"{Title}:"
        };
        Add (label);
        _dimRadioGroup = new () { X = 0, Y = Pos.Bottom (label), RadioLabels = _radioItems };
        _dimRadioGroup.SelectedItemChanged += OnRadioGroupOnSelectedItemChanged;
        _valueEdit = new ()
        {
            X = Pos.Right (label) + 1,
            Y = 0,
            Width = Dim.Func (() => _radioItems.Max (i => i.GetColumns ()) - label.Frame.Width + 1),
            Text = $"{_value}"
        };

        _valueEdit.Accepting += (s, args) =>
        {
            try
            {
                _value = int.Parse (_valueEdit.Text);
                DimChanged ();
            }
            catch
            {
                // ignored
            }
            args.Cancel = true;
        };
        Add (_valueEdit);

        Add (_dimRadioGroup);

        Application.MouseEvent += ApplicationOnMouseEvent;
        Application.Navigation!.FocusedChanged += NavigationOnFocusedChanged;
    }

    private void OnRadioGroupOnSelectedItemChanged (object? s, SelectedItemChangedArgs selected) { DimChanged (); }

    // These need to have same order 
    private readonly List<string> _dimNames = ["Absolute", "Auto", "Fill", "Func", "Percent",];
    private readonly string [] _radioItems = ["Absolute(n)", "Auto", "Fill(n)", "Func(()=>n)", "Percent(n)",];

    private void DimChanged ()
    {
        if (ViewToEdit == null || _updatingSettings)
        {
            return;
        }

        try
        {
            Dim? dim = _dimRadioGroup!.SelectedItem switch
            {
                0 => Dim.Absolute (_value),
                1 => Dim.Auto (),
                2 => Dim.Fill (_value),
                3 => Dim.Func (() => _value),
                4 => Dim.Percent (_value),
                _ => Dimension == Dimension.Width ? ViewToEdit.Width : ViewToEdit.Height
            };

            if (Dimension == Dimension.Width)
            {
                ViewToEdit.Width = dim;
            }
            else
            {
                ViewToEdit.Height = dim;
            }
        }
        catch (Exception e)
        {
            MessageBox.ErrorQuery ("Exception", e.Message, "Ok");
        }
    }
}
