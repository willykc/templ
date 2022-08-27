namespace Willykc.Templ.Samples
{
    using Editor;

    [TemplFunctions]
    public static class TemplFunctionsSample
    {
        public static string Padding(string str) => $"_{str}_";
    }
}