using System;
using System.Collections.Generic;

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

    private static List<EBookType> FileTypes = new List<EBookType>() {
      EBookType.cbr,
      EBookType.rar,
      EBookType.cbz,
      EBookType.zip,
      EBookType.cb7,
      EBookType.pdf
    };

    

    public static EBookType Parse(string source) {
      try {
        return (EBookType)Enum.Parse(typeof(EBookType), source, true);
      } catch {
        return EBookType.unknown;
      }
    }

    public static bool IsFileType(this EBookType bookType) {
      return FileTypes.Contains(bookType);
    }

    public static bool IsFolderType(this EBookType bookType) {
      return bookType == EBookType.folder;
    }

  }


  
}