using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Willykc.Templ.Samples
{
    using Editor;

    public class TextEntry : TemplEntry
    {
        public TextAsset text;

        public override string InputFieldName => nameof(text);

        public override bool IsValidInput => text;

        public override object InputValue => text.text;

        public override bool DelayRender => false;

        public override bool IsInputChanged(AssetChanges changes) =>
            IsValidInput && changes.importedAssets.Contains(AssetDatabase.GetAssetPath(text));

        public override bool WillDeleteInput(string path) => false;
    }
}
