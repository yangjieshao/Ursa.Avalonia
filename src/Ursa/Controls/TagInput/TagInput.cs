using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;

namespace Ursa.Controls;

[TemplatePart(PART_ItemsControl, typeof(ItemsControl))]
public class TagInput : TemplatedControl
{
    public const string PART_ItemsControl = "PART_ItemsControl";

    private readonly TextBox _textBox;
    private ItemsControl? _itemsControl;


    public static readonly StyledProperty<IList<string>> TagsProperty =
        AvaloniaProperty.Register<TagInput, IList<string>>(
            nameof(Tags));

    public IList<string> Tags
    {
        get => GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public static readonly DirectProperty<TagInput, IList> ItemsProperty =
        AvaloniaProperty.RegisterDirect<TagInput, IList>(
            nameof(Items), o => o.Items);

    private IList _items;

    public IList Items
    {
        get => _items;
        private set => SetAndRaise(ItemsProperty, ref _items, value);
    }

    public TagInput()
    {
        _textBox = new TextBox();
        _textBox.AddHandler(KeyDownEvent, OnTextBoxKeyDown, RoutingStrategies.Tunnel);
        _textBox.AddHandler(LostFocusEvent, OnTextBox_LostFocus, RoutingStrategies.Bubble);
        Items = new AvaloniaList<object>
        {
            _textBox
        };
        Tags = new ObservableCollection<string>();
    }

    private void OnTextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        switch (LostFocusBehavior)
        {
            case LostFocusBehavior.Add:
                AddTags();
                break;
            case LostFocusBehavior.Clear:
                _textBox.Text = "";
                break;
        }
    }

    public static readonly StyledProperty<ControlTheme> InputThemeProperty =
        AvaloniaProperty.Register<TagInput, ControlTheme>(
            nameof(InputTheme));

    public ControlTheme InputTheme
    {
        get => GetValue(InputThemeProperty);
        set => SetValue(InputThemeProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<TagInput, IDataTemplate?>(
            nameof(ItemTemplate));

    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly StyledProperty<string> SeparatorProperty = AvaloniaProperty.Register<TagInput, string>(
        nameof(Separator));

    public string Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public static readonly StyledProperty<LostFocusBehavior> LostFocusBehaviorProperty = AvaloniaProperty.Register<TagInput, LostFocusBehavior>(
        nameof(LostFocusBehavior));

    public LostFocusBehavior LostFocusBehavior
    {
        get => GetValue(LostFocusBehaviorProperty);
        set => SetValue(LostFocusBehaviorProperty, value);
    }


    public static readonly StyledProperty<bool> AllowDuplicatesProperty = AvaloniaProperty.Register<TagInput, bool>(
        nameof(AllowDuplicates), defaultValue: true);

    public bool AllowDuplicates
    {
        get => GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        AvaloniaProperty.Register<TagInput, object?>(
            nameof(InnerLeftContent));

    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    public static readonly StyledProperty<object?> InnerRightContentProperty =
        AvaloniaProperty.Register<TagInput, object?>(
            nameof(InnerRightContent));

    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }


    static TagInput()
    {
        InputThemeProperty.Changed.AddClassHandler<TagInput>((o, e) => o.OnInputThemePropertyChanged(e));
        TagsProperty.Changed.AddClassHandler<TagInput>((o, e) => o.OnTagsPropertyChanged(e));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsControl = e.NameScope.Find<ItemsControl>(PART_ItemsControl);
    }

    private void OnInputThemePropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var newTheme = args.GetNewValue<ControlTheme>();
        if (newTheme?.TargetType == typeof(TextBox))
        {
            _textBox.Theme = newTheme;
        }
    }

    private void OnTagsPropertyChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var newTags = args.GetNewValue<IList<string>>();
        var oldTags = args.GetOldValue<IList<string>>();
        
        if (Items is AvaloniaList<object> avaloniaList)
        {
            avaloniaList.RemoveRange(0, avaloniaList.Count - 1);
        }
        else if (Items.Count != 0)
        {
            Items.Clear();
            Items.Add(_textBox);
        }
        
        if (newTags != null)
        {
            for (int i = 0; i < newTags.Count; i++)
            {
                Items.Insert(Items.Count - 1, newTags[i]);
            }
        } 
        if (oldTags is INotifyCollectionChanged inccold)
        {
            inccold.CollectionChanged-= OnCollectionChanged;
        }

        if (Tags is INotifyCollectionChanged incc)
        {
            incc.CollectionChanged += OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            var items = e.NewItems;
            int index = e.NewStartingIndex;
            foreach (var item in items)
            {
                if (item is string s)
                {
                    Items.Insert(index, s);
                    index++;
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            var items = e.OldItems;
            int index = e.OldStartingIndex;
            foreach (var item in items)
            {
                if (item is string s)
                {
                    Items.RemoveAt(index);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Items.Clear();
            Items.Add(_textBox);
            InvalidateVisual();
        }

    }

    private void OnTextBoxKeyDown(object? sender, KeyEventArgs args)
    {
        if (args.Key == Key.Enter)
        {
            AddTags();
        }
        else if (args.Key == Key.Delete || args.Key == Key.Back)
        { 
            if (string.IsNullOrEmpty(_textBox.Text)||_textBox.Text?.Length == 0)
            {
                if (Tags.Count == 0)
                {
                    return;
                }
                int index = Items.Count - 2;
                // Items.RemoveAt(index);
                Tags.RemoveAt(index);
            }
        }
    }
    
    private void AddTags()
    {
        if (!(_textBox.Text?.Length > 0)) return;
        string[] values;
        if (!string.IsNullOrEmpty(Separator))
        {
            values = _textBox.Text.Split(new string[] { Separator },
                StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            values = new[] { _textBox.Text };
        }

        if (!AllowDuplicates && Tags != null)
            values = values.Distinct().Except(Tags).ToArray();

        for (int i = 0; i < values.Length; i++)
        {
            int index = Items.Count - 1;
            // Items.Insert(index, values[i]);
            Tags?.Insert(index, values[i]);
        }

        _textBox.Text = "";
    }

    public void Close(object o)
    {
        if (o is Control t)
        {
            if (t.Parent is ContentPresenter presenter)
            {
                int? index = _itemsControl?.IndexFromContainer(presenter);
                if (index is >= 0 && index < Items.Count - 1)
                {
                    // Items.RemoveAt(index.Value);
                    Tags.RemoveAt(index.Value);
                }
            }
        }
    }
}