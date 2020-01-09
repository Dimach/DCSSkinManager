using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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

    public class UserFile
    {
        public readonly String Name;
        public readonly String Description;
        public readonly String Author;
        public readonly String Date;
        public readonly String Size;
        public readonly String Downloads;
        public readonly String DownloadLink;
    }

    public enum UnitType
    {
        KA50 = 31,
        SU27 = 23,
        SU33 = 28,
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