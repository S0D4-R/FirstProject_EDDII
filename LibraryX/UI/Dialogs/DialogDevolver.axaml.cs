namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogDevolver : Window
{
    public string? CodigoLibro { get; private set; }

    public DialogDevolver()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtCodigo.Text))
        {
            TxtError.Text = "Ingresa el código del libro a devolver.";
            TxtError.IsVisible = true;
            return;
        }

        CodigoLibro = TxtCodigo.Text.Trim();
        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        CodigoLibro = null;
        Close();
    }
}
