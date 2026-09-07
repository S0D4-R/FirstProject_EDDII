namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Globalization;

public partial class DialogAgregar : Window
{
    // Resultado al cerrar: null = cancelado
    public LibraryX.Models.BookModel? Resultado { get; private set; }

    // Códigos existentes para que el generador evite colisiones (el usuario no los escribe)
    private readonly string[] _codigosExistentes;

    public DialogAgregar(string[] codigosExistentes)
    {
        InitializeComponent();
        _codigosExistentes = codigosExistentes;
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        // Validación básica de campos vacíos (el código lo genera CodeGen)
        if (string.IsNullOrWhiteSpace(TxtTitulo.Text)   ||
            string.IsNullOrWhiteSpace(TxtAutor.Text)    ||
            string.IsNullOrWhiteSpace(TxtCategoria.Text)||
            string.IsNullOrWhiteSpace(TxtFechaPub.Text) ||
            string.IsNullOrWhiteSpace(TxtCopias.Text))
        {
            MostrarError("Los campos título, autor, categoría, fecha y copias son obligatorios.");
            return;
        }

        if (!DateTime.TryParse(TxtFechaPub.Text.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaPub))
        {
            MostrarError("Fecha de publicación inválida. Usa el formato AAAA-MM-DD (Ej. 1901-01-16).");
            return;
        }

        if (!int.TryParse(TxtCopias.Text.Trim(), out int copias) || copias < 1)
        {
            MostrarError("Las copias deben ser un número entero mayor a 0.");
            return;
        }

        string fechaPublicacion = fechaPub.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        // El código único lo genera CodeGen con el formato del proyecto
        string codigoGenerado = LibraryX.Models.CodeGen.Generar(TxtTitulo.Text.Trim(), fechaPublicacion, _codigosExistentes);

        Resultado = new LibraryX.Models.BookModel(
            codigoGenerado,
            TxtTitulo.Text.Trim(),
            TxtAutor.Text.Trim(),
            TxtCategoria.Text.Trim(),
            fechaPublicacion,
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