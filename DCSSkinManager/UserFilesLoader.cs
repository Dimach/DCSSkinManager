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
using System.Threading;
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

        [Description("Mig-15bis"), DirectoryName("MiG-15bis")]
        MIG15BIS = 153,

        [Description("Mig-19P"), DirectoryName("MiG-19P")]
        MIG19P = 543,

        [Description("Mig-21bis"), DirectoryName("MiG-21Bis")]
        MIG21BIS = 154,

        [Description("Mig-29A"), DirectoryName("mig-29a")]
        MIG29A = 21,

        [Description("Mig-29S"), DirectoryName("mig-29s")]
        MIG29S = 26,

        [Description("L-39"), DirectoryName("L-39C")]
        L39 = 247,

        [Description("J-11A"), DirectoryName("J-11A")]
        J11A = 538,

        [Description("JF-17"), DirectoryName("JF-17")]
        JF17 = 562,

        [Description("SA342"), DirectoryName("SA342M")]
        SA342 = 376,

        [Description("M-2000C"), DirectoryName("M-2000C")]
        M2000C = 313,

        [Description("AJS-37"), DirectoryName("AJS37")]
        AJS37 = 431,

        [Description("UH-1H"), DirectoryName("uh-1h")]
        UH1H = 90,

        [Description("A-10A"), DirectoryName("a-10a")]
        A10A = 25,

        [Description("A-10C"), DirectoryName("a-10c")]
        A10C = 60,

        [Description("AV-8B"), DirectoryName("AV8BNA")]
        AV8B = 501,

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

        [Description("F-86F"), DirectoryName("f-86f sabre")]
        F86F = 150,

        [Description("Fw 190 D-9"), DirectoryName("FW-190D9")]
        FW190D9 = 148,

        [Description("Bf 109 K-4"), DirectoryName("Bf-109K-4")]
        BF109K4 = 149,

        [Description("Yak-52"), DirectoryName("Yak-52")]
        YAK52 = 537,

        [Description("P-51D"), DirectoryName("P-51D")]
        P51D = 85,

        [Description("Spitfire LF Mk.IX"), DirectoryName("SpitfireLFMkIX")]
        SPITFIREMK9 = 430,

        [Description("I-16"), DirectoryName("I-16")]
        I16 = 560,

        [Description("C-101"), DirectoryName("C-101CC")]
        C101 = 151,

        [Description("Christen Eagle II"), DirectoryName("Christen Eagle II")]
        CE2 = 544,

        [Description("Hawk T.1A"), DirectoryName("Hawk")]
        HAWK = 152,
    }

    public class DataLoader
    {
        public string DcsInstallDirectory;
        private Dictionary<string, Task<string>> downloadingPreviewImages = new Dictionary<string, Task<string>>();

        public DataLoader(string dcsInstallDirectory)
        {
            DcsInstallDirectory = dcsInstallDirectory;
        }

        private async Task<Stream> DownloadResource(String url, CancellationToken token)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var response = (HttpWebResponse) request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                var memory = new MemoryStream();
                await stream.CopyToAsync(memory, 81920, token);
                memory.Position = 0;
                return memory;
            }
        }

        private void CheckDownloadedFiles(UserFiles list)
        {
            var moduleDir = Path.Combine(DcsInstallDirectory, "Liveries", list.UnitType.DirectoryName());
            if (!Directory.Exists(moduleDir))
                return;
            var dirs = Directory.GetDirectories(moduleDir).Select(dir =>
            {
                dir = Path.GetFileName(dir);
                var index = dir.IndexOf('.');
                return index == -1 ? String.Empty : dir.Substring(0, index);
            }).ToArray();
            foreach (var file in list.Files)
            {
                file.Downloaded = dirs.Any(str => str.Equals(file.Id));
            }
        }

        public Task<string> GetPreviewImage(string url, CancellationToken token)
        {
            lock (downloadingPreviewImages)
            {
                if (downloadingPreviewImages.ContainsKey(url))
                {
                    return downloadingPreviewImages[url];
                }

                var task = Task.Run(async () =>
                {
                    var cacheFolderPath = Path.Combine(DcsInstallDirectory, @".DCSSkinManager\PreviewCache");
                    if (!Directory.Exists(cacheFolderPath))
                        Directory.CreateDirectory(cacheFolderPath);
                    var fileInfo = new FileInfo(Path.Combine(cacheFolderPath, WebUtility.UrlDecode(url.Replace("/", "$"))));
                    if (fileInfo.Exists && fileInfo.Length > 0)
                        return fileInfo.FullName;
                    var resource = await DownloadResource($@"https://www.digitalcombatsimulator.com/upload/iblock/{url}", token);
                    using (var fileStream = fileInfo.Create())
                    {
                        resource.CopyTo(fileStream);
                    }

                    lock (downloadingPreviewImages)
                    {
                        downloadingPreviewImages.Remove(url);
                    }

                    return fileInfo.FullName;
                }, token);
                downloadingPreviewImages[url] = task;
                return task;
            }
        }

        public Task<UserFiles> LoadUserFiles(UnitType unit, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                var url = $@"https://www.digitalcombatsimulator.com/en/files/?PER_PAGE=10000&set_filter=Y&arrFilter_pf%5Bfiletype%5D=6&arrFilter_pf%5Bgameversion%5D=&arrFilter_pf%5Bfilelang%5D=&arrFilter_pf%5Baircraft%5D={(int) unit}&arrFilter_DATE_CREATE_1_DAYS_TO_BACK=&CREATED_BY=&sort_by_order=TIMESTAMP_X_DESC&set_filter=Filter";
                var resource = await DownloadResource(url, token);
                var userFiles = ParsePage(unit, new StreamReader(resource).ReadToEnd());
                CheckDownloadedFiles(userFiles);
                return userFiles;
            }, token);
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

                var match = Regex.Match(dataString, "<h2><a href=\"\\/en\\/files\\/(.+)\\/\">(.+)<\\/a>");
                userFile.Id = match.Groups[1].Value;
                userFile.Name = WebUtility.HtmlDecode(match.Groups[2].Value.Trim());
                userFile.Author = WebUtility.HtmlDecode(Regex.Match(dataString, "Author - <a href=\".+\">(.+)<\\/a>").Groups[1].Value.Trim());
                userFile.Date = Regex.Match(dataString, "Date - (.+)<\\/div>").Groups[1].Value.Trim();
                userFile.Description = WebUtility.HtmlDecode(Regex.Match(dataString, "<div class=\"row file-preview-text\">[\\s\\S]+?>([\\s\\S]+?)<\\/div>").Groups[1].Value.Replace("\n", "").Trim().Replace("<br />", "\n"));
                userFile.Size = Regex.Match(dataString, "Size:<\\/b>(.+)<\\/li>").Groups[1].Value.Trim();
                userFile.Downloads = Regex.Match(dataString, "Downloaded:<\\/b>(.+)<\\/li>").Groups[1].Value.Trim();
                userFile.DownloadLink = WebUtility.HtmlDecode(Regex.Match(dataString, "<a href=\"(.+?)\">Download<\\/a>").Groups[1].Value.Trim());
                var matches = Regex.Matches(dataString, "<a href=\"\\/upload\\/iblock(.+?)\"");
                userFile.Preview = new string[matches.Count];
                for (var j = 0; j < userFile.Preview.Length; j++)
                {
                    userFile.Preview[j] = matches[j].Groups[1].Value;
                }
                list.Files.Add(userFile);
            }

            return list;
        }

        public Task DeleteFile(UserFile file, CancellationToken token)
        {
            return Task.Run(() =>
            {
                foreach (var i in Directory.GetDirectories(Path.Combine(DcsInstallDirectory, "Liveries", file.UnitType.DirectoryName())))
                {
                    var dir = Path.GetFileName(i);
                    var index = dir.IndexOf(".");
                    if (index != -1 && dir.Substring(0, index).Equals(file.Id))
                    {
                        Directory.Delete(i, true);
                    }
                }
            });
        }

        public Task InstallFile(UserFile file, CancellationToken token)
        {
            return Task.Run(async () =>
            {
                var url = $@"https://www.digitalcombatsimulator.com{file.DownloadLink}";

                var resource = await DownloadResource(url, token);
                using (var extractor = new SevenZipExtractor(resource))
                {
                    var skinList = new List<Skin>();
                    foreach (var archiveFile in extractor.ArchiveFileData)
                    {
                        if (!archiveFile.IsDirectory && archiveFile.FileName.EndsWith("\\description.lua"))
                        {
                            var archivePath = archiveFile.FileName.Substring(0, archiveFile.FileName.Length - 16);
                            skinList.Add(new Skin(archivePath + "\\", file.Id + "." + Path.GetFileName(archivePath)));
                        }
                    }

                    foreach (var archiveFile in extractor.ArchiveFileData)
                    {
                        if (!archiveFile.IsDirectory)
                        {
                            skinList.FirstOrDefault(skin => archiveFile.FileName.StartsWith(skin.ArchivePath))?.Indexes?.Add(archiveFile.Index);
                        }
                    }

                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (var skin in skinList)
                    {
                        var directoryName = Path.Combine(DcsInstallDirectory, "Liveries", file.UnitType.DirectoryName(), skin.DirectoryName);
                        if (Directory.Exists(directoryName))
                            Directory.Delete(directoryName, true);
                        Directory.CreateDirectory(directoryName);
                        foreach (var i in skin.Indexes)
                        {
                            using (var fileStream = new FileStream(Path.Combine(directoryName, Path.GetFileName(extractor.ArchiveFileNames[i])), FileMode.Create))
                            {
                                extractor.ExtractFile(i, fileStream);
                            }
                        }
                    }
                }
            }, token);
        }

        private class Skin
        {
            public String ArchivePath;
            public String DirectoryName;
            public List<int> Indexes = new List<int>();

            public Skin(String archivePath, String directoryName)
            {
                ArchivePath = archivePath;
                DirectoryName = directoryName;
            }
        }
    }
}