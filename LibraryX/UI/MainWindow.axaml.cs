using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using LibraryX.Models;
using LibraryX.StructuresMain;

namespace LibraryX.UI;

public partial class MainWindow : Window
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;

    // Estructuras de datos integradas
    private MinHeap _minHeap = new();
    private MaxHeap _maxHeap = new();
    private readonly BookShelf _arbolBPlus = new();
    private readonly string _rutaCsv = System.IO.Path.Combine(AppContext.BaseDirectory, "Data", "book_packets.csv");

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
        CargarInventarioInicial();

        // Diálogo de bienvenida al arrancar
        IniciarDialogo(
            ["Chihiro Fujisaki", "Chihiro Fujisaki"],
            [
                "Bienvenido a la Biblioteca!... Sus pasillos guardan siglos de conocimiento.",
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
    // BOTONES SIDEBAR — Operaciones de la Biblioteca
    // =========================================================

    private async void OnBuscarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Thinking");
        MostrarDialogo("Chihiro Fujisaki", "Consultando la base de datos...");

        var dlg = new Dialogs.DialogBuscar();
        await dlg.ShowDialog(this);

        if (dlg.CodigoBuscado is not null)
        {
            BookModel? libro = _arbolBPlus.search(dlg.CodigoBuscado);

            if (libro != null)
            {
                ActualizarSpritePorAccion("Sure");
                MostrarDialogo("Chihiro Fujisaki",
                    $"Encontrado: [{libro.Codigo}] «{libro.Titulo}» por {libro.Autor} ({libro.Categoria}) | Disponibles: {libro.CopiasDisponibles} | Préstamos: {libro.VecesPrestado}");
            }
            else
            {
                ActualizarSpritePorAccion("Thinking");
                MostrarDialogo("Chihiro Fujisaki", $"No se encontró ningún volumen con el código: {dlg.CodigoBuscado}.");
            }
        }
        else
        {
            ActualizarSpritePorAccion("Thinking");
            MostrarDialogo("Chihiro Fujisaki", "La búsqueda fue cancelada.");
        }
    }

    private async void OnAgregarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Happy");
        MostrarDialogo("Chihiro Fujisaki", "¡Un nuevo volumen para la colección!");

        string[] codigosExistentes = _arbolBPlus.ObtenerTodos().Select(l => l.Codigo).ToArray();
        var dlg = new Dialogs.DialogAgregar(codigosExistentes);
        await dlg.ShowDialog(this);

        if (dlg.Resultado is not null)
        {
            _minHeap.Insertar(dlg.Resultado);
            _maxHeap.Insertar(dlg.Resultado);
            _arbolBPlus.insert(dlg.Resultado);

            await new Dialogs.DialogCodigoGenerado(dlg.Resultado.Codigo, dlg.Resultado.Titulo).ShowDialog(this);

            ActualizarSpritePorAccion("Happy");
            MostrarDialogo("Chihiro Fujisaki",
                $"«{dlg.Resultado.Titulo}» de {dlg.Resultado.Autor} ha sido registrado con el código {dlg.Resultado.Codigo}.");
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Chihiro Fujisaki", "Registro cancelado. El estante espera pacientemente.");
        }
    }

    private async void OnListarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Sure");

        var elementos = _arbolBPlus.ObtenerTodos();
        if (elementos.Length == 0)
        {
            MostrarDialogo("Chihiro Fujisaki", "El inventario está vacío. Carga un archivo CSV o agrega un libro manualmente.");
            return;
        }

        await new Dialogs.DialogListado(elementos).ShowDialog(this);
        MostrarDialogo("Chihiro Fujisaki", $"Mostrando el catálogo completo ({elementos.Length} títulos). ¿Exploramos otro rincón?");
    }

    private async void OnPrestamoClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Doubt");
        MostrarDialogo("Chihiro Fujisaki", "¿Un préstamo? Registrando la salida del volumen...");

        var dlg = new Dialogs.DialogPrestamo();
        await dlg.ShowDialog(this);

        if (dlg.TituloLibro is not null)
        {
            BookModel? libro = BuscarPorTitulo(_arbolBPlus.ObtenerTodos(), dlg.TituloLibro);

            if (libro != null)
            {
                if (libro.CopiasDisponibles > 0)
                {
                    libro.CopiasDisponibles--;
                    libro.VecesPrestado++;
                    ActualizarSpritePorAccion("Happy");
                    MostrarDialogo("Chihiro Fujisaki",
                        $"El préstamo del libro «{libro.Titulo}» se registró a nombre de {dlg.NombreLector}. Copias restantes: {libro.CopiasDisponibles}.");
                }
                else
                {
                    ActualizarSpritePorAccion("Doubt");
                    MostrarDialogo("Chihiro Fujisaki", $"No hay copias disponibles del libro «{libro.Titulo}» en este momento.");
                }
            }
            else
            {
                ActualizarSpritePorAccion("Thinking");
                MostrarDialogo("Chihiro Fujisaki", $"No se encontró ningún libro con el título: {dlg.TituloLibro}.");
            }
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Chihiro Fujisaki", "Préstamo cancelado.");
        }
    }

    private async void OnDevolverClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Sure");
        MostrarDialogo("Chihiro Fujisaki", "¿Retorno de un volumen prestado?");

        var dlg = new Dialogs.DialogDevolver();
        await dlg.ShowDialog(this);

        if (dlg.TituloLibro is not null)
        {
            BookModel? libro = BuscarPorTitulo(_arbolBPlus.ObtenerTodos(), dlg.TituloLibro);

            if (libro != null)
            {
                libro.CopiasDisponibles++;
                ActualizarSpritePorAccion("Happy");
                MostrarDialogo("Chihiro Fujisaki",
                    $"El libro «{libro.Titulo}» fue devuelto con éxito. Copias disponibles: {libro.CopiasDisponibles}.");
            }
            else
            {
                ActualizarSpritePorAccion("Thinking");
                MostrarDialogo("Chihiro Fujisaki", $"No se encontró ningún libro con el título: {dlg.TituloLibro}.");
            }
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Chihiro Fujisaki", "Devolución cancelada.");
        }
    }

    private static BookModel? BuscarPorTitulo(BookModel[] libros, string titulo)
    {
        foreach (var b in libros)
        {
            if (string.Equals(b.Titulo, titulo, StringComparison.OrdinalIgnoreCase)) return b;
        }
        return null;
    }

    private async void OnEliminarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Thinking");
        MostrarDialogo("Chihiro Fujisaki", "¿Qué volumen retiramos de la colección? Indica su código.");

        var dlg = new Dialogs.DialogBuscar();
        await dlg.ShowDialog(this);

        if (dlg.CodigoBuscado is not null)
        {
            BookModel? libro = _arbolBPlus.search(dlg.CodigoBuscado);
            if (libro == null)
            {
                ActualizarSpritePorAccion("Thinking");
                MostrarDialogo("Chihiro Fujisaki", $"No se encontró ningún volumen con el código: {dlg.CodigoBuscado}.");
                return;
            }

            bool eliminado = _arbolBPlus.delete(dlg.CodigoBuscado);
            if (eliminado)
            {
                // Los heaps no tienen eliminación por clave: se reconstruyen sin el libro eliminado.
                _minHeap = new MinHeap();
                _maxHeap = new MaxHeap();
                foreach (var restante in _arbolBPlus.ObtenerTodos())
                {
                    _minHeap.Insertar(restante);
                    _maxHeap.Insertar(restante);
                }

                ActualizarSpritePorAccion("Sure");
                MostrarDialogo("Chihiro Fujisaki",
                    $"El volumen «{libro.Titulo}» ({libro.Codigo}) fue retirado de la colección.");
            }
            else
            {
                ActualizarSpritePorAccion("Doubt");
                MostrarDialogo("Chihiro Fujisaki", $"No fue posible eliminar el código {dlg.CodigoBuscado}.");
            }
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Chihiro Fujisaki", "Eliminación cancelada.");
        }
    }

    private void OnReporteClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Thinking");

        var todos = _arbolBPlus.ObtenerTodos();
        if (todos.Length == 0)
        {
            MostrarDialogo("Chihiro Fujisaki", "Aún no hay registros cargados para generar el reporte.");
            return;
        }

        // Reporte 1: listado del catálogo ordenado por título (requisito del PDF)
        BookModel[] porTitulo = OrdenarPorTitulo(todos);

        string reporteText = "📊 REPORTES RÁPIDOS\n\n📍 Catálogo ordenado por título:\n";
        int maxLista = Math.Min(porTitulo.Length, 5);
        for (int i = 0; i < maxLista; i++)
        {
            reporteText += $"{i + 1}. [{porTitulo[i].Codigo}] {porTitulo[i].Titulo} ({porTitulo[i].CopiasDisponibles} dispon.)\n";
        }
        if (porTitulo.Length > maxLista) reporteText += $"... y {porTitulo.Length - maxLista} más.";

        // Reporte 2: top de libros más prestados (MaxHeap)
        var topLibros = _maxHeap.ObtenerTop(5);
        if (topLibros.Length == 0)
        {
            reporteText += "\n\n🏆 Todavía no hay préstamos registrados.";
        }
        else
        {
            reporteText += "\n\n🏆 Top libros más prestados:\n";
            for (int i = 0; i < topLibros.Length; i++)
            {
                reporteText += $"{i + 1}. [{topLibros[i].Codigo}] {topLibros[i].Titulo} ({topLibros[i].VecesPrestado} préstamos)\n";
            }
        }

        ActualizarSpritePorAccion("Sure");
        MostrarDialogo("Chihiro Fujisaki", reporteText);
    }

    // Ordena una copia del arreglo por título usando inserción propia (sin tipos nativos de colección)
    private static BookModel[] OrdenarPorTitulo(BookModel[] libros)
    {
        BookModel[] copia = (BookModel[])libros.Clone();
        for (int i = 1; i < copia.Length; i++)
        {
            BookModel actual = copia[i];
            int j = i - 1;
            while (j >= 0 && string.Compare(copia[j].Titulo, actual.Titulo, StringComparison.OrdinalIgnoreCase) > 0)
            {
                copia[j + 1] = copia[j];
                j--;
            }
            copia[j + 1] = actual;
        }
        return copia;
    }

    private async void OnCargarClick(object? sender, RoutedEventArgs e)
    {
        OcultarOpciones();
        ActualizarSpritePorAccion("Sure");
        MostrarDialogo("Chihiro Fujisaki", "¿Desde qué archivo CSV deseas importar el inventario?");

        var dlg = new Dialogs.DialogCargar();
        await dlg.ShowDialog(this);

        if (dlg.RutaArchivo is not null)
        {
            var libros = CsvLoader.CargarDesdeCsv(dlg.RutaArchivo);
            foreach (var libro in libros)
            {
                _minHeap.Insertar(libro);
                _maxHeap.Insertar(libro);
                _arbolBPlus.insert(libro);
            }

            ActualizarSpritePorAccion("Happy");
            MostrarDialogo("Chihiro Fujisaki",
                $"Se han cargado exitosamente {libros.Length} libros desde: {System.IO.Path.GetFileName(dlg.RutaArchivo)}.");
        }
        else
        {
            ActualizarSpritePorAccion("Sure");
            MostrarDialogo("Chihiro Fujisaki", "Carga cancelada.");
        }
    }

    private void CargarInventarioInicial()
    {
        string[] posiblesRutas = [
            _rutaCsv,
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data", "book_packets.csv"),
            System.IO.Path.Combine("Data", "book_packets.csv"),
            System.IO.Path.Combine(AppContext.BaseDirectory, "UI", "Assets", "book_packets.csv"),
            System.IO.Path.Combine(Directory.GetCurrentDirectory(), "book_packets.csv")
        ];

        foreach (var p in posiblesRutas)
        {
            if (System.IO.File.Exists(p))
            {
                var libros = CsvLoader.CargarDesdeCsv(p);
                foreach (var libro in libros)
                {
                    _minHeap.Insertar(libro);
                    _maxHeap.Insertar(libro);
                    _arbolBPlus.insert(libro);
                }
                break;
            }
        }
    }

    private void GuardarInventario()
    {
        var elementos = _arbolBPlus.ObtenerTodos();
        CsvLoader.GuardarEnCsv(_rutaCsv, elementos);
        string rutaProyecto = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data", "book_packets.csv");
        CsvLoader.GuardarEnCsv(rutaProyecto, elementos);
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
        MostrarDialogo("Chihiro Fujisaki", "Hasta pronto. Los libros siempre te esperarán aquí...");
        GuardarInventario();
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await System.Threading.Tasks.Task.Delay(1800);
            Close();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        GuardarInventario();
        _mediaPlayer?.Dispose();
        _libVLC?.Dispose();
        base.OnClosed(e);
    }
}
