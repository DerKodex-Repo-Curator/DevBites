using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevBites.Models;
using DevBites.Services;
using System.Collections.ObjectModel;

namespace DevBites.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly IAIContentService _aiService;
    private CancellationTokenSource? _loadingCts;
    private bool _isFetchingMore;

    public FeedViewModel(IAIContentService aiService)
    {
        _aiService = aiService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInitialLoading))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private string _selectedTopic = ".NET";

    public ObservableCollection<ProgrammingReel> Reels { get; } = [];

    public List<string> Topics { get; } =
    [
        ".NET", "Python", "OOP", "SOLID", "Design Patterns", "Data Structures"
    ];

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsInitialLoading => IsLoading && Reels.Count == 0;

    partial void OnSelectedTopicChanged(string value)
    {
        _ = StartFreshLoadAsync();
    }

    [RelayCommand]
    private async Task LoadInitialAsync()
    {
        if (IsLoading)
            return;

        await StartFreshLoadAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoading || _isFetchingMore)
            return;

        _isFetchingMore = true;
        try
        {
            await FetchReelsAsync(_loadingCts?.Token ?? CancellationToken.None);
        }
        finally
        {
            _isFetchingMore = false;
        }
    }

    private async Task StartFreshLoadAsync()
    {
        _loadingCts?.Cancel();
        _loadingCts = new CancellationTokenSource();

        Reels.Clear();
        ErrorMessage = null;
        OnPropertyChanged(nameof(IsInitialLoading));

        await FetchReelsAsync(_loadingCts.Token);
    }

    private async Task FetchReelsAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var newReels = await _aiService.GetReelsAsync(SelectedTopic, cancellationToken);
            foreach (var reel in newReels)
                Reels.Add(reel);
        }
        catch (OperationCanceledException)
        {
            // Topic changed — silently discard in-flight results
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not load content: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(IsInitialLoading));
        }
    }
}
