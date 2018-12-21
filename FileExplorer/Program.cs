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


        static List<string> GetUsers(List<String> ToCheck) {
            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            StringBuilder UsersList = new StringBuilder();
            List<string> Users = new List<string>();

            foreach (string check in ToCheck)
            {
                // find the group in question
                GroupPrincipal group = GroupPrincipal.FindByIdentity(ctx, check);
                // if found....
                if (group != null)
                {
                    // iterate over members
                    foreach (Principal p in group.GetMembers())
                    {
                        Users.Add(p.DisplayName + " <" + p.UserPrincipalName + ">");
                        // do whatever you need to do to those members
                        UserPrincipal theUser = p as UserPrincipal;
                        if (theUser != null)
                        {
                            if (p.DisplayName != null)
                            {
                                UsersList.Append(p.DisplayName + " <" + p.UserPrincipalName + ">");
                            }
                        }
                    }
                }
                else
                {
                    UserPrincipal foundUser = UserPrincipal.FindByIdentity(ctx, check);
                    if (foundUser != null)
                    {
                        if (foundUser.DisplayName != null)
                        {
                            Users.Add(foundUser.DisplayName + " <" + foundUser.UserPrincipalName + ">");
                        }
                    }
                }
            }

            return Users = Users.Union(Users).ToList();
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
                    List<string> accessList = new List<string>();

                    foreach (FileSystemAccessRule ace in acl)
                    {
                        accessList.Add(ace.IdentityReference.Value);
                    }

                    /* get size */ 
                    long length = DirSize(new ZetaLongPaths.ZlpDirectoryInfo(d));

                    List<string> users = GetUsers(accessList);

                    

                    /* add to CSV file */
                    csvFile.Add(new Csv(d, ToBytesCount(length), dInfo.LastAccessTime.Date, dInfo.LastWriteTime.Date, dInfo.CreationTime, String.Join("; ", accessList.ToArray()), String.Join("; ", users.ToArray())));

                    Console.WriteLine(d + ToBytesCount(length).ToString() + Directory.GetLastAccessTime(d).ToString() + Directory.GetLastWriteTime(d) + dInfo.CreationTime + String.Join("; ", accessList.ToArray()));

                    if (Deep > 0) {
                        DirSearch(csvFile, d, Deep - 1);
                    }

                }
            }
            catch (UnauthorizedAccessException) { }
        }

        internal class Csv
        {
            public Csv(string _Name, string _size, DateTime _lastAccessed, DateTime _lastWriteTime, DateTime _creationTime, string _access, string _users) {
                Name = _Name;
                Size = _size;
                LastAccessed = _lastAccessed;
                LastWriteTime = _lastWriteTime;
                CreationTime = _creationTime;
                Access = _access;
                Users = _users;
            }
            public string Name { get; set; }
            public string Size { get; set; }
            public DateTime LastAccessed { get; set; }
            public DateTime LastWriteTime { get; set; }
            public DateTime CreationTime { get; set; }
            public string Access { get; set; }
            public string Users { get; set; }
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
            string path = "S:\\Faculty-of-Medicine-and-Health\\Research-Projects";
            var filename = Directory.GetCurrentDirectory() + @"\file"+ DateTimeOffset.UtcNow.ToUnixTimeSeconds() + ".csv";
            List<Csv> CsvFile = new List<Csv>();
            DirSearch(CsvFile, path, 2);
            WriteCsvFile(filename, CsvFile);
            Console.WriteLine("here");
            Console.ReadKey();
        }
    }
}
