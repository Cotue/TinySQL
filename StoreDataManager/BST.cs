using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataManager
{
    public class BST
    {
        private BSTNode _root;

        public BST()
        {
            _root = null;
        }

        // Insertar una clave en el árbol binario de búsqueda
        public void Insert(int key)
        {
            _root = InsertRec(_root, key);
        }

        private BSTNode InsertRec(BSTNode root, int key)
        {
            if (root == null)
            {
                root = new BSTNode(key);
                return root;
            }

            if (key < root.Key)
                root.Left = InsertRec(root.Left, key);
            else if (key > root.Key)
                root.Right = InsertRec(root.Right, key);

            return root;
        }

        // Buscar una clave en el árbol binario de búsqueda
        public bool Search(int key)
        {
            return SearchRec(_root, key);
        }

        private bool SearchRec(BSTNode root, int key)
        {
            if (root == null)
                return false;

            if (root.Key == key)
                return true;

            return key < root.Key ? SearchRec(root.Left, key) : SearchRec(root.Right, key);
        }
    }

}

