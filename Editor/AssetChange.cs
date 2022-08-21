namespace Willykc.Templ.Editor
{
    public struct AssetChange
    {
        private const string Empty = "";

        public ChangeType type;
        public string currentPath;
        public string previousPath;

        internal AssetChange(
            ChangeType changeType,
            string currentPath,
            string previousPath = Empty)
        {
            this.type = changeType;
            this.currentPath = currentPath
                ?? throw new System.ArgumentNullException(nameof(currentPath));
            this.previousPath = previousPath
                ?? throw new System.ArgumentNullException(nameof(previousPath));
        }
    }
}
