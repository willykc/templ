using UnityEngine;

namespace Willykc.Templ.Editor.Entry
{
    public interface IEntryFacade
    {
        TemplEntry[] GetEntries();
        string AddEntry<T> (Object inputAsset, ScribanAsset template, string outputPath)
            where T : TemplEntry, new();
        void UpdateEntry(
            string id,
            Object inputAsset = null,
            ScribanAsset template = null,
            string outputPath = null);
        void RemoveEntry(TemplEntry entry);
        void RemoveEntry(string id);
    }
}