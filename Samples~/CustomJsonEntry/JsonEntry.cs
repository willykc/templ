using UnityEngine;

namespace Willykc.Templ.Samples
{
    using Editor.Entry;

    [TemplEntryInfo(changeTypes: ChangeType.Import, DisplayName = "JSON")]
    public class JsonEntry : TemplEntry
    {
        [TemplInput]
        public TextAsset json;

        protected override object InputValue => JsonUtility.FromJson<JsonData>(json.text);

        protected override bool IsValidInputField
        {
            get
            {
                try
                {
                    return base.IsValidInputField && InputValue != null;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
