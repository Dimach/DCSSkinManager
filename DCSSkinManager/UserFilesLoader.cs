using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace DCSSkinManager
{
    public class UserFiles
    {
        public readonly UnitType Filter;
        public readonly List<UserFile> Files = new List<UserFile>();

        public UserFiles(UnitType filter)
        {
            Filter = filter;
        }
    }

    public class UserFile
    {
        public String Name;
        public String Description;
        public String Author;
        public String Date;
        public String Size;
        public String Downloads;
        public String DownloadLink;
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
            var url =
                $@"http://www.digitalcombatsimulator.com/en/files/?PER_PAGE=100&set_filter=Y&arrFilter_pf%5Bfiletype%5D=6&arrFilter_pf%5Bgameversion%5D=&arrFilter_pf%5Bfilelang%5D=&arrFilter_pf%5Baircraft%5D={(int)list.Filter}&arrFilter_DATE_CREATE_1_DAYS_TO_BACK=&CREATED_BY=&sort_by_order=TIMESTAMP_X_DESC&set_filter=Filter";
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;


            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var page = reader.ReadToEnd();
                foreach (var i in page.Split(new[] {"<div class=\"col-xs-10\">"}, StringSplitOptions.None).Skip(1))
                {
                    var endIndex = i.IndexOf("<div class=\"col-xs-4 col-xs-offset-2\">");
                    if (endIndex < 0) continue;
                    var userFile = new UserFile();
                    var dataString = i.Substring(0, endIndex);
                    var name = Regex.Match(i.Substring(0, endIndex), "<a href=\".+\">.+<\\/a>").Value;
                    var startIndex = name.IndexOf(">") + 1;
                    userFile.Name = name.Substring(startIndex, name.IndexOf("</a>") - startIndex);
                    list.Files.Add(userFile);
                }
            }
        }
    }
}