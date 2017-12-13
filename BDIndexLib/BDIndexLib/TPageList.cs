using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BLTools;
using BLTools.Json;

namespace BDIndexLib {
  public class TPageList : BaseItem, IToJson, IEnumerable<TPage>, IDisposable {

    private object _LockItems = new object();
    public List<TPage> Items { get; } = new List<TPage>();

    public TBook ParentBook => GetParent<TBook>();
    public TBookList ParentBookList => GetParent<TBookList>();
    public TRepository ParentRepository => GetParent<TRepository>();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPageList() { }

    public void Dispose() {
      lock ( _LockItems ) {
        foreach ( TPage PageItem in Items ) {
          PageItem.Dispose();
        }
        Items.Clear();
      }
      Parent = null;
    } 
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public IJsonValue ToJson() {
      JsonArray RetVal = new JsonArray();
      foreach(TPage PageItem in Items) {
        RetVal.Add(PageItem.ToJson());
      }
      return RetVal;
    }

    #region --- Linq --------------------------------------------
    public IEnumerator<TPage> GetEnumerator() {
      return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
      return Items.GetEnumerator();
    }
    #endregion --- Linq --------------------------------------------

    #region --- Items management --------------------------------------------
    public void Clear() {
      lock ( _LockItems ) {
        Items.Clear();
      }
    }

    public void Add(TPage pageItem) {
      pageItem.Parent = this;
      lock ( _LockItems ) {
        Items.Add(pageItem);
      }
    } 
    #endregion --- Items management --------------------------------------------

  }
}
