using System;
using System.IO;
using LibraryX.Models;

namespace LibraryX.StructuresMain;

public static class CsvLoader
{
    public static BookModel[] CargarDesdeCsv(string rutaArchivo)
    {
        if (string.IsNullOrWhiteSpace(rutaArchivo) || !File.Exists(rutaArchivo))
        {
            return Array.Empty<BookModel>();
        }

        string[] lineas = File.ReadAllLines(rutaArchivo);
        if (lineas.Length == 0) return Array.Empty<BookModel>();

        int inicio = 0;
        // Detectar si la primera línea es encabezado
        if (lineas[0].StartsWith("Codigo", StringComparison.OrdinalIgnoreCase) ||
            lineas[0].StartsWith("Código", StringComparison.OrdinalIgnoreCase))
        {
            inicio = 1;
        }

        int total = lineas.Length - inicio;
        if (total <= 0) return Array.Empty<BookModel>();

        var libros = new BookModel[total];
        int contador = 0;

        for (int i = inicio; i < lineas.Length; i++)
        {
            string linea = lineas[i].Trim();
            if (string.IsNullOrEmpty(linea)) continue;

            string[] campos = DividirCampos(linea);
            if (campos.Length >= 5)
            {
                string codigo = campos[0].Trim();
                string titulo = campos[1].Trim();
                string autor = campos[2].Trim();
                string categoria = campos[3].Trim();
                int copias = int.TryParse(campos[4].Trim(), out int c) ? c : 1;
                int prestado = (campos.Length >= 6 && int.TryParse(campos[5].Trim(), out int p)) ? p : 0;
                string fechaPub = (campos.Length >= 7) ? campos[6].Trim() : string.Empty;

                var libro = new BookModel(codigo, titulo, autor, categoria, fechaPub, copias, prestado);
                libros[contador++] = libro;
            }
        }

        if (contador < total)
        {
            var resultado = new BookModel[contador];
            Array.Copy(libros, resultado, contador);
            return resultado;
        }

        return libros;
    }

    public static void CargarEnEstructuras(string rutaArchivo, BookShelf? arbolBPlus, MinHeap? minHeap, MaxHeap? maxHeap)
    {
        var libros = CargarDesdeCsv(rutaArchivo);
        foreach (var libro in libros)
        {
            arbolBPlus?.insert(libro);
            minHeap?.Insertar(libro);
            maxHeap?.Insertar(libro);
        }
    }

    public static void GuardarEnCsv(string rutaArchivo, BookModel[] libros)
    {
        try
        {
            string? dir = Path.GetDirectoryName(rutaArchivo);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using var writer = new StreamWriter(rutaArchivo, false, System.Text.Encoding.UTF8);
            writer.WriteLine("Codigo,Titulo,Autor,Categoria,CopiasDisponibles,VecesPrestado,FechaPublicacion");
            foreach (var b in libros)
            {
                if (b != null)
                {
                    writer.WriteLine($"{Escapar(b.Codigo)},{Escapar(b.Titulo)},{Escapar(b.Autor)},{Escapar(b.Categoria)},{b.CopiasDisponibles},{b.VecesPrestado},{Escapar(b.FechaPublicacion)}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al guardar CSV: {ex.Message}");
        }
    }

    // Entrecomilla un campo si contiene comas, comillas o saltos de línea
    // (las comillas internas se duplican según el estándar CSV).
    private static string Escapar(string campo)
    {
        if (!campo.Contains(',') && !campo.Contains('"') && !campo.Contains('\n'))
        {
            return campo;
        }
        return "\"" + campo.Replace("\"", "\"\"") + "\"";
    }

    // Divide una línea respetando los campos entrecomillados, para que un título
    // con comas o comillas no rompa las columnas al volver a cargar el archivo.
    private static string[] DividirCampos(string linea)
    {
        var campos = new List<string>();
        var actual = new System.Text.StringBuilder();
        bool dentroDeComillas = false;

        for (int i = 0; i < linea.Length; i++)
        {
            char c = linea[i];

            if (c == '"')
            {
                if (dentroDeComillas && i + 1 < linea.Length && linea[i + 1] == '"')
                {
                    actual.Append('"');
                    i++;
                }
                else
                {
                    dentroDeComillas = !dentroDeComillas;
                }
            }
            else if (c == ',' && !dentroDeComillas)
            {
                campos.Add(actual.ToString());
                actual.Clear();
            }
            else
            {
                actual.Append(c);
            }
        }

        campos.Add(actual.ToString());
        return campos.ToArray();
    }
}
