using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using BLTools;

namespace FolderManagement {
  public class TLibrary : IToXml {

    #region XML Tag names
    internal const string TAG_THIS_ELEMENT = "Library";
    internal const string TAG_ATTRIBUTE_NAME = "Name";
    internal const string TAG_ATTRIBUTE_ROOTFOLDER = "RootFolder";
    internal const string TAG_ELEMENT_DESCRIPTION = "Description";
    #endregion XML Tag names

    #region Public properties
    public string Name { get; set; }
    public string Description { get; set; }
    public string StorageName { get; set; }
    public string RootFolder { get; set; }
    public TGroupCollection Groups { 
      get {
        TGroupCollection RetVal = new TGroupCollection();
        foreach (TBook BookItem in Books) {
          TGroup TestGroup = RetVal.Find(g => g.Name == BookItem.Serie);
          if (TestGroup==null) {
            TGroup NewGroup = new TGroup(BookItem.Serie, BookItem.Pathname);
            NewGroup.Books.Add(new TBook(BookItem));
            RetVal.Add(NewGroup);
          } else {
            TestGroup.Books.Add(BookItem);
          }
        }
        return RetVal;
      }
    }
    public TBookCollection Books { get; set; }
    #endregion Public properties

    #region Constructor(s)
    public TLibrary() {
      StorageName = "";
      Name = "";
      Description = "";
      //Groups = new TGroupCollection();
      Books = new TBookCollection();
      RootFolder = "";
    }

    public TLibrary(string name, string rootFolder = "", string description = "")
      : this() {
      Name = name;
      RootFolder = rootFolder;
      Description = description;
    }

    public TLibrary(TLibrary library)
      : this() {
      Name = library.Name;
      StorageName = library.StorageName;
      Description = library.Description;
      //Groups = new TGroupCollection(library.Groups);
      Books = new TBookCollection(library.Books);
      RootFolder = library.RootFolder;
    }
    #endregion Constructor(s)

    #region Converters
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendLine(string.Format("Library: {0} - {1}", Name, Description));
      RetVal.AppendLine(string.Format("  => {0}", RootFolder));
      RetVal.AppendLine(Books.ToString());
      RetVal.AppendLine(Groups.ToString());
      return RetVal.ToString();
    }
    public XElement ToXml() {
      XElement RetVal = new XElement(TAG_THIS_ELEMENT);
      RetVal.SetAttributeValue(TAG_ATTRIBUTE_NAME, Name);
      RetVal.SetAttributeValue(TAG_ATTRIBUTE_ROOTFOLDER, RootFolder);
      RetVal.SetElementValue(TAG_ELEMENT_DESCRIPTION, Description);
      RetVal.Add(Books.ToXml());
      RetVal.Add(Groups.ToXml());
      return RetVal;
    }
    #endregion Converters

    #region Public methods
    public void ImportAsync() {
      ImportAsync(RootFolder);
    }
    public void ImportAsync(string folder) {
      #region Validate parameters
      if (string.IsNullOrWhiteSpace(folder)) {
        Trace.WriteLine("Unable to import empty folder");
        return;
      }

      string FolderFullPath = Path.GetFullPath(folder);
      if (!Directory.Exists(FolderFullPath)) {
        Trace.WriteLine(string.Format("Unable to access directory \"{0}\" : directory is missing or access is denied.", folder));
        return;
      }
      string FolderPath = FolderFullPath.Left(FolderFullPath.LastIndexOf("\\"));
      string FolderName = FolderFullPath.Substring(FolderFullPath.LastIndexOf("\\") + 1);

      #endregion Validate parameters

      DirectoryInfo FolderInfo = new DirectoryInfo(FolderFullPath);
      List<DirectoryInfo> InnerDirectories = FolderInfo.GetDirectories().ToList();

      if (OnBeforeImport != null) {
        OnBeforeImport(this, new BeforeImportEventArgs(InnerDirectories.Count));
      }

      Task ImportTask = Task.Factory.StartNew(() => Import(folder));

      ImportTask.ContinueWith(x => {
        if (OnImportCompleted != null) {
          OnImportCompleted(this, EventArgs.Empty);
        }
      });

    }


    //public void Import(string folder) {
    //  #region Validate parameters
    //  if (string.IsNullOrWhiteSpace(folder)) {
    //    Trace.WriteLine("Unable to import empty folder");
    //    return;
    //  }

    //  string FolderFullPath = Path.GetFullPath(folder);
    //  if (!Directory.Exists(FolderFullPath)) {
    //    Trace.WriteLine(string.Format("Unable to access directory \"{0}\" : directory is missing or access is denied.", folder));
    //    return;
    //  }
    //  string FolderPath = FolderFullPath.Left(FolderFullPath.LastIndexOf("\\"));
    //  string FolderName = FolderFullPath.Substring(FolderFullPath.LastIndexOf("\\") + 1);

    //  #endregion Validate parameters
    //  DirectoryInfo FolderInfo = new DirectoryInfo(FolderFullPath);
    //  List<DirectoryInfo> InnerDirectories = FolderInfo.GetDirectories().ToList();

    //  if (OnBeforeImport != null) {
    //    OnBeforeImport(this, new BeforeImportEventArgs(InnerDirectories.Count));
    //  }

    //  if (InnerDirectories.Count > 0) {
    //    List<DirectoryInfo> TestListOfBD = (new DirectoryInfo(FolderFullPath)).GetDirectories("*.*", SearchOption.AllDirectories)
    //      .Where(d => d.GetFiles().Count() >= 1 && d.GetDirectories().Count() == 0)
    //      .ToList();
    //    foreach (DirectoryInfo DirectoryItem in TestListOfBD) {
    //      this.Books.Add(new TBook(DirectoryItem.Name, DirectoryItem.FullName, RootFolder, TBook.BookTypeEnum.Folder));
    //      if (OnImportedBook != null) {
    //        OnImportedBook(this, new ImportedItemEventArgs(DirectoryItem.Name));
    //      }
    //    }
    //  }

    //}

    public void Import(string folder) {
      #region Validate parameters
      if (string.IsNullOrWhiteSpace(folder)) {
        Trace.WriteLine("Unable to import empty folder");
        return;
      }

      string FolderFullPath = Path.GetFullPath(folder);
      if (!Directory.Exists(FolderFullPath)) {
        Trace.WriteLine(string.Format("Unable to access directory \"{0}\" : directory is missing or access is denied.", folder));
        return;
      }
      string FolderPath = FolderFullPath.Left(FolderFullPath.LastIndexOf("\\"));
      string FolderName = FolderFullPath.Substring(FolderFullPath.LastIndexOf("\\") + 1);

      #endregion Validate parameters
      DirectoryInfo FolderInfo = new DirectoryInfo(FolderFullPath);
      List<DirectoryInfo> InnerDirectories = FolderInfo.GetDirectories().ToList();

      if (OnBeforeImport != null) {
        OnBeforeImport(this, new BeforeImportEventArgs(InnerDirectories.Count));
      }

      if (InnerDirectories.Count > 0) {
        foreach (DirectoryInfo DirectoryItem in InnerDirectories) {

          _ImportFolder(DirectoryItem.FullName);
          if (OnImportedBook != null) {
            OnImportedBook(this, new ImportedItemEventArgs(DirectoryItem.Name));
          }

        }
      }
    }

    private void _ImportFolder(string folder) {
      #region Validate parameters
      if (string.IsNullOrWhiteSpace(folder)) {
        Trace.WriteLine("Unable to import empty folder");
        return;
      }

      string FolderFullPath = Path.GetFullPath(folder);
      if (!Directory.Exists(FolderFullPath)) {
        Trace.WriteLine(string.Format("Unable to access directory \"{0}\" : directory is missing or access is denied.", folder));
        return;
      }
      string FolderPath = FolderFullPath.Left(FolderFullPath.LastIndexOf("\\"));
      string FolderName = FolderFullPath.Substring(FolderFullPath.LastIndexOf("\\") + 1);
      #endregion Validate parameters

      DirectoryInfo FolderInfo = new DirectoryInfo(FolderFullPath);

      List<DirectoryInfo> InnerDirectories = FolderInfo.GetDirectories().ToList();
      List<FileInfo> InnerFiles = FolderInfo.GetFiles().Where(f => f.Name.ToLower() != "thumbs.db").ToList();

      #region Folder as book or folder as group
      if (InnerDirectories.Count > 0) {
        foreach (DirectoryInfo DirectoryItem in InnerDirectories) {
          List<FileInfo> PageFiles = DirectoryItem.GetFiles().Where(f => f.Name.ToLower() != "thumbs.db").ToList();
          if (PageFiles.Count > 0) {
            #region Found a book as a folder
            TBook NewBook = new TBook(DirectoryItem.Name, DirectoryItem.FullName, RootFolder, TBook.BookTypeEnum.Folder);
            Books.Add(NewBook);
            #endregion Found a book as a folder
          } else {
            #region Dig deeper into directories
            _ImportFolder(DirectoryItem.FullName);
            #endregion Dig deeper into directories
          }
        }
      }
      #endregion Folder as book or folder as group

      #region Archived files as books
      if (InnerFiles.Count > 0) {
        Books.Import(FolderFullPath, RootFolder);
      }
      #endregion Archived files as books

    }

    public List<string> GetGroupList() {
      List<string> RetVal = new List<string>();
      Books.ForEach(b => RetVal.Add(b.Category));
      return RetVal.OrderBy(x => x).Distinct().ToList();
    }

    #endregion Public methods

    #region Events
    public event EventHandler<BeforeImportEventArgs> OnBeforeImport;
    public event EventHandler<ImportedItemEventArgs> OnImportedBook;
    public event EventHandler OnImportCompleted;
    #endregion Events
  }
}
