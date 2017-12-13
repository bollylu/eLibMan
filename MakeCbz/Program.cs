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

    static List<string> ExcludedExtensions = new List<string> { ".cbz", ".cbr", ".pdf" };
    enum EProcessStatus {
      Ok,
      Warning,
      Error
    }

    static void Main(string[] args) {

      TraceFactory.AddTraceConsole();
      SplitArgs Args = new SplitArgs(args);

      string SourceDir = Args.GetValue("sourcedir", "");
      Trace.WriteLine($"Source directory = {SourceDir}");

      if ( string.IsNullOrWhiteSpace(SourceDir) || !Directory.Exists(SourceDir) ) {
        Usage($"Missing source directory : directory is missing or access is denied");
      }

      InitTitle($"Processing {SourceDir}");

      EProcessStatus ProcessStatus = ProcessDirectory(new DirectoryInfo(SourceDir));
      if ( ProcessStatus != EProcessStatus.Ok ) {
        ConsoleExtension.Pause();
        Environment.Exit(1);
      }

      //ConsoleExtension.Pause();
      Environment.Exit(0);

    }



    static private EProcessStatus ProcessDirectory(DirectoryInfo directory) {

      try {
        Trace.WriteLine($"Processing {directory.FullName}");
        Trace.Indent();

        string CbzFilename = $"{directory.FullName}.cbz";

        Trace.WriteLine($"Attempt to create {CbzFilename}");
        try {
          if ( directory == null || !Directory.Exists(directory.FullName) ) {
            Trace.WriteLine($"  => Unable to create {CbzFilename} file : source directory is missing or access is denied", Severity.Error);
            return EProcessStatus.Error;
          }

          DirectoryInfo[] SubDirs = directory.GetDirectories("*", SearchOption.TopDirectoryOnly);

          if ( SubDirs.Count() > 0 ) {
            Trace.WriteLine($"  => ## Found {SubDirs.Count()} sub-directories ##");
            EProcessStatus GlobalProcessStatus = EProcessStatus.Ok;
            foreach ( DirectoryInfo DirectoryItem in SubDirs ) {
              EProcessStatus ProcessStatus = ProcessDirectory(DirectoryItem);
              if ( ProcessStatus != EProcessStatus.Ok ) {
                GlobalProcessStatus = ProcessStatus;
              }
            }
            return GlobalProcessStatus;

          } else {

            IEnumerable<FileInfo> ExcludedFiles = directory.GetFiles("*", SearchOption.TopDirectoryOnly).Where(x => ExcludedExtensions.Contains(x.Extension.ToLower()));
            if ( ExcludedFiles.Count() > 0 ) {
              Trace.WriteLine($"  => *** Skipping {CbzFilename} : Found excluded files in folder ***", Severity.Warning);
              foreach ( string FileItem in ExcludedFiles.Select(x => x.Name) ) {
                Trace.WriteLine($"    {FileItem}");
              }
              return EProcessStatus.Warning;
            }

            if ( File.Exists(CbzFilename) ) {
              Trace.WriteLine($"  => Unable to create {CbzFilename} : file already exists", Severity.Error);
              return EProcessStatus.Error;
            }

            ChangeTitle($"Building {CbzFilename}");
            Trace.WriteLine($"  => Generate {CbzFilename}");
            ZipFile.CreateFromDirectory(directory.FullName, CbzFilename, CompressionLevel.Fastest, false);
            directory.Delete(true);
            ChangeTitle();
            return EProcessStatus.Ok;

          }

        } catch ( Exception ex ) {
          Trace.WriteLine($"  => Error while processing : {ex.Message}", Severity.Error);
          return EProcessStatus.Error;
        }
      } finally {
        Trace.Unindent();
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

    private static void Usage(string message = "") {
      Console.WriteLine("MakeCbz v2.0");
      Console.WriteLine("Usage: MakeCbz /SourceDir=<folder to convert>");
      Environment.Exit(2);
    }
  }
}
