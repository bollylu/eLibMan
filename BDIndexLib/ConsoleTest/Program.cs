using System;
using BLTools;
using BLTools.Debugging;
using BLTools.Json;
using BDIndexLib;
using System.Linq;
using BLTools.ConsoleExtension;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using BLTools.Text;
using System.Collections.Generic;

namespace ConsoleTest {
  class Program {
    static void Main(string[] args) {

      Console.WriteLine("Starting BDIndex");
      TraceFactory.AddTraceConsole();
      Task.Run(async () => { await MainAsync(args); }).Wait();

    }

    static async Task MainAsync(string[] args) {
      TChrono chrono = new TChrono();
      chrono.Start();

      SplitArgs Args = new SplitArgs(args);

      string SourceRepository = Args.GetValue<string>("source", "");
      string RepositoryName = Args.GetValue<string>("name", SourceRepository.AfterLast(Path.DirectorySeparatorChar));
      string OutputFilename = Args.GetValue<string>("output", "bdindex");
      string OutputFormat = Args.GetValue<string>("outputformat", "txt").ToLower();
      List<string> AllowedFormats = new List<string>() { "txt", "json", "xml" };
      if (!AllowedFormats.Contains(OutputFormat)) {
        OutputFormat = "txt";
      }

      Console.WriteLine(TextBox.BuildDynamic($"Index folder {SourceRepository}..."));
      Console.WriteLine($"  Name = {RepositoryName}");
      Console.WriteLine($"  Output to {OutputFilename}.{OutputFormat}");
      Console.WriteLine();

      if ( !Directory.Exists(SourceRepository) ) {
        Usage("Missing source directory");
      }

      TRepository CurrentRepository = new TRepository(SourceRepository, RepositoryName);
      await CurrentRepository.BuildIndex();

      OutputFilename += $".{OutputFormat}";
      if ( File.Exists(OutputFilename) ) {
        File.Delete(OutputFilename);
      }

      switch (OutputFormat) {
        case "txt":
          foreach ( TBookCollection BookCollectionItem in CurrentRepository.Books.EnumerateCollections() ) {
            File.AppendAllText(OutputFilename, BookCollectionItem.ToString());
            File.AppendAllText(OutputFilename, Environment.NewLine);
          }
          break;
        case "json":
          JsonValue.Save(OutputFilename, CurrentRepository.ToJson());
          break;
        case "xml":
          File.AppendAllText(OutputFilename, CurrentRepository.ToXml().ToString());
          break;
      }

      

      //TBookCollection MissingInBrilly = new TBookCollection(BdBrilly.Books.GetMissingFrom(BdLuc.Books));
      //TBookCollection MissingInLuc = new TBookCollection(BdLuc.Books.GetMissingFrom(BdBrilly.Books));

      //Console.WriteLine("=== Missing in Luc =================================================");
      //string OutputMissingLuc = @"i:\# BDS\MissingInLuc.txt";
      //File.Delete(OutputMissingLuc);
      //File.AppendAllText(OutputMissingLuc, $"{MissingInLuc.Count()} books{Environment.NewLine}");
      //foreach ( TBookCollection BookCollectionItem in MissingInLuc.EnumerateCollections() ) {
      //  File.AppendAllText(OutputMissingLuc, BookCollectionItem.ToString());
      //  File.AppendAllText(OutputMissingLuc, Environment.NewLine);
      //}
      //Console.WriteLine("=== Missing in Brilly ==============================================");
      //foreach ( TBookCollection BookCollectionItem in MissingInBrilly.EnumerateCollections() ) {
      //  Console.WriteLine(BookCollectionItem.ToString());
      //}

      Console.WriteLine($"Indexing process completed in {chrono.ElapsedTime.TotalSeconds} secs");
      //ConsoleExtension.Pause();

      //Environment.Exit(0);
    }

    static void Usage(string ErrorMessage = "") {
      if ( !string.IsNullOrWhiteSpace(ErrorMessage) ) {
        Console.WriteLine(ErrorMessage);
      }
      Console.WriteLine("BDIndex v0.1 (c) 2017 Luc Bolly");
      Console.WriteLine("Usage : BDIndex -source=<BD root folder>");
      Console.WriteLine("                [-name=\"repository name\"]");
      Console.WriteLine("                [-output=<output filename (without extension)> (default=bdindex)");
      Console.WriteLine("                [-outputformat=txt|json|xml (default=txt)");
      ConsoleExtension.Pause();
      Environment.Exit(1);
    }
  }
}
