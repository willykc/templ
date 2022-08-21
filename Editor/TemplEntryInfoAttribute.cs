using System;

namespace Willykc.Templ.Editor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TemplEntryInfoAttribute : Attribute
    {
        internal ChangeType ChangeTypes { get; }
        internal bool Deferred { get; }

        public TemplEntryInfoAttribute(
            ChangeType changeTypes,
            bool deferred = false)
        {
            ChangeTypes = changeTypes;
            Deferred = deferred;
        }
    }
}
