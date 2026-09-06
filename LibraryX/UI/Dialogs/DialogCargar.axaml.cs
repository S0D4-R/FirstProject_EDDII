namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogCargar : Window
{
    public string? RutaArchivo { get; private set; }

    public DialogCargar()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtRuta.Text))
        {
            TxtError.Text = "Ingresa la ruta del archivo.";
            TxtError.IsVisible = true;
            return;
        }

        if (!System.IO.File.Exists(TxtRuta.Text.Trim()))
        {
            TxtError.Text = "El archivo no existe en la ruta indicada.";
            TxtError.IsVisible = true;
            return;
        }

        RutaArchivo = TxtRuta.Text.Trim();
        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        RutaArchivo = null;
        Close();
    }
}
