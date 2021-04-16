using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoardTile : Tile 
{
    public string ownerNickname = "";
    public int voteCount = 0;
    public Vector3Int tpos;

#if UNITY_EDITOR
// The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/RoadTile")]
    public static void CreateBoardTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Road Tile", "New Road Tile", "Asset", "Save Road Tile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BoardTile>(), path);
    }
#endif
}