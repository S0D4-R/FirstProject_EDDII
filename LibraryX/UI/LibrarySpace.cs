using LibraryX.StructuresMain;

namespace LibraryX.UI;

public static class LibrarySpace
{
    private static readonly SpriteList _sprites = new();

    static LibrarySpace()
    {
        _sprites.Agregar("happyto.png");
        _sprites.Agregar("mightbe.png");
        _sprites.Agregar("ofc.png");
        _sprites.Agregar("doubts.png");
    }

    // Por acción específica (botón fijo → cara fija)
    public static string ObtenerSpritePorAccion(string accion)
    {
        return accion switch
        {
            "Happy"   => _sprites.ObtenerPorIndice(0) ?? "happyto.png",
            "Thinking"  => _sprites.ObtenerPorIndice(1) ?? "mightbe.png",
            "Sure" => _sprites.ObtenerPorIndice(2) ?? "ofc.png",
            "Doubt" => _sprites.ObtenerPorIndice(3) ?? "doubts.png",
            _          => "ofc.png"
        };
    }

    // Alternando en secuencia con cada click
    public static string ObtenerSiguienteSprite()
    {
        return _sprites.Siguiente() ?? "mightbe.png";
    }
}