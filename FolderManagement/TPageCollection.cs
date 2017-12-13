using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;
using BLTools;

namespace FolderManagement {

  public class TPageCollection : List<TPage>, IToXml {

    #region Constructor(s)
    public TPageCollection() {
    }
    public TPageCollection(TPageCollection pages) {
      if (pages != null) {
        foreach (TPage PageItem in pages) {
          this.Add(new TPage(PageItem));
        }
      }
    }
    public TPageCollection(IEnumerable<TPage> pages) {
      if (pages != null) {
        foreach (TPage PageItem in pages) {
          this.Add(new TPage(PageItem));
        }
      }
    }
    public TPageCollection(IList<XElement> pages) {
      foreach (XElement PageItem in pages) {
        this.Add(new TPage(PageItem));
      }
    }
    public TPageCollection(XElement pages) {
      foreach (XElement PageItem in pages.Elements("page")) {
        this.Add(new TPage(PageItem));
      }
    }
    #endregion Constructor(s)

    #region Converters
    public XElement ToXml() {
      XElement RetVal = new XElement("pages");
      foreach (TPage PageItem in this) {
        RetVal.Add(PageItem.ToXml());
      }
      return RetVal;
    }
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      foreach (TPage PageItem in this) {
        RetVal.AppendLine(string.Format("Page: {0}", PageItem.ToString()));
      }
      return RetVal.ToString();
    }
    #endregion Converters

    public bool IsIdentical(TPageCollection otherPages) {
      if (otherPages == null || this.Count != otherPages.Count) {
        return false;
      }
      foreach (TPage PageItem in this) {
        if (!PageItem.IsIdentical(otherPages.Find(p => p.Name == PageItem.Name))) {
          return false;
        }
      }
      return true;
    }

    internal void Import(string bookPath) {
      Trace.WriteLine(string.Format("Importing pages for {0}", bookPath));
    }
  }

}
