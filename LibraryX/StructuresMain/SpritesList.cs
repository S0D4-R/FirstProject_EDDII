namespace LibraryX.StructuresMain;

public class SpriteNode
{
    public string SpriteName { get; set; }
    public SpriteNode? Next { get; set; }

    public SpriteNode(string spriteName)
    {
        SpriteName = spriteName;
    }
}

public class SpriteList
{
    private SpriteNode? _head;
    private SpriteNode? _tail;
    private SpriteNode? _current;
    private int _count;

    public int Count => _count;

    public void Agregar(string sprite)
    {
        if (string.IsNullOrWhiteSpace(sprite)) return;

        var newNode = new SpriteNode(sprite);
        if (_head == null)
        {
            _head = newNode;
            _tail = newNode;
        }
        else
        {
            _tail!.Next = newNode;
            _tail = newNode;
        }
        _count++;
    }

    public string? ObtenerPorIndice(int index)
    {
        if (index < 0 || index >= _count || _head == null) return null;

        var curr = _head;
        for (int i = 0; i < index; i++)
        {
            if (curr == null) return null;
            curr = curr.Next;
        }
        return curr?.SpriteName;
    }

    public string? Siguiente()
    {
        if (_head == null) return null;

        if (_current == null || _current.Next == null)
        {
            _current = _head;
        }
        else
        {
            _current = _current.Next;
        }

        return _current.SpriteName;
    }
}
