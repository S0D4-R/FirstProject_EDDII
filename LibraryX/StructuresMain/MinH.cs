using System;
using LibraryX.Models;

namespace LibraryX.StructuresMain;

public class MinHeap
{
    private BookModel[] _heap;
    private int _capacidad;
    private int _tamano;

    public int Cantidad => _tamano;

    public MinHeap(int capacidad = 100)
    {
        _capacidad = capacidad;
        _tamano = 0;
        _heap = new BookModel[_capacidad];
    }

    public void Insertar(BookModel libro)
    {
        if (libro == null) return;

        if (_tamano == _capacidad)
        {
            Agrandar();
        }

        _heap[_tamano] = libro;
        Flotar(_tamano);
        _tamano++;
    }

    public BookModel? ExtraerMinimo()
    {
        if (_tamano == 0) return null;

        BookModel minimo = _heap[0];
        _heap[0] = _heap[_tamano - 1];
        _tamano--;
        Hundir(0);

        return minimo;
    }

    public BookModel? ObtenerMinimo()
    {
        if (_tamano == 0) return null;
        return _heap[0];
    }

    private void Flotar(int i)
    {
        while (i > 0)
        {
            int padre = (i - 1) / 2;
            if (_heap[i].CopiasDisponibles < _heap[padre].CopiasDisponibles)
            {
                Intercambiar(i, padre);
                i = padre;
            }
            else
            {
                break;
            }
        }
    }

    private void Hundir(int i)
    {
        while (true)
        {
            int izq = 2 * i + 1;
            int der = 2 * i + 2;
            int menor = i;

            if (izq < _tamano && _heap[izq].CopiasDisponibles < _heap[menor].CopiasDisponibles)
            {
                menor = izq;
            }

            if (der < _tamano && _heap[der].CopiasDisponibles < _heap[menor].CopiasDisponibles)
            {
                menor = der;
            }

            if (menor != i)
            {
                Intercambiar(i, menor);
                i = menor;
            }
            else
            {
                break;
            }
        }
    }

    private void Intercambiar(int i, int j)
    {
        var temp = _heap[i];
        _heap[i] = _heap[j];
        _heap[j] = temp;
    }

    private void Agrandar()
    {
        _capacidad *= 2;
        var nuevo = new BookModel[_capacidad];
        Array.Copy(_heap, nuevo, _tamano);
        _heap = nuevo;
    }

    public BookModel[] ObtenerElementos()
    {
        var resultado = new BookModel[_tamano];
        Array.Copy(_heap, resultado, _tamano);
        return resultado;
    }
}