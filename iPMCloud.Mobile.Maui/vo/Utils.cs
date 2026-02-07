using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace iPMCloud.Mobile
{
    public class Utils
    {
        public Utils() { }


        public static List<string> DaysSmallInUtils = new List<string> { "So", "Mo", "Di", "Mi", "Do", "Fr", "Sa" };
        public static List<string> DaysInUtils = new List<string> { "Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag" };
        public static List<string> DaysInUtilsDE = new List<string> { "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag", "Sonntag" };
        public static List<string> MonthsInUtils = new List<string> { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" };
        public static List<string> MonthsInUtilsSmall = new List<string> { "Jan", "Feb", "Mär", "Apr", "Mai", "Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dez" };

        public static DateTime StringDateToDateTime(string d)
        {
            var i = d.Split('-');
            var t = i[0].Trim().Split('.');
            var z = i[1].Trim().Split(':');
            return new DateTime(int.Parse(t[2]), int.Parse(t[1]), int.Parse(t[0]), int.Parse(z[0]), int.Parse(z[1]), 0, 0, 0);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static byte[] ConvertImageSourceToByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }

        }

        /*********************************************************************************/
        /*********************************************************************************/
        /*********************************************************************************/

        public static String formatDEStr(decimal d)
        {
            return d.ToString("N2", new CultureInfo("de-de")); // 0,00
        }
        public static String formatDEStr3(decimal d)
        {
            return d.ToString("N3", new CultureInfo("de-de")); // 0,00
        }

        public static string ConvertStringListToString(List<string> list, string seperator = ",")
        {
            if (list.Count == 0) { return ""; }
            string str = "";
            list.ForEach(s => { str += seperator + s; });
            return str.Length > 0 ? str.Substring(seperator.Length) : "";
        }
        public static List<string> ConvertStringToListString(string list, char seperator = ',')
        {
            return list.Split(seperator).ToList();
        }
        public static List<Int32> ConvertStringToListInt(string list, char seperator = ',')
        {
            List<Int32> l = new List<Int32>();
            foreach (string i in list.Split(seperator).ToList())
            {
                l.Add(Convert.ToInt32(i));
            }
            return l;
        }

        /*********************************************************************************/
        /*********************************************************************************/
        /*********************************************************************************/

        public static Int32 getIntervalInYearValue(string timeval)
        {
            if (timeval == "täglich")
            { //alert(unescape("%F6"));
                return 365;
            }
            else if (timeval == "1x wöchentlich")
            {
                return 52;
            }
            else if (timeval == "2x wöchentlich")
            {
                return 104;
            }
            else if (timeval == "3x wöchentlich")
            {
                return 156;
            }
            else if (timeval == "4x wöchentlich")
            {
                return 208;
            }
            else if (timeval == "5x wöchentlich")
            {
                return 260;
            }
            else if (timeval == "6x wöchentlich")
            {
                return 312;
            }
            else if (timeval == "14-tägig")
            {
                return 26;
            }
            else if (timeval == "1x monatlich")
            {
                return 12;
            }
            else if (timeval == "2x monatlich")
            {
                return 24;
            }
            else if (timeval == "3x monatlich")
            {
                return 36;
            }
            else if (timeval == "4x monatlich")
            {
                return 48;
            }
            else if (timeval == "1x jährlich")
            {
                return 1;
            }
            else if (timeval == "2x jährlich")
            {
                return 2;
            }
            else if (timeval == "3x jährlich")
            {
                return 3;
            }
            else if (timeval == "4x jährlich")
            {
                return 4;
            }
            else if (timeval == "5x jährlich")
            {
                return 5;
            }
            else if (timeval == "6x jährlich")
            {
                return 6;
            }
            else if (timeval == "7x jährlich")
            {
                return 7;
            }
            else if (timeval == "8x jährlich")
            {
                return 8;
            }
            else if (timeval == "9x jährlich")
            {
                return 9;
            }
            else if (timeval == "10x jährlich")
            {
                return 10;
            }
            else if (timeval == "11x jährlich")
            {
                return 11;
            }
            else if (timeval == "12x jährlich")
            {
                return 12;
            }
            else if (timeval == "13x jährlich")
            {
                return 13;
            }
            else if (timeval == "14x jährlich")
            {
                return 14;
            }
            else if (timeval == "Einmalig")
            {
                return 1; // Nach Intervall berechnen!  z.B. Monatsrechnung = "1x Monatlich" angepasst / dynamisch
            }
            else if (timeval == "nach Bedarf")
            {
                return 0;
            }
            else if (timeval == "nach Abfuhrplan")
            {
                return 0;
            }
            else if (timeval == "nach der Leerung")
            {
                return 0;
            }
            else if (timeval == "Pauschal")
            {
                return 1;
            }
            return 0;
        }

        public static string getIntervalInYearValueSaison(string timeval)
        {
            if (timeval == "täglich")
            {
                return "täglich i.d.Saison";
            }
            else if (timeval == "1x wöchentlich")
            {
                return "1x wöchentlich i.d. Saison";
            }
            else if (timeval == "2x wöchentlich")
            {
                return "2x wöchentlich i.d. Saison";
            }
            else if (timeval == "3x wöchentlich")
            {
                return "3x wöchentlich i.d. Saison";
            }
            else if (timeval == "4x wöchentlich")
            {
                return "4x wöchentlich i.d. Saison";
            }
            else if (timeval == "5x wöchentlich")
            {
                return "5x wöchentlich i.d. Saison";
            }
            else if (timeval == "6x wöchentlich")
            {
                return "6x wöchentlich i.d. Saison";
            }
            else if (timeval == "14-tägig")
            {
                return "14-tägig i.d. Saison";
            }
            else if (timeval == "1x monatlich")
            {
                return "1x monatlich i.d. Saison";
            }
            else if (timeval == "2x monatlich")
            {
                return "2x monatlich i.d. Saison";
            }
            else if (timeval == "3x monatlich")
            {
                return "3x monatlich i.d. Saison";
            }
            else if (timeval == "4x monatlich")
            {
                return "4x monatlich i.d. Saison";
            }
            else if (timeval == "1x jährlich")
            {
                return "1x i.d. Saison";
            }
            else if (timeval == "2x jährlich")
            {
                return "2x i.d. Saison";
            }
            else if (timeval == "3x jährlich")
            {
                return "3x i.d. Saison";
            }
            else if (timeval == "4x jährlich")
            {
                return "4x i.d. Saison";
            }
            else if (timeval == "5x jährlich")
            {
                return "5x i.d. Saison";
            }
            else if (timeval == "6x jährlich")
            {
                return "6x i.d. Saison";
            }
            else if (timeval == "7x jährlich")
            {
                return "7x i.d. Saison";
            }
            else if (timeval == "8x jährlich")
            {
                return "8x i.d. Saison";
            }
            else if (timeval == "9x jährlich")
            {
                return "9x i.d. Saison";
            }
            else if (timeval == "10x jährlich")
            {
                return "10x i.d. Saison";
            }
            else if (timeval == "11x jährlich")
            {
                return "11x i.d. Saison";
            }
            else if (timeval == "12x jährlich")
            {
                return "12x i.d. Saison";
            }
            else if (timeval == "13x jährlich")
            {
                return "13x i.d. Saison";
            }
            else if (timeval == "14x jährlich")
            {
                return "14x i.d. Saison";
            }
            else if (timeval == "Einmalig")
            {
                return "Einmalig"; // Nach Intervall berechnen!  z.B. Monatsrechnung = "1x Monatlich" angepasst / dynamisch
            }
            else if (timeval == "nach Bedarf")
            {
                return "nach Bedarf";
            }
            else if (timeval == "nach Abfuhrplan")
            {
                return "nach Abfuhrplan";
            }
            else if (timeval == "nach der Leerung")
            {
                return "nach der Leerung";
            }
            else if (timeval == "Pauschal")
            {
                return "Pauschal";
            }
            return "";
        }

        public static String getEinheitStr(String e, Boolean small = true)
        {
            switch (e)
            {
                case "ausf":
                    return small ? "Ausf." : "je Ausführung";
                case "stdz":
                    return small ? "StdZ" : "nach Stunde(Zeit)";
                case "std":
                    return small ? "Std" : "je Stunde";
                case "stck":
                    return small ? "Stck" : "je Stück";
                case "pck":
                    return small ? "Pck" : "je Box/Paket";
                case "ve":
                    return small ? "VE" : "je Verpackungseinheit";
                case "qm":
                    return small ? "m²" : "Quadratmeter";
                case "3qm":
                    return small ? "m³" : "Kubikmeter";
                case "m":
                    return small ? "Mtr" : "Lfd.Meter";
                case "fl":
                    return small ? "Fl" : "je Flasche";
                case "l":
                    return small ? "Ltr" : "je Liter";
                case "t":
                    return small ? "t" : "je Tonne";
                case "kg":
                    return small ? "Kg" : "je Kilogramm";
                case "g":
                    return small ? "g" : "je Gramm";
                case "km":
                    return small ? "Km" : "je Kilometer";
                case "fahrt":
                    return small ? "Fahrt" : "je Fahrt";
                case "wegstrecke":
                    return small ? "Weg" : "je Wegstrecke";
                case "pauschal":
                    return small ? "Pausch." : "Pauschal";
                case "festpreis":
                    return small ? "FP" : "Festpreis";
                case "personen":
                    return small ? "Pers." : "Personen";
                default:
                    return "";
            }
        }


        /*********************************************************************************/
        /*********************************************************************************/
        /*********************************************************************************/

    }



    public class JavaScriptDateConverter
    {
        public JavaScriptDateConverter() { }

        private static DateTime _jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        /// <summary>DateTime to JSTimeStamp</summary>
        public static long Convert(DateTime from, long offsetInHours = 0)
        {
            return System.Convert.ToInt64((from - _jan1st1970).TotalMilliseconds + (offsetInHours * 1000 * 60 * 60));
        }
        /// <summary>DateTime to JSTimeStamp FIX TIME</summary>
        public static long ConvertFixTime(DateTime from, int hour = 0, int min = 0, int sec = 0)
        {
            from = new DateTime(from.Year, from.Month, from.Day, hour, min, sec, 0);
            return System.Convert.ToInt64((from - _jan1st1970).TotalMilliseconds);
        }

        /// <summary>JSTimeStamp to DateTime</summary>
        public static DateTime Convert(long from, long offsetInHours = 0)
        {
            return _jan1st1970.AddMilliseconds(from + (offsetInHours * 1000 * 60 * 60));
        }
        /// <summary>JSTimeStamp to DateTime FIX TIME</summary>
        public static long ConvertFixTime(long from, int hour = 0, int min = 0, int sec = 0)
        {
            var nd = _jan1st1970.AddMilliseconds(from);
            nd = new DateTime(nd.Year, nd.Month, nd.Day, hour, min, sec, 0);
            return System.Convert.ToInt64((nd - _jan1st1970).TotalMilliseconds);
        }

        /// <summary>DateTime to JSTimeStamp ONLY TIME</summary>
        public static DateTime ConvertWithoutTime(DateTime date, int offsetInHours = 2)
        {
            return new DateTime(date.Year, date.Month, date.Day, offsetInHours, 0, 0, 0);
        }
        /// <summary>JSTimeStamp to DateTime ONLY DATE</summary>
        public static DateTime ConvertWithoutTime(long from, long offsetInHours = 2)
        {
            return _jan1st1970.AddMilliseconds(from + (offsetInHours * 1000 * 60 * 60));
        }
    }


}