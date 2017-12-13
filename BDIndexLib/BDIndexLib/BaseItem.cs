using System;
using System.Collections.Generic;
using System.Text;

namespace BDIndexLib {
  public class BaseItem : IParent {
    public IParent Parent { get; set; }
    public T GetParent<T>() where T : class, IParent {
      if ( Parent == null ) {
        return null;
      }
      if ( Parent.GetType() == typeof(T) ) {
        return (T)Parent;
      }
      return Parent.GetParent<T>();
    }
  }
}
