using UnityEngine;
using UnityEditor; // �ǉ�
using System.Collections;
using System.IO;
using System;
using System.Reflection;

/// <summary>
/// ���j���[�ɔC�ӂ̏����{�^����ݒu����
/// </summary>
/// 


public class MyMenuItems
{
    // JSON�f�B���N�g��
    static private string dirPath = Path.Combine(Application.dataPath, "MyProject", "JSON");

    /// <summary>
    /// �w��f�B���N�g���S�Ă�JSON����ScriptabelAsset���쐬�E�X�V����
    /// </summary>
    [MenuItem("MyMenuItems/CreateAllScriptableAssets %h")]
    private static void CreateAllScriptableAssets()
    {
        // JSON�f�B���N�g������Json�t�@�C���p�X���擾
        string[] jsonFilePaths = Directory.GetFiles(dirPath, "*.json");

        // JSON�p�X����CreateScriptable�̃A�Z�b�g���쐬����
        foreach (string path in jsonFilePaths)
        {
            // JSON�ƃN���X�̃t�@�C�����͓���Ƃ��A�t�@�C��������擾
            string parentClassName = Path.GetFileNameWithoutExtension(path);

            Type type = GetTypeByClassName(parentClassName);
            var genericType = typeof(CreateScriptableObjectFromJSON<>).MakeGenericType(type);
            dynamic obj = Activator.CreateInstance(genericType);
            obj.CreateAssets(path, parentClassName, type);
        }
    }

    /// <summary>
    /// �N���X������^���擾����
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    public static Type GetTypeByClassName(string className)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                // type.Name���Ɗ����̃^�C�v�Ƃ��Ԃ�ꍇ����ׁA���S��v�Ŕ���
                if (type.ToString() == className)
                {
                    return type;
                }
            }
        }
        return null;
    }
}