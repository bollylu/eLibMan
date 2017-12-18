using System;
using BLTools;
using BLTools.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace BDIndexLib {
  public class TBook : BaseItem, IToJson, IDisposable {

    public const string XML_THIS_ELEMENT = "Book";
    public const string XML_ATTRIBUTE_NAME = "Name";
    public const string XML_ATTRIBUTE_NUMBER = "Number";
    public const string XML_ATTRIBUTE_BOOK_TYPE = "BookType";
    public const string XML_ATTRIBUTE_COLLECTION_NAME = "CollectionName";

#if DEBUG
    public static bool IsDebug = true;
#else
    public static bool IsDebug = false;
#endif

    public static string[] Extensions = new string[] { ".pdf", ".rar", ".zip", ".cbr", ".cbz" };

    #region --- Public properties ------------------------------------------------------------------------------
    public string RelativePath { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public string CollectionName { get; set; }
    public EBookType BookType { get; set; }
    public TPageList Pages { get; } = new TPageList();
    #endregion --- Public properties ---------------------------------------------------------------------------

    #region --- IParent --------------------------------------------
    public TBookList ParentBookList => GetParent<TBookList>();
    public TRepository ParentRepository => GetParent<TRepository>();
    #endregion --- IParent --------------------------------------------

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TBook() : base() {
      Pages.Parent = this;
      BookType = EBookType.unknown;
      CollectionName = "";
      Number = "";
      Name = "";
      RelativePath = "";
    }

    public TBook(string name, EBookType bookType = EBookType.folder, string relativePath = "") : this() {

      if ( bookType == EBookType.unknown ) {
        BookType = _FindBookType(name);
      } else {
        BookType = bookType;
      }

      string ProcessedName = BookType.IsFileType() ? name.BeforeLast(".") : name;

      if ( ProcessedName.Contains("[") || ProcessedName.Contains("=") || ProcessedName.Contains("{") ) {
        if ( !_NewParse(ProcessedName) ) {
          Name = ProcessedName;
        }
      } else {
        if ( !_Parse(ProcessedName) ) {
          Name = ProcessedName;
        }
      }

      RelativePath = relativePath;

    }


    public TBook(TBook book) : this() {
      RelativePath = book.RelativePath;
      Name = book.Name;
      Number = book.Number;
      CollectionName = book.CollectionName;
      BookType = book.BookType;
      Pages.Clear();
      foreach ( TPage PageItem in book.Pages ) {
        Pages.Add(PageItem);
      }
    }

    public void Dispose() {
      Pages.Dispose();
      Parent = null;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"{BookType.ToString().PadRight(8, '.') } | ");
      RetVal.Append($"{CollectionName.PadRight(80, '.')} | ");
      RetVal.Append($"{Number.PadRight(6, '.')} | ");
      RetVal.Append($"{Name.PadRight(80, '.')} | ");
      RetVal.Append($"{RelativePath}");
      return RetVal.ToString();
    }
    public IJsonValue ToJson() {
      JsonObject RetVal = new JsonObject();
      RetVal.Add(nameof(Name), Name);
      RetVal.Add(nameof(Number), Number);
      if ( !string.IsNullOrWhiteSpace(CollectionName) ) {
        RetVal.Add(nameof(CollectionName), CollectionName);
      }
      return RetVal;
    }


    public XElement ToXml() {
      XElement RetVal = new XElement(XML_THIS_ELEMENT);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NAME, Name);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_NUMBER, Number);
      RetVal.SetAttributeValue(XML_ATTRIBUTE_BOOK_TYPE, BookType.ToString());
      RetVal.SetAttributeValue(XML_ATTRIBUTE_COLLECTION_NAME, CollectionName);
      return RetVal;
    }
    #endregion --- Converters -------------------------------------------------------------------------------------

    private static Regex _CTN_Pattern = new Regex(@"^(?<coll>.+) +- +(?<number>S\d+|R\d+|T\d+\.*\d*[a-z]*|THS\d*|HS\d*|BO\d+) +- *(?<name>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex _CTN2_Pattern = new Regex(@"^(?<coll>.+) +- +(?<number>\d+) +- *(?<name>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex _CT_Pattern = new Regex(@"^(?<coll>.+) +- +(?<number>\d+|S\d+|R\d+|T\d+\.*\d*[a-z]*|THS\d*|HS\d*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static Regex _CN_Pattern = new Regex(@"^(?<coll>.+) +- +(?<name>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Regex _GetNumber = new Regex(@"=(?<number>.*)=", RegexOptions.Compiled);
    private static Regex _GetName = new Regex(@"\[(?<name>.*)\]", RegexOptions.Compiled);
    private static Regex _GetCollectionNames = new Regex(@"(?<coll>{.*?})", RegexOptions.Compiled);

    private bool _Parse(string source) {

      Match m;

      m = _CTN_Pattern.Match(source);
      if ( m.Groups.Count == 4 ) {

        Name = m.Groups["name"].Value;
        CollectionName = m.Groups["coll"].Value;
        Number = m.Groups["number"].Value;

        return true;
      }

      m = _CTN2_Pattern.Match(source);
      if ( m.Groups.Count == 4 ) {

        Name = m.Groups["name"].Value;
        CollectionName = m.Groups["coll"].Value;
        Number = m.Groups["number"].Value;

        return true;
      }

      m = _CT_Pattern.Match(source);
      if ( m.Groups.Count == 3 ) {

        CollectionName = m.Groups["coll"].Value;
        Number = m.Groups["number"].Value;

        return true;
      }

      m = _CN_Pattern.Match(source);
      if ( m.Groups.Count == 3 ) {

        Name = m.Groups["name"].Value;
        CollectionName = m.Groups["coll"].Value;

        return true;
      }

      Trace.WriteLine(source);

      return false;
    }

    private EBookType _FindBookType(string name) {
      if ( string.IsNullOrWhiteSpace(name) ) {
        return EBookType.unknown;
      }
      string Extension = Path.GetExtension(name).AfterLast(".");
      return TBookType.Parse(Extension);
    }

    private bool _NewParse(string processedName) {
      if ( string.IsNullOrWhiteSpace(processedName) ) {
        return false;
      }

      Match MatchNumber = _GetNumber.Match(processedName);
      if (MatchNumber.Success) {
        Number = MatchNumber.Groups["number"].Value;
      }

      Match MatchName = _GetName.Match(processedName);
      if ( MatchName.Success ) {
        Name = MatchName.Groups["name"].Value;
      }

      StringBuilder CollectionBuilder = new StringBuilder();
      foreach ( Match MatchCollectionNameItem in _GetCollectionNames.Matches(processedName) ) {
        string TextItem = MatchCollectionNameItem.Groups["coll"].Value.TrimStart('{').TrimEnd('}');
        CollectionBuilder.Insert(0, $"{TextItem}/");
      }
      if (CollectionBuilder.Length>0) {
        CollectionBuilder.Truncate(1);
      }
      CollectionName = CollectionBuilder.ToString();
      return true; 

    }

  }
}
