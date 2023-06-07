using System;
using System.Globalization;
using System.IO;

namespace Ryujinx.Common.SaveGames
{
    public class JKSVSaveGameBackup
    {
        public string BackupPath { get; private set; }
        public string Username { get; private set; }
        public DateTime BackupTimestamp { get; private set; }

        public const string JKSVDateFormat = "yyyy.MM.dd @ HH.mm.ss";

        public static JKSVSaveGameBackup FromString(string backupFolderPath)
        {
            if (string.IsNullOrWhiteSpace(backupFolderPath))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(backupFolderPath));
            }

            var dirName = backupFolderPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? Path.GetFileNameWithoutExtension(backupFolderPath)
                : new DirectoryInfo(backupFolderPath).Name;
            
            if (string.IsNullOrEmpty(dirName))
            {
                throw new ArgumentException($"Could not process {backupFolderPath} as the name of a backup folder",
                    nameof(backupFolderPath));
            }

            var nameParts = dirName.Split('-');
            if (nameParts.Length != 2)
            {
                throw new ArgumentException($"{backupFolderPath} is not in the expected format.",
                    nameof(backupFolderPath));
            }

            var username = nameParts[0].Trim();

            var isValidDate = DateTime.TryParseExact(nameParts[1].Trim(), JKSVDateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var backupTimestamp);

            if (!isValidDate)
            {
                throw new ArgumentException(
                    $"Could not parse {nameParts[1].Trim()} into a valid date according to format {JKSVDateFormat}");
            }

            return new JKSVSaveGameBackup
            {
                BackupPath = backupFolderPath,
                BackupTimestamp = backupTimestamp,
                Username = username
            };
        }
    }
}