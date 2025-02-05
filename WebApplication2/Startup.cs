//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.OpenApi.Models;

//namespace WebApplication2
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }
//        string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {

//            services.AddCors(options =>
//            {
//                options.AddPolicy(name: MyAllowSpecificOrigins,
//                                  builder =>
//                                  {
//                                      //builder.AllowAnyMethod();
//                                      //builder.AllowAnyOrigin();
//                                      //builder.SetIsOriginAllowed(origin => true); // allow any origin
//                                      //builder.AllowAnyMethod();
//                                      //builder.WithOrigins("http://example.com", "http://www.mtsofts.com", "http://mtsofts.com", "http://mtsofts.com/#/",
//                                      //            "http://www.contoso.com", "http://localhost:58159/#/", "http://localhost:58159", "http://localhost/", "http://localhost:58159/", "*")

//                                      builder.AllowAnyOrigin().AllowAnyHeader()

//                            .AllowAnyMethod();
//                                  });
//            });
//            services.AddControllers();
//            services.AddSwaggerGen(c =>
//            {
//                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication2", Version = "v1" });
//            });
//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//        {


//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//                app.UseSwagger();
//                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication2 v1"));
//            }

//            app.UseHttpsRedirection();

//            app.UseRouting();
//            app.UseCors(MyAllowSpecificOrigins);






//            app.UseAuthorization();

//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllers();
//            });
//        }
//    }
//}
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication2.cls;

namespace WebApplication2
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env; // Declare _env

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;

        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddControllers();

            // Add SignalR
            services.AddSignalR();
            if (!_env.IsDevelopment())
            {
                //Add CORS policy
                services.AddCors(options =>
                {
                    options.AddPolicy("AllowSpecificOrigins", builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:54946", // Local testing
                            "http://127.0.0.1:54946", // Optional local variation

                             "http://www.mtsofts.com",
                               "https://api.mtsofts.com",      // Production domain
                "https://www.mtsofts.com",      // Production domain
                "http://localhost:54946",       // Local testing
                "http://127.0.0.1:54946"        // Optional local variation
                        )                //.SetIsOriginAllowed(origin => origin.StartsWith("http://localhost") || origin.StartsWith("https://localhost")) // Allow all localhost variations
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // Required for SignalR
                    });
                });





            }
            else { 
            
            
         
                services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
            }
            services.AddScoped<TableService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("AllowAll");
            }
            else { 
            
            
            
            }
           

            // Apply the CORS policy BEFORE routing
            app.UseCors("AllowSpecificOrigins");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<TableHub>("/tableHub"); // SignalR endpoint
            });
        }
    }
}
