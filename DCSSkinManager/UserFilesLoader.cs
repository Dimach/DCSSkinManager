using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using DCSSkinManager.Utils;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SevenZip;


namespace DCSSkinManager
{
    public class UserFiles
    {
        public readonly UnitType UnitType;
        public List<UserFile> Files { get; } = new List<UserFile>();

        public UserFiles(UnitType unitType)
        {
            UnitType = unitType;
        }
    }

    public class UserFile
    {
        public UnitType UnitType { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String Author { get; set; }
        public String Date { get; set; }
        public String Size { get; set; }
        public String Downloads { get; set; }
        public String DownloadLink { get; set; }
        public String Id { get; set; }
        public bool Downloaded { get; set; }
        public String[] Preview { get; set; }

        public UserFile(UnitType unitType)
        {
            UnitType = unitType;
        }

        public UserFile()
        {
        }
    }

    public class DirectoryName : Attribute
    {
        public string Directory;

        public DirectoryName(string directory)
        {
            Directory = directory;
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum UnitType
    {
        [Description("Ka-50"), DirectoryName("ka-50")]
        KA50 = 31,

        [Description("Mi-8MTV2"), DirectoryName("mi-8mt")]
        MI8MTV2 = 93,

        [Description("Su-27"), DirectoryName("su-27")]
        SU27 = 23,

        [Description("Su-33"), DirectoryName("su-33")]
        SU33 = 28,

        [Description("Su-25"), DirectoryName("su-25")]
        SU25 = 24,

        [Description("Su-25T"), DirectoryName("su-25t")]
        SU25T = 29,

        [Description("Mig-29A"), DirectoryName("mig-29a")]
        MIG29A = 21,

        [Description("Mig-29S"), DirectoryName("mig-29s")]
        MIG29S = 26,

        [Description("M-2000C"), DirectoryName("M-2000C")]
        M2000C = 313,
        
        [Description("A-10A"), DirectoryName("a-10a")]
        A10A = 25,
        
        [Description("A-10C"), DirectoryName("a-10c")]
        A10C = 60,
        
        [Description("F-5E"), DirectoryName("f-5e-3")]
        F5E = 393,

        [Description("F-14B"), DirectoryName("f-14b")]
        F14B = 545,
        
        [Description("F-15C"), DirectoryName("f-15c")]
        F15C = 22,

        [Description("F-16C"), DirectoryName("F-16C_50")]
        F16C = 561,

        [Description("F/A-18C"), DirectoryName("FA-18C_hornet")]
        FA18C = 535,
    }

    public class DataLoader
    {
        public String DcsInstallDirectory;

        public DataLoader(string dcsInstallDirectory)
        {
            DcsInstallDirectory = dcsInstallDirectory;
        }

        private MemoryStream DownloadResource(String url)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var response = (HttpWebResponse) request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var memory = new MemoryStream();
                stream.CopyTo(memory);
                memory.Position = 0;
                return memory;
            }
        }

        public void CheckDownloadedFiles(UserFiles list)
        {
            var dirs = Directory.GetDirectories($@"{DcsInstallDirectory}\{list.UnitType.DirectoryName()}").Select(dir =>
            {
                dir = dir.Substring(dir.LastIndexOf('\\') + 1);
                var index = dir.IndexOf('.');
                return index == -1 ? String.Empty : dir.Substring(0, index);
            }).ToArray();
            foreach (var file in list.Files)
            {
                file.Downloaded = dirs.Any(str => str.Equals(file.Id));
            }
        }

        public Task<Image> GetImage(String url)
        {
            var promise = new TaskCompletionSource<Image>();
            promise.SetResult(Image.FromStream(DownloadResource($@"https://www.digitalcombatsimulator.com/upload/iblock/{url}")));
            return promise.Task;
        }

        public UserFiles LoadUserFiles(UnitType unit)
        {
            var url = $@"https://www.digitalcombatsimulator.com/en/files/?PER_PAGE=10000&set_filter=Y&arrFilter_pf%5Bfiletype%5D=6&arrFilter_pf%5Bgameversion%5D=&arrFilter_pf%5Bfilelang%5D=&arrFilter_pf%5Baircraft%5D={(int) unit}&arrFilter_DATE_CREATE_1_DAYS_TO_BACK=&CREATED_BY=&sort_by_order=TIMESTAMP_X_DESC&set_filter=Filter";
            return ParsePage(unit, new StreamReader(DownloadResource(url)).ReadToEnd());
        }

        private UserFiles ParsePage(UnitType type, String page)
        {
            var list = new UserFiles(type);
            foreach (var i in page.Split(new[] {"<div class=\"col-xs-2 text-center\">"}, StringSplitOptions.None).Skip(1))
            {
                var endIndex = i.IndexOf("class=\"btn btn-default pull-right\">Detail");
                if (endIndex < 0) continue;
                var userFile = new UserFile(type);
                var dataString = i.Substring(0, endIndex);
                {
                    var name = Regex.Match(dataString, "<h2><a href=\".+\">.+<\\/a>").Value.Substring(13);
                    userFile.Id = name.Substring(0, name.IndexOf("\"")).Replace("/en/files/", "").Replace("/", "");
                    var startIndex = name.IndexOf(">") + 1;
                    userFile.Name = WebUtility.HtmlDecode(name.Substring(startIndex, name.IndexOf("</a>") - startIndex).Trim());
                }
                {
                    var name = Regex.Match(dataString, "Author - <a href=\".+\">.+<\\/a>").Value;
                    var startIndex = name.IndexOf(">") + 1;
                    userFile.Author = WebUtility.HtmlDecode(name.Substring(startIndex, name.IndexOf("</a>") - startIndex).Trim());
                }
                {
                    var name = Regex.Match(dataString, "Date - .+<\\/div>").Value;
                    userFile.Date = name.Substring(7, name.IndexOf("</div>") - 7).Trim();
                }
                {
                    var name = Regex.Match(dataString, "<div class=\"row file-preview-text\">[\\s\\S]+?<\\/div>").Value;
                    var startIndex = name.IndexOf(">", 40) + 1;
                    userFile.Description = WebUtility.HtmlDecode(name.Substring(startIndex, name.IndexOf("</div>") - startIndex).Replace("\n", "").Replace("\n", "").Trim().Replace("<br />", "\n"));
                }
                {
                    var name = Regex.Match(dataString, "Size:<\\/b>.+<\\/li>").Value;
                    userFile.Size = name.Substring(10, name.IndexOf("</li>") - 10).Trim();
                }
                {
                    var name = Regex.Match(dataString, "Downloaded:<\\/b>.+<\\/li>").Value;
                    userFile.Downloads = name.Substring(15, name.IndexOf("</li>") - 15).Trim();
                }
                {
                    var name = Regex.Match(dataString, "<a href=\".+\">Download<\\/a>").Value;
                    var startIndex = name.IndexOf("\"") + 1;
                    userFile.DownloadLink = WebUtility.HtmlDecode(name.Substring(startIndex, name.IndexOf("\"", startIndex) - startIndex).Trim());
                }
                {
                    var matches = Regex.Matches(dataString, "<a href=\"\\/upload\\/iblock.+?\"");
                    userFile.Preview = new String[matches.Count];
                    for (int j = 0; j < userFile.Preview.Length; j++)
                    {
                        userFile.Preview[j] = matches[j].Value.Substring(24, matches[j].Value.Length - 25); ///upload/iblock/
                    }
                }
                list.Files.Add(userFile);
            }

            return list;
        }

        public void DeleteFile(UserFile file)
        {
            foreach (var dir in Directory.GetDirectories($@"{DcsInstallDirectory}\{file.UnitType.DirectoryName()}"))
            {
                var index = dir.IndexOf(".");
                if (index != -1 && dir.Substring(0, index).Equals(file.Id))
                {
                    Directory.Delete(dir, true);
                }
            }
        }

        public void InstallFile(UserFile file)
        {
            var url = $@"https://www.digitalcombatsimulator.com{file.DownloadLink}";

            using (var extractor = new SevenZipExtractor(DownloadResource(url)))
            {
                var skinList = new List<Skin>();
                foreach (var archiveFile in extractor.ArchiveFileData)
                {
                    if (!archiveFile.IsDirectory && archiveFile.FileName.EndsWith("\\description.lua"))
                    {
                        var archivePath = archiveFile.FileName.Substring(0, archiveFile.FileName.Length - 16);
                        skinList.Add(new Skin(archivePath + "\\", file.Id + "." + archivePath.Substring(archivePath.LastIndexOf("\\") + 1)));
                    }
                }

                foreach (var archiveFile in extractor.ArchiveFileData)
                {
                    if (!archiveFile.IsDirectory)
                    {
                        skinList.FirstOrDefault(skin => archiveFile.FileName.StartsWith(skin.archivePath))?.indexes?.Add(archiveFile.Index);
                    }
                }

                foreach (var skin in skinList)
                {
                    var directoryName = $@"{DcsInstallDirectory}\{file.UnitType.DirectoryName()}\{skin.directoryName}";
                    if (Directory.Exists(directoryName))
                        Directory.Delete(directoryName, true);
                    Directory.CreateDirectory(directoryName);
                    foreach (var i in skin.indexes)
                    {
                        var archiveFileName = extractor.ArchiveFileNames[i];
                        using (var fileStream = new FileStream($@"{directoryName}\{archiveFileName.Substring(archiveFileName.LastIndexOf("\\") + 1)}", FileMode.Create))
                        {
                            extractor.ExtractFile(i, fileStream);
                        }
                    }
                }
            }
        }

        private class Skin
        {
            public String archivePath;
            public String directoryName;
            public List<int> indexes = new List<int>();

            public Skin(String archivePath, String directoryName)
            {
                this.archivePath = archivePath;
                this.directoryName = directoryName;
            }
        }
    }
}