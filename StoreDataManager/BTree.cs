using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataManager
{
    public class BTree
    {
        private int _degree;
        private BTreeNode _root;

        public BTree(int degree)
        {
            _degree = degree;
            _root = null;
        }

        // Métodos para insertar y buscar en el árbol B
        public void Insert(int key)
        {
            // Implementación del método Insert
        }

        public bool Search(int key)
        {
            // Implementación del método Search
            return false;
        }
    }
}

