using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools;
using BLTools.ConsoleExtension;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using BLTools.Debugging;

namespace MakeCbz {
  class Program {
    static void Main(string[] args) {

      TraceFactory.AddTraceConsole();
      SplitArgs Args = new SplitArgs(args);

      string SourceDir = Args.GetValue("sourcedir", "");
      Console.WriteLine($"Source directory = {SourceDir}");
      InitTitle($"Processing {SourceDir}");

      DirectoryInfo FirstLevelDir = new DirectoryInfo(SourceDir);
      IEnumerable<DirectoryInfo> SubDirs = FirstLevelDir.EnumerateDirectories();

      if ( SubDirs.Count() == 0 ) {
        string CbzFilename = $"{SourceDir}.cbz";
        ChangeTitle($"Building {CbzFilename}");
        if ( !ProcessDirectory(FirstLevelDir, CbzFilename) ) {
          ConsoleExtension.Pause();
          Environment.Exit(1);
        } else {
          //ConsoleExtension.Pause();
          Environment.Exit(0);
        }
      }

      List<string> Errors = new List<string>();
      SubDirs.AsParallel().ForAll((x) => {
        string CbzFilename = $"{x.FullName}.cbz";
        if ( !ProcessDirectory(x, CbzFilename) ) {
          Errors.Add(CbzFilename);
        }
      });

      if ( Errors.Count > 0 ) {
        ConsoleExtension.Pause();
        Environment.Exit(1);
      }

      //ConsoleExtension.Pause();
      Environment.Exit(0);

    }

    static private bool ProcessDirectory(DirectoryInfo directory, string cbzFilename) {
      StringBuilder ActionLog = new StringBuilder();
      ActionLog.AppendLine($"Processing {directory.FullName}");

      try {
        if ( directory == null || !Directory.Exists(directory.FullName) ) {
          ActionLog.AppendLine($"  => Unable to create {cbzFilename} file : source directory is missing or access is denied");
          return false;
        }

        IEnumerable<DirectoryInfo> SubDirs = directory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);

        if ( SubDirs.Count() > 0 ) {
          Trace.WriteLine($"  => ## Found {SubDirs.Count()} sub-directories, digging first ##");
          foreach ( DirectoryInfo DirectoryItem in SubDirs ) {

            if ( !ProcessDirectory(DirectoryItem, $"{DirectoryItem.FullName}.cbz") ) {
              return false;
            }

          }
        }

        if ( directory.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(x => x.Extension.ToLower() != ".cbz").Count() > 0 ) {

          if ( File.Exists(cbzFilename) ) {
            ActionLog.AppendLine($"  => Unable to create {cbzFilename} : file already exists");
            return false;
          }

          ChangeTitle($"Building {cbzFilename}");
          ActionLog.AppendLine($"  => Will generate {cbzFilename}");
          ZipFile.CreateFromDirectory(directory.FullName, cbzFilename, CompressionLevel.Fastest, false);
          directory.Delete(true);
          ChangeTitle();
          return true;
        }

        return true;
      } catch ( Exception ex ) {
        ActionLog.AppendLine($"  => Error while processing : {ex.Message}");
        return false;
      } finally {
        Trace.WriteLine(ActionLog.Trim('\r', '\n'));
      }
    }

    static string BaseTitle = "";
    static void InitTitle(string baseTitle) {
      BaseTitle = baseTitle;
      Console.Title = BaseTitle;
    }
    static void ChangeTitle(string suffixTitle = "") {
      string CombinedTitle = string.IsNullOrWhiteSpace(suffixTitle) ? BaseTitle : $"{BaseTitle} - {suffixTitle}";
      Console.Title = CombinedTitle;
    }
  }
}
