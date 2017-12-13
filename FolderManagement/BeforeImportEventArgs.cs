using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderManagement {
  public class BeforeImportEventArgs : EventArgs {
    public int Count { get; set; }
    public BeforeImportEventArgs(int count) {
      Count = count;
    }
  }
}
