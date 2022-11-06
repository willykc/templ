using UnityEngine;

namespace Willykc.Templ.Samples
{
    using Editor.Entry;

    [TemplEntryInfo(ChangeType.Import)]
    public class TextEntry : TemplEntry
    {
        [TemplInput]
        public TextAsset text;

    }
}
