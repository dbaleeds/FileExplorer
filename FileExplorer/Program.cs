using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ZetaLongPaths;

using System.DirectoryServices;

using System.DirectoryServices.AccountManagement;

namespace FileExplorer
{
    class Program
    {

        static string directoryInfo(string Dir)
        {
            string stringDir = "";

            var di = new DirectoryInfo(Dir);
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                Console.WriteLine(dir.FullName + "<br/>");
                DirectorySecurity ds = dir.GetAccessControl(AccessControlSections.Access);
                foreach (FileSystemAccessRule fsar in ds.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    string userName = fsar.IdentityReference.Value;
                    string userRights = fsar.FileSystemRights.ToString();
                    string userAccessType = fsar.AccessControlType.ToString();
                    string ruleSource = fsar.IsInherited ? "Inherited" : "Explicit";
                    string rulePropagation = fsar.PropagationFlags.ToString();
                    string ruleInheritance = fsar.InheritanceFlags.ToString();
                    stringDir = stringDir + "+ " + userName + " : " + userAccessType + " : " + userRights + " : " + ruleSource + " : " + rulePropagation + " : " + ruleInheritance + "|";
                }
            }
            return stringDir;
        }



        static string ToBytesCount(long bytes)
        {
            int unit = 1024;
            if (bytes < unit) return bytes + " KB";
            int exp = (int)(Math.Log(bytes) / Math.Log(unit));
            return string.Format("{0:##.##} {1}B", bytes / Math.Pow(unit, exp), "KMGTPE"[exp - 1]);
        }

        static long DirSize(ZlpDirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            ZlpFileInfo[] fis = d.GetFiles();
            foreach (ZlpFileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            ZlpDirectoryInfo[] dis = d.GetDirectories();
            foreach (ZlpDirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        static void DirSearch(List<Csv> csvFile, string sDir, int Deep)
        {
            
            try
            {
                foreach (string d in Directory.GetDirectories(sDir, "*", SearchOption.TopDirectoryOnly))
                {
                    /*security*/
                    DirectoryInfo dInfo = new DirectoryInfo(d);
                    DirectorySecurity dSecurity = dInfo.GetAccessControl();
                    AuthorizationRuleCollection acl = dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    StringBuilder accessTxt = new StringBuilder();
                    foreach (FileSystemAccessRule ace in acl)
                    {
                        accessTxt.Append(ace.IdentityReference.Value);
                        accessTxt.Append(" : ");
                    }

                    if (accessTxt.Length > 2) {
                        accessTxt.Length = accessTxt.Length - 2;        
                    }

                    /* get size */ 
                    long length = DirSize(new ZetaLongPaths.ZlpDirectoryInfo(d));

                    /* add to CSV file */
                    csvFile.Add(new Csv(d, ToBytesCount(length), Directory.GetLastAccessTime(d), Directory.GetLastWriteTime(d), accessTxt.ToString()));

                    Console.WriteLine(d + ToBytesCount(length).ToString() + Directory.GetLastAccessTime(d).ToString() + Directory.GetLastWriteTime(d) + accessTxt.ToString());

                    if (Deep > 0) {
                        DirSearch(csvFile, d, Deep - 1);
                    }

                }
            }
            catch (UnauthorizedAccessException) { }
        }

        internal class Csv
        {
            public Csv(string _Name, string _size, DateTime _lastAccessed, DateTime _lastWriteTime, string _access) {
                Name = _Name;
                Size = _size;
                LastWriteTime = _lastWriteTime;
                Access = _access;
            }
            public string Name { get; set; }
            public string Size { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime LastWriteTime { get; set; }
            public string Access { get; set; }
            /*public int Age { get; set; }*/
        }

        static void WriteCsvFile(string filename, IEnumerable<Csv> csv)
        {
            TextWriter textWriter = File.CreateText(filename);
            var csvWriter = new CsvWriter(textWriter);
            csvWriter.WriteRecords(csv);
            textWriter.Close();
        }

        static void Main(string[] args)
        {
            string path = "M:\\Work\\Projects\\Code\\Tools";
            var filename = Directory.GetCurrentDirectory() + @"\file.csv";
            List<Csv> CsvFile = new List<Csv>();
            DirSearch(CsvFile, path, 1);
            WriteCsvFile(filename, CsvFile);
            Console.WriteLine("here");
            Console.ReadKey();
        }
    }
}
