using UnityEngine;

namespace Willykc.Templ.Samples
{
    using Editor.Entry;

    [TemplEntryInfo(changeTypes: ChangeType.Import, DisplayName = "Text")]
    public class TextEntry : TemplEntry
    {
        [TemplInput]
        public TextAsset text;
    }
}
