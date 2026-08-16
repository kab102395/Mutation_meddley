using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using XRL;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    [Serializable]
    public partial class MutationMeddley_BiologySupport : IPart
    {
        private bool MutationMeddley_HasAnyEvolution(MutationMeddley_AdaptiveMutationBase mutation)
        {
            return MutationMeddley_GetEvolutionIds(mutation).Count > 0;
        }

        private bool MutationMeddley_HasEvolution(MutationMeddley_AdaptiveMutationBase mutation, string id)
        {
            List<string> ids = MutationMeddley_GetEvolutionIds(mutation);
            for (int i = 0; i < ids.Count; i++)
            {
                if (ids[i] == id)
                {
                    return true;
                }
            }
            return false;
        }

        private List<string> MutationMeddley_GetEvolutionIds(MutationMeddley_AdaptiveMutationBase mutation)
        {
            List<string> result = new List<string>();
            if (mutation == null || string.IsNullOrEmpty(mutation.MutationMeddley_EvolutionState))
            {
                return result;
            }

            string evolutionSegment = mutation.MutationMeddley_EvolutionState;
            int separator = evolutionSegment.IndexOf('|');
            if (separator >= 0)
            {
                evolutionSegment = evolutionSegment.Substring(0, separator);
            }

            if (string.IsNullOrEmpty(evolutionSegment))
            {
                return result;
            }

            string[] ids = evolutionSegment.Split(
                new char[] { ';' },
                StringSplitOptions.RemoveEmptyEntries);
            result.AddRange(ids);
            return result;
        }

        private Dictionary<string, string> MutationMeddley_GetMetadata(MutationMeddley_AdaptiveMutationBase mutation)
        {
            Dictionary<string, string> metadata =
                new Dictionary<string, string>(StringComparer.Ordinal);
            if (mutation == null || string.IsNullOrEmpty(mutation.MutationMeddley_EvolutionState))
            {
                return metadata;
            }

            string[] segments = mutation.MutationMeddley_EvolutionState.Split('|');
            for (int i = 1; i < segments.Length; i++)
            {
                int separator = segments[i].IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                string key = segments[i].Substring(0, separator);
                string value = segments[i].Substring(separator + 1);
                metadata[key] = value;
            }

            return metadata;
        }

        private int MutationMeddley_GetStateInt(MutationMeddley_AdaptiveMutationBase mutation, string key)
        {
            Dictionary<string, string> metadata = MutationMeddley_GetMetadata(mutation);
            string value;
            int parsed;
            if (metadata.TryGetValue(key, out value)
                && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return parsed;
            }
            return 0;
        }

        private void MutationMeddley_SetStateInt(
            MutationMeddley_AdaptiveMutationBase mutation,
            string key,
            int value)
        {
            if (mutation == null)
            {
                return;
            }

            List<string> ids = MutationMeddley_GetEvolutionIds(mutation);
            Dictionary<string, string> metadata = MutationMeddley_GetMetadata(mutation);
            metadata[key] = Math.Max(0, value).ToString(CultureInfo.InvariantCulture);
            metadata["statev"] = "1";

            StringBuilder state = new StringBuilder();
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) state.Append(';');
                state.Append(ids[i]);
            }

            List<string> keys = new List<string>(metadata.Keys);
            keys.Sort(StringComparer.Ordinal);
            for (int i = 0; i < keys.Count; i++)
            {
                if (string.IsNullOrEmpty(metadata[keys[i]]))
                {
                    continue;
                }

                state.Append('|');
                state.Append(keys[i]);
                state.Append('=');
                state.Append(metadata[keys[i]]);
            }

            mutation.MutationMeddley_EvolutionState = state.ToString();
        }

    }
}
