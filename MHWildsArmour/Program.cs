using CsvHelper;
using CsvHelper.Configuration;
using MHWildsArmour.Json;
using System.Globalization;
using WikiClientLibrary;
using WikiClientLibrary.Client;
using WikiClientLibrary.Pages;
using WikiClientLibrary.Sites;

namespace MHWildsArmour
{
    internal class Program
    {
        static string GenerateArmorListPage(IEnumerable<ArmorSet> sets)
        {
            var setList = new ArmorSetList();
            foreach (var set in sets)
            {
                setList.AddSet(set);
            }

            return setList.GeneratePage();
        }

        static IEnumerable<string> GenerateArmorSetPages(IEnumerable<ArmorSet> sets)
        {
            foreach (var set in sets)
            {
                yield return set.GenerateArmorSetPage();
            }
        }

        static void GenerateLocalFiles(IEnumerable<ArmorSet> sets)
        {
            File.WriteAllText(Path.Combine("output", "setlistpage.txt"), GenerateArmorListPage(sets));

            var setPages = GenerateArmorSetPages(sets);
            foreach (var (set, page) in sets.Zip(setPages))
            {
                var fileName = $"{set.Series.Name}.txt";
                File.WriteAllText(Path.Combine("output", fileName), page);
            }
        }

        static bool PageNeedsEdit(string? newContent, string? currentContent)
        {
            var filt1 = newContent?.ReplaceLineEndings("");
            var filt2 = currentContent?.ReplaceLineEndings("");
            return !string.Equals(filt1, filt2);
        }

        static async Task UpdateWikiPages(IEnumerable<ArmorSet> sets)
        {
            using var wikiClient = new WikiClient()
            {
                ClientUserAgent = "MHWildsArmor/1.0 (Iwuh)"
            };
            var site = new WikiSite(wikiClient, "https://monsterhunterwiki.org/api.php");
            await site.Initialization;
            try
            {
                site.LoginAsync("", "").Wait(); // TODO
            }
            catch (WikiClientException ex)
            {
                Console.WriteLine($"Could not log in: {ex.Message}");
                throw;
            }

            var armorListPageContents = GenerateArmorListPage(sets);
            var armorListPage = new WikiPage(site, "MHWilds/Armor");
            await armorListPage.RefreshAsync(PageQueryOptions.FetchContent);
            if (!armorListPage.Exists || PageNeedsEdit(armorListPageContents, armorListPage.Content))
            {
                Console.WriteLine($"Update {armorListPage.Title}");
                await armorListPage.EditAsync(new WikiPageEditOptions()
                {
                    Content = armorListPageContents,
                    Summary = "Automated edit: Update MHWilds armour set list page",
                    Bot = true
                });
            }

            var setListPageNames = sets.Select(s => $"{s.Series.Name} Set (MHWilds)");
            foreach (var (page, contents) in setListPageNames.Zip(GenerateArmorSetPages(sets)))
            {
                var then = DateTime.Now;
                var armorSetPage = new WikiPage(site, page);
                await armorSetPage.RefreshAsync(PageQueryOptions.FetchContent);
                if (!armorSetPage.Exists || PageNeedsEdit(contents, armorSetPage.Content))
                {
                    Console.WriteLine($"Update {armorSetPage.Title}");
                    await armorSetPage.EditAsync(new WikiPageEditOptions()
                    {
                        Content = contents,
                        Summary = "Automated edit: Update MHWilds armour set page",
                        Bot = true
                    });
                }
                var now = DateTime.Now;
                var durationMs = (now - then).TotalMilliseconds;
                if (durationMs < 1500)
                {
                    await Task.Delay(1500 - (int)durationMs);
                }
                
            }
        }

        static void GenerateWikiJsonDb(IEnumerable<ArmorSet> sets)
        {
            var sortedSets = sets.OrderBy(s => s.Series.Rare).ThenBy(s => s.Series.Name);
            int order = 0;
            var armordb = new List<ArmorSetWikiDb>();
            foreach (var set in sortedSets)
            {
                armordb.Add(set.GenerateArmorSetWikiDb(order++));
            }
            File.WriteAllText(Path.Combine("output", "armordb.json"), armordb.ToJson());
        }

        static void Main(string[] args)
        {
            Directory.CreateDirectory("output");

            var blacklist = File.ReadAllLines("blacklist.txt");
            var armor = DataHelpers.GetAllArmorWithSeries();
            var armorSets = armor.GroupBy(d => d.Series)
                .Select(g => new ArmorSet()
                {
                    Game = "MHWilds",
                    Series = g.Key,
                    HeadPiece = g.FirstOrDefault(d => d.PartsType == "[0]HELM"),
                    ChestPiece = g.FirstOrDefault(d => d.PartsType == "[1]BODY"),
                    ArmPiece = g.FirstOrDefault(d => d.PartsType == "[2]ARM"),
                    WaistPiece = g.FirstOrDefault(d => d.PartsType == "[3]WAIST"),
                    LegPiece = g.FirstOrDefault(d => d.PartsType == "[4]LEG")
                })
                .Where(s => !blacklist.Contains(s.Series.Name));

            File.WriteAllText("armor-combined.json", armor.ToArray().ToJson());

            GenerateLocalFiles(armorSets);
            UpdateWikiPages(armorSets).Wait();
            GenerateWikiJsonDb(armorSets);
        }
    }
}
