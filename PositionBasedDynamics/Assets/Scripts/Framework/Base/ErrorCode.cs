using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public enum RESULT
    {
        OK = 0,
        FAIL = -1,
        ITEM_EXIST = 1,
        ITEM_NOT_FOUND = 2,
        OUT_OF_BOUND = 3,
        INVALID_PARAM = 4,
        CREATE_OBJECT = 5,
        FILE_NOT_FOUND = 6,
        HAS_NO_PERMISSION = 7,
    }
}
