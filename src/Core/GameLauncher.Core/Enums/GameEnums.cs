namespace GameLauncher.Core.Enums
{
    /// <summary>
    /// Available sort fields for game lists. Used in both Desktop and BigScreen views.
    /// </summary>
    public enum SortField
    {
        Title,
        ReleaseDate,
        LastPlayed,
        PlayCount,
        PlayTime,
        StarRating,
        Developer,
        Genre,
        DateAdded
    }

    /// <summary>
    /// Filter presets for game lists.
    /// </summary>
    public enum GameFilter
    {
        All,
        Favorites,
        Completed,
        Installed,
        RecentlyPlayed,
        NeverPlayed,
        Broken
    }

    /// <summary>
    /// View mode for the BigScreen games page. Wheel = horizontal cover-art strip. List = virtualized text list.
    /// </summary>
    public enum BigScreenViewMode
    {
        Wheel,
        List
    }
}
