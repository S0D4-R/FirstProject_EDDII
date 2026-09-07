namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogDevolver : Window
{
    public string? TituloLibro { get; private set; }

    public DialogDevolver()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtTitulo.Text))
        {
            TxtError.Text = "Ingresa el título del libro a devolver.";
            TxtError.IsVisible = true;
            return;
        }

        TituloLibro = TxtTitulo.Text.Trim();
        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        TituloLibro = null;
        Close();
    }
}
