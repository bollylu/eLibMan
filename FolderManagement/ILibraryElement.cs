using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderManagement {
  public interface ILibraryElement {
    string Name { get; set; }
    string Pathname { get; set; }
  }
}
