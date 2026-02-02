using UnityEngine;
using UnityEngine.InputSystem;

public class ChessBoard : ActorComponent
{
    Pieces[,] boardPieces = new Pieces[8, 8];
    BoxCollider[,] boardTrigger = new BoxCollider[8, 8];

    SpawnManager spawnManager;
    ActionManager actionManager;
    HighlightManager highlightManager;


    [Header("Highlight Materials")]
    [SerializeField] Material hoverMaterial;
    [SerializeField] Material moveMaterial;
    [SerializeField] Material attackMaterial;
    [SerializeField] Material specialMaterial;


    void Start()
    {
        spawnManager = GetComponent<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogError("체스보드에 SpawnManager가 없습니다.");
            return;
        }

        spawnManager.Init(this);

        actionManager = new ActionManager(this);

        highlightManager = new HighlightManager(
            this,
            hoverMaterial,
            moveMaterial,
            attackMaterial,
            specialMaterial
        );

        spawnManager.InitialSpawn();
        InitialTriggerBoxes();

        actionManager.GameoverEvent += ActionManager_GameoverEvent;
    }

    void Update()
    {
        UpdateInput();
        UpdateHover();

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            OnClicked();
        }
    }

    public Vector3 QueryLocation(GridIndex gridIndex)
    {
        const float GridUnit = 1.5f;

        return new Vector3(
            (gridIndex.X - 3.5f) * GridUnit,
            0f,
            (gridIndex.Y - 3.5f) * GridUnit
        );
    }

    public Vector3 QueryLocation(int x, int y) =>
        QueryLocation(new GridIndex(x, y));

    public void Turnover()
    {
        foreach (var piece in boardPieces)
        {
            piece?.Turnover();
        }
    }

    void ClearState()
    {
        highlightManager.ClearState();
        actionManager.ClearState();
    }
    void UpdateInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool hasAction = false;

        if (keyboard.zKey.wasPressedThisFrame)
        {
            actionManager.Undo();
            hasAction = true;
        }

        if (keyboard.yKey.wasPressedThisFrame)
        {
            actionManager.Redo();
            hasAction = true;
        }

        if (hasAction)
        {
            ClearState();
        }
    }

    void UpdateHover()
    {
        var cam = Camera.main;
        var mouse = Mouse.current;
        if (cam == null || mouse == null) return;

        var ray = cam.ScreenPointToRay(mouse.position.ReadValue());

        if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("TriggerBox")))
        {
            highlightManager.HoverIndex = FindTriggerCollider(hit.collider);
        }
        else
        {
            highlightManager.HoverIndex = null;
        }
    }

    public Pieces GetPieceById(int id)
    {
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                var p = this[new GridIndex(x, y)];
                if (p != null && p.PieceId == id)
                    return p;
            }
        return null;
    }

    void OnClicked()
    {
        actionManager.AddClick(highlightManager.HoverIndex);

        if (actionManager.HasSelection)
        {
            var piece = actionManager.SelectionPiece;
            highlightManager.BuildMoveHighlight(piece);
            highlightManager.BuildAttackHighlight(piece);
            highlightManager.BuildSpecialHighligh(piece);
        }
        else
        {
            highlightManager.ClearMoveHighlight();
            highlightManager.ClearAttackHighlight();
            highlightManager.ClearSpecialHighlight();
        }
    }

    void InitialTriggerBoxes()
    {
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 8; ++y)
            {
                var trigger = boardTrigger[x, y] =
                    gameObject.AddSceneComponent<BoxCollider>($"TriggerBox[{x},{y}]");

                trigger.transform.localPosition = QueryLocation(x, y);
                trigger.size = new Vector3(1f, 0.05f, 1f);
                trigger.gameObject.layer = LayerMask.NameToLayer("TriggerBox");
                trigger.isTrigger = true;
            }
        }
    }

    GridIndex? FindTriggerCollider(Collider collider)
    {
        for (int x = 0; x < 8; ++x)
        {
            for (int y = 0; y < 8; ++y)
            {
                if (boardTrigger[x, y] == collider)
                {
                    return new GridIndex(x, y);
                }
            }
        }
        return null;
    }
    void ActionManager_GameoverEvent(object sender, GameoverType e)
    {
        Debug.Log($"{e}!");
    }
    public Pieces this[GridIndex index]
    {
        get => boardPieces[index.X, index.Y];
        set => boardPieces[index.X, index.Y] = value;
    }

    public Pieces this[GridIndex? index] =>
        index.HasValue ? this[index.Value] : null;

    public Pieces this[int x, int y]
    {
        get => boardPieces[x, y];
        set => boardPieces[x, y] = value;
    }

    public Pieces[,] GetPieces() => boardPieces;

    public BuildedData BuildedData => actionManager.BuildedData;
    public SpawnManager SpawnMgr => spawnManager;

    public static GridIndex ToBlack => new GridIndex(0, 1);
    public static GridIndex ToWhite => new GridIndex(0, -1);
}