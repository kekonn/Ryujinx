using Ryujinx.Common.SaveGames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Ryujinx.Common.Utilities
{
    [DebuggerDisplay("{ExportRoot}")]
    public class SaveGameHelper
    {
        private string _exportRoot;
        private bool _isJKSVRoot;

        public string ExportRoot
        {
            get => _exportRoot;
            private set => _exportRoot = value;
        }

        /// <summary>
        /// If this is set to true, the export root folder is treated as a JKSV root folder.
        /// </summary>
        public bool IsJKSVRoot
        {
            get => _isJKSVRoot;
            set => _isJKSVRoot = value;
        }

        public SaveGameHelper(string exportRoot)
        {
            if (!Directory.Exists(exportRoot))
            {
                throw new InvalidOperationException($"Directory {exportRoot} does not exist");
            }
            
            _exportRoot = exportRoot;
            _isJKSVRoot = exportRoot.EndsWith("JKSV", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Looks for a directory under the export root folder that contains the given title id in the format <c>[00000000000000]</c>.
        /// </summary>
        /// <param name="titleId">The title id of the game.</param>
        /// <param name="exportPath">The path, if one is found.</param>
        /// <returns>True if a folder matches the given title id, false if otherwise.</returns>
        /// <exception cref="InvalidOperationException">If there is more than one folder that matches the title id search.</exception>
        /// <exception cref="ArgumentException">If <paramref name="titleId"/> is null or empty.</exception>
        public bool TryFindExportPathForTitleId(string titleId, out string exportPath)
        {
            if (string.IsNullOrWhiteSpace(titleId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(titleId));
            }

            if (_isJKSVRoot)
            {
                exportPath = Directory.GetDirectories(_exportRoot, $"*[{titleId}]", SearchOption.TopDirectoryOnly)
                    .SingleOrDefault();
            }
            else
            {
                exportPath = Directory.GetDirectories(_exportRoot, titleId, SearchOption.TopDirectoryOnly)
                    .SingleOrDefault();
            }

            return exportPath != default;
        }

        public IReadOnlyCollection<JKSVSaveGameBackup> GetBackupsInGameFolder(string gameFolderPath)
        {
            if (string.IsNullOrWhiteSpace(gameFolderPath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(gameFolderPath));
            }

            if (!Directory.Exists(gameFolderPath))
            {
                throw new ArgumentException($"Path {gameFolderPath} does not exist.", nameof(gameFolderPath));
            }

            return Directory.GetFiles(gameFolderPath, "*.zip", SearchOption.TopDirectoryOnly)
                .Concat(Directory.GetDirectories(gameFolderPath)).Select(JKSVSaveGameBackup.FromString).ToArray();
        }
    }
}