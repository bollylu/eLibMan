using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BLTools;
using BLTools.Json;
using System.Linq;
using System.Xml.Linq;

namespace BDIndexLib {
  public class TBookList : BaseItem, IToJson, IEnumerable<TBook>, IDisposable {

    public const string XML_THIS_ELEMENT = "Books";

    protected object _LockItems = new object();
    protected List<TBook> Items { get; set; } = new List<TBook>();

    public TRepository ParentRepository => GetParent<TRepository>();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TBookList() : base() { }

    public TBookList(IEnumerable<TBook> books) : this() {
      if ( books == null || books.Count() == 0 ) {
        return;
      }
      lock ( _LockItems ) {
        foreach ( TBook BookItem in books ) {
          Add(new TBook(BookItem));
        }
      }
    }

    public TBookList(TBookList books) : this() {
      if ( books == null ) {
        return;
      }
      lock ( _LockItems ) {
        Items = new List<TBook>(books);
      }
    }

    public void Dispose() {
      lock ( _LockItems ) {
        foreach ( TBook BookItem in Items ) {
          BookItem.Dispose();
        }
        Items.Clear();
      }
      Parent = null;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      lock ( _LockItems ) {
        foreach ( TBook BookItem in Items ) {
          RetVal.AppendLine(BookItem.ToString());
        }
      }
      return RetVal.ToString();
    }

    public virtual IJsonValue ToJson() {
      JsonArray RetVal = new JsonArray();
      lock ( _LockItems ) {
        foreach ( TBook BookItem in Items ) {
          RetVal.Add(BookItem.ToJson());
        }
      }
      return RetVal;
    }


    public XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);
      lock(_LockItems) {
        foreach(TBook BookItem in Items) {
          RetVal.Add(BookItem.ToXml());
        }
      }
      return RetVal;
    }

    #region --- Linq --------------------------------------------
    public IEnumerator<TBook> GetEnumerator() {
      lock ( _LockItems ) {
        return Items.GetEnumerator();
      }
    }

    IEnumerator IEnumerable.GetEnumerator() {
      lock ( _LockItems ) {
        return Items.GetEnumerator();
      }
    }
    #endregion --- Linq --------------------------------------------

    #region --- Items management --------------------------------------------
    public virtual void Clear() {
      lock ( _LockItems ) {
        Items.Clear();
      }
    }

    public virtual void Add(TBook bookItem) {
      lock ( _LockItems ) {
        bookItem.Parent = this;
        Items.Add(bookItem);
      }
    }
    #endregion --- Items management --------------------------------------------

    public TBookCollection GetCollection(string collectionName) {
      lock ( _LockItems ) {
        return new TBookCollection(Items.Where(x => x.CollectionName == collectionName));
      }
    }

    public TBookCollection GetCollectionLess() {
      lock ( _LockItems ) {
        return new TBookCollection(Items.Where(x => x.CollectionName == ""));
      }
    }

    public IEnumerable<TBookCollection> EnumerateCollections() {
      lock ( _LockItems ) {
        if ( !Items.Any() ) {
          yield break;
        }
        foreach ( IEnumerable<TBook> BookItems in Items.OrderBy(x => x.CollectionName).ThenBy(x => x.Number).ThenBy(x => x.Name).GroupBy(x => x.CollectionName) ) {
          yield return new TBookCollection(BookItems);
        }
      }
      yield break;

    }

    public IEnumerable<TBook> GetMissingFrom(TBookList bookList) {
      lock ( _LockItems ) {
        if ( bookList == null ) {
          yield break;
        }

        foreach ( TBook BookItem in bookList ) {
          if ( Items.FirstOrDefault(x => x.CollectionName.ToLower() == BookItem.CollectionName.ToLower() && x.Number.ToLower() == BookItem.Number.ToLower() && x.Name.ToLower() == BookItem.Name.ToLower()) == null ) {
            yield return BookItem;
          }
        }
      }
    }
  }
}
