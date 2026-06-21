using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Shared._Void.PriorityConsole;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client._Void.PriorityConsole;

public sealed class PriorityConsoleMenu : FancyWindow
{
    private readonly IPrototypeManager _prototype;
    private readonly IConfigurationManager _cfg;
    private readonly SpriteSystem _sprite;

    private readonly BoxContainer _list;
    private readonly BoxContainer _colorPanel;
    private readonly ColorSelectorSliders _colorSliders;

    private List<PriorityDepartmentInfo> _departments = new();
    private bool _updatingColor;

    public event Action<(ProtoId<DepartmentPrototype>, bool)>? OnSetAuto;
    public event Action<(ProtoId<JobPrototype>, int)>? OnSetRequired;

    public PriorityConsoleMenu()
    {
        MinSize = SetSize = new Vector2(460, 600);
        _prototype = IoCManager.Resolve<IPrototypeManager>();
        _cfg = IoCManager.Resolve<IConfigurationManager>();
        _sprite = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<SpriteSystem>();

        Title = Loc.GetString("priority-console-menu-title");

        var root = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            VerticalExpand = true,
            Margin = new Thickness(6, 4),
        };

        _colorSliders = new ColorSelectorSliders
        {
            IsAlphaVisible = false,
            Color = GetHighlightColor(),
        };
        _colorSliders.OnColorChanged += OnColorPicked;

        _colorPanel = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Visible = false,
            Margin = new Thickness(0, 0, 0, 4),
            Children =
            {
                new PanelContainer
                {
                    PanelOverride = new StyleBoxFlat { BackgroundColor = new Color(0.12f, 0.12f, 0.14f) },
                    Children = { _colorSliders },
                },
            },
        };

        var colorButton = new Button
        {
            Text = Loc.GetString("priority-console-menu-color"),
        };
        colorButton.OnPressed += _ => _colorPanel.Visible = !_colorPanel.Visible;

        var buttonRow = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Children =
            {
                new Control { HorizontalExpand = true },
                colorButton,
            },
        };

        var subtitle = new Label
        {
            Text = Loc.GetString("priority-console-menu-subtitle"),
            StyleClasses = { "LabelSubText" },
            Margin = new Thickness(0, 4, 0, 6),
        };

        root.AddChild(buttonRow);
        root.AddChild(subtitle);
        root.AddChild(_colorPanel);

        _list = new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            VerticalExpand = true,
        };
        root.AddChild(new ScrollContainer
        {
            VerticalExpand = true,
            HorizontalExpand = true,
            Children = { _list },
        });

        AddChild(root);
    }

    public void UpdateState(List<PriorityDepartmentInfo> departments)
    {
        _departments = departments;
        Rebuild();
    }

    private void OnColorPicked(Color color)
    {
        if (_updatingColor)
            return;

        var clamped = ClampColor(color);
        _cfg.SetCVar(CCVars.VoidPriorityHighlightColor, clamped.ToHex());

        if (clamped != color)
        {
            _updatingColor = true;
            _colorSliders.Color = clamped;
            _updatingColor = false;
        }

        Rebuild();
    }

    private Color GetHighlightColor()
    {
        return ClampColor(Color.TryFromHex(_cfg.GetCVar(CCVars.VoidPriorityHighlightColor)) ?? Color.LimeGreen);
    }

    // Keep highlight colors muted so they can't be made eye-searing.
    private static Color ClampColor(Color color)
    {
        var hsv = Color.ToHsv(color);
        hsv.Y = Math.Min(hsv.Y, 0.55f);
        hsv.Z = Math.Clamp(hsv.Z, 0.30f, 0.75f);
        hsv.W = 1f;
        return Color.FromHsv(hsv);
    }

    private static Color ContrastText(Color background)
    {
        var luminance = 0.299f * background.R + 0.587f * background.G + 0.114f * background.B;
        return luminance > 0.55f ? Color.Black : Color.White;
    }

    private void Rebuild()
    {
        _list.RemoveAllChildren();
        var highlightColor = GetHighlightColor();

        foreach (var department in _departments)
        {
            if (!_prototype.TryIndex(department.Department, out var departmentProto))
                continue;

            _list.AddChild(MakeDepartmentHeader(department, departmentProto));

            var rowIndex = 0;
            foreach (var job in department.Jobs)
            {
                if (!_prototype.TryIndex(job.Job, out var jobProto))
                    continue;

                _list.AddChild(MakeJobRow(department, job, jobProto, highlightColor, rowIndex++));
            }

            _list.AddChild(new Control { MinSize = new Vector2(0, 8) });
        }
    }

    private Control MakeDepartmentHeader(PriorityDepartmentInfo department, DepartmentPrototype proto)
    {
        var accent = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat { BackgroundColor = proto.Color },
            MinSize = new Vector2(4, 0),
            Margin = new Thickness(0, 0, 6, 0),
        };

        var label = new Label
        {
            Text = Loc.GetString(proto.Name),
            StyleClasses = { "LabelBig" },
            HorizontalExpand = true,
            ClipText = true,
            VerticalAlignment = VAlignment.Center,
        };

        var autoCheck = new CheckBox
        {
            Text = Loc.GetString("priority-console-menu-auto"),
            Pressed = department.Auto,
            Margin = new Thickness(0, 0, 6, 0),
        };
        var dept = department.Department;
        autoCheck.OnToggled += args => OnSetAuto?.Invoke((dept, args.Pressed));

        var row = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
            Children = { accent, label, autoCheck },
        };

        return new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = proto.Color.WithAlpha(0.33f),
                ContentMarginTopOverride = 4,
                ContentMarginBottomOverride = 4,
            },
            Margin = new Thickness(0, 6, 0, 3),
            Children = { row },
        };
    }

    private Control MakeJobRow(PriorityDepartmentInfo department, PriorityJobInfo job, JobPrototype proto, Color highlightColor, int rowIndex)
    {
        var row = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalExpand = true,
        };

        var icon = new TextureRect
        {
            TextureScale = new Vector2(2.5f, 2.5f),
            MinSize = new Vector2(36, 36),
            Stretch = TextureRect.StretchMode.KeepCentered,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(4, 0, 8, 0),
        };
        if (_prototype.TryIndex(proto.Icon, out var jobIcon))
            icon.Texture = _sprite.Frame0(jobIcon.Icon);
        row.AddChild(icon);

        var nameLabel = new Label
        {
            Text = proto.LocalizedName,
            HorizontalExpand = true,
            ClipText = true,
            VerticalAlignment = VAlignment.Center,
        };
        row.AddChild(nameLabel);

        var slotsText = job.Slots?.ToString() ?? "∞";
        var statusLabel = new Label
        {
            Text = Loc.GetString("priority-console-menu-status", ("current", job.Current), ("slots", slotsText)),
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            StyleClasses = { "LabelSubText" },
        };
        row.AddChild(statusLabel);

        var spin = new SpinBox
        {
            MinWidth = 96,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0),
        };
        var cap = job.Slots;
        spin.IsValid = value => value >= 0 && (cap == null || value <= cap.Value);
        spin.InitDefaultButtons();
        spin.OverrideValue(job.Required);
        spin.LineEditDisabled = department.Auto;
        spin.SetButtonDisabled(department.Auto);

        var jobId = job.Job;
        spin.ValueChanged += args => OnSetRequired?.Invoke((jobId, args.Value));
        row.AddChild(spin);

        Color background;
        if (job.Highlighted)
        {
            background = highlightColor;
            var text = ContrastText(highlightColor);
            nameLabel.FontColorOverride = text;
            statusLabel.FontColorOverride = text.WithAlpha(0.85f);
        }
        else
        {
            background = rowIndex % 2 == 0 ? new Color(1f, 1f, 1f, 0.04f) : Color.Transparent;
        }

        return new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = background,
                ContentMarginTopOverride = 3,
                ContentMarginBottomOverride = 3,
                ContentMarginLeftOverride = 4,
                ContentMarginRightOverride = 4,
            },
            Margin = new Thickness(0, 1),
            Children = { row },
        };
    }
}
