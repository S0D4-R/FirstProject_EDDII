namespace LibraryX.UI.Dialogs;

using Avalonia.Controls;
using Avalonia.Interactivity;

public partial class DialogCodigoGenerado : Window
{
    public DialogCodigoGenerado(string codigo, string titulo)
    {
        InitializeComponent();
        TxtDescripcion.Text = $"El volumen «{titulo}» fue registrado en el catálogo con el código:";
        TxtCodigo.Text = codigo;
    }

    private void OnAceptarClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}