using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESDemo.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Nest;
using Swashbuckle.AspNetCore.Swagger;

namespace ESDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        private const string _Project_Name = "ESDemo";
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //var settings = new ConnectionSettings(new Uri("http://10.10.10.73:9200/"))                
            //    .DefaultIndex("testusers");

            //var client = new ElasticClient(settings);
            //services.AddControllers();

            //注册Swagger生成器，定义一个和多个Swagger 文档
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton(new ApiTokenConfig("A3FFB16D-D2C0-4F25-BACE-1B9E5AB614A6"));

            services.AddScoped<IApiTokenService, ApiTokenService>();

            services.AddSwaggerGen(c =>
            {
                typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
                {
                    c.SwaggerDoc(version, new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Version = version,
                        Title = $"{_Project_Name} 接口文档",
                        Description = $"{_Project_Name} HTTP API " + version,
                        TermsOfService = "None"
                    });
                });
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = System.IO.Path.Combine(basePath, $"{_Project_Name}.xml");
                c.IncludeXmlComments(xmlPath);
                //添加自定义参数，可通过一些特性标记去判断是否添加
                c.OperationFilter<AssignOperationVendorExtensions>();
                //添加对控制器的标签(描述)
                c.DocumentFilter<ApplyTagDescriptions>();
            });
            services.AddMvc();
            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    typeof(ApiVersions).GetEnumNames().OrderByDescending(e => e).ToList().ForEach(version =>
                    {
                        c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{_Project_Name} {version}");
                    });
                    //注入汉化文件
                    c.InjectJavascript($"/swagger_translator.js");
                });
            }
            ServiceLocator.Configure(app.ApplicationServices);
            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}
