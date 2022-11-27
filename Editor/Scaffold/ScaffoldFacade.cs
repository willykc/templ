using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    internal class ScaffoldFacade : IScaffoldFacade
    {
        Task IScaffoldFacade.GenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input,
            Object selection,
            bool forceOverwrite,
            CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}