using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;

namespace GameLauncher.Infrastructure.Services
{
    /// <summary>
    /// Manages platform operations.
    /// </summary>
    public class PlatformManager : IPlatformManager
    {
        private readonly XmlDataContext _dataContext;
        private readonly GameCacheManager? _cacheManager;
        private readonly IStatisticsTracker _statisticsTracker;

        public PlatformManager(
            XmlDataContext dataContext,
            IStatisticsTracker statisticsTracker,
            GameCacheManager? cacheManager = null)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _statisticsTracker = statisticsTracker ?? throw new ArgumentNullException(nameof(statisticsTracker));
            _cacheManager = cacheManager;
        }

        public async Task<List<Platform>> GetAllPlatformsAsync()
        {
            return await Task.FromResult(new List<Platform>(GetPlatforms()));
        }

        public async Task<Dictionary<string, List<Platform>>> GetPlatformsByCategoryAsync()
        {
            var platforms = GetPlatforms();

            var grouped = platforms
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Category) ? "Uncategorized" : p.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            return await Task.FromResult(grouped);
        }

        public async Task<Dictionary<string, List<Platform>>> GetPlatformsByParentCategoryAsync()
        {
            var platforms = GetPlatforms();
            var parents = _dataContext.LoadParents();

            // Build a lookup: platform name -> parent category name
            var platformToCategory = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var parent in parents)
            {
                if (!string.IsNullOrWhiteSpace(parent.PlatformName) &&
                    !string.IsNullOrWhiteSpace(parent.ParentPlatformCategoryName))
                {
                    platformToCategory[parent.PlatformName] = parent.ParentPlatformCategoryName;
                }
            }

            var grouped = new Dictionary<string, List<Platform>>();
            foreach (var platform in platforms)
            {
                if (string.IsNullOrEmpty(platform.Name))
                    continue;

                string category = platformToCategory.TryGetValue(platform.Name, out var cat)
                    ? cat
                    : "Sin Categoría";

                if (!grouped.ContainsKey(category))
                    grouped[category] = new List<Platform>();

                grouped[category].Add(platform);
            }

            // Sort platforms within each category
            foreach (var key in grouped.Keys.ToList())
            {
                grouped[key] = grouped[key].OrderBy(p => p.Name).ToList();
            }

            return await Task.FromResult(grouped);
        }

        public async Task<List<NavigationNode>> GetNavigationTreeAsync()
        {
            var platforms = GetPlatforms();
            var parents = _dataContext.LoadParents();
            var categories = _dataContext.LoadPlatformCategories();
            var playlists = _dataContext.LoadAllPlaylists();

            // Build lookup maps
            var platformByName = platforms
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var categoryByName = categories
                .Where(c => !string.IsNullOrEmpty(c.Name))
                .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var playlistById = playlists
                .Where(p => !string.IsNullOrEmpty(p.PlaylistId))
                .GroupBy(p => p.PlaylistId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // Build navigation nodes for all categories
            var categoryNodes = new Dictionary<string, NavigationNode>(StringComparer.OrdinalIgnoreCase);
            foreach (var cat in categories)
            {
                if (string.IsNullOrEmpty(cat.Name)) continue;
                if (categoryNodes.ContainsKey(cat.Name)) continue;
                categoryNodes[cat.Name] = new NavigationNode
                {
                    Name = cat.Name,
                    NodeType = NavigationNodeType.Category,
                    PlatformCategory = cat
                };
            }

            // Also ensure categories referenced as parents in Parents.xml exist as nodes
            foreach (var parent in parents)
            {
                if (!string.IsNullOrWhiteSpace(parent.ParentPlatformCategoryName) &&
                    !categoryNodes.ContainsKey(parent.ParentPlatformCategoryName))
                {
                    categoryNodes[parent.ParentPlatformCategoryName] = new NavigationNode
                    {
                        Name = parent.ParentPlatformCategoryName,
                        NodeType = NavigationNodeType.Category
                    };
                }
                if (!string.IsNullOrWhiteSpace(parent.PlatformCategoryName) &&
                    !categoryNodes.ContainsKey(parent.PlatformCategoryName))
                {
                    categoryNodes[parent.PlatformCategoryName] = new NavigationNode
                    {
                        Name = parent.PlatformCategoryName,
                        NodeType = NavigationNodeType.Category
                    };
                }
            }

            // Track which nodes are children (not roots)
            var childNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Process Parent entries to build hierarchy
            foreach (var parent in parents)
            {
                var parentCategoryName = parent.ParentPlatformCategoryName;
                if (string.IsNullOrWhiteSpace(parentCategoryName)) continue;

                if (!categoryNodes.TryGetValue(parentCategoryName, out var parentNode))
                    continue;

                // Category → Parent Category
                if (!string.IsNullOrWhiteSpace(parent.PlatformCategoryName))
                {
                    if (categoryNodes.TryGetValue(parent.PlatformCategoryName, out var childCatNode))
                    {
                        if (!parentNode.Children.Any(c =>
                            c.NodeType == NavigationNodeType.Category &&
                            c.Name.Equals(childCatNode.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            parentNode.Children.Add(childCatNode);
                        }
                        childNames.Add(parent.PlatformCategoryName);
                    }
                }
                // Platform → Parent Category
                else if (!string.IsNullOrWhiteSpace(parent.PlatformName))
                {
                    platformByName.TryGetValue(parent.PlatformName, out var platform);
                    if (!parentNode.Children.Any(c =>
                        c.NodeType == NavigationNodeType.Platform &&
                        c.Name.Equals(parent.PlatformName, StringComparison.OrdinalIgnoreCase)))
                    {
                        parentNode.Children.Add(new NavigationNode
                        {
                            Name = parent.PlatformName,
                            NodeType = NavigationNodeType.Platform,
                            Platform = platform
                        });
                    }
                }
                // Playlist → Parent Category
                else if (!string.IsNullOrWhiteSpace(parent.PlaylistId))
                {
                    playlistById.TryGetValue(parent.PlaylistId, out var playlist);
                    var playlistName = playlist?.Name ?? parent.PlaylistId;
                    if (!parentNode.Children.Any(c =>
                        c.NodeType == NavigationNodeType.Playlist &&
                        c.PlaylistId == parent.PlaylistId))
                    {
                        parentNode.Children.Add(new NavigationNode
                        {
                            Name = playlistName,
                            NodeType = NavigationNodeType.Playlist,
                            Playlist = playlist,
                            PlaylistId = parent.PlaylistId
                        });
                    }
                }
            }

            // Root nodes = category nodes that are NOT children of any other category
            var roots = categoryNodes.Values
                .Where(n => !childNames.Contains(n.Name))
                .OrderBy(n => n.Name)
                .ToList();

            // Collect all platform names already in the tree
            var assignedPlatforms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectPlatformNames(roots, assignedPlatforms);

            // Add unassigned platforms to a "Sin Categoría" root node
            var unassigned = platforms
                .Where(p => !string.IsNullOrEmpty(p.Name) && !assignedPlatforms.Contains(p.Name))
                .OrderBy(p => p.Name)
                .ToList();

            if (unassigned.Count > 0)
            {
                var sinCategoriaNode = new NavigationNode
                {
                    Name = "Sin Categoría",
                    NodeType = NavigationNodeType.Category
                };
                foreach (var platform in unassigned)
                {
                    sinCategoriaNode.Children.Add(new NavigationNode
                    {
                        Name = platform.Name,
                        NodeType = NavigationNodeType.Platform,
                        Platform = platform
                    });
                }
                roots.Add(sinCategoriaNode);
            }

            // Sort children within each node
            SortChildrenRecursive(roots);

            return await Task.FromResult(roots);
        }

        private void SortChildrenRecursive(List<NavigationNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Children.Count > 0)
                {
                    node.Children = node.Children
                        .OrderBy(c => c.NodeType) // Categories first, then Platforms, then Playlists
                        .ThenBy(c => c.Name)
                        .ToList();
                    SortChildrenRecursive(node.Children);
                }
            }
        }

        private void CollectPlatformNames(List<NavigationNode> nodes, HashSet<string> names)
        {
            foreach (var node in nodes)
            {
                if (node.NodeType == NavigationNodeType.Platform && !string.IsNullOrEmpty(node.Name))
                    names.Add(node.Name);
                if (node.Children.Count > 0)
                    CollectPlatformNames(node.Children, names);
            }
        }

        public async Task<Platform?> GetPlatformByNameAsync(string platformName)
        {
            if (string.IsNullOrWhiteSpace(platformName))
                throw new ArgumentException("Platform name cannot be null or empty", nameof(platformName));

            var platforms = GetPlatforms();
            var platform = platforms.FirstOrDefault(p =>
                p.Name.Equals(platformName, StringComparison.OrdinalIgnoreCase));

            return await Task.FromResult(platform);
        }

        public async Task<PlatformStatistics> GetPlatformStatisticsAsync(string platformName)
        {
            return await _statisticsTracker.GetPlatformStatisticsAsync(platformName);
        }

        private List<Platform> GetPlatforms()
        {
            return _cacheManager != null
                ? _cacheManager.GetPlatforms()
                : _dataContext.LoadPlatforms();
        }
    }
}
