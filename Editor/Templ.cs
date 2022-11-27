namespace Willykc.Templ.Editor
{
    using Entry;
    using Scaffold;

    public static class Templ
    {
        private static readonly IEntryFacade entry;
        private static readonly IScaffoldFacade scaffold;

        static Templ()
        {
            entry = new EntryFacade();
            scaffold = new ScaffoldFacade();
        }

        public static IEntryFacade Entry => entry;
        public static IScaffoldFacade Scaffold => scaffold;
    }
}
