using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using DCSSkinManager.Utils;

namespace DCSSkinManager
{
    public class UserFiles
    {
        public readonly UnitType Filter;
        public readonly List<UserFile> Files;

        public UserFiles(UnitType filter)
        {
            Filter = filter;
        }
    }

    public struct UserFile
    {
        public String Name { get; set; }
        public String Description { get; set; }
        public String Author { get; set; }
        public String Date { get; set; }
        public String Size { get; set; }
        public String Downloads { get; set; }
        private String DownloadLink { get; set; }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum UnitType
    {
        [Description("Ka-50")]
        KA50 = 31,
        [Description("Su-27")]
        SU27 = 23,
        [Description("Su-33")]
        SU33 = 28,
        [Description("Su-25")]
        SU25 = 24
    }

    public class DataLoader
    {
        public static void LoadUserFiles(UserFiles list)
        {
            string html = string.Empty;
            string url = $@"http://www.digitalcombatsimulator.com/en/files/?PER_PAGE=100&set_filter=Y&arrFilter_pf%5Bfiletype%5D=6&arrFilter_pf%5Bgameversion%5D=&arrFilter_pf%5Bfilelang%5D=&arrFilter_pf%5Baircraft%5D={list.Filter}&arrFilter_DATE_CREATE_1_DAYS_TO_BACK=&CREATED_BY=&sort_by_order=TIMESTAMP_X_DESC&set_filter=Filter";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            Console.WriteLine(html);
        }
    }
}