// Generated with ChatBot .NET Template version v4.22.0

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ChatBot.Database;
using ChatBot.Repositories.Interfaces;
using ChatBot.Repositories;
using ChatBot.Services;
using ChatBot.Services.Interfaces;
using ChatBot.Dialogs;
using ChatBot.Bots;

namespace ChatBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            services.AddDbContext<BankDbContext>(options => options
            .UseSqlServer(Configuration.GetConnectionString("default"))
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Application Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IComplaintService, ComplaintService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IComplaintRepository, ComplaintRepository>();

            services.AddHttpClient("Paystack", client =>
            {
                client.BaseAddress = new Uri(Configuration["Paystack:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Configuration["Paystack:SecretKey"]}");
            });

            services.AddScoped<IPaymentProvider, PaystackPaymentProvider>();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                                  policy =>
                                  {
                                      policy
                                      .AllowAnyOrigin()
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
            });
            });

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Register LUIS recognizer
            services.AddSingleton<BankOperationRecognizer>();

            // Register the BookingDialog.
            services.AddScoped<OpenAccounDialog>();
            services.AddScoped<AuthDialog>();
            services.AddScoped<CheckAccountBalanceDialog>();
            services.AddScoped<ManageComplaintDialog>();
            services.AddScoped<LogComplaintDialog>();
            services.AddScoped<TrackComplaintDialog>();

            services.AddScoped<MainDialog>();
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.

            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseCors(MyAllowSpecificOrigins)
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
