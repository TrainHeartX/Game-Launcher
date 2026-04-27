using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLauncher.Core.Models;
using GameLauncher.Data.Xml;
using GameLauncher.Infrastructure.Services;

namespace TreeDiagnostic;

class Program
{
    static async Task Main(string[] args)
    {
        const string launchBoxPath = @"H:\LaunchBox\LaunchBox";

        Console.WriteLine("=== TreeDiagnostic: Navigation Tree Builder Test ===");
        Console.WriteLine($"LaunchBox path: {launchBoxPath}");
        Console.WriteLine();

        try
        {
            var dataContext = new XmlDataContext(launchBoxPath);

            // 1. Platform Categories
            Console.WriteLine("--- Platform Categories ---");
            var categories = dataContext.LoadPlatformCategories();
            Console.WriteLine($"Count: {categories.Count}");
            Console.WriteLine("First 5:");
            foreach (var cat in categories.Take(5))
            {
                Console.WriteLine($"  - {cat.Name}");
            }
            Console.WriteLine();

            // 2. Parents
            Console.WriteLine("--- Parents (Hierarchy Relationships) ---");
            var parents = dataContext.LoadParents();
            Console.WriteLine($"Count: {parents.Count}");

            int catToCat = parents.Count(p =>
                !string.IsNullOrWhiteSpace(p.PlatformCategoryName) &&
                !string.IsNullOrWhiteSpace(p.ParentPlatformCategoryName));
            int platToCat = parents.Count(p =>
                !string.IsNullOrWhiteSpace(p.PlatformName) &&
                !string.IsNullOrWhiteSpace(p.ParentPlatformCategoryName));
            int playToCat = parents.Count(p =>
                !string.IsNullOrWhiteSpace(p.PlaylistId) &&
                !string.IsNullOrWhiteSpace(p.ParentPlatformCategoryName));

            Console.WriteLine($"  Category -> Category:  {catToCat}");
            Console.WriteLine($"  Platform -> Category:  {platToCat}");
            Console.WriteLine($"  Playlist -> Category:  {playToCat}");
            Console.WriteLine();

            // 3. Platforms
            Console.WriteLine("--- Platforms ---");
            var platforms = dataContext.LoadPlatforms();
            Console.WriteLine($"Count: {platforms.Count}");
            Console.WriteLine();

            // 4. Navigation Tree
            Console.WriteLine("--- Navigation Tree ---");
            var statisticsTracker = new StatisticsTracker(dataContext);
            var platformManager = new PlatformManager(dataContext, statisticsTracker);
            var tree = await platformManager.GetNavigationTreeAsync();

            Console.WriteLine($"Root nodes: {tree.Count}");
            Console.WriteLine();

            foreach (var root in tree)
            {
                Console.WriteLine($"  [{root.NodeType}] \"{root.Name}\" - {root.Children.Count} children");
                // Print 2 levels deep
                foreach (var child in root.Children.Take(10))
                {
                    Console.WriteLine($"    [{child.NodeType}] \"{child.Name}\" - {child.Children.Count} children");
                }
                if (root.Children.Count > 10)
                    Console.WriteLine($"    ... and {root.Children.Count - 10} more");
            }
            Console.WriteLine();

            // 5. Recursive count of nodes by type
            int totalPlatformNodes = CountNodesByType(tree, NavigationNodeType.Platform);
            int totalCategoryNodes = CountNodesByType(tree, NavigationNodeType.Category);
            int totalPlaylistNodes = CountNodesByType(tree, NavigationNodeType.Playlist);
            Console.WriteLine($"Total Platform nodes in tree (recursive): {totalPlatformNodes}");
            Console.WriteLine($"Total Category nodes in tree (recursive): {totalCategoryNodes}");
            Console.WriteLine($"Total Playlist nodes in tree (recursive): {totalPlaylistNodes}");
            Console.WriteLine();

            Console.WriteLine("=== Done ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

    static int CountNodesByType(List<NavigationNode> nodes, NavigationNodeType nodeType)
    {
        int count = 0;
        foreach (var node in nodes)
        {
            if (node.NodeType == nodeType)
                count++;
            count += CountNodesByType(node.Children, nodeType);
        }
        return count;
    }
}
