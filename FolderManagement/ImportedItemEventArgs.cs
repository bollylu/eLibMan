using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderManagement {
  public class ImportedItemEventArgs : EventArgs {
    public string Item { get; private set; }
    public ImportedItemEventArgs(string item) {
      Item = item;
    }
  }
}
