using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public class PriorityQueue<T>
    {
        #region 成员
        private const int DEFAULT_CAPACITY = 16;
        private List<T> mItems;
        private Comparer<T> mComparer;
        #endregion

        #region 属性
        public int Count
        {
            get { return mItems.Count; }
        }
        public Comparer<T> Comparer
        {
            set { mComparer = value; }
        }
        #endregion

        #region 构造和析构
        public PriorityQueue()
            : this(DEFAULT_CAPACITY)
        {

        }

        public PriorityQueue(int capacity)
        {
            if (capacity < 0)
            {
                throw new IndexOutOfRangeException();
            }

            mItems = new List<T>(capacity);
            mComparer = Comparer<T>.Default;
        }

        ~PriorityQueue()
        {

        }
        #endregion

        #region 公有接口
        public RESULT Add(T value)
        {
            RESULT ret = RESULT.OK;

            mItems.Add(value);
            ShiftUp(mItems.Count-1);

            return ret;
        }

//         public RESULT Remove(T value)
//         {
//             RESULT ret = RESULT.OK;
// 
//             do
//             {
//                 if (mItems.Count == 0)
//                 {
//                     // Heap 空了
//                     break;
//                 }
// 
//                 int index = 0;
//                 // 获取值在堆中的索引
//                 for (int i = 0; i < mItems.Count; ++i)
//                 {
//                     if (mComparer.Compare(value, mItems[i]) < 0)
//                     {
//                         index = i;
//                         break;
//                     }
//                 }
// 
//                 if (index >= mItems.Count)
//                 {
//                     // 堆中没有该值
//                     break;
//                 }
// 
//                 // 使用最后一个结点来代替当前结点，然后再向下调整当前结点
//                 mItems[index] = mItems[mItems.Count-1];
//                 mItems
//                 ShiftDown(index, mItems.Count-1);
//                 mItems.RemoveAt(mItems.Count-1);
//             } while (false);
// 
//             return ret;
//         }

        public int RemoveAll(Predicate<T> match)
        {
            return mItems.RemoveAll(match);
        }

        public bool Remove(T obj)
        {
            return mItems.Remove(obj);
        }

        public T Find(Predicate<T> match)
        {
            return mItems.Find(match);
        }

        public void Clear()
        {
            mItems.Clear();
        }

        public bool IsEmpty()
        {
            return (mItems.Count == 0);
        }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)mItems.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                return mItems[index];
            }
        }

        public T Top()
        {
            return mItems[0];
        }

        public void Pop()
        {
            T value = mItems[0];
            mItems[0] = mItems[mItems.Count - 1];
            mItems.RemoveAt(mItems.Count - 1);
            if (mItems.Count > 0)
                ShiftDown(0);
        }
        #endregion

        #region 私有接口
        private void ShiftUp(int index)
        {
            // 插入的结点
            T value = mItems[index];

            while (index > 0)
            {
                // 如果没有达到根结点，继续调整
                // 获取父结点
                int parent = ((index - 1) >> 1);

                if (mComparer.Compare(value, mItems[parent]) < 0)
                {
                    // 子結點小於父結點，理論上是交換，這裡不用交換了，直接把父結點替換子結點，最後一把對應位置元素設置上插入結點的值就好了
                    mItems[index] = mItems[parent];
                    index = parent;
                }
                else
                {
                    break;
                }
            }

            // 插入最后的位置
            mItems[index] = value;
        }

        private void ShiftDown(int index)
        {
            T value = mItems[index];

            int left = (index << 1) + 1;

            while (left < mItems.Count)
            {
                // 找到子節點較小的那個
                int right = left + 1;
                int child = (right < mItems.Count && mComparer.Compare(mItems[right], mItems[left]) < 0) ? right : left;
                if (mComparer.Compare(mItems[child], mItems[index]) < 0)
                {
                    // 子結點小於父結點，這裡不用交換了，直接把子結點替換父結點，最後一把把堆頂元素設置上去對應的位置就好了
                    // 这里不交换就是 SB，上面还要判断大小，不交换，那么大小怎么判断？
                    T temp = mItems[index];
                    mItems[index] = mItems[child];
                    mItems[child] = temp;
                    index = child;
                    left = (index << 1) + 1;
                }
                else
                {
                    break;
                }
            }

            mItems[index] = value;
        }
        #endregion
    }
}
