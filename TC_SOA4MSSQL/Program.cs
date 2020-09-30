using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Teamcenter.ClientX;
using User = Teamcenter.Soa.Client.Model.Strong.User;

namespace TC_SOA_cmd
{
    class Program
    {
        public static void Main(string[] arg)
        {
            String serverHost = "http://192.168.110.128:7001/tc";


            try
            {

                Session session = new Session(serverHost);
                //HomeFolder home = new HomeFolder();
                //Query query = new Query();
                DataManagement dm = new DataManagement();

                // Establish a session with the Teamcenter Server
                User user = session.login();

                //// Using the User object returned from the login service request
                //// display the contents of the Home Folder
                //home.listHomeFolder(user);

                //// Perform a simple query of the database
                //query.queryItems();

                // Perform some basic data management functions
                //dm.createReviseAndDelete();
                dm.createMyItem("Item");

                //dm.queryItems();
                // Terminate the session with the Teamcenter server
                session.logout();
            }
            catch (SystemException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
