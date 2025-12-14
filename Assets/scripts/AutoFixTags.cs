#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 必要なタグ（Customer）が消えてしまった場合に自動で復旧するスクリプト
/// エディタでコンパイルが走ると自動的に実行されます。
/// </summary>
[InitializeOnLoad]
public class AutoFixTags
{
    static AutoFixTags()
    {
        CheckAndAddTag("Customer");
    }

    static void CheckAndAddTag(string tagName)
    {
        // TagManagerを開く
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // 既に存在するかチェック
        bool found = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(tagName))
            {
                found = true;
                break;
            }
        }

        // なければ追加
        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            n.stringValue = tagName;
            tagManager.ApplyModifiedProperties();
            
            Debug.Log($"<color=green>✅ 自動修復: タグ '{tagName}' を追加しました！これでエラーは消えるはずです。</color>");
        }
    }
}
#endif
