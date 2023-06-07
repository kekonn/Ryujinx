using NUnit.Framework;
using Ryujinx.Common.SaveGames;
using System;

namespace Ryujinx.Tests.Collections.Common
{
    
    [Category("SaveGameExport")]
    public class JKSVSaveGameBackupTests
    {
        [DatapointSource]
        public string[] ValidJksvBackupPaths = new[]
        {
            @"/run/media/user/5021-0000/JKSV/Advance Wars 1+2  Re-Boot Camp [72110370913951744]/User - 2023.06.07 @ 15.47.27",
            @"/run/media/user/5021-0000/JKSV/Game Boy Advance - Nintendo Switch Online [72078416430981120]/User - 2023.06.07 @ 15.47.28"
        };

        [Theory]
        public void ShouldParse_ValidPaths(string path)
        {
            // Act
            var result = JKSVSaveGameBackup.FromString(path);
            
            // Assert
            Assert.AreEqual(path, result.BackupPath);
            Assert.AreEqual("User", result.Username);
            Assert.True(result.BackupTimestamp != DateTime.MinValue, "Backup timestamp should be a real world timestamp");
        }
    }
}