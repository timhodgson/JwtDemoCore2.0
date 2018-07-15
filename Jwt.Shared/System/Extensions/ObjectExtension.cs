namespace System
{
  public static partial class ObjectExtension
  {
    /// <summary>
    /// Check if null or DbNull, 
    /// an Empty string is not null!
    /// </summary>
    /// <param name="Expr"></param>
    /// <returns></returns>
    public static Boolean IsNull(this Object Expr)
    {
      return (Expr == null || Expr == DBNull.Value);
    }



    public static Int32? ToInt32Nullable(this Object Expr)
    {
      Int32? result = null;

      if (Expr.IsNull())
        return result;

      result = Expr is Enum ? (Int32)Expr : Convert.ToInt32(Expr);

      return result;
    }


    public static Int32 ToInt32(this Object Expr, Int32 DefaultValue = 0)
    {
      Int32 result = Expr is Enum ? (Int32)Expr : ToInt32Nullable(Expr) ?? DefaultValue;

      return result;
    }



    //public static Object GetPropertyValue(this Object Instance, String PropertyName)
    //{
    //  Object result = null;

    //  if (Instance == null || PropertyName.IsNullOrEmpty())
    //    return result;

    //  // Bindattributes are needed to get property "Value" from FwField and inherited types
    //  BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

    //  PropertyInfo pi = Instance.GetType().GetProperty(PropertyName, bindingAttr);

    //  if (pi == null)
    //  {
    //    // try again without bindattributes
    //    pi = Instance.GetType().GetProperty(PropertyName);
    //  }
      
    //  if (pi != null)
    //    result = pi.GetValue(Instance, null);

    //  return result;
    //}

  }
}
