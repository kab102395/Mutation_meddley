using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation
{
    public class MutationMeddley_SynergyDefinition
    {
        public string Id;
        public string Title;
        public string Summary;
        public string DetailText;
        public bool IsUnusual;

        public MutationMeddley_SynergyDefinition(
            string id,
            string title,
            string summary,
            string detailText = "",
            bool isUnusual = false)
        {
            Id = id;
            Title = title;
            Summary = summary;
            DetailText = detailText ?? "";
            IsUnusual = isUnusual;
        }
    }

    public static class MutationMeddley_TagRegistry
    {
        private static readonly Dictionary<string, string[]> MutationMeddley_VanillaTags =
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { "Carapace", new string[] { "BIOLOGICAL", "STRUCTURAL", "CHITINOUS", "BODY_PART_INTERACTION" } },
                { "Regeneration", new string[] { "BIOLOGICAL", "REGENERATIVE", "METABOLIC" } },
                { "Multiple Legs", new string[] { "BIOLOGICAL", "MOBILE", "PURSUIT", "BODY_PART_INTERACTION" } },
                { "Quills", new string[] { "BIOLOGICAL", "STRUCTURAL", "RETALIATORY", "BODY_PART_INTERACTION" } },
                { "Electrical Generation", new string[] { "ELECTRICAL", "METABOLIC" } },
                { "Light Manipulation", new string[] { "RADIANT", "LIGHT_INTERACTION" } },
                { "Flaming Ray", new string[] { "THERMAL", "RADIANT" } },
                { "Freezing Ray", new string[] { "CRYOGENIC", "ENVIRONMENTAL" } },
                { "Photosynthetic Skin", new string[] { "BIOLOGICAL", "RADIANT", "METABOLIC" } },
                { "Phasing", new string[] { "PHASED", "DIMENSIONAL", "MOBILE" } },
                { "Amphibious", new string[] { "BIOLOGICAL", "AQUATIC", "LIQUID_INTERACTION" } },
                { "Heightened Hearing", new string[] { "BIOLOGICAL", "RESONANT", "SOUND_INTERACTION" } },
                { "Burrowing Claws", new string[] { "BIOLOGICAL", "STRUCTURAL", "TERRAIN_INTERACTION", "BODY_PART_INTERACTION", "MOBILE" } }
            };

        public static IEnumerable<string> MutationMeddley_GetTagsForVanillaMutation(string mutationName)
        {
            string[] tags;
            if (MutationMeddley_VanillaTags.TryGetValue(mutationName, out tags))
            {
                return tags;
            }

            return new string[0];
        }
    }
}
