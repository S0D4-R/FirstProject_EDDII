using System;
using LibraryX.Models;

namespace LibraryX.StructuresMain;

public class MaxHeap
{
    private BookModel[] _heap;
    private int _capacidad;
    private int _tamano;

    public int Cantidad => _tamano;

    public MaxHeap(int capacidad = 100)
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

    public BookModel? ExtraerMaximo()
    {
        if (_tamano == 0) return null;

        BookModel maximo = _heap[0];
        _heap[0] = _heap[_tamano - 1];
        _tamano--;
        Hundir(0);

        return maximo;
    }

    public BookModel? ObtenerMaximo()
    {
        if (_tamano == 0) return null;
        return _heap[0];
    }

    private void Flotar(int i)
    {
        while (i > 0)
        {
            int padre = (i - 1) / 2;
            if (_heap[i].VecesPrestado > _heap[padre].VecesPrestado)
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
            int mayor = i;

            if (izq < _tamano && _heap[izq].VecesPrestado > _heap[mayor].VecesPrestado)
            {
                mayor = izq;
            }

            if (der < _tamano && _heap[der].VecesPrestado > _heap[mayor].VecesPrestado)
            {
                mayor = der;
            }

            if (mayor != i)
            {
                Intercambiar(i, mayor);
                i = mayor;
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

    public BookModel[] ObtenerTop(int n)
    {
        int limite = Math.Min(n, _tamano);
        var copiaHeap = new MaxHeap(_capacidad);
        for (int i = 0; i < _tamano; i++)
        {
            copiaHeap.Insertar(_heap[i]);
        }

        var resultado = new BookModel[limite];
        for (int i = 0; i < limite; i++)
        {
            var max = copiaHeap.ExtraerMaximo();
            if (max != null) resultado[i] = max;
        }

        return resultado;
    }
}