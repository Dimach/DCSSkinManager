using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using DCSSkinManager.Utils;
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
        [Description("Ka-50")] KA50 = 31,
        [Description("Su-27")] SU27 = 23,
        [Description("Su-33")] SU33 = 28,
        [Description("Su-25")] SU25 = 24
    }

    public class DataLoader
    {
        public static void LoadUserFiles(UserFiles list)
        {
            var url =
                $@"http://www.digitalcombatsimulator.com/en/files/?PER_PAGE=100&set_filter=Y&arrFilter_pf%5Bfiletype%5D=6&arrFilter_pf%5Bgameversion%5D=&arrFilter_pf%5Bfilelang%5D=&arrFilter_pf%5Baircraft%5D={(int) list.Filter}&arrFilter_DATE_CREATE_1_DAYS_TO_BACK=&CREATED_BY=&sort_by_order=TIMESTAMP_X_DESC&set_filter=Filter";
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;


            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                parsePage(list, reader.ReadToEnd());
            }
        }

        public static void parsePage(UserFiles list, String page)
        {
            foreach (var i in page.Split(new[] {"<div class=\"col-xs-10\">"}, StringSplitOptions.None).Skip(1))
            {
                var endIndex = i.IndexOf("<div class=\"row file-head file-type-skn\">");
                if (endIndex < 0) continue;
                var userFile = new UserFile();
                var dataString = i.Substring(0, endIndex);
                {
                    var name = Regex.Match(dataString, "<a href=\".+\">.+<\\/a>").Value;
                    var startIndex = name.IndexOf(">") + 1;
                    userFile.Name = name.Substring(startIndex, name.IndexOf("</a>") - startIndex).Trim();
                }
                {
                    var name = Regex.Match(dataString, "Author - <a href=\".+\">.+<\\/a>").Value;
                    var startIndex = name.IndexOf(">") + 1;
                    userFile.Author = name.Substring(startIndex, name.IndexOf("</a>") - startIndex).Trim();
                }
                {
                    var name = Regex.Match(dataString, "Date - .+<\\/div>").Value;
                    userFile.Date = name.Substring(7, name.IndexOf("</div>") - 7).Trim();
                }
                {
                    var name = Regex.Match(dataString, "<div class=\"row file-preview-text\">[\\s\\S]+?<\\/div>").Value;
                    var startIndex = name.IndexOf(">", 40) + 1;
                    userFile.Description = name.Substring(startIndex, name.IndexOf("</div>") - startIndex).Replace("\n", "").Replace("\n", "").Trim().Replace("<br />", "\n");
                }
                {
                    var name = Regex.Match(dataString, "Size:<\\/b>.+<\\/li>").Value;
                    userFile.Size = name.Substring(10, name.IndexOf("</li>") - 10).Trim();
                }
                {
                    var name = Regex.Match(dataString, "Downloaded:<\\/b>.+<\\/li>").Value;
                    userFile.Downloads = name.Substring(15, name.IndexOf("</li>") - 15).Trim();
                }
                list.Files.Add(userFile);
            }
        }
    }
}