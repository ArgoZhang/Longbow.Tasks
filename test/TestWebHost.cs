#if NETCOREAPP3_1
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.IO;

namespace Longbow.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    public class TestWebHost<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IWebHostBuilder CreateWebHostBuilder() => WebHost.CreateDefaultBuilder<TStartup>(null);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            var dirSeparator = Path.DirectorySeparatorChar;
            var root = Path.Combine(AppContext.BaseDirectory, $"..{dirSeparator}..{dirSeparator}..{dirSeparator}");
            builder.UseContentRoot(root);
        }
    }
}
#endif
