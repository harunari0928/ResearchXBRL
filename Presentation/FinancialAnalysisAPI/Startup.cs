using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Application.Interactors.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Domain.FinancialAnalysis.AnalysisMenus.CorporationMenus;
using ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus.AccountItemMenus;
using ResearchXBRL.Infrastructure.FinancialAnalysis.AnalysisMenus.CorporationMenus;

namespace FinancialAnalysisAPI
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
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinancialAnalysisAPI", Version = "v1" });
            });
            services.AddTransient<ISuggestAccountItemsUsecase, SuggestAccountItemsInteractor>();
            services.AddTransient<ISuggestCorporationsUsecase, SuggestCorporationsInteractor>();
            services.AddTransient<IAccountItemMenuRepository, AccountItemMenuRepository>();
            services.AddTransient<ICorporationMenuRepository, CorporationMenuRepository>();

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.yaml", "FinancialAnalysisAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }
    }
}
