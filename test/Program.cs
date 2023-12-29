// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using Microsoft.AspNetCore.Builder;

namespace Longbow.Tasks.Test;

public class Program
{
    public void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();
        app.Run();
    }

    //public Program()
    //{
    //    Configuration = configuration;
    //    Configuration["TaskServicesOptions:FileStorageOptions:Enabled"] = "true";
    //}

    //public IConfiguration Configuration { get; }

    //// This method gets called by the runtime. Use this method to add services to the container.
    //public void ConfigureServices(IServiceCollection services)
    //{
    //    services.AddLogging(builder => builder.AddFileLogger());
    //    services.AddTaskServices(builder => builder.AddFileStorage<FileStorage>(op => op.DeleteFileByRemoveEvent = false));
    //    services.AddControllers();
    //    services.AddRouting();
    //}

    //// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    //public void Configure(IApplicationBuilder app)
    //{
    //    app.UseRouting();
    //    app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
    //}
}
