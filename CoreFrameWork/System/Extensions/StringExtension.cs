using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace System
{
  public static partial class StringExtension
  {
    //public static String Format(this String format, params Object[] args)
    //{
    //  String result = String.Format(format, args);

    //  return result;
    //}

    public static String Right(this String Expr, Int32 Length)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      if (Length >= Expr.Length)
        return Expr;

      Int32 startPos = Expr.Length - Length;

      result = Expr.Substring(startPos, Length);

      return result;
    }


    public static String ToLetter(this String Expr)
    {
      String result = null;

      if (Expr != null)
      {
        foreach (var c in Expr)
        {
          if (Char.IsLetter(c))
            result += c;
        }
      }

      return result;
    }

    public static String ToUpperEx(this String Expr)
    {
      if (Expr.IsNullOrEmpty())
        return Expr;

      return Expr.ToUpper();
    }


    /// <summary>
    /// Retrieves a substring from this instance. The substring starts at position 0 
    /// and has a specified length.
    /// </summary>
    /// <param name="Expr">String instance</param>
    /// <param name="Length">Length of substring</param>
    /// <returns>substring</returns>
    public static String Left(this String Expr, Int32 Length)
    {
      String result = Expr + "";

      if (Expr.IsNullOrEmpty() || Length <= 0)
        return "";

      if (Length > Expr.Length)
        return result;

      result = Expr.Substring(0, Length);

      return result;
    }


    public static Int32 LengthEx(this String Expr)
    {
      Int32 result = Expr.IsNullOrEmpty() ? 0 : Expr.Length;

      return result;
    }


    public static String Reverse(this String Expr)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      Char[] ReverseChars = Expr.ToCharArray();

      Array.Reverse(ReverseChars);

      result = new String(ReverseChars);

      return result;
    }


    public static String RemoveDoubleSpace(this String Expr)
    {
      String result = (Expr + "").Trim();

      if (!result.IsNullOrEmpty())
      {
        while (result.IndexOf("  ") >= 0)
          result = result.Replace("  ", " ");
      }

      return result;
    }


    public static String SubStringEx(this String Expr, Int32 StartIndex)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      if (StartIndex < Expr.Length)
        result = Expr.Substring(StartIndex);

      return result;
    }


    public static String SubStringEx(this String Expr, Int32 StartIndex, Int32 Length)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      if ((StartIndex + Length) > Expr.Length)
        Length = Expr.Length - StartIndex;

      if ((Length > 0) && ((StartIndex + Length) <= Expr.Length))
        result = Expr.Substring(StartIndex, Length);

      return result;
    }


    public static String RemoveAllSpaces(this String Expr, Int32 MaxLength)
    {
      String result = RemoveAllSpaces(Expr);

      result = TrimToMaxLength(result, MaxLength);

      return result;
    }


    public static Boolean IsNullOrEmpty(this String Expr)
    {
      Boolean result = String.IsNullOrEmpty(Expr);

      return result;
    }


    public static String RemoveAllSpaces(this String Expr)
    {
      String result = (Expr + "").Trim();

      while (result.IndexOf(" ") >= 0)
        result = result.Replace(" ", "");

      return result;
    }




    public static String RemoveChars(this String Expr, Char[] CharsToRemove)
    {
      String result = Expr + "";

      if (Expr.IsNullOrEmpty())
        return result;

      if (CharsToRemove != null && !result.IsNullOrEmpty())
      {
        foreach (Char c in CharsToRemove)
          result = result.Replace("" + c, "");
      }

      return result;
    }


    public static String TrimSmart(this String Expr, Int32 MaxLength)
    {
      String result = RemoveDoubleSpace(Expr) + "";

      List<Char> chars = new List<Char>(Reverse(result).ToCharArray());

      // remove spaces from the rare end
      while (chars.Count > MaxLength)
      {
        if (!chars.Remove(' '))
          break;
      }

      result = Reverse(new String(chars.ToArray()));
      result = TrimToMaxLength(result, MaxLength);

      return result;
    }


    /// <summary>
    /// Copy first <n> chars with length check 
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="MaxLength"></param>
    /// <returns></returns>
    public static String TrimToMaxLength(this String Expr, Int32 MaxLength)
    {
      if (Expr.IsNullOrEmpty() || MaxLength <= 0)
        return Expr;

      String result = Expr + "";

      if (result.Length > MaxLength)
        result = result.Substring(0, MaxLength);

      return result;
    }


    public static String RemoveLetters(this String Expr)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      foreach (Char item in Expr)
      {
        if (Char.IsNumber(item))
          result += item;
      }

      return result;
    }


    public static String RemoveNumbers(this String Expr)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      foreach (Char item in Expr)
      {
        if (!Char.IsNumber(item))
          result += item;
      }

      return result;
    }


    public static String OnlyNumbers(this String Expr)
    {
      return OnlyNumbers(Expr, null);
    }


    public static String OnlyNumbers(this String Expr, Char[] AllowedChars)
    {
      String result = "";

      if (Expr.IsNullOrEmpty())
        return result;

      foreach (Char item in Expr)
      {
        if (!AllowedChars.IsNull())
        {
          foreach (Char allowedChar in AllowedChars)
          {
            if (allowedChar.Equals(item))
            {
              result += item;
              break;
            }
          }
        }

        if (Char.IsNumber(item))
        {
          result += item;
        }
      }

      return result;
    }


    public static String Fill(String FillString, Int32 Repetitions)
    {
      String result = String.Empty;

      if (FillString.IsNullOrEmpty())
        return result;

      for (Int32 Count = 0; Count < Repetitions; Count++)
        result += FillString;

      return result;
    }


    public static String TrimLeadingZeros(this String Expr)
    {
      String result = Expr;

      if (result.IsNullOrEmpty())
        return result;

      result = result.TrimStart(new Char[] { '0' });

      return result;
    }


    public static String Encode(this String PlainText)
    {
      String result = PlainText + "";

      if (!PlainText.IsNullOrEmpty())
      {
        Byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(PlainText);
        result = Convert.ToBase64String(encbuff);
      }

      return result;
    }


    public static String Decode(this String EncodedText)
    {
      String result = EncodedText + "";

      if (!EncodedText.IsNullOrEmpty())
      {
        Byte[] decbuff = Convert.FromBase64String(EncodedText);
        UTF8Encoding encoder = new UTF8Encoding();

        result = encoder.GetString(decbuff, 0, decbuff.Length);
      }

      return result;
    }

    //public static String RemoveAccentsAg(this String Expr)
    //{
    //  if (Expr.IsNullOrEmpty())
    //    return "";

    //  String normalizedString = Expr.Normalize(NormalizationForm.FormD);
    //  StringBuilder sb = new StringBuilder();

    //  for (int i = 0; i < normalizedString.Length; i++)
    //  {
    //    Char c = normalizedString[i];
    //    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
    //      sb.Append(c);
    //  }

    //  return sb.ToString();
    //}

    public static String ToProperCase(this String sb)
    {
      if (sb.IsNullOrEmpty())
        return "";

      String s = sb.ToLower();
      String result = "";

      Char[] seps = new char[] { ' ' };
      foreach (String item in s.Split(seps))
      {
        result += Char.ToUpper(item[0]);
        result +=
        (item.Substring(1, item.Length - 1) + ' ');
      }

      return result;
    }



    private static Char[] FloatingNumberChars(String DecimalSeparator)
    {
      return new Char[] { (Char)DecimalSeparator[0], 'e', '-', '+' };
    }


    public static Single? ToSingleNullable(this String Expr, String DecimalSeparator = ".")
    {
      Single? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      NumberFormatInfo nfi = NumberFormatInfoEx.GetNumberFormatInfo(DecimalSeparator);

      Single newSingle;
      Char[] AllowedChars = DecimalSeparator.IsNullOrEmpty() ? null : FloatingNumberChars(DecimalSeparator);

      Expr = Expr.ToLower().OnlyNumbers(AllowedChars);

      if (Single.TryParse(Expr, NumberStyles.Any, nfi, out newSingle))
        result = newSingle;

      return result;
    }



    public static Single ToSingle(this String Expr, String DecimalSeparator = ".", Single DefaultValue = 0)
    {
      return ToSingleNullable(Expr, DecimalSeparator) ?? DefaultValue;
    }


    public static Double? ToDoubleNullable(this String Expr, String DecimalSeparator = ".")
    {
      Double? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      NumberFormatInfo nfi = NumberFormatInfoEx.GetNumberFormatInfo(DecimalSeparator);

      Double newDouble;
      Char[] AllowedChars = DecimalSeparator.IsNullOrEmpty() ? null : FloatingNumberChars(DecimalSeparator);

      Expr = Expr.ToLower().OnlyNumbers(AllowedChars);

      if (Double.TryParse(Expr, NumberStyles.Any, nfi, out newDouble))
        result = newDouble;

      return result;
    }


    public static Double ToDouble(this String Expr, String DecimalSeparator = ".", Double DefaultValue = 0)
    {
      return ToDoubleNullable(Expr, DecimalSeparator) ?? DefaultValue;
    }



    public static Decimal? ToDecimalNullable(this String Expr, String DecimalSeparator = ".")
    {
      Decimal? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      NumberFormatInfo nfi = NumberFormatInfoEx.GetNumberFormatInfo(DecimalSeparator);

      Decimal newDecimal;
      Char[] AllowedChars = DecimalSeparator.IsNullOrEmpty() ? null : FloatingNumberChars(DecimalSeparator);

      Expr = Expr.OnlyNumbers(AllowedChars);

      if (Decimal.TryParse(Expr, NumberStyles.Any, nfi, out newDecimal))
        result = newDecimal;

      return result;
    }


    public static Decimal ToDecimal(this String Expr, String DecimalSeparator = ".", Decimal DefaultValue = 0)
    {
      return ToDecimalNullable(Expr, DecimalSeparator) ?? DefaultValue;
    }


    public static Int64? ToInt64Nullable(this String Expr)
    {
      Int64? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      Expr = Expr.Trim();

      Int64 newInt64;

      if (Int64.TryParse(Expr, out newInt64))
        result = newInt64;

      return result;
    }


    public static Int64 ToInt64(this String Expr, Int64 DefaultValue = 0)
    {
      return ToInt64Nullable(Expr) ?? DefaultValue;
    }


    public static Int32? ToInt32Nullable(this String Expr)
    {
      Int32? result = null;

      if (Expr.IsNullOrEmpty())
        return null;

      Expr = Expr.Trim();

      Int32 newInt32;

      if (Int32.TryParse(Expr, out newInt32))
        result = newInt32;

      return result;
    }


    public static Int32 ToInt32(this String Expr, Int32 DefaultValue = 0)
    {
      return ToInt32Nullable(Expr) ?? DefaultValue;
    }


    public static Int16? ToInt16Nullable(this String Expr)
    {
      Int16? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      Expr = Expr.Trim();

      Int16 newInt16;

      if (Int16.TryParse(Expr, out newInt16))
        result = newInt16;

      return result;
    }


    public static Int16 ToInt16(this String Expr, Int16 DefaultValue = 0)
    {
      return ToInt16Nullable(Expr) ?? DefaultValue;
    }


    public static Byte? ToByteNullable(this String Expr)
    {
      Byte? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      Expr = Expr.Trim();

      Byte newByte;

      if (Byte.TryParse(Expr, out newByte))
        result = newByte;

      return result;
    }


    public static Byte ToByte(this String Expr, Byte DefaultValue = 0)
    {
      return ToByteNullable(Expr) ?? DefaultValue;
    }


    public static SByte? ToSByteNullable(this String Expr)
    {
      SByte? result = null;

      if (Expr.IsNullOrEmpty())
        return result;

      Expr = Expr.Trim();

      SByte newSByte;

      if (SByte.TryParse(Expr, out newSByte))
        result = newSByte;

      return result;
    }


    public static SByte ToSByte(this String Expr, SByte DefaultValue = 0)
    {
      return ToSByteNullable(Expr) ?? DefaultValue;
    }



    public static DateTime? ToDateTime(this String Expr, DateTimeFormatInfo dti)
    {
      DateTime? result = null;

      if (String.IsNullOrEmpty(Expr))
        return result;

      Expr = Expr.Trim();

      DateTime newDate;
      DateTimeStyles dateStyle = DateTimeStyles.AllowWhiteSpaces;

      try
      {
        if (DateTime.TryParse(Expr, dti, dateStyle, out newDate))
          result = newDate;
      }
      catch (Exception Ex)
      {
        System.Diagnostics.Debug.WriteLine(Ex.Message);
      }

      return result;
    }



    public static DateTime? ToDateTime(this String Expr)
    {
      DateTimeFormatInfo dti = CultureInfo.CurrentCulture.DateTimeFormat;
      return ToDateTime(Expr, dti);
    }


    public static DateTime? ToDateYYYYMMDD(this String Expr)
    {
      return ToDateTime(Expr, "yyyyMMdd");
    }

    public static DateTime? ToDateYYMMDD(this String Expr)
    {
      return ToDateTime(Expr, "yyMMdd");
    }


    public static DateTime? ToDateTime(this String Expr, String ShortDatePattern)
    {
      DateTime? result = null;
      DateTimeStyles dateStyle = DateTimeStyles.AllowWhiteSpaces;

      DateTimeFormatInfo dti = CultureInfo.CurrentCulture.DateTimeFormat;

      if (!String.IsNullOrEmpty(ShortDatePattern))
      {
        dti = new DateTimeFormatInfo();
        dti.ShortDatePattern = ShortDatePattern;
      }

      DateTime newDate;
      if (DateTime.TryParseExact(Expr, dti.ShortDatePattern, dti, dateStyle, out newDate))
        result = newDate;

      return result;
    }


    public static Boolean? ToBooleanNullable(this String value)
    {
      Boolean? result = null;

      if (value.IsNullOrEmpty())
        return result;

      value = value.Trim().ToUpper();

      switch (value)
      {
        case "1":
        case "Y":
        case "YES":
        case "J":
        case "JA":
        case "TRUE":
          result = true;
          break;

        case "0":
        case "F":
        case "N":
        case "NO":
        case "NEE":
        case "FALSE":
          result = false;
          break;
      }

      return result;
    }


    public static Boolean ToBoolean(this String Expr, Boolean DefaultValue)
    {
      Boolean result = ToBooleanNullable(Expr) ?? DefaultValue;

      return result;
    }

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.text.normalizationform.aspx
    /// </summary>
    /// <param name="Input"></param>
    /// <returns></returns>
    public static String RemoveAccents(this String Expr)
    {
      String result = Expr;

      if (!String.IsNullOrEmpty(Expr))
      {
        // FormC	full canonical decomposition, followed by the replacement of sequences with their primary composites, if possible.
        // FormD	full canonical decomposition.
        // FormKC	full compatibility decomposition, followed by the replacement of sequences with their primary composites, if possible.
        // FormKD	full compatibility decomposition.

        String normalizer = Expr.Normalize(NormalizationForm.FormKD);
        Encoding cleaner = Encoding.GetEncoding(Encoding.ASCII.CodePage, new EncoderReplacementFallback(""), new DecoderReplacementFallback(""));

        Byte[] bytes = cleaner.GetBytes(normalizer);
        result = Encoding.ASCII.GetString(bytes);
      }

      return result;
    }


    public static Boolean IsInArray(this String Expr, params String[] array)
    {
      if (array == null || array.Length == 0)
        return false;

      foreach (String item in array)
      {
        if (String.Compare(Expr, item, true) == 0)
          return true;
      }

      return false;
    }


    public static Boolean IsEmpty(this String Expr)
    {
      Boolean result = Expr.IsNull() ? false : Expr == String.Empty;

      return result;
    }

    public static Boolean StartsWithEx(this String Expr, String Text, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
      Boolean result = Expr.IsNullOrEmpty() ? false : Expr.StartsWith(Text, stringComparison);

      return result;
    }


    public static Boolean EndsWithEx(this String Expr, String Text, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
      Boolean result = Expr.IsNullOrEmpty() ? false : Expr.EndsWith(Text, stringComparison);

      return result;
    }


    public static Boolean ContainsString(this String Expr, String value, StringComparison comparison)
    {
      if (Expr == null || value == null)
        return false;

      return Expr.IndexOf(value, comparison) >= 0;
    }


    public static Boolean ContainsEx(this String Expr, params String[] array)
    {
      return ContainsEx(Expr, StringComparison.OrdinalIgnoreCase, array);
    }


    public static Boolean ContainsEx(this String Expr, StringComparison stringComparison, params String[] array)
    {
      if (Expr.IsNullOrEmpty() || array.IsNull() || array.Length == 0)
        return false;

      foreach (String item in array)
      {
        if (Expr.IndexOf(item, stringComparison) >= 0)
          return true;
      }

      return false;
    }


    public static Boolean EqualsEx(this String value, String Expr, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
      if (value == null)
        return Expr == null;

      Boolean result = value.Equals(Expr, comparisonType);

      return result;
    }


    public static String Zip(this String uncompressedString)
    {
      using (var compressedStream = new MemoryStream())
      {
        using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
        {
          using (var compressorStream = new GZipStream(compressedStream, CompressionLevel.Optimal))
          {
            uncompressedStream.CopyTo(compressorStream);
          }

          return Convert.ToBase64String(compressedStream.ToArray());
        }
      }
    }

    /// <summary>
    /// Decompresses a deflate compressed, Base64 encoded string and returns an uncompressed string.
    /// </summary>
    /// <param name="compressedString">String to decompress.</param>
    public static String Unzip(this String compressedString)
    {
      using (var decompressedStream = new MemoryStream())
      {
        using (var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString)))
        {
          using (var decompressorStream = new GZipStream(compressedStream, CompressionMode.Decompress))
          {
            decompressorStream.CopyTo(decompressedStream);
          }
        }

        return Encoding.UTF8.GetString(decompressedStream.ToArray());
      }
    }
  }
}
