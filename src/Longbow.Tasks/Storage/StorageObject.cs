// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System.Collections.Generic;

namespace Longbow.Tasks;

/// <summary>
/// 序列化对象实体类
/// </summary>
internal class StorageObject
{
    /// <summary>
    /// 获得/设置 ITrigger 实例类型
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 获得/设置 ITrigger 序列化属性集合
    /// </summary>
    public Dictionary<string, object> KeyValues { get; set; } = new Dictionary<string, object>();
}
