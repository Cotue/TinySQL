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
        public List<int> Keys;
        public List<BTreeNode> Children;

        public BTreeNode(int degree, bool isLeaf)
        {
            IsLeaf = isLeaf;
            Keys = new List<int>(degree - 1);
            Children = new List<BTreeNode>(degree);
        }
    }
}
