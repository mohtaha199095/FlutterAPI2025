
using System;
using System.Drawing;
using System.Globalization;
using System.IO;

public static partial class Simulate
{


    public static string Percentage(double expression)
    {
        double percentage = Simulate.Val(expression);
        // percentage = percentage / 100;
        return percentage.ToString("0.00%");
    }
    public static double Val(string expression)
    {
        if (expression == null)
            return 0;

        //try the entire string, then progressively smaller
        //substrings to simulate the behavior of VB's 'Val',
        //which ignores trailing characters after a recognizable value:
        for (int size = expression.Length; size > 0; size--)
        {
            double testDouble;
            if (double.TryParse(expression.Substring(0, size), out testDouble))
                if (double.IsNaN(testDouble))
                {
                    return 0;
                }
                else
                {
                    return testDouble;
                }

        }

        //no value is recognized, so return 0:
        return 0;
    }

    // 
    // 
    // 
    //
    //
    public static decimal decimal_(object expression)
    {
        return Convert.ToDecimal(Val(expression));
    }
    public static decimal decimal_(char expression)
    {
        return Convert.ToDecimal(Val(expression));
    }
    public static decimal decimal_(string expression)
    {
        return Convert.ToDecimal(Val(expression));
    }
    public static double Val(object expression)
    {
        try
        {


            if (expression == null)
                return 0;

            double testDouble;
            if (double.TryParse(expression.ToString(), out testDouble))
                return testDouble;

            //VB's 'Val' function returns -1 for 'true':
            bool testBool;
            if (bool.TryParse(expression.ToString(), out testBool))
                return testBool ? -1 : 0;

            //VB's 'Val' function returns the day of the month for dates:
            System.DateTime testDate;
            if (System.DateTime.TryParse(expression.ToString(), out testDate))
                return testDate.Day;

            //no value is recognized, so return 0:
            return 0;
        }
        catch (Exception)
        {

            return 0;
        }
    }
    public static int Val(char expression)
    {
        int testInt;
        if (int.TryParse(expression.ToString(), out testInt))
            return testInt;
        else
            return 0;
    }
    public static double ValCurrencyFormat(string expression)
    {
        if (expression == null)
            return 0;

        //try the entire string, then progressively smaller
        //substrings to simulate the behavior of VB's 'Val',
        //which ignores trailing characters after a recognizable value:
        for (int size = expression.Length; size > 0; size--)
        {
            double testDouble;
            if (double.TryParse(expression.Substring(0, size), out testDouble))
                if (double.IsNaN(testDouble))
                {
                    return 0;
                }
                else
                {

                    string a = Simulate.Currency_format(testDouble);
                    testDouble = Simulate.Val(a);
                    return testDouble;
                }

        }

        //no value is recognized, so return 0:
        return 0;
    }

    // 
    // 
    // 
    //
    //

    public static double ValCurrencyFormat(object expression)
    {
        try
        {


            if (expression == null)
                return 0;

            double testDouble;
            if (double.TryParse(expression.ToString(), out testDouble))
            {
                string a = Simulate.Currency_format(testDouble);
                testDouble = Simulate.Val(a);
                return testDouble;

            }

            //VB's 'Val' function returns -1 for 'true':
            bool testBool;
            if (bool.TryParse(expression.ToString(), out testBool))
                return testBool ? -1 : 0;

            //VB's 'Val' function returns the day of the month for dates:
            System.DateTime testDate;
            if (System.DateTime.TryParse(expression.ToString(), out testDate))
                return testDate.Day;

            //no value is recognized, so return 0:
            return 0;
        }
        catch (Exception)
        {

            return 0;
        }
    }
    public static string Currency_format(double Amount)
    {
        //if (Simulate.String(clsGlobalCompany.CurrencyFormatStyle) == string.Empty)
        //    return Amount.ToString(clsGlobalCompany.CurrencyFormatStyle); //"#,0.000"//
        //else
        return Amount.ToString("#,0.000"); //"#,0.000"//
    }
    public static string Currency_format(object Amount)
    {
        //if (Simulate.String(clsGlobalCompany.CurrencyFormatStyle) == string.Empty)
        //    return Amount.ToString(clsGlobalCompany.CurrencyFormatStyle); //"#,0.000"//
        //else
        return Simulate.decimal_( Amount).ToString("#,0.000"); //"#,0.000"//
    }
    public static string Currency_formatWithThousand(double Amount)
    {
        return Amount.ToString("#,0.000");
    }
    public static int Integer32(bool expression)
    {
        if (expression)
            return 1;
        else
            return 0;
    }
    public static int Integer32(string expression)
    {
        return Convert.ToInt32(decimal.Truncate((decimal_(expression))));
    }
    public static int Integer32(object expression)
    {
        return Convert.ToInt32(decimal.Truncate((decimal_(expression))));

    }
    public static int Integer32(char expression)
    {
        return Convert.ToInt32(decimal.Truncate((decimal_(expression))));
    }
    public static string String(object expression)
    {
        if (expression == null)
            return "";

        return (Convert.ToString(expression) == string.Empty ? "" : Convert.ToString(expression));


    }
    public static string String(string expression)
    {
        if (expression == null)
            return "";

        return (Convert.ToString(expression) == string.Empty ? "" : Convert.ToString(expression));


    }

    public static Guid Guid(string expression)
    {
        if (expression == null || expression == "")
            return new Guid();

        return new Guid(expression);


    }
    public static string String(char expression)
    {

        return (Convert.ToString(expression) == string.Empty ? "" : Convert.ToString(expression));
    }

    //public static bool Bool ( bool Expression )
    //{
    //    return (Convert.ToBoolean (Expression));
    //}
    public static bool Bool(object expression)
    {
        if (expression == null)
            return false;
        bool bool_;

        if (Simulate.Integer32(expression) == 1)
            return true;


        if (bool.TryParse(expression.ToString(), out bool_) == true)
            return (expression == null ? false : Convert.ToBoolean(expression));
        else
            return false;
    }
    public static bool Bool(char expression)
    {
        return Convert.ToBoolean(Val(expression));
    }
    public static Int64 Integer64(string expression)
    {
        try
        {
            return Convert.ToInt64(decimal.Truncate((decimal_(expression))));
        }
        catch (Exception ex)
        {

            throw ex;
        }

    }
    public static Int64 Integer64(object expression)
    {
        return Convert.ToInt64(decimal.Truncate((decimal_(expression))));

    }
    public static Int64 Integer64(char expression)
    {
        return Convert.ToInt64(decimal.Truncate((decimal_(expression))));
    }

    public static DateTime StringToDate(object expression)
    {


        return StringToDate(Simulate.String(expression));

    }
    public static DateTime StringToDate(string expression)
    {

        DateTime dateValue;
        if (DateTime.TryParse(expression, out dateValue))
            return dateValue;
        else
            return DateTime.Today;
    }
    public static string DateString(DateTime expression)
    {

        return expression.ToString("yyyy-MM-dd");
        //if ( expression == null )
        //    return 0;

        //double testDouble;
        //if ( double.TryParse (expression.ToString ( ) , out testDouble) )
        //    return testDouble;

        ////VB's 'Val' function returns -1 for 'true':
        //bool testBool;
        //if ( bool.TryParse (expression.ToString ( ) , out testBool) )
        //    return testBool ? -1 : 0;

        ////VB's 'Val' function returns the day of the month for dates:
        //System.DateTime testDate;
        //if ( System.DateTime.TryParse (expression.ToString ( ) , out testDate) )
        //    return testDate.Day;

        ////no value is recognized, so return 0:
        //return 0;
    }
    public static string TimeString(DateTime expression)
    {

        return expression.ToString("HH:mm tt");
    }
    public static decimal Decimal(string expression)
    {
        return Convert.ToDecimal(Val(expression));
    }
    public static decimal Decimal(object expression)
    {
        return Convert.ToDecimal(Val(expression));

    }
    public static decimal Decimal(char expression)
    {
        return Convert.ToDecimal(Val(expression));
    }

    public static DateTime StringToTime(string expression)
    {
        return DateTime.ParseExact(expression, "HH:mm:ss",
                                        CultureInfo.InvariantCulture);
    }
    //public static Bitmap StringToImg(byte[] expression)
    //{
    //    try
    //    {
    //        // Convert a C# string to a byte array  
    //        //byte[] bytes = Encoding.ASCII.GetBytes(expression);
    //        MemoryStream ms = new MemoryStream(expression);
    //        Bitmap bm = new Bitmap(ms);
    //        return bm;
    //    }
    //    catch (Exception ex)
    //    {

    //        throw;
    //    }

    //}
    public static Bitmap StringToImg(byte[] expression)
    {
        try
        {
            // Convert a C# string to a byte array  
            //byte[] bytes = Encoding.ASCII.GetBytes(expression);
            MemoryStream ms = new MemoryStream(expression);
            Bitmap bm = new Bitmap(ms);
            return bm;
        }
        catch (Exception ex)
        {

            throw;
        }

    }

}