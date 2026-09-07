Bienvenido al repositorio de **Mystic Library**, un sistema de gestión de biblioteca interactivo con una interfaz al estilo de novela visual. Este proyecto se encarga de administrar el inventario, préstamos y devoluciones utilizando estructuras de datos eficientes para garantizar un rendimiento óptimo.

A continuación se presenta la documentación visual de la arquitectura y el comportamiento de la aplicación:

## 1. Arquitectura del Sistema (Diagrama de Clases UML)

Este diagrama detalla la estructura orientada a objetos del código fuente. Muestra cómo se organizan las clases en los diferentes espacios de nombres (`Models`, `StructuresMain`, `UI`), sus atributos, métodos principales y las relaciones de dependencia, composición y agregación entre ellas (como la conexión entre la ventana principal, el Árbol B+ y los Heaps).

<img width="1021" height="556" alt="image" src="https://github.com/user-attachments/assets/cbcd8f04-85cf-4d30-8749-1b574725efaf" />

## 2. Lógica y Ciclo de Vida (Diagrama de Flujo)

Este diagrama ilustra el flujo de ejecución completo de la aplicación. Traza la ruta lógica desde la inicialización de la interfaz y la carga de datos del inventario (vía archivos CSV), pasando por el bucle principal de interacciones del usuario en la interfaz de novela visual (buscar, agregar, prestar, eliminar), hasta el proceso de serialización y cierre ordenado del programa.

<img width="1022" height="558" alt="image" src="https://github.com/user-attachments/assets/a68cff25-5f78-44e7-9769-d2f83374fdad" />


---

### Tecnologías y Estructuras Clave
* **Estructuras de Datos:** Árbol B+ (Buscador principal), MinHeap, MaxHeap.
* **Interfaz de Usuario:** Avalonia UI.
* **Multimedia:** Integración con LibVLC para el audio inmersivo.
* **Persistencia:** Almacenamiento local mediante lectura/escritura de archivos CSV.
Absolute Hope
<img width="1022" height="558" alt="image" src="[https://github.com/user-attachments/assets/a68cff25-5f78-44e7-9769-d2f83374fdad](https://tenor.com/view/danganronpa-dr1-chihiro-fujisaki-gif-7592711753197435046)" />
