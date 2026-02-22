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
using System;
using System.Threading;
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
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 500L * 1024L * 1024L; // 500 MB
            });

     
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
                        ; // Required for SignalR
                    });
                });





            }
            else
            {



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
                app.UseDeveloperExceptionPage();
            app.UseRouting();
            if (env.IsDevelopment())
            {

                app.UseCors("AllowAll");
            }
            else
            {

                // Apply the CORS policy BEFORE routing
                app.UseCors("AllowSpecificOrigins");

            }



            app.UseAuthorization();

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(20),
            });
            app.Map("/ws/tables", wsApp =>
            {
                wsApp.Run(async context =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = 400;
                        return;
                    }

                    var ws = await context.WebSockets.AcceptWebSocketAsync();
                    var hello = System.Text.Encoding.UTF8.GetBytes("{\"type\":\"hello\"}");
                    await ws.SendAsync(new ArraySegment<byte>(hello),
                        System.Net.WebSockets.WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                    var id = Guid.NewGuid().ToString();
                    TablesWsManager.Add(id, ws);

                    var buffer = new byte[4096];
                    try
                    {
                        while (ws.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                                break;
                            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                            {
                                var msg = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                                Console.WriteLine("WS msg: " + msg);

                                try
                                {
                                    using var doc = System.Text.Json.JsonDocument.Parse(msg);
                                    var root = doc.RootElement;

                                    var type = root.TryGetProperty("type", out var t) ? t.GetString() : "";

                                    if (type == "subscribe")
                                    {
                                        int branchId = root.TryGetProperty("branchId", out var b) ? b.GetInt32() : 0;
                                        TablesWsManager.SetBranch(id, branchId);

                                        // Ack اختياري
                                        var ack = System.Text.Encoding.UTF8.GetBytes($"{{\"type\":\"subscribed\",\"branchId\":{branchId}}}");
                                        await ws.SendAsync(new ArraySegment<byte>(ack),
                                            System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    finally
                    {
                        await TablesWsManager.Remove(id);
                    }
                });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

            
            });
        }
    }
}