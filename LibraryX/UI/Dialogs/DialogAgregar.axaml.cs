namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogAgregar : Window
{
    // Resultado al cerrar: null = cancelado
    public LibraryX.Models.BookModel? Resultado { get; private set; }

    public DialogAgregar()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        // Validación básica de campos vacíos
        if (string.IsNullOrWhiteSpace(TxtCodigo.Text)   ||
            string.IsNullOrWhiteSpace(TxtTitulo.Text)   ||
            string.IsNullOrWhiteSpace(TxtAutor.Text)    ||
            string.IsNullOrWhiteSpace(TxtCategoria.Text)||
            string.IsNullOrWhiteSpace(TxtCopias.Text))
        {
            MostrarError("Todos los campos son obligatorios.");
            return;
        }

        if (!int.TryParse(TxtCopias.Text.Trim(), out int copias) || copias < 1)
        {
            MostrarError("Las copias deben ser un número entero mayor a 0.");
            return;
        }

        Resultado = new LibraryX.Models.BookModel(
            TxtCodigo.Text.Trim(),
            TxtTitulo.Text.Trim(),
            TxtAutor.Text.Trim(),
            TxtCategoria.Text.Trim(),
            copias
        );

        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        Resultado = null;
        Close();
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        TxtError.IsVisible = true;
    }
}
