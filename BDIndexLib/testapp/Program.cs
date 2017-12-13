using System;
using BLTools;
using BLTools.Debugging;
using System.IO;
using System.Linq;

namespace testapp {
  class Program {
    static void Main(string[] args) {
      Console.WriteLine("Hello World!");
      SplitArgs Args = new SplitArgs(args);

      if ( Args.Count == 0 ) {
        Console.WriteLine("No argument specified");
      }

      foreach ( ArgElement ArgItem in Args ) {
        Console.WriteLine($"{ArgItem.Name}={ArgItem.Value}");
      }

      Console.WriteLine(ApplicationInfo.BuildRuntimeInfo());

      Console.WriteLine();

      string folder = Args.GetValue<string>("folder", "");

      if ( !Directory.Exists(folder) ) {
        Console.WriteLine("Missing directory or access denied");
        Environment.Exit(1);
      }

      foreach ( string FileItem in Directory.EnumerateFiles(folder) ) {
        Console.WriteLine(FileItem);
      }

      int i = 1;
      foreach ( string DirectoryItem in Directory.EnumerateDirectories(folder).OrderBy(x => x) ) {
        Console.WriteLine($"{i++:000}.{DirectoryItem}");
      }

      Environment.Exit(0);

    }
  }
}
