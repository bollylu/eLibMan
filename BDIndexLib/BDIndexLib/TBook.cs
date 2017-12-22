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

    #region --- Constants --------------------------------------------
    public const string XML_THIS_ELEMENT = "Book";
    public const string XML_ATTRIBUTE_NAME = "Name";
    public const string XML_ATTRIBUTE_NUMBER = "Number";
    public const string XML_ATTRIBUTE_BOOK_TYPE = "BookType";
    public const string XML_ATTRIBUTE_COLLECTION_NAME = "CollectionName";
    #endregion --- Constants --------------------------------------------

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

    public readonly SortedDictionary<int, string> CollectionNameComponents = new SortedDictionary<int, string>();
    public string CollectionName => string.Join(" / ", CollectionNameComponents.Values);
    public string DisplayCollectionName => string.Join(" / ", CollectionNameComponents.Values.Select(x => x.MoveArticleToStart()));

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
      Number = "";
      Name = "";
      RelativePath = "";
    }

    public TBook(string name, TParsingParameters parsingParameters, EBookType bookType = EBookType.folder, string relativePath = "") : this() {

      if ( bookType == EBookType.unknown ) {
        BookType = _FindBookType(name);
      } else {
        BookType = bookType;
      }

      string ProcessedName = BookType.IsFileType() ? name.BeforeLast(".") : name;

      if ( !_NewParse(ProcessedName, parsingParameters) ) {
        if ( !_Parse(ProcessedName) ) {
          Name = ProcessedName;
        }
      }

      if ( !CollectionNameComponents.Any() ) {
        int i = 0;
        foreach ( string PathItem in relativePath.Split('\\') ) {
          CollectionNameComponents.Add(i++, PathItem);
        }

      }

      RelativePath = relativePath;

    }

    public TBook(TBook book) : this() {
      RelativePath = book.RelativePath;
      Name = book.Name;
      Number = book.Number;
      foreach ( KeyValuePair<int, string> CollectionNameComponentItem in book.CollectionNameComponents ) {
        CollectionNameComponents.Add(CollectionNameComponentItem.Key, CollectionNameComponentItem.Value);
      }
      BookType = book.BookType;
      Pages.Clear();
      foreach ( TPage PageItem in book.Pages ) {
        Pages.Add(PageItem);
      }
    }

    public void Dispose() {
      CollectionNameComponents.Clear();
      Pages.Dispose();
      Parent = null;
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"{BookType.ToString().PadRight(8, '.') } | ");
      RetVal.Append($"{CollectionName.PadRight(80, '.')} | ");
      if ( !string.IsNullOrWhiteSpace(Number) ) {
        RetVal.Append($"{Number.PadRight(6, '.')} | ");
      }
      if ( !string.IsNullOrWhiteSpace(Name) ) {
        RetVal.Append($"{Name.PadRight(80, '.')} | ");
      }
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

    private static Regex GetOrder = new Regex(@"^\((?<order>.*?)\).*$", RegexOptions.Compiled);

    private bool _Parse(string source) {

      CollectionNameComponents.Clear();

      Match m;

      m = _CTN_Pattern.Match(source);
      if ( m.Groups.Count == 4 ) {

        Name = m.Groups["name"].Value;
        CollectionNameComponents.Add(1, m.Groups["coll"].Value.Replace(" - ", " / "));
        Number = m.Groups["number"].Value;

        return true;
      }

      m = _CTN2_Pattern.Match(source);
      if ( m.Groups.Count == 4 ) {

        Name = m.Groups["name"].Value;
        CollectionNameComponents.Add(1, m.Groups["coll"].Value.Replace(" - ", " / "));
        Number = m.Groups["number"].Value;

        return true;
      }

      m = _CT_Pattern.Match(source);
      if ( m.Groups.Count == 3 ) {

        CollectionNameComponents.Add(1, m.Groups["coll"].Value.Replace(" - ", " / "));
        Number = m.Groups["number"].Value;

        return true;
      }

      m = _CN_Pattern.Match(source);
      if ( m.Groups.Count == 3 ) {

        Name = m.Groups["name"].Value;
        CollectionNameComponents.Add(1, m.Groups["coll"].Value.Replace(" - ", " / "));

        return true;
      }

      //Trace.WriteLine(source);

      return false;
    }

    private EBookType _FindBookType(string name) {
      if ( string.IsNullOrWhiteSpace(name) ) {
        return EBookType.unknown;
      }
      string Extension = Path.GetExtension(name).AfterLast(".");
      return TBookType.Parse(Extension);
    }

    private bool _NewParse(string processedName, TParsingParameters parsingParameters) {

      if ( string.IsNullOrWhiteSpace(processedName) ) {
        return false;
      }

      Regex _GetNumber = new Regex($@"(?<number>{Regex.Escape(parsingParameters.NumberStartDelimiter)}.*?{Regex.Escape(parsingParameters.NumberEndDelimiter)})");
      Regex _GetName = new Regex($@"(?<title>{Regex.Escape(parsingParameters.TitleStartDelimiter)}.*?{Regex.Escape(parsingParameters.TitleEndDelimiter)})");
      Regex _GetCollectionNames = new Regex($@"(?<coll>{Regex.Escape(parsingParameters.CollectionNameStartDelimiter)}.*?{Regex.Escape(parsingParameters.CollectionNameEndDelimiter)})");

      Match MatchNumber = _GetNumber.Match(processedName);
      Match MatchName = _GetName.Match(processedName);
      MatchCollection MatchCollectionNames = _GetCollectionNames.Matches(processedName);

      if ( !MatchNumber.Success && !MatchName.Success && MatchCollectionNames.Count == 0 ) {
        return false;
      }

      if ( MatchNumber.Success ) {
        Number = MatchNumber.Groups["number"].Value.After(parsingParameters.NumberStartDelimiter).BeforeLast(parsingParameters.NumberEndDelimiter).Trim();
      }

      if ( MatchName.Success ) {
        Name = MatchName.Groups["title"].Value.After(parsingParameters.TitleStartDelimiter).BeforeLast(parsingParameters.TitleEndDelimiter).Trim();
      }
      //} else {
      //  Name = "(missing)";
      //}

      int i;
      switch ( parsingParameters.CollectionNamesOrder ) {
        case TParsingParameters.ECollectionNameOrder.Normal:
          i = 0;
          break;
        case TParsingParameters.ECollectionNameOrder.Reverse:
          i = int.MaxValue;
          break;
        default:
          i = 0;
          break;
      }

      foreach ( Match MatchCollectionNameItem in MatchCollectionNames ) {
        string TextItem = MatchCollectionNameItem.Value.After(parsingParameters.CollectionNameStartDelimiter).BeforeLast(parsingParameters.CollectionNameEndDelimiter).Trim();
        Match MatchOrder = GetOrder.Match(TextItem);
        if ( MatchOrder.Success ) {
          CollectionNameComponents.Add((byte)( MatchOrder.Groups["order"].Value.After('(').First() ), TextItem.Substring(3));
        } else {
          CollectionNameComponents.Add(i, TextItem);
          switch ( parsingParameters.CollectionNamesOrder ) {
            case TParsingParameters.ECollectionNameOrder.Normal:
              i++;
              break;
            case TParsingParameters.ECollectionNameOrder.Reverse:
              i--;
              break;
          }
        }
      }

      return true;

    }


  }
}
