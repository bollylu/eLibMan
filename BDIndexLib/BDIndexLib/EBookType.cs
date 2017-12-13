using System;

namespace BDIndexLib {

  public enum EBookType {
    unknown,
    folder,
    pdf,
    cbr,
    rar,
    cbz,
    zip,
    cb7
  }

  public static class TBookType  {

    public static EBookType Parse(string source) {
      try {
        return (EBookType)Enum.Parse(typeof(EBookType), source, true);
      } catch {
        return EBookType.unknown;
      }
    }

    public static bool IsFileType(this EBookType bookType) {
      switch (bookType) {
        case EBookType.pdf:
        case EBookType.cbr:
        case EBookType.rar:
        case EBookType.cbz:
        case EBookType.zip:
        case EBookType.cb7:
          return true;
        default:
          return false;
      }
    }
  }


  
}