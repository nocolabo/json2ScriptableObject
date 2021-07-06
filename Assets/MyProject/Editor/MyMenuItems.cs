using UnityEngine;
using UnityEditor; // 追加
using System.Collections;
using System.IO;
using System;
using System.Reflection;

/// <summary>
/// メニューに任意の処理ボタンを設置する
/// </summary>
/// 


public class MyMenuItems
{
    // JSONディレクトリ
    static private string dirPath = Path.Combine(Application.dataPath, "MyProject", "JSON");

    /// <summary>
    /// 指定ディレクトリ全てのJSONからScriptabelAssetを作成・更新する
    /// </summary>
    [MenuItem("MyMenuItems/CreateAllScriptableAssets %h")]
    private static void CreateAllScriptableAssets()
    {
        // JSONディレクトリ内のJsonファイルパスを取得
        string[] jsonFilePaths = Directory.GetFiles(dirPath, "*.json");

        // JSONパスからCreateScriptableのアセットを作成する
        foreach (string path in jsonFilePaths)
        {
            // JSONとクラスのファイル名は同一とし、ファイル名から取得
            string parentClassName = Path.GetFileNameWithoutExtension(path);

            Type type = GetTypeByClassName(parentClassName);
            var genericType = typeof(CreateScriptableObjectFromJSON<>).MakeGenericType(type);
            dynamic obj = Activator.CreateInstance(genericType);
            obj.CreateAssets(path, parentClassName, type);
        }
    }

    /// <summary>
    /// クラス名から型を取得する
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    public static Type GetTypeByClassName(string className)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                // type.Nameだと既存のタイプとかぶる場合がる為、完全一致で判定
                if (type.ToString() == className)
                {
                    return type;
                }
            }
        }
        return null;
    }
}