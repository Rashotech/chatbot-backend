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
using DotNetEnv;
using ChatBot.Database.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Azure.Blobs;

namespace ChatBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
            });

            services.AddDbContext<BankDbContext>(options => options
            .UseSqlServer(Env.GetString("DB_URL"), providerOptions => providerOptions.EnableRetryOnFailure())
               .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            );

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Application Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IComplaintService, ComplaintService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IFeedbackService, FeedbackService>();

            // Application Repositories
            services.AddScoped<IComplaintRepository, ComplaintRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();

            services.AddHttpClient("Paystack", client =>
            {
                client.BaseAddress = new Uri(Env.GetString("PAYSTACK_BASE_URL"));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Env.GetString("PAYSTACK_SECRET")}");
            });

            services.AddHttpClient("SendChamp", client =>
            {
                client.BaseAddress = new Uri(Env.GetString("SENDCHAMP_BASE_URL"));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Env.GetString("SENDCHAMP_SECRET")}");
            });

            services.AddScoped<IPaymentProvider, PaystackPaymentProvider>();
            services.AddScoped<INotificationProvider, SendChampNotificationProvider>();

            // Create the Bot Framework Authentication to be used with the Bot Adapter.
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Create the Bot Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<IStorage>(
                new BlobsStorage(
                Env.GetString("BLOB_CONNECTION_STRING"),
                Env.GetString("BLOB_CONTAINER_NAME")
            ));

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Register LUIS recognizer
            services.AddSingleton<BankOperationRecognizer>();

            // Register the Dialogs.
            services.AddScoped<OpenAccountDialog>();
            services.AddScoped<AuthDialog>();
            services.AddScoped<FundTransferDialog>();
            services.AddScoped<CheckAccountBalanceDialog>();
            services.AddScoped<ManageComplaintDialog>();
            services.AddScoped<LogComplaintDialog>();
            services.AddScoped<TrackComplaintDialog>();
            services.AddScoped<TransactionHistoryDialog>();
            services.AddScoped<FeedbackDialog>();
            services.AddScoped<QnADialog>();
            services.AddScoped<MessagePrompts>();
            services.AddScoped<Feedback>();
            services.AddScoped<MainDialog>();
            
            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
            ComponentRegistration.Add(new DialogsComponentRegistration());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

                app
                .UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod())
                .UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
