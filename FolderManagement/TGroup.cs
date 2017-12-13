using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BLTools;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FolderManagement {
  public class TGroup : IToXml, IComparable<TGroup> {

    public static List<string> BookExtensions;

    #region Public properties
    public string Name {
      get {
        return _Name ?? "";
      }
      set {
        _Name = value;
      }
    }
    private string _Name;
    public string Pathname { get; set; }
    public TBookCollection Books { get; set; }
    public int BookCount {
      get {
        return Books.Count;
      }
    }
    //public List<TGroup> ListToCompare { get; set; }
    //public bool ExistsInOtherGroup {
    //  get {
    //    if (ListToCompare == null) {
    //      return false;
    //    }
    //    return ListToCompare.Exists(c => c.Name == Name);
    //  }
    //}
    //public bool IsIdenticalInOtherGroup {
    //  get {
    //    if (ListToCompare == null) {
    //      return false;
    //    }
    //    TGroup OtherGroup = ListToCompare.Find(g => g.Name == Name);
    //    if (OtherGroup == null) {
    //      return false;
    //    }

    //    if (OtherGroup.BookCount != BookCount) {
    //      return false;
    //    }
    //    return true;
    //  }
    //}
    public string ListOfBooks {
      get {
        List<string> RetVal = new List<string>();
        foreach (TBook BookItem in Books.OrderBy(b => b.Pathname).ThenBy(b => b.Name)) {
          RetVal.Add(string.Format("{0}", BookItem.Name));
        }
        return string.Join("\n", RetVal);
      }
    }
    #endregion Public properties

    #region Constructor(s)
    static TGroup() {
      BookExtensions = new List<string>() { ".pdf", ".rar", ".zip", ".cbr", ".cbz" };
    }

    public TGroup() {
      Name = "";
      Pathname = "";
      Books = new TBookCollection();
    }

    public TGroup(string name, string pathname)
      : this() {
      Name = name;
      Pathname = pathname;
    }

    public TGroup(TGroup group) {
      Name = group._Name;
      Pathname = group.Pathname;
      Books = new TBookCollection(group.Books);
    }
    public TGroup(string name, string path, TBookCollection books)
      : this(name, path) {
      Books = books;
    }

    public TGroup(XElement group) {
      Name = group.SafeReadAttribute<string>("Name", "");
      Pathname = group.SafeReadAttribute<string>("Path", "");
    }
    #endregion Constructor(s)

    #region Converters
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendFormat("Group \"{0}\"", Name);
      RetVal.AppendFormat(", path=\"{0}\"", Pathname);
      if (Books != null && Books.Count > 0) {
        RetVal.AppendFormat(", Count of books={0}", Books.Count);
      }
      return RetVal.ToString();
    }
    public XElement ToXml() {
      XElement RetVal = new XElement("Group");
      RetVal.SetAttributeValue("Name", Name);
      RetVal.SetAttributeValue("Path", Pathname);
      if (Books != null && Books.Count > 0) {
        RetVal.Add(Books.ToXml());
      }
      return RetVal;
    }
    #endregion Converters

    

    #region IComparable<Group> Members

    public int CompareTo(TGroup other) {
      return string.Compare(this.Name, other.Name);
    }

    #endregion

  }


}
