#if !NET45
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Longbow.Tasks
{
    /// <summary>
    /// 物理文件持久化扩展操作类
    /// </summary>
    public static class FileStorageExtensions
    {
        /// <summary>
        /// 注入物理文件持久化服务到容器内方法
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ITaskStorageBuilder AddFileStorage(this ITaskStorageBuilder builder, Action<FileStorageOptions>? configure = null)
        {
            builder.Services.AddSingleton<IStorage, FileStorage>();
            builder.Services.AddSingleton<IOptionsChangeTokenSource<FileStorageOptions>, ConfigurationChangeTokenSource<FileStorageOptions>>();
            builder.Services.AddSingleton<IConfigureOptions<FileStorageOptions>, FileStorageOptionsConfigureOptions<FileStorageOptions>>();
            if (configure != null) builder.Services.Configure(configure);
            return builder;
        }
    }
}
#endif
