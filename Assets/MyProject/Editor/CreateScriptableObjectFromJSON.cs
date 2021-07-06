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
	// ScriptableObjectを出力するディレクトリ
	private　string outPutDir = Path.Combine("Assets", "MyProject");
	// ScriptableObjectに付与された名前
	private　string nameForSO = "_SO";

	public void CreateAssets(string jsonPath, string parentClassName, Type parentType)
	{
		// JSONパスからJSONを読み込み、指定の型に合うインスタンスを生成する
		dynamic jsonObj = JsonReaderToCreateInstance(jsonPath);

		//受け取り先の親クラス名を取得
		string SOParentClassName = GetClassNameForSO(parentClassName, nameForSO);

		// リスト除くフィールド名、リストのみフィールド名、小クラスのフィールド名を受渡し元と先両方取得
		var ( parentFieldNames, parentFieldListNames, childClassNames) = GetFieldName(parentClassName);
		var (soParentFieldNames, soParentFieldListNames, soChildClassNames) = GetFieldName(SOParentClassName);

		// リストがあるかどうか判定
		if (parentFieldListNames.Count == 0)
        {
			// データ型のみの場合の処理

			// 受け取り先の親クラスをインスタンス化
			dynamic sopObj = UseClassNameCreateInstance(SOParentClassName);
			// 親クラスのフィールドに指定のデータを設定する
			SetFiledsData(sopObj, jsonObj);
            //// 行数をidとしてファイル名作成
            string filename =  parentClassName + ".asset";
			string path = Path.Combine(outPutDir, filename);
			// Scriptable Object作成
			CreateScriptableAsset(path, sopObj);
		}
        else
        {
			// データ型とリストが混ざっている場合の処理
			List<dynamic> listOfJsonObj = GetFieldsFromObj(jsonObj, true);

			// 受け取り先の小クラスの指定カウンタ
			int childCnt = 0;
			// ファイル出力用カウンタ
			int fileNameCnt = 0;

			// 受け取り先の親クラスをインスタンス化　※ここでないとダメ！
			dynamic sopObj = UseClassNameCreateInstance(SOParentClassName);

			// 親クラスのListを受け取り先に格納
			foreach (string listName in parentFieldListNames) // リスト毎
			{
				// 親オブジェクトからリストデータを取得
				var listDatas = listOfJsonObj[childCnt];


				// 親オブジェクトのリストを渡す
				foreach (var data in listDatas) // リスト無いのデータ毎
				{
					//受け取り先の子クラスのインスタンス化しリストに格納  ※ここでないとダメ！
					dynamic socObj = UseClassNameCreateInstance(soChildClassNames[childCnt]);

					// 親クラスのフィールドに指定のデータを設定する
					SetFiledsData(sopObj, jsonObj);

					//子クラスのフィールドに指定のデータを設定する
					SetFiledsData(socObj, data);

					// 親クラスのリストにデータを追加する
					SetListData(sopObj, socObj, listName);

					// 次のデータ作成の為にカウントアップ
					++fileNameCnt;
				}

				// 次の送り先の子クラスを選択する為のカウントアップ
				++childCnt;
			}
			// 行数をidとしてファイル名作成
			string filename = parentClassName  + ".asset";
			string path = Path.Combine(outPutDir, filename);

			// Scriptable Object作成
			CreateScriptableAsset(path, sopObj);
		}
	}

	/// <summary>
	/// JSONの読み込み、インスタンスを生成。
	/// を生成Unityはutf-8で読み込まれるので、文字化けたら保存がutf-8になっていない。
	/// </summary>
	/// <param name="jsonPath"></param>
	/// <returns></returns>
	public dynamic JsonReaderToCreateInstance(string jsonPath)
    {
		// ファイル読み込みテキスト化
		string json = File.ReadAllText(jsonPath, Encoding.GetEncoding("utf-8"));
		// jsonから指定のオブジェクト型でインスタンスを生成
		dynamic jsonObj = JsonUtility.FromJson<T>(json);
		return jsonObj;
	}

	/// <summary>
	/// データを格納したScriptableObjectを保存パスにアセットとして作成・更新
	/// </summary>
	/// <param name="path"></param>
	/// <param name="data"></param>
	public void CreateScriptableAsset(string path, ScriptableObject data)
	{
		// インスタンス化したものをアセットとして保存
		var asset = AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject));
		//var asset = (ObjectData_SO)AssetDatabase.LoadAssetAtPath(path, typeof(T));
		if (asset == null)
		{
			Debug.Log("ScriptableObject 新規作成");
			// 指定のパスにファイルが存在しない場合は新規作成
			AssetDatabase.CreateAsset(data, path);
		}
		else
		{
			Debug.Log("ScriptableObject 更新");
			// 指定のパスに既に同名のファイルが存在する場合は更新
			EditorUtility.CopySerialized(data, asset);
			AssetDatabase.SaveAssets();
		}
		AssetDatabase.Refresh();
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

	/// <summary>
	/// オブジェクトのフィールド名とListのジェネリスク型から子クラス名を取得しListとして返す
	/// </summary>
	/// <param name="targetClassName"></param>
	/// <returns></returns>
	public (List<string>, List<string>, List<string>) GetFieldName(string targetClassName)
	{
		// リスト以外のフィールド
		List<string> parentFieldNames = new List<string>();
		// リストのみのフィールド
		List<string> parentFieldListNames = new List<string>();
		// リストのGenericsの型から取得したクラス名
		List<string> childClassNames = new List<string>();

		Type type = GetTypeByClassName(targetClassName);
		dynamic targetObj = Activator.CreateInstance(type);
		FieldInfo[] fields = targetObj.GetType().GetFields();
		string searchKey = "System.Collections.Generic.List";

		foreach (FieldInfo field in fields)
        {
			// フィールドタイプを文字列化
			string fieldTypeStr = field.FieldType.ToString();
			// Listかそれ以外で判別
            if (!fieldTypeStr.Contains(searchKey))
            {

				parentFieldNames.Add(field.Name);
			}
            else
            {
				int sindex = fieldTypeStr.IndexOf("[");
				int eindex = fieldTypeStr.IndexOf("]");
				//フィールドの型名からジェネリクスの型を取得
				string genericsName = fieldTypeStr.Substring(sindex + 1, eindex - sindex - 1);
				childClassNames.Add(genericsName);
				parentFieldListNames.Add(field.Name);
			}
		}
		return (parentFieldNames, parentFieldListNames, childClassNames);

	}

	/// <summary>
	/// ScriptableObjectで用いるオブジェクト名を取得する
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
	/// クラス名からインスタンスを生成し、返す。
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
	/// 指定クラスのGenericsタイプを任意のクラス型に設定しインスタンスを返す
	/// ※このクラスはJsonImpoterEditorにしか使えない
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
	/// 指定のオブジェクトのフィールドに指定のデータを設定する
	/// getFields：値取得には対象の値が格納されているクラスからフィールド名を指定しそこから取得
	/// setFields：値設定には対象のクラスのフィールド名を取得し、そのクラスと設定値を指定し設定
	/// </summary>
	/// <param name="receiveObj"> 受け取り先オブジェクト</param>
	/// <param name="sendObj"> データ送り先オブジェクト</param>
	public void SetFiledsData( dynamic receiveObj, dynamic sendObj)
    {
		// データを取得するクラスのフィールド
		FieldInfo[] getFields = sendObj.GetType().GetFields();
		// データを設定するクラスのフィールド
		FieldInfo[] setFields = receiveObj.GetType().GetFields();
		string searchKey = "System.Collections.Generic.List";
		int fieldCnt = 0;
		// 受渡し元から受渡し先オブジェクトへデータを設定
		foreach (FieldInfo field in setFields) // infoReceiveは代入先
		{

			// フィールドタイプを文字列化
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
	/// 受け取り元のリストに対応したクラス型のデータを追加する
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
	/// オブジェクトからListを取得しListに格納し返す
	/// isListGetにtureを渡すとListを、falseならそれ以外のリストを取得
	/// </summary>
	/// <param name="obj"></param>
	public dynamic GetFieldsFromObj(dynamic obj, bool isListGet)
	{
		FieldInfo[] infoFields = obj.GetType().GetFields();
		string searchKey = "System.Collections.Generic.List";
		// リストだけ格納
		List<dynamic> outPut = new List<dynamic>();

        if (infoFields.Length < 1)
        {
			Debug.Log("GetFieldsFromObj：フィールドが取得されていません");
			return null;
        }

		//　GetDataを
		foreach (FieldInfo field in infoFields)
		{
			// フィールドタイプを文字列化
			string fieldTypeStr = field.FieldType.ToString();
			// Listかそれ以外で判別
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