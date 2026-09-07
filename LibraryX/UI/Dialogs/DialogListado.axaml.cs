namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;
using LibraryX.Models;

public partial class DialogListado : Window
{
    public DialogListado(BookModel[] libros)
    {
        InitializeComponent();

        TxtTitulo.Text = $"{libros.Length} título(s) en el catálogo";

        for (int i = 0; i < libros.Length; i++)
        {
            var item = new TextBlock
            {
                Text = $"{i + 1}. [{libros[i].Codigo}] {libros[i].Titulo} — {libros[i].Autor} ({libros[i].Categoria}) | {libros[i].CopiasDisponibles} dispon. | {libros[i].VecesPrestado} prést.",
                Classes = { "item" }
            };
            ListaContenido.Children.Add(item);
        }
    }

    private void OnCerrarClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}