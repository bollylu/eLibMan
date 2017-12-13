﻿using BLTools;
using BLTools.Json;
using BLTools.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BDIndexLib {
  public class TBookCollection : TBookList {

    private static readonly string DirectorySeparator = $"{Path.DirectorySeparatorChar}";

    public string Name {
      get {
        lock ( _LockItems ) {
          if ( !Items.Any() ) {
            return "";
          }
          return Items.First().CollectionName;
        }
      }
    }

    public string RelativePath {
      get {
        if ( string.IsNullOrWhiteSpace(_RelativePath) ) {
          lock ( _LockItems ) {
            if ( !Items.Any() ) {
              _RelativePath = "";
            }
            _RelativePath = Items.First().RelativePath.BeforeLast(DirectorySeparator);
          }
        }
        return _RelativePath;
      }
    }
    private string _RelativePath;

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TBookCollection() : base() { }

    public TBookCollection(IEnumerable<TBook> books) : this() {
      if ( books == null ) {
        return;
      }
      lock ( _LockItems ) {
        Items = new List<TBook>(books);
      }
    }

    public TBookCollection(TBookCollection bookCollection) : this() {
      if ( bookCollection == null ) {
        return;
      }
      lock ( _LockItems ) {
        Items = new List<TBook>(bookCollection);
      }
    }

    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public override void Clear() {
      _RelativePath = null;
      base.Clear();
    }

    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.AppendLine(TextBox.BuildDynamicIBM($"{Name}"));
      lock ( _LockItems ) {
        foreach ( TBook BookItem in Items ) {
          RetVal.Append($"{BookItem.BookType.ToString().PadRight(8, '.') } | ");
          RetVal.Append($"{BookItem.Number.PadRight(6, '.')} | ");
          RetVal.Append($"{BookItem.Name}");
          RetVal.AppendLine();
        }
      }
      RetVal.AppendLine($"  => {RelativePath}");
      return RetVal.ToString();
    }

    public override IJsonValue ToJson() {
      JsonObject RetVal = new JsonObject();
      RetVal.Add(nameof(Name), Name);
      JsonArray BookItems = new JsonArray();
      lock ( _LockItems ) {
        foreach ( TBook BookItem in Items ) {
          JsonObject JsonBook = new JsonObject();
          JsonBook.Add(nameof(TBook.Name), BookItem.Name);
          JsonBook.Add(nameof(TBook.Number), BookItem.Number);
          BookItems.Add(JsonBook);
        }
      }
      RetVal.Add(nameof(Items), BookItems);
      return RetVal;
    }

  }
}
