// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
#if NETSTANDARD2_0
using Newtonsoft.Json;
#else
using System.Text.Json;
#endif

namespace Longbow.Tasks;

/// <summary>
/// 二进制序列化操作类
/// </summary>
internal static class JsonSerializeExtensions
{
#if !NETSTANDARD2_0
    private static readonly Lazy<JsonSerializerOptions> _option = new(() => new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
#endif

    /// <summary>
    /// 通过指定文件得到反序列化对象实例
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public static ITrigger? Deserialize(string fileName, FileStorageOptions option)
    {
        ITrigger? ret = null;
        var data = File.ReadAllText(fileName);
        if (option.Secure)
        {
            data = data.Decrypte(option);
        }
#if NETSTANDARD2_0
        var obj = JsonConvert.DeserializeObject<StorageObject>(data);
#else
        var obj = JsonSerializer.Deserialize<StorageObject>(data, _option.Value);
#endif
        if (obj != null && !string.IsNullOrEmpty(obj.Type))
        {
            var triggerType = Type.GetType(obj.Type);
            if (triggerType != null)
            {
                ret = Activator.CreateInstance(triggerType) as ITrigger;
                if (ret != null)
                {
                    ret.LoadData(obj.KeyValues);
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// 将指定对象实例序列化到指定文件中
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="fileName"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public static void Serialize(this ITrigger trigger, string fileName, FileStorageOptions option)
    {
        var obj = new StorageObject()
        {
            Type = trigger.GetType().FullName,
            KeyValues = trigger.SetData()
        };
        var folder = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder!);
        }
#if NETSTANDARD2_0
        var data = JsonConvert.SerializeObject(obj);
#else
        var data = JsonSerializer.Serialize(obj);
#endif
        if (option.Secure)
        {
            data = data.Encrypte(option);
        }

        File.WriteAllText(fileName, data);
    }

    private static string Encrypte(this string data, FileStorageOptions option)
    {
        using var des = Create(option.Key, option.IV);
        var encryptor = des.CreateEncryptor();

        var buffer = Encoding.UTF8.GetBytes(data);
        var result = encryptor.TransformFinalBlock(buffer, 0, buffer.Length);
        return Convert.ToBase64String(result);
    }

    private static string Decrypte(this string data, FileStorageOptions option)
    {
        using var des = Create(option.Key, option.IV);
        var decryptor = des.CreateDecryptor();
        var buffer = Convert.FromBase64String(data);
        var result = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(result);
    }

    private static TripleDES Create(string key, string iv)
    {
        var des = TripleDES.Create();
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.PKCS7;
        des.IV = Convert.FromBase64String(iv);
        des.Key = Convert.FromBase64String(key);
        return des;
    }
}
