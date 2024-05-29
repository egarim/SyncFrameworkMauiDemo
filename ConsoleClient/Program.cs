using BIT.Data.Sync.EfCore.Npgsql;
using BIT.Data.Sync.EfCore.Pomelo.MySql;
using BIT.Data.Sync.EfCore.SQLite;
using BIT.Data.Sync.EfCore.SqlServer;
using BIT.Data.Sync.Imp;
using BIT.Data.Sync;
using BIT.EfCore.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Model.Contexts;
using System.Reflection.Metadata;

namespace ConsoleClient
{
    internal class Program
    {
        static string serverUrl;
        static void SetServerUrl()
        {
            Console.Write("Enter server URL: ");
            serverUrl = Console.ReadLine();

            Console.WriteLine("Server URL set to " + serverUrl);
            Pause();
        }
        static void ListBlogs()
        {
            var ContextTask = InitSyncFramework(serverUrl);
            ContextTask.Wait();
            var Context = ContextTask.Result as TestSyncFrameworkDbContext;
            Console.WriteLine($"Blog");
            foreach (Blog blog in Context.Blogs)
            {
                Console.WriteLine($"    -{blog.Name}");
            }

            Pause();
        }
        static void CreateBlog()
        {
            var ContextTask=InitSyncFramework(serverUrl);
            ContextTask.Wait();
            var Context = ContextTask.Result;

            Console.Write("Enter blog name: ");
            string blogName = Console.ReadLine();

            Console.Write("Enter blog text: ");
            string blogText = Console.ReadLine();

            Blog blog = new Blog
            {
                Name = blogName,

            };
            Context.Add(blog);
            Context.SaveChanges();
            // Add logic to insert the blog into the database here
            Console.WriteLine($"Blog '{blogName}' created with text: {blogText}");
            // Example: InsertBlogToDatabase(blogName, blogText);
            // Add logic to create a blog here
            Console.WriteLine("Creating blog...");
            Pause();
        }

        static void Pull()
        {
            // Add logic to pull data from the server here
            Console.WriteLine("Pulling data from the server...");
            Pause();
        }

        static void Push()
        {
            // Add logic to push data to the server here
            Console.WriteLine("Pushing data to the server...");
            Pause();
        }

        static void FullSync()
        {
            // Add logic to perform a full sync here
            Console.WriteLine("Performing full sync...");
            Pause();
        }

        static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static async Task<DbContext> InitSyncFramework(string SyncServerAddress)
        {
            MySqlServerVersion serverVersion;
            serverVersion = new MySqlServerVersion(new Version(8, 0, 31));
            ServiceCollection MauiServiceCollection = new ServiceCollection();
            List<DeltaGeneratorBase> DeltaGenerators = new List<DeltaGeneratorBase>();
            DeltaGenerators.Add(new NpgsqlDeltaGenerator());
            DeltaGenerators.Add(new PomeloMySqlDeltaGenerator(serverVersion));
            DeltaGenerators.Add(new SqliteDeltaGenerator());
            DeltaGenerators.Add(new SqlServerDeltaGenerator());
            DeltaGeneratorBase[] additionalDeltaGenerators = DeltaGenerators.ToArray();




            string dbPathData = "Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=ConsoleEfSyncData";
            string dbPathDelta = "Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=ConsoleEfSyncDelta";

            //Data Source=SyncData.db

            DbContextOptionsBuilder MauiContextOptionBuilder = new DbContextOptionsBuilder();

            MauiContextOptionBuilder.UseSqlServer(dbPathData);


            var baseAddress = new Uri(SyncServerAddress);

            // Initialize the HttpClient with the base address
            HttpClient client = new HttpClient { BaseAddress = baseAddress };

            //you can also use the extension method for specific providers
            MauiServiceCollection.AddSyncFrameworkForSqlServer(dbPathDelta, client, "MemoryDeltaStore1", "Node A", additionalDeltaGenerators);


            YearSequencePrefixStrategy implementationInstance = new YearSequencePrefixStrategy();
            MauiServiceCollection.AddSingleton(typeof(ISequencePrefixStrategy), implementationInstance);
            MauiServiceCollection.AddSingleton(typeof(ISequenceService), typeof(EfSequenceService));

            var MauiServiceProvider = MauiServiceCollection.BuildServiceProvider();
            var DbContext = new TestSyncFrameworkDbContext(MauiContextOptionBuilder.Options, MauiServiceProvider);

            await DbContext.Database.EnsureCreatedAsync();
            return DbContext;
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1) Set Server URL");
                Console.WriteLine("2) Create Blog");
                Console.WriteLine("3) List Blogs");
                Console.WriteLine("4) Pull");
                Console.WriteLine("5) Push");
                Console.WriteLine("6) Full Sync");
                Console.WriteLine("7) Exit");
                Console.Write("Please select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SetServerUrl();
                        break;
                    case "2":
                        CreateBlog();
                        break;
                    case "3":
                        ListBlogs();
                        break;
                    case "4":
                        Pull();
                        break;
                    case "5":
                        Push();
                        break;
                    case "6":
                        FullSync();
                        break;
                    case "7":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
    }
}
