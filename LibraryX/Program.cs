using Avalonia;

namespace LibraryX;

class Program
{
    // Punto de entrada de la app. Si tu Program.cs actual ya tenía lógica
    // propia (ej. llamadas a Loader.cs), muévela a donde corresponda
    // (ej. dentro de App.axaml.cs u otra clase) — este Main es el que
    // Avalonia necesita para arrancar la ventana.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
