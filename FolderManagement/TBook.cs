using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Linq;
using BLTools;
using System.Diagnostics;
using System.IO;

namespace FolderManagement {
  public class TBook : IToXml, IComparable<TBook>, ILibraryElement {

    #region XML Tag names
    internal const string TAG_THIS_ELEMENT = "Book";
    internal const string TAG_ATTRIBUTE_NAME = "Name";
    internal const string TAG_ELEMENT_PATHNAME = "PathName";
    internal const string TAG_ELEMENT_GROUPNAME = "GroupName";
    internal const string TAG_ATTRIBUTE_AUTHOR = "Author";
    internal const string TAG_ATTRIBUTE_BOOKTYPE = "BookType";
    #endregion XML Tag names

    public static List<string> ArchivedBookExtensions = new List<string>() { ".rar", ".cbr", ".zip", ".cbz", ".pdf" };

    public enum BookTypeEnum {
      unknown,
      Folder,
      Pdf,
      Rar,
      Zip
    }

    public string Name { get; set; }
    public string Pathname { get; set; }
    public string Author { get; set; }
    public bool IsExcluded { get; set; }
    public BookTypeEnum BookType { get; set; }
    public TPageCollection Pages { get; set; }
    public string Category { get; set; }
    public string Serie {
      get {
        return GetSerie(Name, Pathname);
      }
    }
    public string Description {
      get {
        if (string.IsNullOrWhiteSpace(Serie)) {
          return Name;
        }
        return Name.Substring(Serie.Length + 3);
      }
    }
    public string CoverPage {
      get {
        string Cover = Path.Combine(Pathname, "folder.jpg");
        if (File.Exists(Cover)) {
          return Cover;
        }
        return Directory.GetFiles(Pathname).OrderBy(x => x).First();
      }
    }

    #region Constructors
    public TBook() {
      Name = "";
      Pathname = "";
      Author = "";
      IsExcluded = false;
      BookType = BookTypeEnum.unknown;
      Pages = new TPageCollection();
      Category = "";
    }

    public TBook(string name, string pathname)
      : this() {
      Name = name;
      Pathname = pathname;
      BookType = BookTypeEnum.Folder;
    }

    public TBook(string name, string pathname, string rootFolder, BookTypeEnum type)
      : this() {
      Name = name;
      Pathname = pathname;
      Category = GetGroupName(rootFolder, pathname);
      BookType = type;
    }



    public TBook(TBook book) {
      Name = book.Name;
      Pathname = book.Pathname;
      Author = book.Author;
      IsExcluded = book.IsExcluded;
      BookType = book.BookType;
      Pages = new TPageCollection(book.Pages);
      Category = book.Category;
    }
    public TBook(XElement book) {
      Name = book.SafeReadAttribute<string>(TAG_ATTRIBUTE_NAME, "");
      Pathname = book.SafeReadElementValue<string>(TAG_ELEMENT_PATHNAME, "");
      Category = book.SafeReadElementValue<string>(TAG_ELEMENT_GROUPNAME, "");
      Author = book.SafeReadAttribute<string>(TAG_ATTRIBUTE_AUTHOR, "");
      IsExcluded = book.SafeReadAttribute<bool>("IsExcluded", false);
      BookType = (BookTypeEnum)Enum.Parse(typeof(BookTypeEnum), book.SafeReadAttribute<string>(TAG_ATTRIBUTE_BOOKTYPE, "unknown"));
      try { Pages = new TPageCollection(book.Element("Pages")); } catch { Pages = null; }
    }
    #endregion Constructors

    public override string ToString() {
      return Name;
    }

    #region IToXml Members
    public XElement ToXml(bool full) {
      XElement RetVal = new XElement(TAG_THIS_ELEMENT);
      RetVal.SetAttributeValue(TAG_ATTRIBUTE_NAME, Name);
      RetVal.SetElementValue(TAG_ELEMENT_PATHNAME, Pathname);
      RetVal.SetElementValue(TAG_ELEMENT_GROUPNAME, Category);
      RetVal.SetAttributeValue(TAG_ATTRIBUTE_AUTHOR, Author);
      RetVal.SetAttributeValue("IsExcluded", IsExcluded);
      RetVal.SetAttributeValue(TAG_ATTRIBUTE_BOOKTYPE, BookType.ToString());
      if (full) {
        if (Pages != null && Pages.Count > 0) {
          RetVal.Add(Pages.ToXml());
        }
      } else {
        if (Pages != null && Pages.Count > 0) {
          XElement PagesXml = new XElement("Pages");
          PagesXml.SetAttributeValue("Count", Pages.Count);
          RetVal.Add(PagesXml);
        }
      }
      return RetVal;
    }
    public XElement ToXml() {
      return ToXml(true);
    }

    #endregion

    #region IComparable<Book> Members

    public int CompareTo(TBook other) {
      return string.Compare(this.Name, other.Name);
    }

    #endregion

    public bool IsIdentical(TBook otherBook) {
      if (otherBook == null) {
        return false;
      }
      if (Name != otherBook.Name) {
        return false;
      }
      if (BookType != otherBook.BookType) {
        return false;
      }
      if (!Pages.IsIdentical(otherBook.Pages)) {
        return false;
      }
      return true;
    }

    public void Import(string bookPath, string bookName, string rootFolder = "") {
      #region Validate parameters
      if (string.IsNullOrWhiteSpace(bookName) || string.IsNullOrWhiteSpace(bookPath)) {
        Trace.WriteLine("Unable to import an empty book name");
        return;
      }
      #endregion Validate parameters
      BookType = GetBookType(bookName);
      Name = Path.GetFileNameWithoutExtension(bookName);
      Category = GetGroupName(rootFolder, bookPath);
      Pathname = bookPath;
      if (BookType == BookTypeEnum.Folder) {
        Pages.Import(bookPath);
      }
    }

    private BookTypeEnum GetBookType(string bookName) {
      if (bookName == "") {
        return BookTypeEnum.Folder;
      }
      switch (Path.GetExtension(bookName)) {
        case ".rar":
        case ".cbr":
          return BookTypeEnum.Rar;
        case ".zip":
        case ".cbz":
          return BookTypeEnum.Zip;
        case ".pdf":
          return BookTypeEnum.Pdf;
        default:
          return BookTypeEnum.unknown;
      }
    }
    private string GetGroupName(string rootFolder, string bookPath) {
      if (rootFolder == bookPath) {
        return "";
      }
      return bookPath.Substring(rootFolder.Length + 1);
    }
    private string GetSerie(string name, string pathname) {
      if (name == "" || pathname == "") {
        return "";
      }
      string PathWithoutBookName = pathname.Left(pathname.Length - name.Length - 1);
      return PathWithoutBookName.Substring(PathWithoutBookName.LastIndexOf("\\") + 1);
    }
  }

}
