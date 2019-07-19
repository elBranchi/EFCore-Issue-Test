using EFC.Issues.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EFC.Issues.Test
{
    public enum DBProvider
    {
        InMemory,
        SqlServer
    }
    public abstract class EFCBaseTest
    {
        //Modify connection string to match target Database
        protected static string SqlServerConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=db_efc_issues;Integrated Security=True";


        protected static InMemoryDatabaseRoot inMemoryDbRoot = new InMemoryDatabaseRoot();

        protected static EFCContext GetEFCContext(DBProvider dBProvider, string connectionString = null,
            [CallerFilePath]string callingFile = null)
        {

            var optionsBuilder = new DbContextOptionsBuilder<EFCContext>();

            var services = new ServiceCollection();

            services.AddLogging(
                loggingBuilder =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddConsole();
                });
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseLoggerFactory(loggerFactory);


            switch (dBProvider)
            {
                case DBProvider.InMemory:
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(callingFile);
                    var dbName = $"db_{fileName}";
                    optionsBuilder.UseInMemoryDatabase(dbName, inMemoryDbRoot);
                    break;

                case DBProvider.SqlServer:
                    optionsBuilder.UseSqlServer(connectionString);
                    break;
            }

            var context = new EFCContext(optionsBuilder.Options);


            return context;
        }

    }
}
