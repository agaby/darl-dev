using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using GraphQL;
using GraphQL.Authorization.AspNetCore;
using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Darl.GraphQL
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {

            Environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
                .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));


            services.AddMvc().AddNewtonsoftJson();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"));
            });


            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetSection("AppSettings")["RedisConnection"];
                option.InstanceName = "darlai";
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.HttpOnly = true;
            });

            services.AddGraphQLAuth();


            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            //services
            services.AddSingleton<IConnectivity, CosmosDBConnectivity>();
            services.AddSingleton<IFormApi, FormApi>();

            //types
            services.AddTransient<BotFormatType>();
            services.AddTransient<BotInputFormatType>();
            services.AddTransient<BotModelType>();
            services.AddTransient<BotOutputFormatType>();
            services.AddTransient<DictionarySequenceType>();
            services.AddTransient<DisplayTypeEnum>();
            services.AddTransient<FormFormatType>();
            services.AddTransient<InputFormatType>();
            services.AddTransient<InputTypeEnum>();
            services.AddTransient<LanguageFormatType>();
            services.AddTransient<LanguageTextType>();
            services.AddTransient<LineageAnnotationNodeType>();
            services.AddTransient<LineageElementType>();
            services.AddTransient<LineageMatchNodePairType>();
            services.AddTransient<LineageMatchNodeType>();
            services.AddTransient<LineageMatchTreeType>();
            services.AddTransient<LineageModelType>();
            services.AddTransient<LineageTypeEnum>();
            services.AddTransient<MLModelType>();
            services.AddTransient<MLSpecType>();
            services.AddTransient<OutputFormatType>();
            services.AddTransient<OutputTypeEnum>();
            services.AddTransient<PostTypeEnum>();
            services.AddTransient<RuleFormType>();
            services.AddTransient<RuleSetType>();
            services.AddTransient<SourceTypeEnum>();
            services.AddTransient<StringDoublePairType>();
            services.AddTransient<StringStringPairType>();
            services.AddTransient<TriggerViewType>();
            services.AddTransient<VariantTextType>();
            services.AddTransient<SetDefinitionType>();
            services.AddTransient<ServiceConnectivityType>();
            services.AddTransient<AzureCredentialsType>();
            services.AddTransient<SellerCenterCredentialsType>();
            services.AddTransient<ZendeskCredentialsType>();
            services.AddTransient<SendGridCredentialsType>();
            services.AddTransient<TwilioCredentialsType>();
            services.AddTransient<BotConnectionType>();
            services.AddTransient<UserUsageType>();
            services.AddTransient<ContactType>();
            services.AddTransient<DefaultType>();
            services.AddTransient<ContactInputType>();
            services.AddTransient<ContactUpdateType>();
            services.AddTransient<InputFormatUpdateType>();
            services.AddTransient<OutputFormatUpdateType>();
            services.AddTransient<BotOutputFormatUpdateType>();
            services.AddTransient<LineageNodeDefinitionType>();
            services.AddTransient<LineageRecordType>();
            services.AddTransient<LineageNodeAttributeUpdateType>();
            services.AddTransient<LineageNodeAttributeType>();
            services.AddTransient<DarlVarType>();
            services.AddTransient<DarlVarDataTypeEnum>();
            services.AddTransient<StringStringPairInputType>();
            services.AddTransient<DarlVarInputType>();
            services.AddTransient<MLResultType>();
            services.AddTransient<AuthorizationType>();
            services.AddTransient<LineageAssociationType>();
            services.AddTransient<LineageElementUnionType>();
            services.AddTransient<AuthorizationUpdateType>();
            services.AddTransient<DarlUserType>();
            services.AddTransient<AccountStateEnum>();
            services.AddTransient<DarlUserInputType>();
            services.AddTransient<DarlUserUpdateType>();
            services.AddTransient<MLSpecUpdateType>();
            services.AddTransient<SetGraphType>();
            services.AddTransient<PercentGraphType>();
            services.AddTransient<QuestionSetType>();
            services.AddTransient<QuestionType>();
            services.AddTransient<QuestionTypeEnum>();
            services.AddTransient<ResponseType>();
            services.AddTransient<ResponseTypeEnum>();
            services.AddTransient<QuestionSetInputType>();
            services.AddTransient<QuestionInputType>();
            services.AddTransient<DarlLintErrorType>();


            //root
            services.AddTransient<DarlSchema>();
            services.AddTransient<DarlMutation>();
            services.AddTransient<DarlSubscription>();
            services.AddTransient<DarlQuery>();


            services.AddSingleton<IDependencyResolver>(
                c => new FuncDependencyResolver(type => c.GetRequiredService(type)));
            services.AddGraphQL(options => {
                options.EnableMetrics = true;
                options.ExposeExceptions = Environment.IsDevelopment();
            })
            .AddWebSockets()
            .AddDataLoader()
            .AddUserContextBuilder(context => new GraphQLUserContext { User = context.User });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseWebSockets();
            app.UseGraphQLWebSockets<DarlSchema>("/graphql");
            app.UseGraphQL<DarlSchema>("/graphql");
            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions()
            {
                Path = "/ui/playground"
            });
            app.UseGraphiQLServer(new GraphiQLOptions
            {
                GraphiQLPath = "/ui/graphiql",
                GraphQLEndPoint = "/graphql"
            });
            app.UseGraphQLVoyager(new GraphQLVoyagerOptions()
            { 
                GraphQLEndPoint = "/graphql",
                Path = "/ui/voyager"
            });
        }
    }
}
