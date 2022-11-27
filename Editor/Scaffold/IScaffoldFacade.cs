using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Willykc.Templ.Editor.Scaffold
{
    public interface IScaffoldFacade
    {
        Task GenerateScaffoldAsync(
            TemplScaffold scaffold,
            string targetPath,
            object input = null,
            Object selection = null,
            bool forceOverwrite = false,
            CancellationToken cancellationToken = default);
    }
}