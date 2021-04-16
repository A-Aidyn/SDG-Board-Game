using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Mirror;

public class TileManager : NetworkBehaviour
{
    public Grid boardGrid;
    public GameManagerScript gameManager;
    Tilemap boardTilemap;
    public bool cannotClick = false;
    // BoardTile redTile;
    // BoardTile blueTile;
    
    // Start is called before the first frame update
    void Start()
    {
        boardTilemap = boardGrid.GetComponentInChildren<Tilemap>();
        // redTile = Resources.Load<BoardTile>("Tiles/redCard") as BoardTile;
        // blueTile = Resources.Load<BoardTile>("Tiles/blueCard") as BoardTile;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManagerScript>();
        if(gameManager) {
            Debug.Log("[In TileManager] gameManager was set!");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // mouse position in screen space
            var mpos = Input.mousePosition;

            // Create a ray from camera to world
            var ray = Camera.main.ScreenPointToRay(mpos);

            // Create a Plane object to raycast against
            // Assume Tilemap resides on plane z 0
            var plane = new Plane(Vector3.back, Vector3.zero);

            // Do a Plane Raycast with your screen to world ray
            float hitDist;
            plane.Raycast(ray, out hitDist);

            // If you aimed towards this infinite Plane, it hit
            var point = ray.GetPoint(hitDist);
            Debug.Log("Point: " + point);

            // Convert hitpoint to Tilemap / GridLayout space
            // Cell position is an integer positions in GridLayout
            var tpos = boardTilemap.WorldToCell(point);

            // Try to get a tile from cell position
            var tile = boardTilemap.GetTile<BoardTile>(tpos);
            if (tile != null)
            {
                tile.tpos = tpos;
                Debug.Log("There is a tile!");
                tileClicked(tile, tpos);        
            } else {
                Debug.Log("Tile was not found!");
            }       
        }
    }

    void tileClicked(BoardTile tile, Vector3Int tpos) {
        Debug.Log("[tileClicked]");
        // If we are not in voting/putting (clicking) state we don't perform action
        if(gameManager.gameState != GameManagerScript.States.Vote && gameManager.gameState != GameManagerScript.States.PutCard) 
            return;
        if(cannotClick)
            return;
        PlayerManager playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        
        if(gameManager.gameState == GameManagerScript.States.PutCard) {
            // If its not our turn (current client's turn) then do nothing 
            if(gameManager.getNickname(gameManager.currentPlayer) != playerManager.nickname)
                return;
            playerManager.UpdateCurrentPlayer(tpos);
        } else {
            // Voting
            Debug.Log("[TileManager] [In voting!]");
            gameManager.ShowVote(tile, tpos);
        }
    }

    public void IncrementVoteCount(Vector3Int tpos) {
        BoardTile tile = boardTilemap.GetTile<BoardTile>(tpos);
        if(tile) {
            tile.voteCount ++;
            Debug.Log("[TileManage. IncrementVoteCount] incremented tile votecount: " + tile.voteCount.ToString());
        }
    }

    // Updating tile with given cards group and its index (find it from resources folder) and cell position of tile
    public void UpdateTile(int currentGroup, int chosenCardIndex, Vector3Int tpos, string nickname) {
        Debug.Log("[UpdateTile]");
        Debug.Log("currentGroup: " + currentGroup.ToString() + " chosenCard: " + chosenCardIndex.ToString());
        string name = chosenCardIndex.ToString();
        if(name.Length < 2) name = "0" + name;
        BoardTile tile = Resources.Load<BoardTile>("Tiles/" + currentGroup.ToString() + '/' + name) as BoardTile;
        if(tile)
            Debug.Log("Tile was found!");
        tile.ownerNickname = nickname;
        boardTilemap.SetTile(tpos, tile);
        boardTilemap.RefreshTile(tpos);
    }
    
    public void setCentralTile(int categoryNum) {
        string name = categoryNum.ToString();
        if(name.Length < 2)
            name = "0" + name;
        Debug.Log("[setCentralTile]");
        BoardTile chosenTile = Resources.Load<BoardTile>("Tiles/Category/" + name) as BoardTile;
        if(chosenTile) {
            Debug.Log("Tile was set!");
        }
        var tpos = new Vector3Int(0, 0, 0);
        boardTilemap.SetTile(tpos, chosenTile);
        boardTilemap.RefreshTile(tpos);
    }

    public void OnClick(Sprite ChangeTo) 
    {
        Debug.Log("Clicked!");
        gameObject.GetComponent<SpriteRenderer>().sprite = ChangeTo;
    }
}
