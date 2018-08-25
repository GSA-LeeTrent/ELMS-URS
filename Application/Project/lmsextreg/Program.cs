using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using lmsextreg.Data;
using lmsextreg.Constants;

namespace lmsextreg
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var services = scope.ServiceProvider;
                    //var context = services.GetRequiredService<ApplicationDbContext>();
                    //context.Database.Migrate();

                    var config = host.Services.GetRequiredService<IConfiguration>();
                    var tempPW = config[MiscConstants.SEED_TEMP_PW];

                    Console.WriteLine("[Program] tempPW: " +  tempPW);
                    DataSeed.Initialize(services, tempPW).Wait();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.StackTrace);
                }
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}