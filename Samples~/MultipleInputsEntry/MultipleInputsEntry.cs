using System;
using System.Linq;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace Willykc.Templ.Samples
{
    using Editor;
    using Editor.Entry;

    [TemplEntryInfo(changeTypes: ChangeType.Import | ChangeType.Delete,
        DisplayName = "Multiple Inputs")]
    public class MultipleInputsEntry : TemplEntry
    {
        private static readonly Type[] invalidInputTypes =
        {
            typeof(MultipleInputs),
            typeof(ScribanAsset),
            typeof(TemplSettings)
        };

        [TemplInput]
        public MultipleInputs inputs;

        protected override object InputValue => inputs.inputs?.Where(i => i).ToArray();

        protected override bool IsValidInputField =>
            base.IsValidInputField &&
            inputs.inputs?
            .Where(i => i)
            .All(i => !invalidInputTypes.Contains(i.GetType())) == true;

        protected override bool IsInputChanged(AssetChange change) =>
            base.IsInputChanged(change) ||
            (inputs && inputs.inputs?.Any(i => IsMatch(i, change)) == true);

        private static bool IsMatch(UnityObject input, AssetChange change) =>
            AssetDatabase.GetAssetPath(input) == change.currentPath ||
            AssetDatabase.GetAssetPath(input) == change.previousPath;
    }
}
