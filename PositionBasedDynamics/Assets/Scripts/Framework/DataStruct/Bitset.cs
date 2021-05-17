

namespace Framework
{
    public static class Bitset
    {
        public static void SetBit(this ref int value, int bit, bool bitValue)
        {
            if (bit >= 0 && bit < 32)
            {
                if (bitValue)
                {
                    // 设置位
                    value |= (1 << bit);
                }
                else
                {
                    // 清除位
                    value &= ~(1 << bit);
                }
            }
        }

        public static bool HasBit(this int value, int bit)
        {
            bool ret = false;

            if (bit >= 0 && bit < 32)
            {
                int val = (1 << bit);
                ret = ((value & val) == val);
            }

            return ret;
        }

        public static bool HasValue(this int value, int test)
        {
            return (value & test) == test;
        }

        public static int BitCount(this int value)
        {
            int count = 0;

            for (int i = 0; i < 31; i++)
            {
                int val = (1 << i);
                if ((value & val) == val)
                {
                    count++;
                }
            }

            return count;
        }

        public static void SetBit(this ref long value, int bit, bool bitValue)
        {
            if (bit >= 0 && bit < 64)
            {
                const long one = 1;
                if (bitValue)
                {
                    // 设置位
                    value |= (one << bit);
                }
                else
                {
                    // 清除位
                    value &= ~(one << bit);
                }
            }
        }

        public static bool HasBit(this long value, int bit)
        {
            bool ret = false;

            if (bit >= 0 && bit < 64)
            {
                const long one = 1;
                long val = (one << bit);
                ret = ((value & val) == val);
            }

            return ret;
        }

        public static bool HasValue(this long value, long test)
        {
            return (value & test) == test;
        }

        public static int BitCount(this long value)
        {
            int count = 0;

            for (int i = 0; i < 31; i++)
            {
                int val = (1 << i);
                if ((value & val) == val)
                {
                    count++;
                }
            }

            return count;
        }
    }
}

