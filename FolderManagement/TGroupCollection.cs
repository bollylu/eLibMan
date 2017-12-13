using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using BLTools;

namespace FolderManagement {

  public class TGroupCollection : List<TGroup>, IToXml {

    public string StorageName { get; set; }
    public int BookCount {
      get {
        return this.Sum(g => g.BookCount);
      }
    }
    

    #region Constructor(s)
    public TGroupCollection() {
    }
    public TGroupCollection(TGroupCollection Groups) {
      foreach (TGroup GroupItem in Groups) {
        this.Add(new TGroup(GroupItem));
      }
    }
    public TGroupCollection(IEnumerable<TGroup> Groups) {
      foreach (TGroup GroupItem in Groups) {
        this.Add(new TGroup(GroupItem));
      }
    }
    public TGroupCollection(IList<XElement> Groups) {
      foreach (XElement GroupItem in Groups) {
        this.Add(new TGroup(GroupItem));
      }
    }
    public TGroupCollection(XElement Groups) {
      foreach (XElement GroupItem in Groups.Elements("group")) {
        this.Add(new TGroup(GroupItem));
      }
    }
    #endregion Constructor(s)

    #region Converters
    public XElement ToXml() {
      XElement RetVal = new XElement("groups");
      foreach (TGroup GroupItem in this) {
        RetVal.Add(GroupItem.ToXml());
      }
      return RetVal;
    }
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      foreach (TGroup GroupItem in this) {
        RetVal.AppendLine(string.Format("Group: {0}", GroupItem.ToString()));
      }
      return RetVal.ToString();
    }
    #endregion Converters

    

    public event EventHandler<BeforeImportEventArgs> OnBeforeImport;
  }

}
