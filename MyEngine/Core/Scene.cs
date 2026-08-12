namespace RetroEngine.Core;

/// <summary>
/// A collection of entities plus scene-level load/update/draw hooks.
/// </summary>
public abstract class Scene
{
    private readonly List<GameObject> _entities = [];
    private readonly List<GameObject> _pendingAdd = [];
    private readonly HashSet<GameObject> _pendingRemove = [];

    protected IReadOnlyList<GameObject> Entities => _entities;

    public bool IsLoaded { get; private set; }

    /// <summary>
    /// Called by <see cref="SceneManager"/>. Prefer overriding <see cref="Load"/> in game scenes.
    /// </summary>
    public void LoadScene()
    {
        if (IsLoaded)
        {
            return;
        }

        Load();
        IsLoaded = true;
    }

    protected virtual void Load()
    {
    }

    public virtual void Unload()
    {
        _entities.Clear();
        _pendingAdd.Clear();
        _pendingRemove.Clear();
        IsLoaded = false;
    }

    public void AddEntity(GameObject entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingAdd.Add(entity);
    }

    public void RemoveEntity(GameObject entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingRemove.Add(entity);
    }

    public virtual void Update(float deltaTime)
    {
        FlushPending();

        for (int i = 0; i < _entities.Count; i++)
        {
            GameObject entity = _entities[i];
            if (entity.IsActive)
            {
                entity.Update(deltaTime);
            }
        }

        FlushPending();
    }

    public virtual void Draw()
    {
        FlushPending();

        for (int i = 0; i < _entities.Count; i++)
        {
            GameObject entity = _entities[i];
            if (entity.IsActive && entity.IsVisible)
            {
                entity.Draw();
            }
        }
    }

    private void FlushPending()
    {
        if (_pendingRemove.Count > 0)
        {
            _entities.RemoveAll(_pendingRemove.Contains);
            _pendingRemove.Clear();
        }

        if (_pendingAdd.Count > 0)
        {
            _entities.AddRange(_pendingAdd);
            _pendingAdd.Clear();
        }
    }
}
