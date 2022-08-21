using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Samples
{
    using Editor;

    [TemplEntryInfo(ChangeType.Import)]
    public class TextEntry : TemplEntry
    {
        [TemplInput]
        public TextAsset text;

        protected override bool IsValidInputField => text;

        protected override object InputValue => text.text;

        protected override bool IsInputChanged(AssetChange change) =>
            change.currentPath == AssetDatabase.GetAssetPath(text);
    }
}
