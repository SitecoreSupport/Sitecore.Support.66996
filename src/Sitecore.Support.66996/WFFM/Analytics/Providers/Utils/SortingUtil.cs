using System.Linq;
using Sitecore.WFFM.Abstractions.Data;

namespace Sitecore.Support.WFFM.Analytics.Providers.Utils
{
  public static class SortingUtil
  {
    private const string Ascending = "a";

    private const string Descending = "d";

    public static SortCriteria Create(string sortArgument)
    {
      bool flag = string.IsNullOrEmpty(sortArgument);
      SortCriteria result;
      if (flag)
      {
        result = null;
      }
      else
      {
        sortArgument = sortArgument.Trim();
        bool flag2 = sortArgument.Contains("|");
        if (flag2)
        {
          sortArgument = sortArgument.Split(new char[]
          {
                        '|'
          }).First<string>();
        }
        bool flag3 = sortArgument.StartsWith("a");
        if (flag3)
        {
          result = new SortCriteria(sortArgument.Substring("a".Length, sortArgument.Length - "a".Length), SortDirection.Asc);
        }
        else
        {
          bool flag4 = sortArgument.StartsWith("d");
          if (flag4)
          {
            result = new SortCriteria(sortArgument.Substring("d".Length, sortArgument.Length - "d".Length), SortDirection.Desc);
          }
          else
          {
            result = null;
          }
        }
      }
      return result;
    }
  }
}