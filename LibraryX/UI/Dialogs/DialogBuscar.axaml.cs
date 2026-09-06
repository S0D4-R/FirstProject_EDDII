namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogBuscar : Window
{
    public string? CodigoBuscado { get; private set; }

    public DialogBuscar()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtCodigo.Text))
        {
            TxtError.Text = "Ingresa el código del libro.";
            TxtError.IsVisible = true;
            return;
        }

        CodigoBuscado = TxtCodigo.Text.Trim();
        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        CodigoBuscado = null;
        Close();
    }
}
