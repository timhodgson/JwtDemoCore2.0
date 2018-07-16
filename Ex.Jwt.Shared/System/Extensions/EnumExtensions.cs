using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System
{
  public static class EnumEx
  {
    public static String ToDescription(this Enum instance)
    {
      FieldInfo fi = instance.GetType().GetField(instance.ToString());

      DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

      if (attributes != null && attributes.Length > 0)
        return attributes[0].Description;
      else
        return instance.ToString();
    }


    public static Boolean IsNullableEnum(this Type T)
    {
      Type u = Nullable.GetUnderlyingType(T);

      Boolean result = (u != null) && u.GetTypeInfo().IsEnum;

      return result;
    }


    public static Boolean IsCharEnum(this Enum instance)
    {
      return IsCharEnum(instance.GetType());
    }


    public static Boolean IsCharEnum(Type T)
    {
      Type enumType = T.IsNullableEnum() ? Nullable.GetUnderlyingType(T) : T;

      if (!enumType.GetTypeInfo().IsEnum)
        return false;

      try
      {
        return ((Int32[])Enum.GetValues(enumType)).All(i => Char.IsLetter((Char)i));
      }
      catch
      {
        return false;
      }
    }

    public static Object ToDbValue(this Enum instance)
    {
      if (instance.IsNull())
        return DBNull.Value;

      Int32 intValue = instance.ToInt32();

      if (IsCharEnum(instance.GetType()))
      {
        String s = Convert.ToString((Char)intValue);
        return s;
      }
      else
        return intValue;
    }


    public static List<Enum> ToEnumList(this Enum instance)
    {
      List<Enum> result = Enum.GetValues(instance.GetType()).Cast<Enum>().ToList();
      return result;
    }


    public static Boolean IsInSet(this Enum instance, params Enum[] items)
    {
      Boolean result = false;

      foreach (Enum item in items)
      {
        if (instance.ToString() == item.ToString())
        {
          result = true;
          break;
        }
      }

      return result;
    }


    public static List<Enum> ToEnumList(this Type enumType)
    {
      if (!enumType.GetTypeInfo().IsEnum)
        throw new Exception(String.Format("{0} must be an enum type", enumType));

      List<Enum> result = ((Enum)Activator.CreateInstance(enumType)).ToEnumList();

      return result;
    }

    public static Char ToChar(this Enum instance)
    {
      Char result = (Char)instance.ToInt32();
      return result;
    }

    public static Nullable<TEnum> ParseNullable<TEnum>(Object value) where TEnum : struct
    {
      Nullable<TEnum> result = null;

      if (value == null || value == DBNull.Value)
        result = null;

      else if (value is String)
        result = EnumEx.Parse<TEnum>((String)value);

      else if (value is Char)
        result = EnumEx.Parse<TEnum>((Char)value);

      else if (value is Int32)
        result = EnumEx.Parse<TEnum>((Int32)value);

      return result;
    }


    public static TEnum Parse<TEnum>(Object value) where TEnum : struct
    {
      return EnumEx.ParseNullable<TEnum>(value).Value;
    }


    public static TEnum Parse<TEnum>(String value) where TEnum : struct
    {
      TEnum result;

      if (value.Length == 1)
        result = Parse<TEnum>(Char.Parse(value));
      else
        result = (TEnum)Enum.Parse(typeof(TEnum), value, true);

      return result;
    }


    public static TEnum Parse<TEnum>(Char value) where TEnum : struct
    {
      TEnum result = (TEnum)Enum.ToObject(typeof(TEnum), value);

      return result;
    }


    public static TEnum Parse<TEnum>(Int32 value) where TEnum : struct
    {
      TEnum result = (TEnum)Enum.ToObject(typeof(TEnum), value);

      return result;
    }

    public static List<String> GetDescriptions(this Enum instance)
    {
      List<String> result = new List<String>();

      //Array values = Enum.GetValues(enumType);

      List<Enum> values = instance.ToEnumList();

      foreach (Enum value in values)
        result.Add(value.ToDescription());

      return result;
    }

    public static List<String> GetDescriptions(Type enumType)
    {
      if (!enumType.GetTypeInfo().IsEnum)
        throw new Exception(String.Format("{0} must be an enum type", enumType));

      List<String> result = new List<String>();

      List<Enum> values = EnumEx.ToEnumList(enumType);

      foreach (Enum value in values)
        result.Add(value.ToDescription());

      return result;
    }

    public static TEnum ParseDescription<TEnum>(String value) where TEnum : struct
    {
      Object rawEnum = null;

      var items = Enum.GetValues(typeof(TEnum));

      foreach (Enum item in items)
      {
        String description = item.ToDescription();

        if (description.EqualsEx(value))
        {
          rawEnum = item;
          break;
        }
      }

      TEnum result = (TEnum)(Object)rawEnum;

      return result;
    }

  }



  //public static class EnumParser<TEnum> where TEnum : struct
  //{
  //  public static TEnum Parse(Object value)
  //  {
  //    Nullable<TEnum> result = ParseNullable(value);

  //    return result.Value;
  //  }


  //  public static Nullable<TEnum> ParseNullable(Object value)
  //  {
  //    if (value == null || value == DBNull.Value)
  //      return null;

  //    TEnum result;

  //    if (value is String)
  //    {
  //      String sv = (String)value;

  //      if (sv.Length == 1)
  //        result = (TEnum)(Object)(Int32)sv[0];
  //      else
  //      {
  //        if (!Enum.TryParse<TEnum>(sv, true, out result))
  //          return null;
  //      }
  //    }
  //    else if (value is Char)
  //      result = (TEnum)(Object)(Int32)(Char)value;
  //    else
  //      result = (TEnum)Enum.ToObject(typeof(TEnum), value);

  //    return result;
  //  }
  //}



}