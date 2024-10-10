using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataManager
{
    public class BSTNode
    {
        public int Key;
        public BSTNode Left;
        public BSTNode Right;

        public BSTNode(int key)
        {
            Key = key;
            Left = null;
            Right = null;
        }
    }
}
