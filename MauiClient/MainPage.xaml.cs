﻿using BIT.Data.Sync;
using BIT.Data.Sync.EfCore.Npgsql;
using BIT.Data.Sync.EfCore.Pomelo.MySql;
using BIT.Data.Sync.EfCore.SQLite;
using BIT.Data.Sync.EfCore.SqlServer;
using BIT.Data.Sync.Imp;
using BIT.EfCore.Sync;

using Microsoft.EntityFrameworkCore;
using Model.Contexts;
using System.Collections.ObjectModel;

namespace MauiClient
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
            Items=new ObservableCollection<Blog>();
            this.BlogsListView.ItemsSource = Items;
        }

        public ObservableCollection<Blog> Items { get; set; }
        private async void OnPush(object sender, EventArgs e)
        {
            var Context = await InitSyncFramework(this.ServerUrl.Text) as TestSyncFrameworkDbContext;
            await Context.PushAsync();
        }
        private async void OnPull(object sender, EventArgs e)
        {
            var Context = await InitSyncFramework(this.ServerUrl.Text) as TestSyncFrameworkDbContext;
         
            await Context.PullAsync();

            Context.Blogs.ToList().ForEach(x => Items.Add(x));
        }
        private async void Save(object sender, EventArgs e)
        {
            var Context = await InitSyncFramework(this.ServerUrl.Text);
            Blog blog = new Blog
            {
                Name = this.BlogName.Text,
            };
            Context.Add(blog);
            await Context.SaveChangesAsync();
            Items.Add(blog);
            this.BlogName.Text = "";
           
        }
       
        private async void OnInitSyncFrameworkBtn(object sender, EventArgs e)
        {
          var Context=  await InitSyncFramework(this.ServerUrl.Text);
          this.ServerUrlLabel.Text = this.ServerUrl.Text;

            await DisplayAlert("SyncFramework", "Initialized", "OK");
            this.ServerUrl.Text = "";

        }
        async Task<DbContext> InitSyncFramework(string SyncServerAddress)
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




            string dbPathData = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "Data.db3")}";
            string dbPathDelta = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "Delta.db3")}";

            //Data Source=SyncData.db

            DbContextOptionsBuilder MauiContextOptionBuilder = new DbContextOptionsBuilder();

            MauiContextOptionBuilder.UseSqlite(dbPathData);


            var baseAddress = new Uri(SyncServerAddress);

            // Initialize the HttpClient with the base address
            HttpClient client = new HttpClient { BaseAddress = baseAddress };

            //you can also use the extension method for specific providers
            MauiServiceCollection.AddSyncFrameworkForSQLite(dbPathDelta, client, "MemoryDeltaStore1", "Maui", additionalDeltaGenerators);


            YearSequencePrefixStrategy implementationInstance = new YearSequencePrefixStrategy();
            MauiServiceCollection.AddSingleton(typeof(ISequencePrefixStrategy), implementationInstance);
            MauiServiceCollection.AddSingleton(typeof(ISequenceService), typeof(EfSequenceService));

            var MauiServiceProvider = MauiServiceCollection.BuildServiceProvider();
            var DbContext = new TestSyncFrameworkDbContext(MauiContextOptionBuilder.Options, MauiServiceProvider);

            await DbContext.Database.EnsureCreatedAsync();
            return DbContext;
        }
    }

}
