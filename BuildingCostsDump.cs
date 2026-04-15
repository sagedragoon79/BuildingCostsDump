using System;
using System.IO;
using System.Text;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BuildingCostsDumpMod.BuildingCostsDump), "BuildingCostsDump", "1.1.0", "sagedragoon79")]
[assembly: MelonGame("Crate Entertainment", "Farthest Frontier")]

namespace BuildingCostsDumpMod
{
    public class BuildingCostsDump : MelonMod
    {
        private bool dumped = false;

        public override void OnUpdate()
        {
            if (dumped) return;

            try
            {
                var setupData = GlobalAssets.buildingSetupData;
                if (setupData == null || setupData.buildingData == null || setupData.buildingData.Count == 0)
                    return;

                DumpAll(setupData);
                dumped = true;
                MelonLogger.Msg("Building costs + desirability dumped successfully!");
            }
            catch (Exception)
            {
                // Not ready yet, keep trying
            }
        }

        private void DumpAll(BuildingSetupData setupData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Farthest Frontier Building Costs + Desirability Dump ===");
            sb.AppendLine($"Total buildings: {setupData.buildingData.Count}");
            sb.AppendLine($"Dumped: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            sb.AppendLine(string.Format("{0,-32} {1,-22} {2,-5} {3,-5} {4,-6} {5,-9} {6,-7} {7,-7} {8,-5} {9,-7} {10,-15} {11}",
                "Identifier", "Category", "GW", "GH", "Labor", "Relocate", "DesBon", "DesRad", "Drop", "AllMul", "Tag", "Materials"));
            sb.AppendLine(new string('-', 220));

            var csv = new StringBuilder();
            csv.AppendLine("Identifier,Category,GridW,GridH,Labor,RelocateGold,Deconstruct,DesirabilityBonus,DesirabilityRadius,DropoffEnabled,AllowMultiple,Tag,Prerequisites,Materials");

            foreach (var b in setupData.buildingData)
            {
                if (b == null) continue;

                // Cost materials
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

                // Read desirability from prefab Building component
                float desBonus = 0f, desRadius = 0f;
                bool desDropoff = false, desAllowMultiple = false;
                string desTag = "";
                try
                {
                    GameObject prefab = b.placeablePrefab;
                    if (prefab == null && b.prefabEntries != null && b.prefabEntries.Count > 0)
                    {
                        prefab = b.prefabEntries[0]?.PREFAB();
                    }
                    if (prefab != null)
                    {
                        var building = prefab.GetComponent<Building>();
                        if (building == null) building = prefab.GetComponentInChildren<Building>(true);
                        if (building != null)
                        {
                            desBonus = building.strategicPlanningBonus;
                            desRadius = building.strategicPlanningRadius;
                            desDropoff = building.strategicPlanningRadiusDropoff;
                            desAllowMultiple = building.strategicPlanningAllowMultiple;
                            desTag = building.strategicPlanningTagOverride ?? "";
                            if (string.IsNullOrEmpty(desTag)) desTag = building.tag ?? "";
                        }
                    }
                }
                catch { }

                sb.AppendLine(string.Format("{0,-32} {1,-22} {2,-5} {3,-5} {4,-6} {5,-9} {6,7:0.##} {7,7:0.##} {8,-5} {9,-7} {10,-15} {11}",
                    Truncate(b.identifier, 32),
                    Truncate(b.uiBuildCategory.ToString(), 22),
                    (int)b.gridSize.x,
                    (int)b.gridSize.y,
                    b.workRequiredToConstruct,
                    b.goldRequiredToRelocate,
                    desBonus,
                    desRadius,
                    desDropoff ? "yes" : "-",
                    desAllowMultiple ? "yes" : "-",
                    Truncate(desTag, 15),
                    mats.ToString()));

                var prereq = b.prerequisiteIdentifiers != null ? string.Join("|", b.prerequisiteIdentifiers) : "";
                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7:0.##},{8:0.##},{9},{10},\"{11}\",\"{12}\",\"{13}\"",
                    EscapeCsv(b.identifier ?? "?"),
                    b.uiBuildCategory.ToString(),
                    (int)b.gridSize.x,
                    (int)b.gridSize.y,
                    b.workRequiredToConstruct,
                    b.goldRequiredToRelocate,
                    b.workRequiredToDeconstruct,
                    desBonus,
                    desRadius,
                    desDropoff,
                    desAllowMultiple,
                    EscapeCsv(desTag),
                    EscapeCsv(prereq),
                    EscapeCsv(mats.ToString())));
            }

            string userDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData");
            Directory.CreateDirectory(userDataDir);

            string txtPath = Path.Combine(userDataDir, "BuildingCostsDump.txt");
            string csvPath = Path.Combine(userDataDir, "BuildingCostsDump.csv");

            File.WriteAllText(txtPath, sb.ToString());
            File.WriteAllText(csvPath, csv.ToString());

            MelonLogger.Msg($"Wrote: {txtPath}");
            MelonLogger.Msg($"Wrote: {csvPath}");
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }

        private static string EscapeCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\"", "\"\"");
        }
    }
}
