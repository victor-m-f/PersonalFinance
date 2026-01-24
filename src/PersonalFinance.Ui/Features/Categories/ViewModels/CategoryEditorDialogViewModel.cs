using System.ComponentModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PersonalFinance.Shared.Constraints;
using PersonalFinance.Ui.Features.Categories.Models;
using Wpf.Ui.Controls;

namespace PersonalFinance.Ui.Features.Categories.ViewModels;

public sealed class CategoryEditorDialogViewModel : ObservableObject, IDataErrorInfo
{
    private static readonly Regex ColorHexRegex = new(EntityConstraints.Category.ColorHexRegex, RegexOptions.Compiled);

    private Guid? _id;
    private string _name = string.Empty;
    private string _colorHex = string.Empty;
    private Guid? _parentId;
    private bool _isEditMode;
    private IReadOnlyList<CategoryLookupItemVm> _parentOptions = Array.Empty<CategoryLookupItemVm>();
    private bool _isValid;
    private bool _suppressValidation;
    private bool _isNameTouched;
    private bool _isColorTouched;
    private string _parentSearchText = string.Empty;

    public IRelayCommand NameLostFocusCommand { get; }
    public IRelayCommand ColorLostFocusCommand { get; }
    public IRelayCommand<AutoSuggestBoxSuggestionChosenEventArgs> ParentSuggestionChosenCommand { get; }
    public IRelayCommand<AutoSuggestBoxTextChangedEventArgs> ParentTextChangedCommand { get; }

    public Guid? Id
    {
        get => _id;
        private set => SetProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                if (!_suppressValidation)
                {
                    _isNameTouched = true;
                }

                UpdateValidationState();
            }
        }
    }

    public string ColorHex
    {
        get => _colorHex;
        set
        {
            var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;
            if (SetProperty(ref _colorHex, normalized))
            {
                if (!_suppressValidation)
                {
                    _isColorTouched = true;
                }

                UpdateValidationState();
            }
        }
    }

    public Guid? ParentId
    {
        get => _parentId;
        set => SetProperty(ref _parentId, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            SetProperty(ref _isEditMode, value);
        }
    }

    public IReadOnlyList<CategoryLookupItemVm> ParentOptions
    {
        get => _parentOptions;
        private set => SetProperty(ref _parentOptions, value);
    }

    public string ParentSearchText
    {
        get => _parentSearchText;
        set
        {
            if (SetProperty(ref _parentSearchText, value))
            {
                if (!_suppressValidation && string.IsNullOrWhiteSpace(_parentSearchText))
                {
                    ParentId = null;
                }
            }
        }
    }

    public bool IsValid
    {
        get => _isValid;
        private set => SetProperty(ref _isValid, value);
    }

    public string Error => string.Empty;

    public string this[string columnName] => columnName switch
    {
        nameof(Name) => _isNameTouched ? ValidateName() : string.Empty,
        nameof(ColorHex) => _isColorTouched ? ValidateColorHex() : string.Empty,
        _ => string.Empty
    };

    public void Initialize(CategoryEditorDialogOptions options)
    {
        _suppressValidation = true;

        Id = options.Id;
        IsEditMode = options.IsEditMode;
        Name = options.Name;
        ColorHex = string.IsNullOrWhiteSpace(options.ColorHex) ? "#FF3B82F6" : options.ColorHex;
        ParentId = options.ParentId;
        ParentOptions = options.ParentOptions;
        ParentSearchText = ResolveParentSearchText(options.ParentId, options.ParentOptions);

        _isNameTouched = false;
        _isColorTouched = false;
        _suppressValidation = false;
        UpdateValidationState();
    }

    public CategoryEditorDialogViewModel()
    {
        NameLostFocusCommand = new RelayCommand(OnNameLostFocus);
        ColorLostFocusCommand = new RelayCommand(OnColorLostFocus);
        ParentSuggestionChosenCommand = new RelayCommand<AutoSuggestBoxSuggestionChosenEventArgs>(OnParentSuggestionChosen);
        ParentTextChangedCommand = new RelayCommand<AutoSuggestBoxTextChangedEventArgs>(OnParentTextChanged);
    }

    public CategoryEditorResult BuildResult()
    {
        return new CategoryEditorResult
        {
            Id = Id,
            Name = Name.Trim(),
            ColorHex = ColorHex.Trim(),
            ParentId = string.IsNullOrWhiteSpace(ParentSearchText) ? null : ParentId,
            IsEditMode = IsEditMode
        };
    }

    private void UpdateValidationState()
    {
        IsValid = string.IsNullOrEmpty(ValidateName()) && string.IsNullOrEmpty(ValidateColorHex());
    }

    private void OnNameLostFocus()
    {
        _isNameTouched = true;
        OnPropertyChanged(nameof(Name));
        UpdateValidationState();
    }

    private void OnColorLostFocus()
    {
        _isColorTouched = true;
        OnPropertyChanged(nameof(ColorHex));
        UpdateValidationState();
    }

    private void OnParentSuggestionChosen(AutoSuggestBoxSuggestionChosenEventArgs? args)
    {
        if (args?.SelectedItem is not CategoryLookupItemVm option)
        {
            return;
        }

        ParentId = option.Id;
        ParentSearchText = option.DisplayName;
    }

    private void OnParentTextChanged(AutoSuggestBoxTextChangedEventArgs? args)
    {
        if (args is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(args.Text))
        {
            ParentId = null;
        }
    }

    private static string ResolveParentSearchText(Guid? parentId, IReadOnlyList<CategoryLookupItemVm> options)
    {
        if (parentId is null)
        {
            return string.Empty;
        }

        var selected = options.FirstOrDefault(option => option.Id == parentId);
        return selected?.DisplayName ?? string.Empty;
    }

    private string ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return "Name is required.";
        }

        var trimmed = Name.Trim();
        if (trimmed.Length < EntityConstraints.Category.NameMinLength || trimmed.Length > EntityConstraints.Category.NameMaxLength)
        {
            return $"Name must be between {EntityConstraints.Category.NameMinLength} and {EntityConstraints.Category.NameMaxLength} characters.";
        }

        return string.Empty;
    }

    private string ValidateColorHex()
    {
        if (string.IsNullOrWhiteSpace(ColorHex))
        {
            return "Color is required.";
        }

        if (!ColorHexRegex.IsMatch(ColorHex))
        {
            return "Color must be in format #AARRGGBB.";
        }

        return string.Empty;
    }
}
