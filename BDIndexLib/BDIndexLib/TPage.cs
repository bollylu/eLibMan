using System;
using System.Collections.Generic;
using System.Text;
using BLTools;
using BLTools.Json;

namespace BDIndexLib {
  public class TPage : BaseItem, IToJson, IDisposable {

    public static string[] Extensions = new string[] { ".jpg", ".jpeg", ".tiff", ".png", ".bmp" };

    public string Name { get; set; }
    public string PageType { get; set; }

    #region --- IParent --------------------------------------------
    public TPageList ParentPageList => GetParent<TPageList>();
    public TBook ParentBook => GetParent<TBook>();
    public TBookList ParentBookList => GetParent<TBookList>();
    public TRepository ParentRepository => GetParent<TRepository>(); 
    #endregion --- IParent --------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPage() { }
    public TPage(string name, string pageType = "jpg") {
      Name = name;
      PageType = pageType;
    }
    public TPage(TPage pageItem) {
      if ( pageItem == null ) {
        return;
      }
      Name = pageItem.Name;
      PageType = pageItem.PageType;
    }

    public void Dispose() {
      Name = null;
      PageType = null;
      Parent = null;
    } 
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public IJsonValue ToJson() {
      JsonObject RetVal = new JsonObject();
      RetVal.Add(nameof(Name), Name);
      if ( PageType != "jpg" ) {
        RetVal.Add(nameof(PageType), PageType);
      }
      return RetVal;
    }


  }
}
