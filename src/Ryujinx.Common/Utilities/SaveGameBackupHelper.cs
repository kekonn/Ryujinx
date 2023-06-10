using Ryujinx.Common.Logging;
using Ryujinx.Common.SaveGames;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Common.Utilities
{
    [DebuggerDisplay("{ExportRoot}")]
    public class SaveGameBackupHelper
    {
        private string _exportRoot;
        private bool _isJKSVRoot;

        public string ExportRoot
        {
            get => _exportRoot;
            private set => _exportRoot = value;
        }

        public SaveGameBackupHelper(string exportRoot)
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

        private IReadOnlyCollection<JKSVSaveGameBackup> GetBackupsInGameFolder(string gameFolderPath)
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

        /// <summary>
        /// Creates a backup directory in the given root dir and copies all the save game files to that directory.
        /// </summary>
        /// <param name="saveGameDir">Directory that contains the save files.</param>
        /// <param name="backupRootDir">The root directory to create the backup directory in.</param>
        /// <param name="username">The username of the user on the Switch.</param>
        /// <returns>An awaitable <see cref="Task"/> that completes when all the files are copied.</returns>
        /// <exception cref="ArgumentException">If any of the arguments are null or if <paramref name="backupRootDir"/> isn't under the root directory set in settings.</exception>
        public Task CreateBackupFromSaveGameDirectoryAsync(string saveGameDir, string backupRootDir, string username)
        {
            if (string.IsNullOrWhiteSpace(saveGameDir))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(saveGameDir));
            }

            if (string.IsNullOrWhiteSpace(backupRootDir))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(backupRootDir));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            }
            
            if (!Directory.Exists(saveGameDir))
            {
                throw new ArgumentException($"{saveGameDir} does not exist.", nameof(saveGameDir));
            }

            if (!Directory.Exists(backupRootDir))
            {
                if (backupRootDir.StartsWith(_exportRoot))
                {
                    Directory.CreateDirectory(backupRootDir);
                }
                else
                {
                    throw new ArgumentException(
                        $"{backupRootDir} does not exist and the specified path is not under the backup root directory.",
                        nameof(backupRootDir));
                }
            }

            var backupDir = CreateBackupDir(username, backupRootDir);

            var saveFiles = Directory.GetFiles(saveGameDir, "*", SearchOption.AllDirectories);

            Logger.Info?.PrintMsg(LogClass.Application, $"Copying {saveFiles.Length} save files to {backupDir.FullName}");

            var copyTasks = saveFiles.Select(f => Task.Run(() =>
            {
                var fileName = Path.GetFileName(f);
                File.Copy(f, Path.Join(backupDir.FullName, fileName));
            }));

            return Task.WhenAll(copyTasks);
        }

        private DirectoryInfo CreateBackupDir(string username, string backupRootDir)
        {
            if (!backupRootDir.StartsWith(_exportRoot))
            {
                throw new ArgumentException($"{backupRootDir} is not parth of {_exportRoot}.", nameof(backupRootDir));
            }
            
            var backupTimestamp = DateTime.Now.ToString("yyyy.MM.dd @ HH.mm.ss");
            var joinedPath = Path.Join(backupRootDir, $"{username} - {backupTimestamp}");

            return Directory.CreateDirectory(joinedPath);
        }
    }
}