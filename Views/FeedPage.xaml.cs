using DevBites.ViewModels;

namespace DevBites.Views;

public partial class FeedPage : ContentPage
{
    private readonly FeedViewModel _viewModel;

    public FeedPage(FeedViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.IsLoading && _viewModel.Reels.Count == 0)
            _viewModel.LoadInitialCommand.Execute(null);
    }
}
