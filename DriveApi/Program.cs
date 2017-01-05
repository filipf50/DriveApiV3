
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DriveQuickstart
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/drive-dotnet-quickstart.json
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Drive API .NET Quickstart";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 1000;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = "name contains '.osiris'";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
                .Files;
            Console.WriteLine("Files:");
            if (files != null && files.Count > 0)
            {
                using (StreamWriter w = System.IO.File.AppendText ("log.txt"))
                {


                    foreach (var file in files)
                    {
                        
                        RevisionList revlist = service.Revisions.List(file.Id).Execute();
                        string strRevisions = "";
                        foreach (Revision rev in revlist.Revisions)
                        {
                            
                            if (DateTime.Compare(rev.ModifiedTime ?? DateTime.Now, Convert.ToDateTime("15/12/2016 18:40:00")) < 0)
                            {
                                strRevisions += rev.Id + " - " + rev.ModifiedTime + Environment.NewLine;
                                rev.KeepForever = true;
                                
                            } else if (DateTime.Compare(rev.ModifiedTime ?? DateTime.Now, Convert.ToDateTime("15/12/2016 18:40:00")) > 0)
                            {
                                service.Revisions.Delete(file.Id, rev.Id).Execute();
                            }
                        }

                        Log("File " + file.Name + " renamed to " + file.OriginalFilename + " and restored " + strRevisions + " revision.",w);
                        

                        Console.WriteLine("{0} ({1})-{2}", file.Name, file.Id, strRevisions);
                    }
                }
            }
            else
            {
                Console.WriteLine("No files found.");
            }
            Console.Read();
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }
    }
}