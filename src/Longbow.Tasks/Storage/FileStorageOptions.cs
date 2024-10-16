// Copyright (c) Argo Zhang (argo@live.ca). All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Longbow.Tasks;

/// <summary>
/// 物理文件持久化配置类
/// </summary>
public class FileStorageOptions
{
    /// <summary>
    /// 获得/设置 是否启用物理文件持久化 默认 true 启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 获得/设置 物理文件持久化文件目录名称 默认 TaskStorage
    /// </summary>
    /// <remarks>请使用相对路径 内部程序会根据当前程序运行所在目录进行拼接</remarks>
    public string Folder { get; set; } = "TaskStorage";

    /// <summary>
    /// 获得/设置 是否对持久化文件加密 默认为 true
    /// </summary>
    public bool Secure { get; set; } = true;

    /// <summary>
    /// 获得/设置 加密解密密钥
    /// </summary>
    public string Key { get; set; } = "LIBSFjql+0qPjAjBaQYQ9Ka2oWkzR1j6";

    /// <summary>
    /// 获得/设置 加密解密向量值
    /// </summary>
    public string IV { get; set; } = "rNWuCRQAWjI=";

    /// <summary>
    /// 获得/设置 任务被移除时是否删除持久化文件
    /// </summary>
    public bool DeleteFileByRemoveEvent { get; set; } = true;
}

/// <summary>
/// 缓存配置类
/// </summary>
/// <typeparam name="TOptions"></typeparam>
/// <remarks>
/// 构造函数
/// </remarks>
/// <param name="configuration"></param>
internal class FileStorageOptionsConfigureOptions<TOptions>(IConfiguration configuration) : ConfigureFromConfigurationOptions<TOptions>(configuration.GetSection("FileStorageOptions")) where TOptions : class
{
}
