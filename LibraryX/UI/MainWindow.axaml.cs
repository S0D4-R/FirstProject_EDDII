using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LibVLCSharp.Shared;

namespace LibraryX.UI;

public partial class MainWindow : Window
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;

    // Sistema de diálogos por pasos
    private string[][] _dialogoActual = [];
    private string[] _spritesActual   = [];
    private int _pasoDialogo = 0;

    // Callbacks de las opciones del ChoiceBar
    private Action? _onChoice1;
    private Action? _onChoice2;

    public MainWindow()
    {
        InitializeComponent();
        InicializarAudio();
        CargarFondo("library.jpg");
        ActualizarSpritePorAccion("Happy");
        CargarAudioInicial();

        // Diálogo de bienvenida al arrancar
        IniciarDialogo(
            ["Bibliotecaria", "Bibliotecaria"],
            [
                "Bienvenido a la Biblioteca Arcana... Sus pasillos guardan siglos de conocimiento.",
                "Puedes buscar, agregar o prestar volúmenes desde el panel lateral. ¿En qué puedo ayudarte?"
            ],
            ["Happy", "Sure"]
        );
    }

    // =========================================================
    // SISTEMA DE DIÁLOGOS POR PASOS
    // =========================================================

    private void IniciarDialogo(string[] speakers, string[] lines, string[] acciones)
    {
        _dialogoActual = new string[lines.Length][];
        for (int i = 0; i < lines.Length; i++)
        {
            _dialogoActual[i] = [speakers[i], lines[i], acciones[i]];
        }
        _pasoDialogo = 0;
        MostrarPasoActual();
    }

    private void MostrarPasoActual()
    {
        if (_dialogoActual.Length == 0) return;
        var paso = _dialogoActual[_pasoDialogo];
        ActualizarSpritePorAccion(paso[2]);
        MostrarDialogo(paso[0], paso[1]);
    }

    private void AvanzarDialogo()
    {
        if (_dialogoActual.Length == 0) return;
        _pasoDialogo++;
        if (_pasoDialogo < _dialogoActual.Length)
        {
            MostrarPasoActual();
        }
        else
        {
            _pasoDialogo = _dialogoActual.Length - 1;
        }
    }

    private void MostrarOpciones(string texto1, string texto2, Action cb1, Action cb2)
    {
        ChoiceButton1.Content = texto1;
        ChoiceButton2.Content = texto2;
        _onChoice1 = cb1;
        _onChoice2 = cb2;
        ChoiceBar.IsVisible = true;
    }

    private void OcultarOpciones()
    {
        ChoiceBar.IsVisible = false;
        _onChoice1 = null;
        _onChoice2 = null;
    }

    private void OnChoice1Click(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        _onChoice1?.Invoke();
    }

    private void OnChoice2Click(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        _onChoice2?.Invoke();
    }

    // =========================================================
    // UTILIDADES PÚBLICAS
    // =========================================================

    public void MostrarDialogo(string nombrePersonaje, string texto)
    {
        SpeakerName.Text = nombrePersonaje;
        DialogueText.Text = texto;
    }

    public void MostrarOpciones(bool visible)
    {
        ChoiceBar.IsVisible = visible;
    }

    // =========================================================
    // FONDO
    // =========================================================

    public void CargarFondo(string rutaImagen)
    {
        var bitmap = CargarBitmap(rutaImagen);
        if (bitmap != null) BackgroundImage.Source = bitmap;
    }

    // =========================================================
    // SPRITES
    // =========================================================

    public void CargarSpriteIzquierdo(string rutaImagen)
    {
        var bitmap = CargarBitmap(rutaImagen);
        if (bitmap != null) SpriteLeft.Source = bitmap;
    }

    public void ActualizarSpritePorAccion(string accion)
    {
        CargarSpriteIzquierdo(LibrarySpace.ObtenerSpritePorAccion(accion));
    }

    private Bitmap? CargarBitmap(string nombreOPath)
    {
        string nombreArchivo = System.IO.Path.GetFileName(nombreOPath);

        try
        {
            var uri = new Uri($"avares://LibraryX/UI/Assets/{nombreArchivo}");
            if (AssetLoader.Exists(uri))
            {
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }
        }
        catch { }

        string[] rutas = [
            nombreOPath,
            System.IO.Path.Combine(AppContext.BaseDirectory, "UI", "Assets", nombreArchivo),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "UI", "Assets", nombreArchivo)
        ];

        foreach (var ruta in rutas)
        {
            if (System.IO.File.Exists(ruta))
            {
                try { return new Bitmap(ruta); } catch { }
            }
        }
        return null;
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void InicializarAudio()
    {
        try
        {
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error LibVLC: {ex.Message}");
        }
    }

    private void CargarAudioInicial()
    {
        string nombre = "Hallways of Redepmtion [W1BRVRv1PEg].mp3";
        string[] rutas = [
            System.IO.Path.Combine(AppContext.BaseDirectory, "UI", "Assets", nombre),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "UI", "Assets", nombre),
            System.IO.Path.Combine("UI", "Assets", nombre)
        ];
        foreach (var p in rutas)
        {
            if (System.IO.File.Exists(p)) { CargarAudio(p); break; }
        }
    }

    public void CargarAudio(string rutaAudio)
    {
        if (_libVLC is null || _mediaPlayer is null) return;
        try
        {
            string fullPath = System.IO.Path.GetFullPath(rutaAudio);
            using var media = new Media(_libVLC, fullPath, FromType.FromPath);
            _mediaPlayer.Media = media;
            AudioFileLabel.Text = $"[ {System.IO.Path.GetFileName(rutaAudio)} ]";
            _mediaPlayer.Play();
            _mediaPlayer.Volume = 25;
        }
        catch (Exception ex)
        {
            AudioFileLabel.Text = $"[ {System.IO.Path.GetFileName(rutaAudio)} ]";
            System.Diagnostics.Debug.WriteLine($"Error audio: {ex.Message}");
        }
    }

    private void OnPlayClick(object? sender, RoutedEventArgs e)  => _mediaPlayer?.Play();
    private void OnPauseClick(object? sender, RoutedEventArgs e) => _mediaPlayer?.Pause();

    private void OnVolumeChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (_mediaPlayer is not null) _mediaPlayer.Volume = (int)e.NewValue;
    }

    // =========================================================
    // BOTONES SIDEBAR — abren Dialog Windows
    // =========================================================

    private async void OnBuscarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Thinking");
        MostrarDialogo("Bibliotecaria", "Consultando los estantes arcanos...");

        var dlg = new Dialogs.DialogBuscar();
        await dlg.ShowDialog(this);

        if (dlg.CodigoBuscado is not null)
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Bibliotecaria", $"Buscando el código: {dlg.CodigoBuscado}. Un momento...");
        }
        else
        {
            ActualizarSpritePorAccion("Thinking");
            MostrarDialogo("Bibliotecaria", "La búsqueda fue cancelada. Aquí estaré cuando lo necesites.");
        }
    }

    private async void OnAgregarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Happy");
        MostrarDialogo("Bibliotecaria", "¡Un nuevo volumen para la colección!");

        var dlg = new Dialogs.DialogAgregar();
        await dlg.ShowDialog(this);

        if (dlg.Resultado is not null)
        {
            ActualizarSpritePorAccion("Happy");
            MostrarDialogo("Bibliotecaria",
                $"«{dlg.Resultado.Titulo}» de {dlg.Resultado.Autor} ha sido registrado con el código {dlg.Resultado.Codigo}.");
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Bibliotecaria", "Registro cancelado. El estante espera pacientemente.");
        }
    }

    private void OnListarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Sure");
        MostrarDialogo("Bibliotecaria", "Desplegando el inventario completo de la Biblioteca Arcana...");
    }

    private async void OnPrestamoClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Doubt");
        MostrarDialogo("Bibliotecaria", "¿Un préstamo? Espero que el lector cuide bien el tomo...");

        var dlg = new Dialogs.DialogPrestamo();
        await dlg.ShowDialog(this);

        if (dlg.CodigoLibro is not null)
        {
            ActualizarSpritePorAccion("Thinking");
            MostrarDialogo("Bibliotecaria",
                $"El libro {dlg.CodigoLibro} ha sido registrado a nombre de {dlg.NombreLector}. Que aproveche la lectura.");
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Bibliotecaria", "Préstamo cancelado. El volumen permanece en sus estantes.");
        }
    }

    private async void OnDevolverClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Sure");
        MostrarDialogo("Bibliotecaria", "¿El viajero trae de regreso un volumen prestado?");

        var dlg = new Dialogs.DialogDevolver();
        await dlg.ShowDialog(this);

        if (dlg.CodigoLibro is not null)
        {
            ActualizarSpritePorAccion("Happy");
            MostrarDialogo("Bibliotecaria",
                $"El libro {dlg.CodigoLibro} ha sido devuelto sano y salvo. Los estantes agradecen su retorno.");
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Bibliotecaria", "Devolución cancelada. Cuando estés listo, aquí estaré.");
        }
    }

    private void OnReporteClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Thinking");
        MostrarDialogo("Bibliotecaria", "Consultando los anales... Generando el reporte de préstamos.");
    }

    private async void OnCargarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Sure");
        MostrarDialogo("Bibliotecaria", "¿Desde qué grimorio deseas importar el inventario?");

        var dlg = new Dialogs.DialogCargar();
        await dlg.ShowDialog(this);

        if (dlg.RutaArchivo is not null)
        {
            ActualizarSpritePorAccion("Happy");
            MostrarDialogo("Bibliotecaria",
                $"El archivo ha sido encontrado. Absorbiendo los registros de: {System.IO.Path.GetFileName(dlg.RutaArchivo)}...");
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Bibliotecaria", "Carga cancelada. Los estantes esperan pacientemente.");
        }
    }

    private void OnSpriteClick(object? sender, PointerPressedEventArgs e)
    {
        if (_dialogoActual.Length > 1 && _pasoDialogo < _dialogoActual.Length - 1)
        {
            AvanzarDialogo();
        }
        else
        {
            string sprite = LibrarySpace.ObtenerSiguienteSprite();
            CargarSpriteIzquierdo(sprite);
        }
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Doubt");
        MostrarDialogo("Bibliotecaria", "Hasta pronto, viajero. Los libros siempre te esperarán aquí...");
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await System.Threading.Tasks.Task.Delay(1800);
            Close();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
        base.OnClosed(e);
    }
}
