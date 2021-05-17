using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class TreeNode<T>
    {
        public TreeNode<T> Parent { get; set; }
        public TreeNode<T> Child { get; set; }
        public TreeNode<T> Next { get; set; }
        public T Data { get; set; }

        public TreeNode()
        {
            Parent = null;
            Child = null;
            Next = null;
            Data = default(T);
        }
    }

    public delegate bool TraverseDelegate<T>(Object observer, T data);

    public class Tree<T>
    {
        public TreeNode<T> Root { get; set; }

        public Tree()
        {
            Root = null;
        }

        public TreeNode<T> AddNode(TreeNode<T> parent, T data)
        {
            TreeNode<T> node = new TreeNode<T>();
            node.Data = data;
            AddNode(parent, node);
            return node;
        }

        public void AddNode(TreeNode<T> parent, TreeNode<T> node)
        {
            if (parent == null)
            {
                // 根结点
                Root = node;
            }
            else
            {
                node.Parent = parent;

                if (parent.Child == null)
                {
                    // 没有任何子结点，直接插入子结点链表头
                    parent.Child = node;
                }
                else
                {
                    // 插入子结点链表尾
                    TreeNode<T> n = parent.Child;

                    while (n.Next != null)
                    {
                        n = n.Next;
                    }

                    n.Next = node;
                }
            }
        }

        public void RemoveNode(ref TreeNode<T> node)
        {
            TreeNode<T> parent = node.Parent;
            TreeNode<T> n = parent.Child;

            while (n.Next != node)
            {
                n = n.Next;
            }

            n.Next = node.Next;
            node = null;
        }

        public bool GetNode(T data, ref TreeNode<T> node)
        {
            bool found = false;
            TreeNode<T> n = null;

            if (node == null)
                n = Root;
            else
                n = node;

            while (n != null)
            {
                if (n.Data.Equals(data))
                {
                    node = n;
                    found = true;
                    break;
                }

                TreeNode<T> child = n.Child;

                while (child != null)
                {
                    if (GetNode(data, ref node))
                    {
                        found = true;
                        break;
                    }

                    child = child.Next;
                }

                if (found)
                {
                    break;
                }
            }

            return found;
        }

        public void Traverse(Object observer, TraverseDelegate<T> traverse)
        {
            TraverseInternal(Root, observer, traverse);
        }

        private bool TraverseInternal(TreeNode<T> node, Object observer, TraverseDelegate<T> traverse)
        {
            bool ret = false;

            if (traverse != null)
            {
                ret = traverse(observer, node.Data);
            }

            if (ret)
            {
                TreeNode<T> child = node.Child;
                while (child != null)
                {
                    ret = TraverseInternal(child, observer, traverse);

                    if (!ret)
                        break;

                    child = child.Next;
                }
            }

            return ret;
        }

        public void MoveNode(TreeNode<T> parent, TreeNode<T> node)
        {
            TreeNode<T> temp = node;
            RemoveNode(ref node);
            node = temp;
            AddNode(parent, node);
        }
    }
}
