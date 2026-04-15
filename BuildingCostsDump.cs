using System;
using System.IO;
using System.Text;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BuildingCostsDumpMod.BuildingCostsDump), "BuildingCostsDump", "1.0.0", "sagedragoon79")]
[assembly: MelonGame("Crate Entertainment", "Farthest Frontier")]

namespace BuildingCostsDumpMod
{
    public class BuildingCostsDump : MelonMod
    {
        private bool dumped = false;

        public override void OnUpdate()
        {
            // Wait a few frames for GlobalAssets to initialize, then dump
            if (dumped) return;

            try
            {
                var setupData = GlobalAssets.buildingSetupData;
                if (setupData == null || setupData.buildingData == null || setupData.buildingData.Count == 0)
                    return;

                DumpAll(setupData);
                dumped = true;
                MelonLogger.Msg("Building costs dumped successfully!");
            }
            catch (Exception ex)
            {
                // Not ready yet, keep trying
            }
        }

        private void DumpAll(BuildingSetupData setupData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Farthest Frontier Building Costs Dump ===");
            sb.AppendLine($"Total buildings: {setupData.buildingData.Count}");
            sb.AppendLine($"Dumped: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine(string.Format("{0,-35} {1,-30} {2,-8} {3,-8} {4,-8} {5,-10} {6}",
                "Identifier", "Category", "GridW", "GridH", "Labor", "Relocate", "Materials"));
            sb.AppendLine(new string('-', 160));

            foreach (var b in setupData.buildingData)
            {
                if (b == null) continue;

                var mats = new StringBuilder();
                if (b.buildingMaterials != null)
                {
                    for (int i = 0; i < b.buildingMaterials.Count; i++)
                    {
                        if (i > 0) mats.Append(", ");
                        var m = b.buildingMaterials[i];
                        if (m != null) mats.Append($"{m.quantity} {m.item}");
                    }
                }

                sb.AppendLine(string.Format("{0,-35} {1,-30} {2,-8} {3,-8} {4,-8} {5,-10} {6}",
                    b.identifier ?? "?",
                    b.uiBuildCategory.ToString(),
                    (int)b.gridSize.x,
                    (int)b.gridSize.y,
                    b.workRequiredToConstruct,
                    b.goldRequiredToRelocate,
                    mats.ToString()));
            }

            // Also dump as CSV for easy spreadsheet import
            var csv = new StringBuilder();
            csv.AppendLine("Identifier,Category,GridW,GridH,Labor,RelocateGold,Deconstruct,Prerequisites,Materials");
            foreach (var b in setupData.buildingData)
            {
                if (b == null) continue;
                var mats = new StringBuilder();
                if (b.buildingMaterials != null)
                {
                    for (int i = 0; i < b.buildingMaterials.Count; i++)
                    {
                        if (i > 0) mats.Append("; ");
                        var m = b.buildingMaterials[i];
                        if (m != null) mats.Append($"{m.quantity} {m.item}");
                    }
                }
                var prereq = b.prerequisiteIdentifiers != null ? string.Join("|", b.prerequisiteIdentifiers) : "";
                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},\"{7}\",\"{8}\"",
                    b.identifier ?? "?",
                    b.uiBuildCategory.ToString(),
                    (int)b.gridSize.x,
                    (int)b.gridSize.y,
                    b.workRequiredToConstruct,
                    b.goldRequiredToRelocate,
                    b.workRequiredToDeconstruct,
                    prereq,
                    mats.ToString()));
            }

            // UserData is always alongside the game exe
            string userDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");
            Directory.CreateDirectory(userDataDir);

            string txtPath = Path.Combine(userDataDir, "BuildingCostsDump.txt");
            string csvPath = Path.Combine(userDataDir, "BuildingCostsDump.csv");

            File.WriteAllText(txtPath, sb.ToString());
            File.WriteAllText(csvPath, csv.ToString());

            MelonLogger.Msg($"Wrote: {txtPath}");
            MelonLogger.Msg($"Wrote: {csvPath}");
        }
    }
}
