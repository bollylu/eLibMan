using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BLTools;

namespace FolderManagement {
  public class TPage : IToXml {
    
    public string Name { get; set; }
    public string Quality {get; set;}

    public TPage() { }
    public TPage(string name) {
      Name = name;
    }
    public TPage(TPage page) {
      Name = page.Name;
      Quality = page.Quality;
    }
    public TPage(XElement page) {
      Name = page.SafeReadAttribute<string>("Name", "");
      Quality = page.SafeReadAttribute<string>("Quality", "V");
    }

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendFormat("Page \"{0}\", Quality=\"{1}\"", Name, Quality);
      return RetVal.ToString();
    }


    #region IToXml Members

    public XElement ToXml() {
      XElement RetVal = new XElement("Page");
      RetVal.SetAttributeValue("Name", Name);
      RetVal.SetAttributeValue("Quality", Quality);
      return RetVal;
    }

    #endregion

    public bool IsIdentical(TPage otherPage) {
      if (otherPage == null) {
        return false;
      }
      if (Name != otherPage.Name) {
        return false;
      }
      return true;
    }
  }

  
}
