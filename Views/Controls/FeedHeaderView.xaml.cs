using System.ComponentModel;
using DevBites.ViewModels;

namespace DevBites.Views.Controls;

public partial class FeedHeaderView : ContentView
{
    private FeedViewModel? _vm;

    public FeedHeaderView()
    {
        InitializeComponent();
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (_vm is not null)
            _vm.PropertyChanged -= OnViewModelPropertyChanged;

        _vm = BindingContext as FeedViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FeedViewModel.IsHeaderCollapsed))
            await AnimatePickersAsync(_vm!.IsHeaderCollapsed);
    }

    private async Task AnimatePickersAsync(bool collapse)
    {
        if (collapse)
        {
            await Task.WhenAll(
                PickersSection.FadeTo(0, 180, Easing.CubicIn),
                PickersSection.TranslateTo(0, -8, 180, Easing.CubicIn),
                ChevronLabel.RotateTo(180, 200, Easing.CubicInOut)
            );
            PickersSection.IsVisible = false;
            PickersSection.Opacity = 1;
            PickersSection.TranslationY = 0;
        }
        else
        {
            PickersSection.IsVisible = true;
            PickersSection.Opacity = 0;
            PickersSection.TranslationY = -8;
            await Task.WhenAll(
                PickersSection.FadeTo(1, 180, Easing.CubicOut),
                PickersSection.TranslateTo(0, 0, 180, Easing.CubicOut),
                ChevronLabel.RotateTo(0, 200, Easing.CubicInOut)
            );
        }
    }
}
