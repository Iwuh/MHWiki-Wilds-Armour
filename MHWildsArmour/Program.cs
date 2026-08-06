using System.CommandLine;
using MHWildsArmour.Json;
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

        static void GenerateLocalFiles(IEnumerable<ArmorSet> sets, string outPath)
        {
            Directory.CreateDirectory(outPath);

            File.WriteAllText(Path.Combine(outPath, "setlistpage.txt"), GenerateArmorListPage(sets));

            var setPages = GenerateArmorSetPages(sets);
            foreach (var (set, page) in sets.Zip(setPages))
            {
                var fileName = $"{set.Series.Name}.txt";
                File.WriteAllText(Path.Combine(outPath, fileName), page);
            }
        }

        static bool PageNeedsEdit(string? newContent, string? currentContent)
        {
            var filt1 = newContent?.ReplaceLineEndings("");
            var filt2 = currentContent?.ReplaceLineEndings("");
            return !string.Equals(filt1, filt2);
        }

        static async Task UpdateWikiPages(IEnumerable<ArmorSet> sets, string user, string pass)
        {
            using var wikiClient = new WikiClient()
            {
                ClientUserAgent = "MHWildsArmor/1.0 (Iwuh)"
            };
            var site = new WikiSite(wikiClient, "https://monsterhunterwiki.org/api.php");
            await site.Initialization;
            try
            {
                await site.LoginAsync(user, pass);
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

        static IEnumerable<ArmorSet> GetData(string dataPath)
        {
            //var blacklist = File.ReadAllLines("blacklist.txt");
            var armor = DataHelpers.GetAllArmorWithSeries(dataPath);
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
                });
                //.Where(s => !blacklist.Contains(s.Series.Name));

            return armorSets;
        }

        static ParseResult Parse(string[] args)
        {
            var rootCommand = new RootCommand("Armour page generator for monsterhunterwiki.org")
            {
                new Option<string>("--data-dir", "-d")
                {
                    Description = "Location of the input data files created by update.py.",
                    Recursive = true,
                    DefaultValueFactory = result => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MHWildsArmour", "data")
                }
            };

            var generateCommand = new Command("generate", "Generate pages for the wiki.");
            var generateLocalCommand = new Command("local", "Create files on the local PC.")
            {
                new Option<string>("--out-dir", "-o")
                {
                    Description = "Directory to create the output files in.",
                    DefaultValueFactory = result => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MHWildsArmour", "out")
                }
            };
            var generateRemoteCommand = new Command("remote", "Update the pages on the wiki.")
            {
                new Option<string>("--user", "-u")
                {
                    Description = "Username to login with.",
                    Required = true
                },
                new Option<string>("--pass", "-p")
                {
                    Description = "Password to login with.",
                    Required = true
                }
            };
            rootCommand.Add(generateCommand);
            generateCommand.Add(generateLocalCommand);
            generateCommand.Add(generateRemoteCommand);

            generateLocalCommand.SetAction(async result =>
            {
                var armorSets = GetData(result.GetRequiredValue<string>("--data-dir"));
                GenerateLocalFiles(armorSets, result.GetRequiredValue<string>("--out-dir"));
                return 0;
            });
            generateRemoteCommand.SetAction(async result =>
            {
                var armorSets = GetData(result.GetRequiredValue<string>("--data-dir"));
                await UpdateWikiPages(armorSets, result.GetRequiredValue<string>("--user"), result.GetRequiredValue<string>("--pass"));
                return 0;
            });

            return rootCommand.Parse(args);
        }

        static async Task Main(string[] args)
        {
            await Parse(args).InvokeAsync();

            // Directory.CreateDirectory("output");

            // var blacklist = File.ReadAllLines("blacklist.txt");
            // var armor = DataHelpers.GetAllArmorWithSeries();
            // var armorSets = armor.GroupBy(d => d.Series)
            //     .Select(g => new ArmorSet()
            //     {
            //         Game = "MHWilds",
            //         Series = g.Key,
            //         HeadPiece = g.FirstOrDefault(d => d.PartsType == "[0]HELM"),
            //         ChestPiece = g.FirstOrDefault(d => d.PartsType == "[1]BODY"),
            //         ArmPiece = g.FirstOrDefault(d => d.PartsType == "[2]ARM"),
            //         WaistPiece = g.FirstOrDefault(d => d.PartsType == "[3]WAIST"),
            //         LegPiece = g.FirstOrDefault(d => d.PartsType == "[4]LEG")
            //     })
            //     .Where(s => !blacklist.Contains(s.Series.Name));

            // File.WriteAllText("armor-combined.json", armor.ToArray().ToJson());

            // GenerateLocalFiles(armorSets);
            // UpdateWikiPages(armorSets).Wait();
            // GenerateWikiJsonDb(armorSets);
        }
    }
}
