using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevBites.Models;
using DevBites.Services;
using System.Collections.ObjectModel;

namespace DevBites.ViewModels;

public partial class FeedViewModel : ObservableObject
{
    private readonly IAIContentService _aiService;
    private readonly IFavoritesService _favoritesService;
    private CancellationTokenSource? _loadingCts;
    private bool _isFetchingMore;

    public FeedViewModel(IAIContentService aiService, IFavoritesService favoritesService)
    {
        _aiService = aiService;
        _favoritesService = favoritesService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInitialLoading))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isHeaderCollapsed;

    [ObservableProperty]
    private string _selectedTopic = ".NET";

    [ObservableProperty]
    private string _selectedExpertiseLevel = "Beginner";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFeedVisible))]
    [NotifyPropertyChangedFor(nameof(IsFavoritesVisible))]
    private bool _showFavorites;

    public ObservableCollection<ProgrammingReel> Reels { get; } = [];
    public ObservableCollection<ProgrammingReel> FavoriteReels { get; } = [];

    public List<string> Topics { get; } =
    [
        ".NET", "Python", "OOP", "SOLID", "Design Patterns", "Data Structures"
    ];

    public List<string> ExpertiseLevels { get; } =
    [
        "Beginner", "Medium", "Advanced", "Pro"
    ];

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsInitialLoading => IsLoading && Reels.Count == 0;
    public bool IsFeedVisible => !ShowFavorites;
    public bool IsFavoritesVisible => ShowFavorites;

    partial void OnSelectedTopicChanged(string value)
    {
        _ = StartFreshLoadAsync();
    }

    partial void OnSelectedExpertiseLevelChanged(string value)
    {
        _ = StartFreshLoadAsync();
    }

    [RelayCommand]
    private void ToggleHeader() => IsHeaderCollapsed = !IsHeaderCollapsed;

    [RelayCommand]
    private async Task ShowFeedAsync()
    {
        ShowFavorites = false;
        if (Reels.Count == 0)
            await StartFreshLoadAsync();
    }

    [RelayCommand]
    private async Task ShowFavoritesAsync()
    {
        ShowFavorites = true;
        await LoadFavoritesAsync();
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(ProgrammingReel reel)
    {
        var isFavorite = await _favoritesService.ToggleFavoriteAsync(reel);
        reel.IsFavorite = isFavorite;

        var reelId = _favoritesService.GetReelId(reel);
        var existing = FavoriteReels.FirstOrDefault(x => _favoritesService.GetReelId(x) == reelId);

        if (isFavorite)
        {
            if (existing is null)
                FavoriteReels.Insert(0, CreateCopy(reel, true));
        }
        else if (existing is not null)
        {
            FavoriteReels.Remove(existing);
        }
    }

    [RelayCommand]
    private async Task LoadInitialAsync()
    {
        if (IsLoading)
            return;

        await LoadFavoritesAsync();
        await StartFreshLoadAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsLoading || _isFetchingMore)
            return;

        if (ShowFavorites)
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
            var newReels = await _aiService.GetReelsAsync(SelectedTopic, SelectedExpertiseLevel, cancellationToken);
            var favoriteIds = await _favoritesService.GetFavoriteIdsAsync(cancellationToken);
            foreach (var reel in newReels)
            {
                reel.IsFavorite = favoriteIds.Contains(_favoritesService.GetReelId(reel));
                Reels.Add(reel);
            }
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

    private async Task LoadFavoritesAsync(CancellationToken cancellationToken = default)
    {
        var favorites = await _favoritesService.GetFavoritesAsync(cancellationToken);
        FavoriteReels.Clear();
        foreach (var reel in favorites)
            FavoriteReels.Add(reel);
    }

    private static ProgrammingReel CreateCopy(ProgrammingReel source, bool isFavorite) => new()
    {
        Title = source.Title,
        Hook = source.Hook,
        Explanation = source.Explanation,
        CodeExample = source.CodeExample,
        KeyTakeaway = source.KeyTakeaway,
        Language = source.Language,
        IsFavorite = isFavorite
    };
}
