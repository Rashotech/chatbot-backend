// Generated with ChatBot .NET Template version v4.22.0

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatBot
{
     public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            DotNetEnv.Env.Load();
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging((logging) =>
                    {
                        logging.AddDebug();
                        logging.AddConsole();
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
