namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogPrestamo : Window
{
    public string? CodigoLibro  { get; private set; }
    public string? NombreLector { get; private set; }

    public DialogPrestamo()
    {
        InitializeComponent();
    }

    private void OnConfirmarClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtCodigo.Text) ||
            string.IsNullOrWhiteSpace(TxtLector.Text))
        {
            TxtError.Text = "Código y nombre del lector son obligatorios.";
            TxtError.IsVisible = true;
            return;
        }

        CodigoLibro  = TxtCodigo.Text.Trim();
        NombreLector = TxtLector.Text.Trim();
        Close();
    }

    private void OnCancelarClick(object? sender, RoutedEventArgs e)
    {
        CodigoLibro  = null;
        NombreLector = null;
        Close();
    }
}
