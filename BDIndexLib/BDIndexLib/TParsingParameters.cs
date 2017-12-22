using BLTools;
using BLTools.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BDIndexLib {
  public class TParsingParameters {

    #region --- Constants --------------------------------------------
    public const string JSON_THIS_ELEMENT = "Parsing";
    public const string JSON_ITEM_TITLE_START_DELIMITER = "TitleStartDelimiters";
    public const string JSON_ITEM_TITLE_END_DELIMITER = "TitleEndDelimiters";
    public const string JSON_ITEM_NUMBER_START_DELIMITER = "NumberStartDelimiters";
    public const string JSON_ITEM_NUMBER_END_DELIMITER = "NumberEndDelimiters";
    public const string JSON_ITEM_COLLECTION_NAME_START_DELIMITER = "CollectionNameStartDelimiters";
    public const string JSON_ITEM_COLLECTION_NAME_END_DELIMITER = "CollectionNameEndDelimiters";
    public const string JSON_ITEM_COLLECTION_NAME_ORDER = "CollectionNamesOrder";

    public const string DEFAULT_TITLE_START_DELIMITER = @"[";
    public const string DEFAULT_TITLE_END_DELIMITER = @"]";

    public const string DEFAULT_NUMBER_START_DELIMITER = "=";
    public const string DEFAULT_NUMBER_END_DELIMITER = "=";

    public const string DEFAULT_COLLECTION_NAME_START_DELIMITER = @"{";
    public const string DEFAULT_COLLECTION_NAME_END_DELIMITER = @"}";

    public const ECollectionNameOrder DEFAULT_COLLECTION_NAMES_ORDER = ECollectionNameOrder.Normal;
    #endregion --- Constants --------------------------------------------

    public enum ECollectionNameOrder {
      Unknown,
      Normal,
      Reverse,
      Numbered
    }

    #region --- Public properties ------------------------------------------------------------------------------
    public string TitleStartDelimiter {
      get {
        if ( string.IsNullOrEmpty(_TitleStartDelimiter) ) {
          return DEFAULT_TITLE_START_DELIMITER;
        }
        return _TitleStartDelimiter;
      }
      set {
        _TitleStartDelimiter = value;
      }
    }
    private string _TitleStartDelimiter;

    public string TitleEndDelimiter {
      get {
        if ( string.IsNullOrEmpty(_TitleEndDelimiter) ) {
          return DEFAULT_TITLE_END_DELIMITER;
        }
        return _TitleEndDelimiter;
      }
      set {
        _TitleEndDelimiter = value;
      }
    }
    private string _TitleEndDelimiter;

    public string NumberStartDelimiter {
      get {
        if ( string.IsNullOrEmpty(_NumberStartDelimiter) ) {
          return DEFAULT_NUMBER_START_DELIMITER;
        }
        return _NumberStartDelimiter;
      }
      set {
        _NumberStartDelimiter = value;
      }
    }
    private string _NumberStartDelimiter;

    public string NumberEndDelimiter {
      get {
        if ( string.IsNullOrEmpty(_NumberEndDelimiter) ) {
          return DEFAULT_NUMBER_END_DELIMITER;
        }
        return _NumberEndDelimiter;
      }
      set {
        _NumberEndDelimiter = value;
      }
    }
    private string _NumberEndDelimiter;

    public string CollectionNameStartDelimiter {
      get {
        if ( string.IsNullOrEmpty(_CollectionNameStartDelimiter) ) {
          return DEFAULT_COLLECTION_NAME_START_DELIMITER;
        }
        return _CollectionNameStartDelimiter;
      }
      set {
        _CollectionNameStartDelimiter = value;
      }
    }
    private string _CollectionNameStartDelimiter;

    public string CollectionNameEndDelimiter {
      get {
        if ( string.IsNullOrEmpty(_CollectionNameEndDelimiter) ) {
          return DEFAULT_COLLECTION_NAME_END_DELIMITER;
        }
        return _CollectionNameEndDelimiter;
      }
      set {
        _CollectionNameEndDelimiter = value;
      }
    }
    private string _CollectionNameEndDelimiter;

    public ECollectionNameOrder CollectionNamesOrder {
      get {
        if ( _CollectionNamesOrder == ECollectionNameOrder.Unknown ) {
          return DEFAULT_COLLECTION_NAMES_ORDER;
        }
        return _CollectionNamesOrder;
      }
    }
    private ECollectionNameOrder _CollectionNamesOrder = ECollectionNameOrder.Unknown;
    #endregion --- Public properties ---------------------------------------------------------------------------

    public static TParsingParameters Default => new TParsingParameters();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TParsingParameters() { }

    public TParsingParameters(IJsonValue parameters) {
      _Init(parameters);
    }

    public TParsingParameters(TParsingParameters parameters) {
      _TitleStartDelimiter = parameters.TitleStartDelimiter;
      _TitleEndDelimiter = parameters.TitleEndDelimiter;
      _NumberStartDelimiter = parameters.NumberStartDelimiter;
      _NumberEndDelimiter = parameters.NumberEndDelimiter;
      _CollectionNameStartDelimiter = parameters.CollectionNameStartDelimiter;
      _CollectionNameEndDelimiter = parameters.CollectionNameEndDelimiter;
      _CollectionNamesOrder = parameters.CollectionNamesOrder;
    }

    private void _Init(IJsonValue parameters) {
      using ( JsonObject ParsingObject = parameters as JsonObject ) {
        _TitleStartDelimiter = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_TITLE_START_DELIMITER);
        _TitleEndDelimiter = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_TITLE_END_DELIMITER);

        _NumberStartDelimiter = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_NUMBER_START_DELIMITER);
        _NumberEndDelimiter = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_NUMBER_END_DELIMITER);

        _CollectionNameStartDelimiter = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_COLLECTION_NAME_START_DELIMITER);
        _CollectionNameEndDelimiter = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_COLLECTION_NAME_END_DELIMITER);

        string TempCollectionNamesOrder = ParsingObject.SafeGetValueFirst<string>(JSON_ITEM_COLLECTION_NAME_ORDER);
        if ( !string.IsNullOrEmpty(TempCollectionNamesOrder) ) {
          if ( !Enum.TryParse<ECollectionNameOrder>(TempCollectionNamesOrder, true, out _CollectionNamesOrder) ) {
            _CollectionNamesOrder = ECollectionNameOrder.Unknown;
          }
        }
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public bool Load(string filename) {
      if ( string.IsNullOrWhiteSpace(filename) ) {
        return false;
      }
      if ( !File.Exists(filename) ) {
        return false;
      }
      try {
        JsonObject Parameters = JsonValue.Load(filename) as JsonObject;
        JsonObject Parsing = Parameters.SafeGetValueFirst<JsonObject>(JSON_THIS_ELEMENT);
        _Init(Parsing);
        return true;
      } catch ( Exception ex ) {
        Trace.WriteLine($"Unable to load TParsingParameters from {filename} : {ex.Message}");
        return false;
      }
    }
  }
}
