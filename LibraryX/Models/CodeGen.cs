using System;
using System.Globalization;
using System.Text;

namespace LibraryX.Models;

public static class CodeGen
{
    // Formato de código generado:
    //   B-[primeras 2 letras del título][últimos 2 dígitos del año de publicación]
    //     [día-mes de publicación][día de registro]-[001, 002, ... según colisiones]
    //
    // Ejemplo: el libro "El Carpintero" publicado el 1901-01-16, registrado el día 06,
    //          genera el código: B-EL0116-0106-001
    public static string Generar(string titulo, string fechaPublicacion, string[] codigosExistentes, DateTime? diaRegistro = null)
    {
        DateTime registro = diaRegistro ?? DateTime.Today;

        string primerasDos = PrimerasDosLetras(titulo);

        DateTime publicacion = DateTime.TryParse(fechaPublicacion, CultureInfo.InvariantCulture, DateTimeStyles.None, out var pub)
            ? pub
            : registro;
        string anioPublicacion = (publicacion.Year % 100).ToString("00");
        string diaMesPublicacion = $"{publicacion.Day:00}-{publicacion.Month:00}";
        string diaRegistroStr = $"{registro.Day:00}";

        string baseCodigo = $"B-{primerasDos}{anioPublicacion}{diaMesPublicacion}{diaRegistroStr}";

        // El sufijo final depende de cuántos códigos con la misma base ya existan
        int colisiones = 0;
        if (codigosExistentes != null)
        {
            foreach (string codigo in codigosExistentes)
            {
                if (string.IsNullOrWhiteSpace(codigo)) continue;
                if (codigo.StartsWith(baseCodigo + "-", StringComparison.OrdinalIgnoreCase)) colisiones++;
            }
        }

        return $"{baseCodigo}-{(colisiones + 1):000}";
    }

    // Extrae las primeras dos letras del título, sin acentos y en mayúsculas.
    private static string PrimerasDosLetras(string titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo)) return "XX";

        string sinAcentos = QuitarAcentos(titulo.Trim()).ToUpperInvariant();
        var letras = new StringBuilder();
        for (int i = 0; i < sinAcentos.Length && letras.Length < 2; i++)
        {
            char c = sinAcentos[i];
            if (char.IsLetter(c)) letras.Append(c);
        }

        string resultado = letras.ToString();
        return resultado.Length >= 2 ? resultado : resultado.PadRight(2, 'X');
    }

    private static string QuitarAcentos(string texto)
    {
        string normalizado = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (char c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}