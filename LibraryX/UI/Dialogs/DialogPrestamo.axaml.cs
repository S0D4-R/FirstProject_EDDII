namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogPrestamo : Window
{
    public string? TituloLibro { get; private set; }
    public string? NombreLector { get; private set; }

    public DialogPrestamo()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtTitulo.Text) ||
            string.IsNullOrWhiteSpace(TxtLector.Text))
        {
            TxtError.Text = "Título y nombre del lector son obligatorios.";
            TxtError.IsVisible = true;
            return;
        }

        TituloLibro  = TxtTitulo.Text.Trim();
        NombreLector = TxtLector.Text.Trim();
        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        TituloLibro  = null;
        NombreLector = null;
        Close();
    }
}
