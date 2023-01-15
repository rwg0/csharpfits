using System;
namespace nom.tam.util
{
    /*
     * Copyright: Thomas McGlynn 1997-2007.
     * 
     * The CSharpFITS package is a C# port of Tom McGlynn's
     * nom.tam.fits Java package, initially ported by  Samuel Carliles
     *
     * Copyright: 2007 Virtual Observatory - India.   
     *
     * Use is subject to license terms
     */
    public class SupportClass
    {
        public static int StringIndexOf(String s, Char c, int startIndex)
        {
            int result = -1;

            try
            {
                result = s.IndexOf(c, startIndex);
            }
            catch (Exception)
            {
                result = -1;
            }

            return result;
        }

        /*******************************/
        /// <summary>This method is used as a dummy method to simulate VJ++ behavior</summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static long Identity(long literal)
        {
            return literal;
        }

        /// <summary>This method is used as a dummy method to simulate VJ++ behavior</summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static ulong Identity(ulong literal)
        {
            return literal;
        }

        /// <summary>This method is used as a dummy method to simulate VJ++ behavior</summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static float Identity(float literal)
        {
            return literal;
        }

        /// <summary>This method is used as a dummy method to simulate VJ++ behavior</summary>
        /// <param name="literal">The literal to return</param>
        /// <returns>The received value</returns>
        public static double Identity(double literal)
        {
            return literal;
        }

        /*******************************/
        /// <summary>Converts a string to an array of bytes</summary>
        /// <param name="sourceString">The string to be converted</param>
        /// <returns>The new array of bytes</returns>
        public static byte[] ToByteArray(string sourceString)
        {
            byte[] byteArray = new byte[sourceString.Length];
            for (int index = 0; index < sourceString.Length; index++)
                byteArray[index] = (byte)sourceString[index];
            return byteArray;
        }

        /*******************************/
        public class Tokenizer
        {
            private System.Collections.ArrayList elements;
            private string source;
            //The tokenizer uses the default delimiter set: the space character, the tab character, the newline character, and the carriage-return character
            private string delimiters = " \t\n\r";

            public Tokenizer(string source)
            {
                elements = new System.Collections.ArrayList();
                elements.AddRange(source.Split(delimiters.ToCharArray()));
                RemoveEmptyStrings();
                this.source = source;
            }

            public Tokenizer(string source, string delimiters)
            {
                elements = new System.Collections.ArrayList();
                this.delimiters = delimiters;
                elements.AddRange(source.Split(this.delimiters.ToCharArray()));
                RemoveEmptyStrings();
                this.source = source;
            }

            public int Count => (elements.Count);

            public bool HasMoreTokens()
            {
                return (elements.Count > 0);
            }

            public string NextToken()
            {
                string result;
                if (source == "") throw new Exception();
                else
                {
                    elements = new System.Collections.ArrayList();
                    elements.AddRange(source.Split(delimiters.ToCharArray()));
                    RemoveEmptyStrings();
                    result = (string)elements[0];
                    elements.RemoveAt(0);
                    source = source.Remove(source.IndexOf(result), result.Length);
                    source = source.TrimStart(delimiters.ToCharArray());
                    return result;
                }
            }

            public string NextToken(string delimiters)
            {
                this.delimiters = delimiters;
                return NextToken();
            }

            private void RemoveEmptyStrings()
            {
                //VJ++ does not treat empty strings as tokens
                for (int index = 0; index < elements.Count; index++)
                    if ((string)elements[index] == "")
                    {
                        elements.RemoveAt(index);
                        index--;
                    }
            }
        }

        /*******************************/
        public static bool FileCanWrite(System.IO.FileInfo file)
        {
            return (System.IO.File.GetAttributes(file.FullName) & System.IO.FileAttributes.ReadOnly) !=
                    System.IO.FileAttributes.ReadOnly;
        }

        /*******************************/
        public static void WriteStackTrace(Exception throwable, System.IO.TextWriter stream)
        {
            stream.Write(throwable.StackTrace);
            stream.Flush();
        }

        /*******************************/
        public class TextNumberFormat
        {
            // Declaration of fields
            private readonly System.Globalization.NumberFormatInfo numberFormat;
            private enum formatTypes { General, Number, Currency, Percent };
            private readonly int numberFormatType;
            private bool groupingActivated;
            private readonly string separator;
            private int maxIntDigits;
            private int minIntDigits;
            private int maxFractionDigits;
            private int minFractionDigits;

            // CONSTRUCTORS
            public TextNumberFormat()
            {
                numberFormat = new System.Globalization.NumberFormatInfo();
                numberFormatType = (int)formatTypes.General;
                groupingActivated = true;
                separator = GetSeparator((int)formatTypes.General);
                maxIntDigits = 127;
                minIntDigits = 1;
                maxFractionDigits = 3;
                minFractionDigits = 0;
            }

            private TextNumberFormat(formatTypes theType, int digits)
            {
                numberFormat = System.Globalization.NumberFormatInfo.CurrentInfo;
                numberFormatType = (int)theType;
                groupingActivated = true;
                separator = GetSeparator((int)theType);
                maxIntDigits = 127;
                minIntDigits = 1;
                maxFractionDigits = 3;
                minFractionDigits = 0;
            }

            private TextNumberFormat(formatTypes theType, System.Globalization.CultureInfo cultureNumberFormat, int digits)
            {
                numberFormat = cultureNumberFormat.NumberFormat;
                numberFormatType = (int)theType;
                groupingActivated = true;
                separator = GetSeparator((int)theType);
                maxIntDigits = 127;
                minIntDigits = 1;
                maxFractionDigits = 3;
                minFractionDigits = 0;
            }

            public static TextNumberFormat getTextNumberInstance()
            {
                TextNumberFormat instance = new TextNumberFormat(formatTypes.Number, 3);
                return instance;
            }

            public static TextNumberFormat getTextNumberCurrencyInstance()
            {
                TextNumberFormat instance = new TextNumberFormat(formatTypes.Currency, 3);
                return instance;
            }

            public static TextNumberFormat getTextNumberPercentInstance()
            {
                TextNumberFormat instance = new TextNumberFormat(formatTypes.Percent, 3);
                return instance;
            }

            public static TextNumberFormat getTextNumberInstance(System.Globalization.CultureInfo culture)
            {
                TextNumberFormat instance = new TextNumberFormat(formatTypes.Number, culture, 3);
                return instance;
            }

            public static TextNumberFormat getTextNumberCurrencyInstance(System.Globalization.CultureInfo culture)
            {
                TextNumberFormat instance = new TextNumberFormat(formatTypes.Currency, culture, 3);
                return instance;
            }

            public static TextNumberFormat getTextNumberPercentInstance(System.Globalization.CultureInfo culture)
            {
                TextNumberFormat instance = new TextNumberFormat(formatTypes.Percent, culture, 3);
                return instance;
            }

            public Object Clone()
            {
                return this;
            }

            public override bool Equals(Object textNumberObject)
            {
                return System.Object.Equals(this, textNumberObject);
            }

            public string FormatDouble(double number)
            {
                if (groupingActivated)
                {
                    return number.ToString(GetCurrentFormatString() + maxFractionDigits, numberFormat);
                }
                else
                {
                    return (number.ToString(GetCurrentFormatString() + maxFractionDigits, numberFormat)).Replace(separator, "");
                }
            }

            public string FormatLong(long number)
            {
                if (groupingActivated)
                {
                    return number.ToString(GetCurrentFormatString() + maxFractionDigits, numberFormat);
                }
                else
                {
                    return (number.ToString(GetCurrentFormatString() + maxFractionDigits, numberFormat)).Replace(separator, "");
                }
            }

            public static System.Globalization.CultureInfo[] GetAvailableCultures()
            {
                return System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            }

            //public override int GetHashCode()
            //{
            //    return base.GetHashCode();
            //}

            private string GetCurrentFormatString()
            {
                string currentFormatString = "n";  //Default value
                switch (numberFormatType)
                {
                    case (int)formatTypes.Currency:
                        currentFormatString = "c";
                        break;

                    case (int)formatTypes.General:
                        currentFormatString = $"n{numberFormat.NumberDecimalDigits}";
                        break;

                    case (int)formatTypes.Number:
                        currentFormatString = $"n{numberFormat.NumberDecimalDigits}";
                        break;

                    case (int)formatTypes.Percent:
                        currentFormatString = "p";
                        break;
                }
                return currentFormatString;
            }

            private string GetSeparator(int numberFormatType)
            {
                string separatorItem = " ";  //Default Separator

                switch (numberFormatType)
                {
                    case (int)formatTypes.Currency:
                        separatorItem = numberFormat.CurrencyGroupSeparator;
                        break;

                    case (int)formatTypes.General:
                        separatorItem = numberFormat.NumberGroupSeparator;
                        break;

                    case (int)formatTypes.Number:
                        separatorItem = numberFormat.NumberGroupSeparator;
                        break;

                    case (int)formatTypes.Percent:
                        separatorItem = numberFormat.PercentGroupSeparator;
                        break;
                }
                return separatorItem;
            }

            public bool GroupingUsed
            {
                get => (groupingActivated);
                set => groupingActivated = value;
            }

            public int MinIntDigits
            {
                get => minIntDigits;
                set => minIntDigits = value;
            }

            public int MaxIntDigits
            {
                get => maxIntDigits;
                set => maxIntDigits = value;
            }

            public int MinFractionDigits
            {
                get => minFractionDigits;
                set => minFractionDigits = value;
            }

            public int MaxFractionDigits
            {
                get => maxFractionDigits;
                set => maxFractionDigits = value;
            }
        }
        /*******************************/
        /// <summary>
        /// Converts an array of bytes to an array of chars
        /// </summary>
        /// <param name="byteArray">The array of bytes to convert</param>
        /// <returns>The new array of chars</returns>
        public static char[] ToCharArray(byte[] byteArray)
        {
            char[] charArray = new char[byteArray.Length];
            byteArray.CopyTo(charArray, 0);
            return charArray;
        }

        /*******************************/
        public static int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2 << ~bits);
        }

        public static int URShift(int number, long bits)
        {
            return URShift(number, (int)bits);
        }

        public static long URShift(long number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2L << ~bits);
        }

        public static long URShift(long number, long bits)
        {
            return URShift(number, (int)bits);
        }

        public static char NextChar()
        {
            byte[] buf = new byte[2];
            Random.NextBytes(buf);
            char c = BitConverter.ToChar(buf, 0);
            while (!Char.IsLetterOrDigit(c) && !Char.IsPunctuation(c) && !Char.IsWhiteSpace(c))
            {
                Random.NextBytes(buf);
                c = BitConverter.ToChar(buf, 0);
            }

            return c;
        }

        public static Random Random = new Random();
    }
}
