using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DBOrm.Common
{
    class CommonUtils
    {
        #region Constructors

        #endregion

        #region Methods

        public static object ChangeType(object value, Type conversionType)
        {
            if (conversionType == null)
            {
                throw new ArgumentNullException("conversionType");
            } 
            if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                } 
                NullableConverter nullableConverter = new NullableConverter(conversionType);
                conversionType = nullableConverter.UnderlyingType;
            } 
            return Convert.ChangeType(value, conversionType);
        }

        public static void FormatedDate(string monthname, int iyear, out DateTime startdate, out DateTime enddate)
        {
            int imonth = GetMonthFromName(monthname);
            startdate = new System.DateTime(iyear, imonth, 1);
            enddate = startdate.AddMonths(1).AddDays(-1);
        }

        public static int GetMonthFromName(string monthname)
        {
            DateTime dt = new DateTime(2005, 1, 1);
            for (; monthname != dt.ToString("MMMM"); dt = dt.AddMonths(1)) ;
            return dt.Month;
        }

        public static string GetMonthName(int imonth)
        {
            DateTime dt = new DateTime(2005, imonth, 1);
            return dt.ToString("MMMM");
        }

        public static string GetStartDate(int imonth, int iyear)
        {
            return new DateTime(iyear, imonth, 1).ToString("d");
        }

        public static string GetStartDate(string monthname, int iyear)
        {
            return new DateTime(iyear, GetMonthFromName(monthname), 1).ToString("d");
        }

        public static string GetStartDate(string monthname, string sYear)
        {
            return new DateTime(Convert.ToInt16(sYear), GetMonthFromName(monthname), 1).ToString("d");
        }

        public static string GetEndDate(int imonth, int iyear)
        {
            return new DateTime(iyear, imonth, 1).AddMonths(1).AddDays(-1).ToString("d");
        }

        public static string GetEndDate(string monthname, int iyear)
        {
            return new DateTime(iyear, GetMonthFromName(monthname), 1).AddMonths(1).AddDays(-1).ToString("d");
        }

        public static string GetEndDate(string monthname, string sYear)
        {
            return new DateTime(Convert.ToInt16(sYear), GetMonthFromName(monthname), 1).AddMonths(1).AddDays(-1).ToString("d");
        }

        public static string GetEndDate(int month, string sYear)
        {
            return new DateTime(Convert.ToInt16(sYear), month, 1).AddMonths(1).AddDays(-1).ToString("d");
        }

        public static string GetYTD(int year)
        {
            return new DateTime(year, 1, 1).ToString("d");
        }

        public static string EncryptPassword(string Password)
        {

            byte[] data = new byte[15];

            MD5 md5 = new MD5CryptoServiceProvider();

            data = Encoding.ASCII.GetBytes(Password);

            byte[] result = md5.ComputeHash(data);

            return Encoding.ASCII.GetString(result);

        }

        public static string GetSearchString(string val)
        {
            return val.Trim().Replace("'", "''").Replace(";", "");
        }

        public static bool IsFileExists(string filePath)
        {
            FileInfo myFile = new FileInfo(filePath);
            return myFile.Exists;
        }

        private static double HoursDiffLocalTime(DateTime dtTime)
        {
            return ((TimeZone.CurrentTimeZone.GetUtcOffset(dtTime).TotalMilliseconds) / (1000 * 60 * 60));
        }

        public static double GetDaylightSavingsChanges(DateTime dtTime)
        {
            double st = CommonUtils.ServerUtcOffset;
            double lt = CommonUtils.HoursDiffLocalTime(dtTime);
            return st - lt;
        }

        public static DateTime GetTimeStamp(DateTime dtTime, double hoursDiffStdTime, bool isUniversalTime, bool isDaylightSavingTime)
        {
            if (isUniversalTime)
                dtTime = dtTime.ToLocalTime();
            double hoursDiffLocalTime = CommonUtils.HoursDiffLocalTime(dtTime) * -1;
            if (isDaylightSavingTime)
                hoursDiffStdTime += (CommonUtils.GetDaylightSavingsChanges(dtTime) * -1);
            hoursDiffLocalTime += hoursDiffStdTime;
            return dtTime.AddHours(hoursDiffLocalTime);
        }

        public static DateTime GetTimeStamp(DateTime dtTime, double hoursDiffStdTime, bool isUniversalTime)
        {
            return GetTimeStamp(dtTime, hoursDiffStdTime, isUniversalTime, IsDaylightSavingTime);
        }

        public static DateTime GetTimeStamp(DateTime dtTime, string timeZoneName)
        {
            double hoursDiffStdTime = GetTimeZoneOffset(timeZoneName);
            return GetTimeStamp(dtTime, hoursDiffStdTime, false, IsDaylightSavingTime);
        }

        public static DateTime GetTimeStamp(DateTime dtTime, string timeZoneName, bool isUniversalTime)
        {
            double hoursDiffStdTime = GetTimeZoneOffset(timeZoneName);
            return GetTimeStamp(dtTime, hoursDiffStdTime, isUniversalTime, IsDaylightSavingTime);
        }

        public static SortedDictionary<string, double> GetTimeZones()
        {
            try
            {
                var srl = new SortedDictionary<string, double>();
                RegistryKey okey = Registry.LocalMachine.OpenSubKey("SoftWare\\Microsoft\\" +
                    "Windows NT\\CurrentVersion\\Time Zones");
                string[] subkeys = okey.GetSubKeyNames();
                foreach (string subkey in subkeys)
                {
                    string timezonename = okey.OpenSubKey(subkey).GetValue("Display").ToString();
                    double hoursDiffStdTime = GetTimeZoneOffset(timezonename);
                    srl.Add(timezonename, hoursDiffStdTime);
                }
                return srl;
            }
            catch { return null; }
        }

        public static double GetTimeZoneOffset(string timezonename)
        {
            try
            {
                string tvalue = timezonename.Substring(0, timezonename.LastIndexOf(')')).Substring(4).Trim();
                if (tvalue == "") tvalue = "+00:00";
                string op = tvalue.Substring(0, 1);
                double hour = Convert.ToDouble(tvalue.Substring(1, 2));
                double minute = Convert.ToDouble(tvalue.Substring(4, 2));
                double hoursDiffStdTime = ((hour * 60) + minute) / 60;
                if (op == "-") hoursDiffStdTime = hoursDiffStdTime * -1;
                return hoursDiffStdTime;
            }
            catch { return 0; }
        }

        #endregion

        #region Properties

        public static DateTime GetEntryDate
        {
            get
            {
                if (DateTime.Today.Day <= 10) return DateTime.Today.AddMonths(-1);
                return DateTime.Today;
            }
        }

        #endregion
    }
}
