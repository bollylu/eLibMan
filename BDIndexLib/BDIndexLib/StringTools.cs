using BLTools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BDIndexLib {
  public static class StringTools {

    private static Regex _GetArticle = new Regex(@"^.+(?<article>\(.+?\))$", RegexOptions.Compiled);

    public static string MoveArticleToStart(this string source) {
      if ( string.IsNullOrWhiteSpace(source) ) {
        return source;
      }

      Match MatchArticle = _GetArticle.Match(source);
      if ( !MatchArticle.Success ) {
        return source;
      }

      string Article = MatchArticle.Groups["article"].Value;
      string CleanedArticle = Article.TrimStart('(').TrimEnd(')');
      string Space = CleanedArticle.EndsWith("'") ? "" : " ";
      StringBuilder CleanedSource = new StringBuilder(source.BeforeLast(Article));
      CleanedSource[0] = char.ToLower(CleanedSource[0]);

      string RetVal = $"{CleanedArticle}{Space}{CleanedSource}";
      return RetVal;

    }
  }
}
