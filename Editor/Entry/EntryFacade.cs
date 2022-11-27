using UnityEngine;

namespace Willykc.Templ.Editor.Entry
{
    internal class EntryFacade : IEntryFacade
    {
        TemplEntry[] IEntryFacade.GetEntries()
        {
            throw new System.NotImplementedException();
        }

        string IEntryFacade.AddEntry<T>(
            Object inputAsset,
            ScribanAsset template,
            string outputPath)
        {
            throw new System.NotImplementedException();
        }

        void IEntryFacade.UpdateEntry(
            string id,
            Object inputAsset,
            ScribanAsset template,
            string outputPath)
        {
            throw new System.NotImplementedException();
        }

        void IEntryFacade.RemoveEntry(TemplEntry entry)
        {
            throw new System.NotImplementedException();
        }

        void IEntryFacade.RemoveEntry(string id)
        {
            throw new System.NotImplementedException();
        }
    }
}