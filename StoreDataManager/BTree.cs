using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataManager
{
    public class BTree
    {
        private int _degree;  // Grado del árbol B
        private BTreeNode _root;  // Raíz del árbol B

        public BTree(int degree)
        {
            _degree = degree;
            _root = null;
        }

        // Buscar una clave en el árbol B
        public bool Search(int key)
        {
            if (_root == null)
            {
                return false; // Si el árbol está vacío, retornar falso
            }
            else
            {
                return Search(_root, key); // Llamar al método auxiliar
            }
        }

        // Método auxiliar para la búsqueda
        private bool Search(BTreeNode node, int key)
        {
            int i = 0;

            // Encontrar la primera clave que sea mayor o igual que la clave buscada
            while (i < node.Keys.Count && key > node.Keys[i])
            {
                i++;
            }

            // Si la clave es igual a la clave actual, retornarla
            if (i < node.Keys.Count && node.Keys[i] == key)
            {
                return true;
            }

            // Si el nodo es una hoja, no encontramos la clave
            if (node.IsLeaf)
            {
                return false;
            }

            // Buscar en el hijo correspondiente
            return Search(node.Children[i], key);
        }

        // Insertar una clave en el árbol B
        public void Insert(int key)
        {
            // Si el árbol está vacío, crea un nuevo nodo raíz
            if (_root == null)
            {
                _root = new BTreeNode(_degree, true);
                _root.Keys.Add(key);
            }
            else
            {
                // Si la raíz está llena, hay que dividirla
                if (_root.Keys.Count == _degree - 1)
                {
                    BTreeNode newRoot = new BTreeNode(_degree, false);
                    newRoot.Children.Add(_root);
                    SplitChild(newRoot, 0, _root);
                    InsertNonFull(newRoot, key);
                    _root = newRoot;
                }
                else
                {
                    InsertNonFull(_root, key);
                }
            }
        }

        // Método auxiliar para insertar en un nodo que no está lleno
        private void InsertNonFull(BTreeNode node, int key)
        {
            int i = node.Keys.Count - 1;

            if (node.IsLeaf)
            {
                // Insertar la nueva clave en la posición correcta
                node.Keys.Add(0); // Reservar espacio
                while (i >= 0 && key < node.Keys[i])
                {
                    node.Keys[i + 1] = node.Keys[i];
                    i--;
                }
                node.Keys[i + 1] = key;
            }
            else
            {
                // Encontrar el hijo que debe recibir la nueva clave
                while (i >= 0 && key < node.Keys[i])
                {
                    i--;
                }
                i++;

                // Si el hijo está lleno, dividirlo
                if (node.Children[i].Keys.Count == _degree - 1)
                {
                    SplitChild(node, i, node.Children[i]);

                    if (key > node.Keys[i])
                    {
                        i++;
                    }
                }
                InsertNonFull(node.Children[i], key);
            }
        }

        // Dividir un hijo lleno de un nodo
        private void SplitChild(BTreeNode parent, int index, BTreeNode fullChild)
        {
            BTreeNode newNode = new BTreeNode(_degree, fullChild.IsLeaf);

            // Mover la mitad de las claves de fullChild a newNode
            for (int j = 0; j < _degree / 2 - 1; j++)
            {
                newNode.Keys.Add(fullChild.Keys[j + _degree / 2]);
            }

            if (!fullChild.IsLeaf)
            {
                for (int j = 0; j < _degree / 2; j++)
                {
                    newNode.Children.Add(fullChild.Children[j + _degree / 2]);
                }
            }

            // Reducir el número de claves en el nodo lleno
            fullChild.Keys.RemoveRange(_degree / 2 - 1, _degree / 2);

            parent.Children.Insert(index + 1, newNode);
            parent.Keys.Insert(index, fullChild.Keys[_degree / 2 - 1]);
            fullChild.Keys.RemoveAt(_degree / 2 - 1);
        }
    }

}

