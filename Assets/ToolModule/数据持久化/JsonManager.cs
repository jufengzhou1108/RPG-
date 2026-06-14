using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// json工具，需根据需求修改json存储位置
/// </summary>
public static class JsonTool
{
    private const string JSON_PATH = @"D:\code\DUnity\丧尸围城\Assets\ToolModule\数据持久化\Json\";

    /// <summary>
    /// 反序列化指定类
    /// </summary>
    /// <typeparam name="T">类型名，例如PlayerData</typeparam>
    /// <param name="name">对象名，Player1</param>
    /// <param name="dicPath">文件夹路径</param>
    /// <returns></returns>
    public static T LoadJson<T>(string name,string dicPath= JSON_PATH) where T : class,new()
    {
        if (!Directory.Exists(dicPath))
        {
            Debug.Log("文件夹不存在"+ dicPath);
            return new T();
        }
        string path = dicPath + typeof(T).Name +"_"+ name + ".json";
        if(!File.Exists(path))
        {
            Debug.Log("文件不存在"+path);
            return new T();
        }
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
    }

    /// <summary>
    /// 序列化指定的类
    /// </summary>
    /// <typeparam name="T">类型名</typeparam>
    /// <param name="data">序列化对象</param>
    /// <param name="name">对象名</param>
    /// <param name="dicPath">文件夹路径</param>
    public static void SaveData<T>(T data,string name, string dicPath = JSON_PATH) where T : class
    {
        if (!Directory.Exists(dicPath))
        {
            Debug.Log("文件夹不存在"+ dicPath);
        }

        string path = dicPath + typeof(T).Name +"_"+ name + ".json";
        string content=JsonConvert.SerializeObject(data);
        File.WriteAllText(path, content);
    }
}
