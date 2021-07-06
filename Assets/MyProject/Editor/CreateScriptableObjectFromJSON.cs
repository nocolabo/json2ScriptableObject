using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Dynamic;


public class CreateScriptableObjectFromJSON<T> 
{
	// ScriptableObject���o�͂���f�B���N�g��
	private�@string outPutDir = Path.Combine("Assets", "MyProject");
	// ScriptableObject�ɕt�^���ꂽ���O
	private�@string nameForSO = "_SO";

	public void CreateAssets(string jsonPath, string parentClassName, Type parentType)
	{
		// JSON�p�X����JSON��ǂݍ��݁A�w��̌^�ɍ����C���X�^���X�𐶐�����
		dynamic jsonObj = JsonReaderToCreateInstance(jsonPath);

		//�󂯎���̐e�N���X�����擾
		string SOParentClassName = GetClassNameForSO(parentClassName, nameForSO);

		// ���X�g�����t�B�[���h���A���X�g�̂݃t�B�[���h���A���N���X�̃t�B�[���h������n�����Ɛ旼���擾
		var ( parentFieldNames, parentFieldListNames, childClassNames) = GetFieldName(parentClassName);
		var (soParentFieldNames, soParentFieldListNames, soChildClassNames) = GetFieldName(SOParentClassName);

		// ���X�g�����邩�ǂ�������
		if (parentFieldListNames.Count == 0)
        {
			// �f�[�^�^�݂̂̏ꍇ�̏���

			// �󂯎���̐e�N���X���C���X�^���X��
			dynamic sopObj = UseClassNameCreateInstance(SOParentClassName);
			// �e�N���X�̃t�B�[���h�Ɏw��̃f�[�^��ݒ肷��
			SetFiledsData(sopObj, jsonObj);
            //// �s����id�Ƃ��ăt�@�C�����쐬
            string filename =  parentClassName + ".asset";
			string path = Path.Combine(outPutDir, filename);
			// Scriptable Object�쐬
			CreateScriptableAsset(path, sopObj);
		}
        else
        {
			// �f�[�^�^�ƃ��X�g���������Ă���ꍇ�̏���
			List<dynamic> listOfJsonObj = GetFieldsFromObj(jsonObj, true);

			// �󂯎���̏��N���X�̎w��J�E���^
			int childCnt = 0;
			// �t�@�C���o�͗p�J�E���^
			int fileNameCnt = 0;

			// �󂯎���̐e�N���X���C���X�^���X���@�������łȂ��ƃ_���I
			dynamic sopObj = UseClassNameCreateInstance(SOParentClassName);

			// �e�N���X��List���󂯎���Ɋi�[
			foreach (string listName in parentFieldListNames) // ���X�g��
			{
				// �e�I�u�W�F�N�g���烊�X�g�f�[�^���擾
				var listDatas = listOfJsonObj[childCnt];


				// �e�I�u�W�F�N�g�̃��X�g��n��
				foreach (var data in listDatas) // ���X�g�����̃f�[�^��
				{
					//�󂯎���̎q�N���X�̃C���X�^���X�������X�g�Ɋi�[  �������łȂ��ƃ_���I
					dynamic socObj = UseClassNameCreateInstance(soChildClassNames[childCnt]);

					// �e�N���X�̃t�B�[���h�Ɏw��̃f�[�^��ݒ肷��
					SetFiledsData(sopObj, jsonObj);

					//�q�N���X�̃t�B�[���h�Ɏw��̃f�[�^��ݒ肷��
					SetFiledsData(socObj, data);

					// �e�N���X�̃��X�g�Ƀf�[�^��ǉ�����
					SetListData(sopObj, socObj, listName);

					// ���̃f�[�^�쐬�ׂ̈ɃJ�E���g�A�b�v
					++fileNameCnt;
				}

				// ���̑����̎q�N���X��I������ׂ̃J�E���g�A�b�v
				++childCnt;
			}
			// �s����id�Ƃ��ăt�@�C�����쐬
			string filename = parentClassName  + ".asset";
			string path = Path.Combine(outPutDir, filename);

			// Scriptable Object�쐬
			CreateScriptableAsset(path, sopObj);
		}
	}

	/// <summary>
	/// JSON�̓ǂݍ��݁A�C���X�^���X�𐶐��B
	/// �𐶐�Unity��utf-8�œǂݍ��܂��̂ŁA������������ۑ���utf-8�ɂȂ��Ă��Ȃ��B
	/// </summary>
	/// <param name="jsonPath"></param>
	/// <returns></returns>
	public dynamic JsonReaderToCreateInstance(string jsonPath)
    {
		// �t�@�C���ǂݍ��݃e�L�X�g��
		string json = File.ReadAllText(jsonPath, Encoding.GetEncoding("utf-8"));
		// json����w��̃I�u�W�F�N�g�^�ŃC���X�^���X�𐶐�
		dynamic jsonObj = JsonUtility.FromJson<T>(json);
		return jsonObj;
	}

	/// <summary>
	/// �f�[�^���i�[����ScriptableObject��ۑ��p�X�ɃA�Z�b�g�Ƃ��č쐬�E�X�V
	/// </summary>
	/// <param name="path"></param>
	/// <param name="data"></param>
	public void CreateScriptableAsset(string path, ScriptableObject data)
	{
		// �C���X�^���X���������̂��A�Z�b�g�Ƃ��ĕۑ�
		var asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
		//var asset = (ObjectData_SO)AssetDatabase.LoadAssetAtPath(path, typeof(T));
		if (asset == null)
		{
			Debug.Log("ScriptableObject �V�K�쐬");
			// �w��̃p�X�Ƀt�@�C�������݂��Ȃ��ꍇ�͐V�K�쐬
			AssetDatabase.CreateAsset(data, path);
		}
		else
		{
			Debug.Log("ScriptableObject �X�V");
			// �w��̃p�X�Ɋ��ɓ����̃t�@�C�������݂���ꍇ�͍X�V
			EditorUtility.CopySerialized(data, asset);
			AssetDatabase.SaveAssets();
		}
		AssetDatabase.Refresh();
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

	/// <summary>
	/// �I�u�W�F�N�g�̃t�B�[���h����List�̃W�F�l���X�N�^����q�N���X�����擾��List�Ƃ��ĕԂ�
	/// </summary>
	/// <param name="targetClassName"></param>
	/// <returns></returns>
	public (List<string>, List<string>, List<string>) GetFieldName(string targetClassName)
	{
		// ���X�g�ȊO�̃t�B�[���h
		List<string> parentFieldNames = new List<string>();
		// ���X�g�݂̂̃t�B�[���h
		List<string> parentFieldListNames = new List<string>();
		// ���X�g��Generics�̌^����擾�����N���X��
		List<string> childClassNames = new List<string>();

		Type type = GetTypeByClassName(targetClassName);
		dynamic targetObj = Activator.CreateInstance(type);
		FieldInfo[] fields = targetObj.GetType().GetFields();
		string searchKey = "System.Collections.Generic.List";

		foreach (FieldInfo field in fields)
        {
			// �t�B�[���h�^�C�v�𕶎���
			string fieldTypeStr = field.FieldType.ToString();
			// List������ȊO�Ŕ���
            if (!fieldTypeStr.Contains(searchKey))
            {

				parentFieldNames.Add(field.Name);
			}
            else
            {
				int sindex = fieldTypeStr.IndexOf("[");
				int eindex = fieldTypeStr.IndexOf("]");
				//�t�B�[���h�̌^������W�F�l���N�X�̌^���擾
				string genericsName = fieldTypeStr.Substring(sindex + 1, eindex - sindex - 1);
				childClassNames.Add(genericsName);
				parentFieldListNames.Add(field.Name);
			}
		}
		return (parentFieldNames, parentFieldListNames, childClassNames);

	}

	/// <summary>
	/// ScriptableObject�ŗp����I�u�W�F�N�g�����擾����
	/// </summary>
	/// <param name="className"></param>
	/// <param name="key"></param>
	/// <returns></returns>
	public string GetClassNameForSO(string className, string key)
    {
		string combinName = className + key;
		return combinName;
	}

	/// <summary>
	/// �N���X������C���X�^���X�𐶐����A�Ԃ��B
	/// </summary>
	/// <param name="className"></param>
	/// <returns></returns>
	public dynamic UseClassNameCreateInstance(string className)
    {
		Type type = GetTypeByClassName(className);
		dynamic instance = Activator.CreateInstance(type);
		return instance;
    }

	/// <summary>
	/// �w��N���X��Generics�^�C�v��C�ӂ̃N���X�^�ɐݒ肵�C���X�^���X��Ԃ�
	/// �����̃N���X��JsonImpoterEditor�ɂ����g���Ȃ�
	/// </summary>
	/// <param name="genericTypeName"></param>
	/// <returns></returns>
	public dynamic UseGenericTypeCreateInstance(string genericTypeName)
    {
		Type type = GetTypeByClassName(genericTypeName);
		var genericType = typeof(CreateScriptableObjectFromJSON<>).MakeGenericType(type);
		dynamic obj = Activator.CreateInstance(genericType);
		return obj;
	}

	/// <summary>
	/// �w��̃I�u�W�F�N�g�̃t�B�[���h�Ɏw��̃f�[�^��ݒ肷��
	/// getFields�F�l�擾�ɂ͑Ώۂ̒l���i�[����Ă���N���X����t�B�[���h�����w�肵��������擾
	/// setFields�F�l�ݒ�ɂ͑Ώۂ̃N���X�̃t�B�[���h�����擾���A���̃N���X�Ɛݒ�l���w�肵�ݒ�
	/// </summary>
	/// <param name="receiveObj"> �󂯎���I�u�W�F�N�g</param>
	/// <param name="sendObj"> �f�[�^�����I�u�W�F�N�g</param>
	public void SetFiledsData( dynamic receiveObj, dynamic sendObj)
    {
		// �f�[�^���擾����N���X�̃t�B�[���h
		FieldInfo[] getFields = sendObj.GetType().GetFields();
		// �f�[�^��ݒ肷��N���X�̃t�B�[���h
		FieldInfo[] setFields = receiveObj.GetType().GetFields();
		string searchKey = "System.Collections.Generic.List";
		int fieldCnt = 0;
		// ��n���������n����I�u�W�F�N�g�փf�[�^��ݒ�
		foreach (FieldInfo field in setFields) // infoReceive�͑����
		{

			// �t�B�[���h�^�C�v�𕶎���
			string fieldTypeStr = field.FieldType.ToString();
			string getfieldTypeStr = getFields[fieldCnt].FieldType.ToString();

            if (!fieldTypeStr.Contains(searchKey))
            {
                field.SetValue(receiveObj, getFields[fieldCnt].GetValue(sendObj));
            }
            ++fieldCnt;
		}
	}

	/// <summary>
	/// �󂯎�茳�̃��X�g�ɑΉ������N���X�^�̃f�[�^��ǉ�����
	/// </summary>
	/// <param name="pObj"></param>
	/// <param name="cObj"></param>
	/// <param name="listName"></param>
	public void SetListData(dynamic pObj, dynamic cObj, string listName)
    {
		var field = pObj.GetType().GetField(listName);
		var instance = field.GetValue(pObj);
		instance.Add(cObj);
	}

	/// <summary>
	/// �I�u�W�F�N�g����List���擾��List�Ɋi�[���Ԃ�
	/// isListGet��ture��n����List���Afalse�Ȃ炻��ȊO�̃��X�g���擾
	/// </summary>
	/// <param name="obj"></param>
	public dynamic GetFieldsFromObj(dynamic obj, bool isListGet)
	{
		FieldInfo[] infoFields = obj.GetType().GetFields();
		string searchKey = "System.Collections.Generic.List";
		// ���X�g�����i�[
		List<dynamic> outPut = new List<dynamic>();

        if (infoFields.Length < 1)
        {
			Debug.Log("GetFieldsFromObj�F�t�B�[���h���擾����Ă��܂���");
			return null;
        }

		//�@GetData��
		foreach (FieldInfo field in infoFields)
		{
			// �t�B�[���h�^�C�v�𕶎���
			string fieldTypeStr = field.FieldType.ToString();
			// List������ȊO�Ŕ���
			if (fieldTypeStr.Contains(searchKey))
			{
				if (isListGet)
				{
					outPut.Add(field.GetValue(obj));
				}
			}
			else
			{
				if (!isListGet)
				{
					outPut.Add(field.GetValue(obj));
				}
			}
		}
		return outPut;
	}

}