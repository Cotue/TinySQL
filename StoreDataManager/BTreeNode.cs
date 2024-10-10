using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataManager
{
    public class BTreeNode
    {
        public bool IsLeaf;
        public List<int> Keys;        // Las claves en el nodo
        public List<BTreeNode> Children; // Los hijos del nodo

        public BTreeNode(int degree, bool isLeaf)
        {
            IsLeaf = isLeaf;
            Keys = new List<int>(degree - 1); // Máximo número de claves en un nodo (degree - 1)
            Children = new List<BTreeNode>(degree); // Máximo número de hijos (degree)
        }
    }

}
