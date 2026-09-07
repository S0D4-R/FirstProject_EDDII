namespace LibraryX.Models;

public class BookModel
{
    public string Codigo { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string FechaPublicacion  { get; set; } = string.Empty;
    public int    CopiasDisponibles { get; set; }
    public int    VecesPrestado     { get; set; }

    public BookModel() { }

    public BookModel(string codigo, string titulo, string autor,string categoria, string fechaPublicacion,int copiasDisponibles, int vecesPrestado = 0)
    {
        Codigo = codigo;
        Titulo = titulo;
        Autor = autor;
        Categoria = categoria;
        FechaPublicacion  = fechaPublicacion;
        CopiasDisponibles = copiasDisponibles;
        VecesPrestado = vecesPrestado;
    }

    public override string ToString()
    {
        return $"[{Codigo}] {Titulo} - {Autor} ({Categoria}) | " + $"Publicado: {FechaPublicacion} | " + $"Disponibles: {CopiasDisponibles} | Préstamos: {VecesPrestado}";
    }
}