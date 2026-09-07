# MANUAL TÉCNICO - PROYECTO 1: MYSTIC LIBRARY
**Asignatura:** Estructura de Datos II
**Lenguaje:** C# (.NET 10.0 / Avalonia UI)
**Nota:** Este manual reemplaza al anterior (`diagramas_manual_tecnico.md`) y refleja el estado **actual** del código: `BookShelf`/`BookNode`, `CodeGen`, `FechaPublicacion`, diálogos nuevos y guardado al cierre.

---

## 1. ARQUITECTURA GENERAL

App de escritorio estilo Visual Novel (Avalonia UI). Tres estructuras de datos propias viven **sincronizadas** en `MainWindow`; cada una resuelve una operación del sistema:

```
Program.cs ──> App (Avalonia) ──> MainWindow ──────► BookShelf (Árbol B+)  → índice por Código (búsqueda O(log m n))
              (arranca ventana)    (UI + controlador) ├─► MinHeap         → mínimas copias disponibles
                                                       └─► MaxHeap         → top libros más prestados
                                   EstructurasFeed:   CsvLoader (persistencia) · CodeGen (códigos) · Dialogs (UI)
```

Principios de diseño:
- Todo libro se inserta en **las tres estructuras** a la vez (Agregar, Cargar, Inventario inicial).
- Las tres guardan la **misma referencia** del objeto `BookModel`: mutar copias/préstamos se refleja en todas.
- Lógica de estructuras con **arreglos propios** (`BookModel[]`, `string[]`); sin `List<T>`/`Dictionary`/`SortedSet`/`PriorityQueue` en la lógica. (El único `List<string>` está dentro del parser CSV = soporte auxiliar de cadenas, permitido.)

---

## 2. DIAGRAMA DE CLASES (UML)

```mermaid
classDiagram
    namespace Models {
        class BookModel {
            +string Codigo
            +string Titulo
            +string Autor
            +string Categoria
            +string FechaPublicacion
            +int CopiasDisponibles
            +int VecesPrestado
            +BookModel(codigo, titulo, autor, categoria, fechaPublicacion, copias, vecesPrestado=0)
            +ToString() string
        }

        class CodeGen {
            +Generar(titulo, fechaPublicacion, codigosExistentes, diaRegistro=null) string
            +PrimerasDosLetras(texto) string
            +QuitarAcentos(texto) string
        }
    }

    namespace StructuresMain {
        class BookNode {
            +bool IsLeaf
            +string[] Keys
            +BookModel[] Vals
            +BookNode[] children_vals
            +BookNode Next
            +int KeyCounter
        }

        class BookShelf {
            -BookNode _root
            -int _mFactor
            +BookShelf(int m = 4)
            +insert(BookModel libro)
            +search(string codigo) BookModel
            +delete(string codigo) int
            +ObtenerTodos() BookModel[]
            -InsertNonFull(...)
            -SplitChild(...)
            -RepararSubMinimo(...)
        }

        class MinHeap {
            -BookModel[] _heap
            -int _capacidad
            -int _tamano
            +Cantidad int
            +Insertar(BookModel libro)
            +ExtraerMinimo() BookModel
            +ObtenerMinimo() BookModel
            +ObtenerElementos() BookModel[]
            -Flotar(int i)
            -Hundir(int i)
            -Intercambiar(int i, int j)
            -Agrandar()
        }

        class MaxHeap {
            -BookModel[] _heap
            -int _capacidad
            -int _tamano
            +Cantidad int
            +Insertar(BookModel libro)
            +ExtraerMaximo() BookModel
            +ObtenerMaximo() BookModel
            +ObtenerTop(int n) BookModel[]
            -Flotar(int i)
            -Hundir(int i)
            -Intercambiar(int i, int j)
            -Agrandar()
        }

        class CsvLoader {
            +CargarDesdeCsv(ruta) BookModel[]
            +CargarEnEstructuras(ruta, BookShelf arbol, MinHeap minH, MaxHeap maxH)
            +GuardarEnCsv(ruta, BookModel[] libros)
            -Escapar(campo) string
            -DividirCampos(linea) string[]
        }

        class SpriteNode {
            +string SpriteName
            +SpriteNode Next
        }

        class SpriteList {
            -SpriteNode _head
            -SpriteNode _tail
            -SpriteNode _current
            -int _count
            +Agregar(string sprite)
            +ObtenerPorIndice(int index) string
            +Siguiente() string
        }
    }

    namespace UI {
        class LibrarySpace {
            -SpriteList _sprites
            +ObtenerSpritePorAccion(string accion) string
            +ObtenerSiguienteSprite() string
        }

        class MainWindow {
            -LibVLC _libVLC
            -MediaPlayer _mediaPlayer
            -MinHeap _minHeap
            -MaxHeap _maxHeap
            -BookShelf _arbolBPlus
            -string _rutaCsv
            +InitializeComponent()
            +IniciarDialogo(speakers, lines, acciones)
            +MostrarDialogo(string speaker, string text)
            +CargarFondo(string ruta)
            +ActualizarSpritePorAccion(string accion)
            +InicializarAudio()
            +CargarAudio(string ruta)
            -OnBuscarClick()
            -OnAgregarClick()
            -OnListarClick()
            -OnPrestamoClick()
            -OnDevolverClick()
            -OnReporteClick()
            -OnCargarClick()   // handler definido (sin botón en la sidebar)
            -OnExitClick()
            -OnClosed()
            -CargarInventarioInicial()
            -GuardarInventario()
            -BuscarPorTitulo(libros, titulo) BookModel
            -OrdenarPorTitulo(libros) BookModel[]
        }
    }

    namespace UI.Dialogs {
        class DialogBuscar      { +string CodigoBuscado }
        class DialogAgregar     { +BookModel Resultado }
        class DialogPrestamo    { +string TituloLibro; +string NombreLector }
        class DialogDevolver    { +string TituloLibro }
        class DialogListado     { +BookModel[] dentro de ScrollViewer }
        class DialogCodigoGenerado
        class DialogCargar      { +string RutaArchivo }
    }

    BookShelf "1" *-- "many" BookNode : contiene
    BookNode "1" o-- "many" BookModel : almacena en hojas
    MinHeap "1" o-- "many" BookModel : almacena
    MaxHeap "1" o-- "many" BookModel : almacena
    CsvLoader ..> BookModel : crea y serializa
    CodeGen ..> BookModel : asigna Codigo
    LibrarySpace "1" *-- "1" SpriteList : gestiona
    SpriteList "1" *-- "many" SpriteNode : enlaza
    MainWindow "1" *-- "1" MinHeap : utiliza
    MainWindow "1" *-- "1" MaxHeap : utiliza
    MainWindow "1" *-- "1" BookShelf : utiliza
    MainWindow ..> CsvLoader : usa para persistencia
    MainWindow ..> CodeGen : generación de códigos
    MainWindow ..> UI.Dialogs : abre ventanas hijas
```

---

## 3. DIAGRAMA DE FLUJO DEL SISTEMA

```mermaid
flowchart TD
    Start([Inicio de la Aplicación]) --> InitAudio[Inicializar LibVLC\nVolumen 25%]
    InitAudio --> LoadBg[Cargar fondo library.jpg + sprite Chihiro]
    LoadBg --> AutoLoad[CargarInventarioInicial:\nbuscar Data/book_packets.csv]
    AutoLoad --> Exists{¿Existe el CSV?}
    Exists -- Sí --> ParseCSV[CsvLoader.CargarDesdeCsv\n(parser que respeta comillas)]
    ParseCSV --> Populate[Insertar en BookShelf, MinHeap y MaxHeap]
    Exists -- No --> Welcome[Diálogo de bienvenida con Chihiro]
    Populate --> Welcome

    Welcome --> Menu{Esperar acción\n(panel lateral)}

    Menu -- 📖 Buscar --> DialogSearch[Abrir DialogBuscar: pedir CÓDIGO]
    DialogSearch --> SearchExec[BookShelf.search(código)] 
    SearchExec --> ShowSearchResult[Resultado en caja de diálogo] --> Menu

    Menu -- ➕ Agregar --> DialogAdd[Abrir DialogAgregar\n(título, autor, categoría, fecha publicación)]
    DialogAdd --> CodeGenExec[CodeGen.Generar: B-XXYYdd-mmdd-001...]
    CodeGenExec --> InsertAll[insertar en BookShelf + MinHeap + MaxHeap]
    InsertAll --> ShowCode[DialogCodigoGenerado: mostrar código] --> Menu

    Menu -- 📋 Listar --> ListExec[BookShelf.ObtenerTodos:\nrecorrer hoja más izquierda + Next]
    ListExec --> ShowList[DialogListado con ScrollViewer] --> Menu

    Menu -- 📚 Préstamo --> DialogLoan[Abrir DialogPrestamo: pedir TÍTULO + lector]
    DialogLoan --> LoanExec[BuscarPorTitulo en ObtenerTodos\nCopiasDisponibles--, VecesPrestado++]
    LoanExec --> Menu

    Menu -- ↩️ Devolver --> DialogReturn[Abrir DialogDevolver: pedir TÍTULO]
    DialogReturn --> ReturnExec[BuscarPorTitulo\nCopiasDisponibles++]
    ReturnExec --> Menu

    Menu -- 📊 Reporte --> ReportExec[Catálogo por título (inserción propia)\n+ MaxHeap.ObtenerTop(5)]
    ReportExec --> ShowReport[Mostrar ambos en la caja de diálogo] --> Menu

    Menu -- 🚪 Salir / cerrar ventana --> SaveFinal[GuardarInventario:\nBookShelf.ObtenerTodos -> CsvLoader.GuardarEnCsv]
    SaveFinal --> DisposeAudio[Liberar LibVLC & MediaPlayer]
    DisposeAudio --> End([Fin del Programa])
```

---

## 4. DETALLE DE IMPLEMENTACIÓN DE LAS ESTRUCTURAS

### 4.1 Árbol B+ (`StructuresMain/ShelvesB+.cs`)
- **Orden** `_mFactor` (default 4): un nodo guarda a lo más `m-1` llaves; mínimo `⌈m/2⌉-1`. El constructor **redondea órdenes impares a pares** (con `m` impar la división del nodo lleno dejaría una mitad bajo el mínimo).
- **Estructura del nodo**: `Keys[]` (llaves = códigos), `Vals[]` (solo se usan en hojas: los `BookModel`), `children_vals[]` (hijos o siguiente nivel), `Next` (enlace hoja→hoja para recorrido ordenado).
- **`insert`** con estrategia **top-down / dividir antes de bajar**: garantiza que al llegar a la hoja quepa el nuevo elemento; si la raíz está llena, crea raíz nueva (`SplitChild`).
- **`SplitChild`**: hoja → la mitad superior pasa a nodo nuevo y enlaza `Next`; sube `newNode.Keys[0]` al padre. Interno → promueve la llave del medio.
- **`search`**: baja comparando el código por los internos; en la hoja hace búsqueda lineal. Búsqueda directa **O(log_m n)**.
- **`delete`**: borra de la hoja; si queda bajo el mínimo, rebalancea con **préstamo izquierda/derecha o fusión** y propaga la reparación al padre (código de retorno 0/1/2). Si la raíz queda vacía, se encoge al único hijo.
- **`ObtenerTodos`**: baja por `children_vals[0]` hasta la hoja más izquierda y recorre la cadena `Next` → catálogo ordenado por código **O(n)**.
- **Detalle clave**: los nodos internos NO guardan libros, solo llaves índice; los `BookModel` viven únicamente en las hojas.

### 4.2 MinHeap (`StructuresMain/MinH.cs`)
- Ordena por **`CopiasDisponibles`** (`<`). El mínimo (menos copias) queda en `_heap[0]`.
- `Insertar` → final del arreglo + `Flotar`. `ExtraerMinimo` → raíz + `Hundir`. `Agrandar` duplica el arreglo cuando se llena.

### 4.3 MaxHeap (`StructuresMain/MaxH.cs`)
- Ordena por **`VecesPrestado`** (`>`). El más prestado queda en la raíz.
- `ObtenerTop(n)` copia el heap y extrae `n` veces → reporte "Top libros más prestados". Nota: al mutar `VecesPrestado` en préstamo, el heap no se reordena; `ObtenerTop` reconstruye con la copia así que el reporte siempre lee los valores vigentes.

### 4.4 Generador de códigos (`Models/CodeGen.cs`)
Formato: `B-` + **2 primeras letras del título** (sin tildes) + **últimos 2 dígitos del año** + **dd-mm de publicación** + **dd de registro** + `-001/002/…` según colisiones.
Ejemplo: "El Carpintero", publicado 1901-01-16, registrado el día 06 → `B-EL0116-0106-001`.

### 4.5 Persistencia (`StructuresMain/CsvLoader.cs`)
- Elige las 3 rutas de `Codigo,Titulo,Autor,Categoria,CopiasDisponibles,VecesPrestado,FechaPublicacion`.
- `Escapar` entrecomilla campos con comas/comillas (comillas internas duplicadas); `DividirCampos` respeta entrecomillados al leer, para que un título con comas no rompa columnas.
- Guardado en `Data/book_packets.csv` al **salir o cerrar** la ventana (`OnExitClick` y `OnClosed`).

---

## 5. AUDITORÍA DE CUMPLIMIENTO CON EL PDF (PROYECTO 1)

| Requisito Técnico del PDF | Estado | Evidencia en el Código |
| :--- | :---: | :--- |
| **1. Estructuras Obligatorias** | 🟢 Cumplido | • **Árbol B+**: [`ShelvesB+.cs`](LibraryX/StructuresMain/ShelvesB+.cs) (clase `BookShelf`/`BookNode`)<br>• **Min Heap**: [`MinH.cs`](LibraryX/StructuresMain/MinH.cs) ordenado por `CopiasDisponibles`<br>• **Max Heap**: [`MaxH.cs`](LibraryX/StructuresMain/MaxH.cs) ordenado por `VecesPrestado` |
| **2. Operaciones Básicas** | 🟢 Cumplido en código · 🟡 Sin botón | • Insertar (`insert`) — Agregar/Listar manual y CSV<br>• Buscar por clave (`search`) — DialogBuscar<br>• Eliminar (`delete`) — **existe en el B+ pero no hay botón en la UI** (cambio en vivo probable)<br>• Recorrido ordenado (`ObtenerTodos`) — DialogListado<br>• Préstamos y devoluciones actualizando los nodos compartidos |
| **3. Datos Dinámicos** | 🟢 Cumplido | • Carga automática al arrancar (`CargarInventarioInicial`)<br>• Guardado automático al cerrar/salir (`GuardarInventario`)<br>• Registro manual interactivo (DialogAgregar + CodeGen) |
| **4. Menú Interactivo** | 🟢 Cumplido | Interfaz gráfica estilo Visual Novel (Avalonia UI) con panel lateral y Chihiro Fujisaki. Nota: hay un handler `OnCargarClick` definido pero **no enlazado** a ningún botón de la sidebar; la carga de CSV ocurre automáticamente al inicio |
| **5. Implementación Propia** *(sin colecciones prohibidas)* | 🟢 Cumplido | Sin `List<T>`/`Dictionary`/`SortedSet`/`PriorityQueue` en la lógica de estructuras: arreglos `BookModel[]`, `string[]` y algoritmos propios (`Flotar`, `Hundir`, `Intercambiar`, `Agrandar`, `SplitChild`, préstamo/fusión). Único `List<string>` = parser CSV (auxiliar de cadenas, permitido) |

---

## 6. COMPLEJIDADES (para defensor)

| Operación | Estructura | Complejidad |
| :--- | :--- | :--- |
| Buscar por código | BookShelf (B+) | O(log_m n) |
| Insertar / Eliminar | BookShelf (B+) | O(log_m n) |
| Recorrer catálogo completo | BookShelf `ObtenerTodos` | O(n) |
| Insertar / Extraer mínimo o máximo | MinHeap / MaxHeap | O(log n) |
| Top N más prestados | MaxHeap `ObtenerTop(n)` | O(n log n) |
| Ver mínimo de copias / máximo de préstamos | raíz del heap | O(1) |