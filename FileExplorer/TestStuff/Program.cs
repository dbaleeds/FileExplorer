using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace TestStuff
{
    class Program
    {
        static void GetDirectorySecurity(string dir, int levels)
        {
            int curLevel = 1;
            string[] dirs = Directory.GetDirectories(dir);
            foreach (string directory in dirs)
            {
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine(directory);
                try
                {
                    string tabs = "\t";
                    DirectoryInfo dInfo = new DirectoryInfo(directory);
                    DirectorySecurity dSecurity = dInfo.GetAccessControl();
                    AuthorizationRuleCollection acl = dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    foreach (FileSystemAccessRule ace in acl)
                    {
                        Console.WriteLine("{0}Account: {1}", tabs, ace.IdentityReference.Value);
                        Console.WriteLine("{0}Type: {1}", tabs, ace.AccessControlType);
                        Console.WriteLine("{0}Rights: {1}", tabs, ace.FileSystemRights);
                        Console.WriteLine("{0}Inherited: {1}", tabs, ace.IsInherited);
                        Console.WriteLine();
                    }
                    if (curLevel < levels)
                        GetDirectorySecurity(@directory, curLevel + 1, levels);
                }
                catch
                {
                    Console.WriteLine("Could not access {0}", directory);
                }
            }
        }
        static void GetDirectorySecurity(string dir, int curLevel, int levels)
        {
            string[] dirs = Directory.GetDirectories(@dir);
            string tabs = "";
            for (int i = 0; i < curLevel; i++)
                tabs += "\t";
            foreach (string directory in dirs)
            {
                Console.WriteLine(tabs.Substring(0, tabs.Length - 1) + "---------------------------------------------------------");
                Console.WriteLine(tabs.Substring(0, tabs.Length - 1) + directory);
                try
                {
                    DirectoryInfo dInfo = new DirectoryInfo(directory);
                    DirectorySecurity dSecurity = dInfo.GetAccessControl();
                    AuthorizationRuleCollection acl = dSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));
                    foreach (FileSystemAccessRule ace in acl)
                    {
                        Console.WriteLine("{0}Account: {1}", tabs, ace.IdentityReference.Value);
                        Console.WriteLine("{0}Type: {1}", tabs, ace.AccessControlType);
                        Console.WriteLine("{0}Rights: {1}", tabs, ace.FileSystemRights);
                        Console.WriteLine("{0}Inherited: {1}", tabs, ace.IsInherited);
                        Console.WriteLine();
                    }
                    if (curLevel < levels)
                        GetDirectorySecurity(@directory, curLevel + 1, levels);
                }
                catch
                {
                    Console.WriteLine("Could not access {0}", directory);
                }
            }
        }
        static void Main(string[] args)
        {
            GetDirectorySecurity("S:\\Faculty-of-Medicine-and-Health\\Anatomy", 4);

            /*try
            {
                if (args[0] != null && args[1] != null)
                    GetDirectorySecurity(@args[0], int.Parse(args[1]));
                else
                {
                    Console.WriteLine("This program requires the input of a starting directory path");
                    Console.WriteLine("including the letter drive followed by an integer specifying");
                    Console.WriteLine("how many directories deep to recursively scan");
                    Console.WriteLine();
                    Console.WriteLine("Example: programname C:\\shared\\ 2");
                }
            }
            catch
            {
                {
                    Console.WriteLine("This program requires the input of a starting directory path");
                    Console.WriteLine("including the letter drive followed by an integer specifying");
                    Console.WriteLine("how many directories deep to recursively scan");
                    Console.WriteLine();
                    Console.WriteLine("Example: FolderPermissions C:\\shared\\ 2");
                    Console.WriteLine("\tThis will scan 2 levels deep on the shared folder on C:");
                }
            }*/
            Console.ReadKey();
        }
    }
}
