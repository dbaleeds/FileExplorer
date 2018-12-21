using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdGroupExtractor
{
    class Program
    {

    

        static void Main(string[] args)
        {

            // set up domain context
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
            List<string> ToCheck = new List<string>(new string[]{ "meddbat", "FMH-SEED-YDN-Users" });
            StringBuilder UsersList = new StringBuilder();
            List<string> Users = new List<string>();

            foreach (string check in ToCheck) {
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
                            UsersList.Append(p.DisplayName + " <" + p.UserPrincipalName + ">");
                        }
                    }
                }
                else
                {
                    UserPrincipal foundUser = UserPrincipal.FindByIdentity(ctx, check);
                    Users.Add(foundUser.DisplayName + " <" + foundUser.UserPrincipalName + ">");
                }
            }

            Users = Users.Union(Users).ToList();

            Console.WriteLine(String.Join("; ", Users.ToArray()));
            Console.ReadKey();
        }
    }
}
