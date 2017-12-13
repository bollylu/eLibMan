using BLTools;
using BLTools.Json;
using BLTools.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BDIndexLib {
  public class TRepository : BaseItem, IDisposable {

    public const string XML_THIS_ELEMENT = "Repository";
    public const string XML_ATTRIBUTE_NAME = "Name";

    public string RootPath { get; set; }

    public string Name { get; set; }

    private object _LockBooks = new object();
    public TBookList Books { get; } = new TBookList();

    public int CollectionCount {
      get {
        if ( !Books.Any() ) {
          return 0;
        }
        return Books.OrderBy(x => x.CollectionName).Select(x => x.CollectionName).Distinct().Count();
      }
    }
    public int CollectionLessCount {
      get {
        if ( !Books.Any() ) {
          return 0;
        }
        return Books.Count(x => x.CollectionName == "");
      }
    }

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TRepository() {
      Books.Parent = this;
      Name = "";
    }

    public TRepository(string rootPath, string name = "") {
      RootPath = $@"{Path.GetFullPath(rootPath)}{Path.DirectorySeparatorChar}";
      Books.Parent = this;
      Name = name;
    }

    public void Dispose() {
      Books.Dispose();
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------


    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendLine(TextBox.BuildDynamic($"Repository {Name}, location={RootPath}"));
      RetVal.AppendLine($"Found {Books.Count()} books");
      RetVal.AppendLine($"Found {CollectionCount} different collections");
      RetVal.AppendLine($"{CollectionLessCount} books have no collection assigned");
      return RetVal.ToString();
    }

    public IJsonValue ToJson() {
      JsonObject RetVal = new JsonObject();
      RetVal.Add(nameof(Name), Name);
      RetVal.Add(nameof(Books), Books.ToJson());
      return RetVal;
    }


    public XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      RetVal.Add(Books.ToXml());
      return RetVal;
    }

    public async Task BuildIndex() {

      Trace.WriteLine(TextBox.BuildFixedWidth($"Indexing {RootPath} ..."));
      if ( !Directory.Exists(RootPath) ) {
        return;
      }

      Books.Clear();

      await _Import(RootPath);
      Trace.WriteLine(TextBox.BuildFixedWidth($"{RootPath} indexed successfully."));
    }

    private Task _Import(string folder) {
      #region Validate parameters
      if ( string.IsNullOrWhiteSpace(folder) ) {
        Trace.WriteLine("Unable to import empty folder");
        return Task.FromException(new ArgumentException("Missing folder name"));
      }

      string FolderFullPath = Path.GetFullPath(folder);
      if ( !Directory.Exists(FolderFullPath) ) {
        Trace.WriteLine($"Unable to access directory \"{folder}\" : directory is invalid or access is denied.");
        return Task.FromException(new ArgumentException("Invalid folder name or access denied"));
      }
      #endregion Validate parameters

      DirectoryInfo FolderInfo = new DirectoryInfo(FolderFullPath);
      IEnumerable<DirectoryInfo> InnerDirectories = FolderInfo.EnumerateDirectories().Where(x => !x.Name.StartsWith("#"));

      if ( InnerDirectories.Any() ) {
        foreach ( DirectoryInfo DirectoryItem in InnerDirectories ) {
          _ImportFolder(DirectoryItem.FullName);
        }
      }

      return Task.CompletedTask;

    }

    private void _ImportFolder(string folder) {
      #region Validate parameters
      if ( string.IsNullOrWhiteSpace(folder) ) {
        Trace.WriteLine("Unable to import empty folder");
        return;
      }

      string FolderFullPath = Path.GetFullPath(folder);
      if ( !Directory.Exists(FolderFullPath) ) {
        Trace.WriteLine($"Unable to access directory \"{folder}\" : directory is missing or access is denied.");
        return;
      }
      #endregion Validate parameters

      DirectoryInfo FolderInfo = new DirectoryInfo(FolderFullPath);

      IEnumerable<DirectoryInfo> InnerDirectories = FolderInfo.EnumerateDirectories();
      IEnumerable<FileInfo> InnerFiles = FolderInfo.EnumerateFiles()
                                                   .Where(f => TBook.Extensions.Contains(f.Extension.ToLower()));

      #region Folder as book or folder as group
      foreach ( DirectoryInfo DirectoryItem in InnerDirectories ) {
        IEnumerable<FileInfo> PageFiles = DirectoryItem.EnumerateFiles()
                                                       .Where(f => TPage.Extensions.Contains(f.Extension.ToLower()));
        if ( PageFiles.Any() ) {
          #region Found a book as a folder
          TBook NewBook = new TBook(DirectoryItem.Name, EBookType.folder, DirectoryItem.FullName.After(RootPath));
          //Trace.WriteLine(NewBook.ToString());
          Books.Add(NewBook);
          #endregion Found a book as a folder
        } else {
          #region Dig deeper into directories
          _ImportFolder(DirectoryItem.FullName);
          #endregion Dig deeper into directories
        }
      }
      #endregion Folder as book or folder as group

      #region Archived files as books
      foreach ( FileInfo FileItem in InnerFiles ) {
        TBook NewBook = new TBook(FileItem.Name, EBookType.unknown, FileItem.FullName.After(RootPath).BeforeLast(@"\"));
        //Trace.WriteLine(NewBook.ToString());
        Books.Add(NewBook);
      }
      #endregion Archived files as books

    }

    //public TBookCollection GetCollection(string collectionName) {
    //  lock ( _LockBooks ) {
    //    return new TBookCollection(Books.Where(x => x.CollectionName == collectionName));
    //  }
    //}

    //public TBookCollection GetCollectionLess() {
    //  lock ( _LockBooks ) {
    //    return new TBookCollection(Books.Where(x => x.CollectionName == ""));
    //  }
    //}

    //public IEnumerable<TBookCollection> EnumerateCollections() {
    //  lock ( _LockBooks ) {
    //    if ( !Books.Any() ) {
    //      yield break;
    //    }
    //    foreach ( IEnumerable<TBook> BookItems in Books.OrderBy(x=>x.CollectionName).ThenBy(x=>x.Number).ThenBy(x=>x.Name).GroupBy(x => x.CollectionName) ) {
    //      yield return new TBookCollection(BookItems);
    //    }
    //  }
    //  yield break;

    //}
  }
}
