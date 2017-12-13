using System;
using System.Collections.Generic;
using System.Text;

namespace BDIndexLib {
  public interface IParent {
    IParent Parent { get; }
    T GetParent<T>() where T : class, IParent;
  }
}
