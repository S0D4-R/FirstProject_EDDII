using System;
using LibraryX.Models;
//The shelves
namespace LibraryX.StructuresMain;

public class BookNode
{
    public bool IsLeaf { get; set; }
    public string[] Keys { get; set; }
    public BookModel[] Vals { get; set; }
    public BookNode[] children_vals { get; set; }
    public BookNode? Next { get; set; }
    public int KeyCounter { get; set; }

    public BookNode(int mFactor, bool isLeaf)
    {
        IsLeaf = isLeaf;
        Keys = new string[mFactor];
        Vals = new BookModel[mFactor];
        children_vals = new BookNode[mFactor + 1];
        Next = null;
        KeyCounter = 0;
    }
}

public class BookShelf
{
    private BookNode? _root;
    private readonly int _mFactor;

    // Cantidad mínima de llaves que un nodo (distinto de la raíz) debe conservar: ceil(m/2) - 1
    private int MinLlaves => (int)Math.Ceiling(_mFactor / 2.0) - 1;

    public BookShelf(int mFactor = 4)
    {
        // El orden del B+ debe ser par. Con inserción top-down (dividir antes de bajar),
        // un orden impar no permite partir un nodo lleno (m-1 llaves) en dos mitades que
        // cumplan el mínimo ceil(m/2)-1; por eso, si viene impar se redondea al siguiente par.
        _mFactor = Math.Max(3, mFactor);
        if (_mFactor % 2 != 0) _mFactor++;
        _root = new BookNode(_mFactor, isLeaf: true);
    }

    public BookModel? search(string code)
    {
        if (_root == null || string.IsNullOrWhiteSpace(code)) return null;

        BookNode curr = _root;
        while (!curr.IsLeaf)
        {
            int i = 0;
            while (i < curr.KeyCounter && string.Compare(code, curr.Keys[i], StringComparison.OrdinalIgnoreCase) >= 0)
            {
                i++;
            }
            curr = curr.children_vals[i];
        }

        for (int i = 0; i < curr.KeyCounter; i++)
        {
            if (string.Equals(curr.Keys[i], code, StringComparison.OrdinalIgnoreCase))
            {
                return curr.Vals[i];
            }
        }

        return null;
    }

    public void insert(BookModel book)
    {
        if (book == null || string.IsNullOrWhiteSpace(book.Codigo)) return;

        if (_root == null)
        {
            _root = new BookNode(_mFactor, isLeaf: true);
        }

        // Si ya existe un libro con la misma clave, actualizamos sus datos
        BookModel? existente = search(book.Codigo);
        if (existente != null)
        {
            existente.Titulo = book.Titulo;
            existente.Autor = book.Autor;
            existente.Categoria = book.Categoria;
            existente.CopiasDisponibles = book.CopiasDisponibles;
            existente.VecesPrestado = book.VecesPrestado;
            return;
        }

        if (_root.KeyCounter == _mFactor - 1)
        {
            BookNode newRoot = new BookNode(_mFactor, isLeaf: false);
            newRoot.children_vals[0] = _root;
            SplitChild(newRoot, 0, _root);
            _root = newRoot;
        }

        InsertNonFull(_root, book);
    }

    private void InsertNonFull(BookNode node, BookModel book)
    {
        int i = node.KeyCounter - 1;

        if (node.IsLeaf)
        {
            while (i >= 0 && string.Compare(book.Codigo, node.Keys[i], StringComparison.OrdinalIgnoreCase) < 0)
            {
                node.Keys[i + 1] = node.Keys[i];
                node.Vals[i + 1] = node.Vals[i];
                i--;
            }

            node.Keys[i + 1] = book.Codigo;
            node.Vals[i + 1] = book;
            node.KeyCounter++;
        }
        else
        {
            while (i >= 0 && string.Compare(book.Codigo, node.Keys[i], StringComparison.OrdinalIgnoreCase) < 0)
            {
                i--;
            }
            i++;

            if (node.children_vals[i].KeyCounter == _mFactor - 1)
            {
                SplitChild(node, i, node.children_vals[i]);
                if (string.Compare(book.Codigo, node.Keys[i], StringComparison.OrdinalIgnoreCase) > 0)
                {
                    i++;
                }
            }

            InsertNonFull(node.children_vals[i], book);
        }
    }

    private void SplitChild(BookNode parent, int i, BookNode child)
    {
        int mid = child.KeyCounter / 2;
        BookNode newNode = new BookNode(_mFactor, child.IsLeaf);

        if (child.IsLeaf)
        {
            int numKeysMove = child.KeyCounter - mid;
            for (int j = 0; j < numKeysMove; j++)
            {
                newNode.Keys[j] = child.Keys[mid + j];
                newNode.Vals[j] = child.Vals[mid + j];
                child.Keys[mid + j] = string.Empty;
                child.Vals[mid + j] = null!;
            }
            newNode.KeyCounter = numKeysMove;
            child.KeyCounter = mid;

            newNode.Next = child.Next;
            child.Next = newNode;

            for (int j = parent.KeyCounter; j >= i + 1; j--)
            {
                parent.children_vals[j + 1] = parent.children_vals[j];
            }
            parent.children_vals[i + 1] = newNode;

            for (int j = parent.KeyCounter - 1; j >= i; j--)
            {
                parent.Keys[j + 1] = parent.Keys[j];
            }
            parent.Keys[i] = newNode.Keys[0];
            parent.KeyCounter++;
        }
        else
        {
            string promoKey = child.Keys[mid];
            int numKeysMove = child.KeyCounter - mid - 1;

            for (int j = 0; j < numKeysMove; j++)
            {
                newNode.Keys[j] = child.Keys[mid + 1 + j];
                child.Keys[mid + 1 + j] = string.Empty;
            }

            for (int j = 0; j <= numKeysMove; j++)
            {
                newNode.children_vals[j] = child.children_vals[mid + 1 + j];
                child.children_vals[mid + 1 + j] = null!;
            }

            newNode.KeyCounter = numKeysMove;
            child.Keys[mid] = string.Empty;
            child.KeyCounter = mid;

            for (int j = parent.KeyCounter; j >= i + 1; j--)
            {
                parent.children_vals[j + 1] = parent.children_vals[j];
            }
            parent.children_vals[i + 1] = newNode;

            for (int j = parent.KeyCounter - 1; j >= i; j--)
            {
                parent.Keys[j + 1] = parent.Keys[j];
            }
            parent.Keys[i] = promoKey;
            parent.KeyCounter++;
        }
    }

    public bool delete(string code)
    {
        if (_root == null || string.IsNullOrWhiteSpace(code)) return false;

        // Caso especial: el árbol es una única hoja (la raíz)
        if (_root.IsLeaf)
        {
            int idx = BuscarIndice(_root, code);
            if (idx == -1) return false;
            RemoverDeHoja(_root, idx);
            return true;
        }

        // 0 = no encontrado, 1 = eliminado OK, 2 = eliminado y el nodo quedó bajo el mínimo
        int resultado = EliminarRecursivo(_root, code);
        if (resultado == 0) return false;

        // Si la raíz quedó sin llaves, el árbol se encoge hacia su único hijo
        if (_root.KeyCounter == 0 && _root.children_vals[0] != null)
        {
            _root = _root.children_vals[0];
        }

        return true;
    }

    // Devuelve la posición de la llave dentro de la hoja, o -1 si no existe
    private int BuscarIndice(BookNode hoja, string code)
    {
        for (int i = 0; i < hoja.KeyCounter; i++)
        {
            if (string.Equals(hoja.Keys[i], code, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    // Corrimiento a la izquierda para sacar la llave de la hoja y llevar el contador a su valor
    private void RemoverDeHoja(BookNode hoja, int idx)
    {
        for (int j = idx; j < hoja.KeyCounter - 1; j++)
        {
            hoja.Keys[j] = hoja.Keys[j + 1];
            hoja.Vals[j] = hoja.Vals[j + 1];
        }
        hoja.Keys[hoja.KeyCounter - 1] = string.Empty;
        hoja.Vals[hoja.KeyCounter - 1] = null!;
        hoja.KeyCounter--;
    }

    // Recorre el árbol buscando la llave y rebalancea de abajo hacia arriba.
    // Retorna: 0 = no encontrado, 1 = eliminado sin reparación, 2 = eliminado y nodo bajo el mínimo.
    private int EliminarRecursivo(BookNode nodo, string code)
    {
        if (nodo.IsLeaf)
        {
            int idx = BuscarIndice(nodo, code);
            if (idx == -1) return 0;

            RemoverDeHoja(nodo, idx);
            return nodo.KeyCounter < MinLlaves ? 2 : 1;
        }

        // Descender por el hijo donde debería vivir la llave
        int i = 0;
        while (i < nodo.KeyCounter && string.Compare(code, nodo.Keys[i], StringComparison.OrdinalIgnoreCase) >= 0)
        {
            i++;
        }

        BookNode hijo = nodo.children_vals[i];
        int resultado = EliminarRecursivo(hijo, code);
        if (resultado == 0) return 0;

        if (resultado == 2)
        {
            // El hijo quedó bajo el mínimo: tomar prestado de un hermano o fusionar
            RepararSubMinimo(nodo, i);
        }
        else if (i > 0 && hijo.IsLeaf && hijo.KeyCounter > 0)
        {
            // La clave índice del padre debe seguir siendo el mínimo de la hoja derecha
            nodo.Keys[i - 1] = hijo.Keys[0];
        }

        // Si este nodo también quedó bajo el mínimo, el abuelo lo reparará
        return nodo.KeyCounter < MinLlaves ? 2 : 1;
    }

    // Repara un hijo que quedó con menos llaves que el mínimo.
    // Estrategia: préstamo del hermano izquierdo, luego del derecho; si no hay, fusión.
    private void RepararSubMinimo(BookNode padre, int i)
    {
        // Préstamo desde el hermano izquierdo (si este tiene llaves de sobra)
        if (i > 0 && padre.children_vals[i - 1].KeyCounter > MinLlaves)
        {
            PrestarDeIzquierda(padre, i);
            return;
        }

        // Préstamo desde el hermano derecho
        if (i < padre.KeyCounter && padre.children_vals[i + 1].KeyCounter > MinLlaves)
        {
            PrestarDeDerecha(padre, i);
            return;
        }

        // Ningún hermano puede prestar: fusionar con el izquierdo, o con el derecho si no lo hay
        if (i > 0)
        {
            FusionarConIzquierda(padre, i);
        }
        else
        {
            FusionarConDerecha(padre, i);
        }
    }

    // Mueve la última llave del hermano izquierdo al inicio del hijo (hojas o internos).
    private void PrestarDeIzquierda(BookNode padre, int i)
    {
        BookNode izquierdo = padre.children_vals[i - 1];
        BookNode hijo = padre.children_vals[i];

        if (hijo.IsLeaf)
        {
            // Desplazar las llaves del hijo para abrir espacio al inicio
            for (int j = hijo.KeyCounter; j > 0; j--)
            {
                hijo.Keys[j] = hijo.Keys[j - 1];
                hijo.Vals[j] = hijo.Vals[j - 1];
            }
            hijo.Keys[0] = izquierdo.Keys[izquierdo.KeyCounter - 1];
            hijo.Vals[0] = izquierdo.Vals[izquierdo.KeyCounter - 1];

            izquierdo.Keys[izquierdo.KeyCounter - 1] = string.Empty;
            izquierdo.Vals[izquierdo.KeyCounter - 1] = null!;
            izquierdo.KeyCounter--;
            hijo.KeyCounter++;

            // El nuevo mínimo del hijo sube como índice del padre
            padre.Keys[i - 1] = hijo.Keys[0];
        }
        else
        {
            // La llave separadora baja al hijo; la última llave e hijo del izquierdo suben
            for (int j = hijo.KeyCounter; j > 0; j--)
            {
                hijo.Keys[j] = hijo.Keys[j - 1];
            }
            for (int j = hijo.KeyCounter + 1; j > 0; j--)
            {
                hijo.children_vals[j] = hijo.children_vals[j - 1];
            }

            hijo.Keys[0] = padre.Keys[i - 1];
            hijo.children_vals[0] = izquierdo.children_vals[izquierdo.KeyCounter];

            padre.Keys[i - 1] = izquierdo.Keys[izquierdo.KeyCounter - 1];
            izquierdo.Keys[izquierdo.KeyCounter - 1] = string.Empty;
            izquierdo.children_vals[izquierdo.KeyCounter] = null!;
            izquierdo.KeyCounter--;
            hijo.KeyCounter++;
        }
    }

    // Mueve la primera llave del hermano derecho al final del hijo.
    private void PrestarDeDerecha(BookNode padre, int i)
    {
        BookNode derecho = padre.children_vals[i + 1];
        BookNode hijo = padre.children_vals[i];

        if (hijo.IsLeaf)
        {
            hijo.Keys[hijo.KeyCounter] = derecho.Keys[0];
            hijo.Vals[hijo.KeyCounter] = derecho.Vals[0];
            hijo.KeyCounter++;

            for (int j = 0; j < derecho.KeyCounter - 1; j++)
            {
                derecho.Keys[j] = derecho.Keys[j + 1];
                derecho.Vals[j] = derecho.Vals[j + 1];
            }
            derecho.Keys[derecho.KeyCounter - 1] = string.Empty;
            derecho.Vals[derecho.KeyCounter - 1] = null!;
            derecho.KeyCounter--;

            padre.Keys[i] = derecho.Keys[0];
        }
        else
        {
            // La separadora baja al final del hijo; la primera llave del derecho sube
            hijo.Keys[hijo.KeyCounter] = padre.Keys[i];
            hijo.children_vals[hijo.KeyCounter + 1] = derecho.children_vals[0];
            hijo.KeyCounter++;

            padre.Keys[i] = derecho.Keys[0];

            for (int j = 0; j < derecho.KeyCounter - 1; j++)
            {
                derecho.Keys[j] = derecho.Keys[j + 1];
            }
            derecho.Keys[derecho.KeyCounter - 1] = string.Empty;

            for (int j = 0; j < derecho.KeyCounter; j++)
            {
                derecho.children_vals[j] = derecho.children_vals[j + 1];
            }
            derecho.children_vals[derecho.KeyCounter] = null!;
            derecho.KeyCounter--;
        }
    }

    // Fusiona el hijo (índice i) con su hermano izquierdo y elimina la llave separadora del padre.
    private void FusionarConIzquierda(BookNode padre, int i)
    {
        BookNode izquierdo = padre.children_vals[i - 1];
        BookNode hijo = padre.children_vals[i];

        if (hijo.IsLeaf)
        {
            for (int j = 0; j < hijo.KeyCounter; j++)
            {
                izquierdo.Keys[izquierdo.KeyCounter + j] = hijo.Keys[j];
                izquierdo.Vals[izquierdo.KeyCounter + j] = hijo.Vals[j];
            }
            izquierdo.KeyCounter += hijo.KeyCounter;
            izquierdo.Next = hijo.Next;

            // Quitar al hijo de la lista de hijos del padre y su llave separadora
            for (int j = i; j < padre.KeyCounter; j++)
            {
                padre.children_vals[j] = padre.children_vals[j + 1];
            }
            padre.children_vals[padre.KeyCounter] = null!;

            for (int j = i - 1; j < padre.KeyCounter - 1; j++)
            {
                padre.Keys[j] = padre.Keys[j + 1];
            }
            padre.Keys[padre.KeyCounter - 1] = string.Empty;
            padre.KeyCounter--;
        }
        else
        {
            // El izquierdo recupera la separadora y después las llaves e hijos del nodo fusionado
            izquierdo.Keys[izquierdo.KeyCounter] = padre.Keys[i - 1];
            izquierdo.KeyCounter++;

            for (int j = 0; j < hijo.KeyCounter; j++)
            {
                izquierdo.Keys[izquierdo.KeyCounter + j] = hijo.Keys[j];
            }
            for (int j = 0; j <= hijo.KeyCounter; j++)
            {
                izquierdo.children_vals[izquierdo.KeyCounter + j] = hijo.children_vals[j];
            }
            izquierdo.KeyCounter += hijo.KeyCounter;

            // Quitar al hijo de la lista de hijos del padre y su llave separadora
            for (int j = i; j < padre.KeyCounter; j++)
            {
                padre.children_vals[j] = padre.children_vals[j + 1];
            }
            padre.children_vals[padre.KeyCounter] = null!;

            for (int j = i - 1; j < padre.KeyCounter - 1; j++)
            {
                padre.Keys[j] = padre.Keys[j + 1];
            }
            padre.Keys[padre.KeyCounter - 1] = string.Empty;
            padre.KeyCounter--;
        }
    }

    // Fusiona el hijo (índice i) con su hermano derecho; el hijo izquierdo es el que sobrevive.
    private void FusionarConDerecha(BookNode padre, int i)
    {
        BookNode hijo = padre.children_vals[i];
        BookNode derecho = padre.children_vals[i + 1];

        if (hijo.IsLeaf)
        {
            for (int j = 0; j < derecho.KeyCounter; j++)
            {
                hijo.Keys[hijo.KeyCounter + j] = derecho.Keys[j];
                hijo.Vals[hijo.KeyCounter + j] = derecho.Vals[j];
            }
            hijo.KeyCounter += derecho.KeyCounter;
            hijo.Next = derecho.Next;

            // Quitar al derecho: eliminar su llave separadora y su puntero de hijo
            for (int j = i; j < padre.KeyCounter - 1; j++)
            {
                padre.Keys[j] = padre.Keys[j + 1];
            }
            padre.Keys[padre.KeyCounter - 1] = string.Empty;

            for (int j = i + 2; j <= padre.KeyCounter; j++)
            {
                padre.children_vals[j - 1] = padre.children_vals[j];
            }
            padre.children_vals[padre.KeyCounter] = null!;
            padre.KeyCounter--;
        }
        else
        {
            // El hijo izquierdo recupera la separadora y después las llaves e hijos del derecho
            hijo.Keys[hijo.KeyCounter] = padre.Keys[i];
            hijo.KeyCounter++;

            for (int j = 0; j < derecho.KeyCounter; j++)
            {
                hijo.Keys[hijo.KeyCounter + j] = derecho.Keys[j];
            }
            for (int j = 0; j <= derecho.KeyCounter; j++)
            {
                hijo.children_vals[hijo.KeyCounter + j] = derecho.children_vals[j];
            }
            hijo.KeyCounter += derecho.KeyCounter;

            // Quitar al derecho: eliminar su llave separadora y su puntero de hijo
            for (int j = i; j < padre.KeyCounter - 1; j++)
            {
                padre.Keys[j] = padre.Keys[j + 1];
            }
            padre.Keys[padre.KeyCounter - 1] = string.Empty;

            for (int j = i + 2; j <= padre.KeyCounter; j++)
            {
                padre.children_vals[j - 1] = padre.children_vals[j];
            }
            padre.children_vals[padre.KeyCounter] = null!;
            padre.KeyCounter--;
        }
    }

    public BookModel[] ObtenerTodos()
    {
        if (_root == null) return Array.Empty<BookModel>();

        BookNode curr = _root;
        while (!curr.IsLeaf)
        {
            curr = curr.children_vals[0];
        }

        // Contar total de elementos en las hojas
        int total = 0;
        BookNode? temp = curr;
        while (temp != null)
        {
            total += temp.KeyCounter;
            temp = temp.Next;
        }

        var resultado = new BookModel[total];
        int idx = 0;
        temp = curr;
        while (temp != null)
        {
            for (int i = 0; i < temp.KeyCounter; i++)
            {
                if (temp.Vals[i] != null)
                {
                    resultado[idx++] = temp.Vals[i];
                }
            }
            temp = temp.Next;
        }

        if (idx < total)
        {
            var ajustado = new BookModel[idx];
            Array.Copy(resultado, ajustado, idx);
            return ajustado;
        }

        return resultado;
    }
}