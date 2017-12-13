using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using BLTools;

namespace FolderManagement {

  public class TBookCollection : List<TBook>, IToXml {

    public string StorageName { get; set; }

    #region Constructor(s)
    public TBookCollection() {
    }
    public TBookCollection(TBookCollection books) {
      if (books != null) {
        foreach (TBook BookItem in books) {
          this.Add(new TBook(BookItem));
        }
      }
    }
    public TBookCollection(IList<XElement> books) {
      foreach (XElement BookItem in books) {
        this.Add(new TBook(BookItem));
      }
    }
    public TBookCollection(IEnumerable<TBook> books) {
      foreach (TBook BookItem in books) {
        this.Add(new TBook(BookItem));
      }
    }
    public TBookCollection(XElement books) {
      foreach (XElement BookItem in books.Elements("book")) {
        this.Add(new TBook(BookItem));
      }
    }
    #endregion Constructor(s)

    #region Converters
    public XElement ToXml() {
      XElement RetVal = new XElement("books");
      foreach (TBook BookItem in this) {
        RetVal.Add(BookItem.ToXml());
      }
      return RetVal;
    }
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      foreach (TBook BookItem in this) {
        RetVal.AppendLine(string.Format("Book: {0}", BookItem.ToString()));
      }
      return RetVal.ToString();
    }
    #endregion Converters

    public string DumpFlatList() {
      StringBuilder RetVal = new StringBuilder();
      this.ForEach(b => RetVal.AppendFormat("{0}\n", b.Name));
      return RetVal.ToString();
    }

    public void Import(string folder, string rootFolder) {
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
      IEnumerable<FileInfo> InnerFiles = FolderInfo.EnumerateFiles().Where(f => f.Name.ToLower() != "thumbs.db");
      if (InnerFiles.Any()) {
        foreach (FileInfo FileItem in InnerFiles.Where(f => TBook.ArchivedBookExtensions.Contains(f.Extension.ToLower()))) {
          TBook NewBook = new TBook();
          NewBook.Import(FileItem.DirectoryName, FileItem.Name, rootFolder);
          this.Add(NewBook);
          if (OnImportedBook != null) {
            OnImportedBook(this, new ImportedItemEventArgs(FileItem.Name));
          }
        }
      }
    }

    public event EventHandler<ImportedItemEventArgs> OnImportedBook;
  }
}
