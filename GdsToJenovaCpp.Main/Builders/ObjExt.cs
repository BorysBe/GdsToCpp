﻿namespace GdsToJenovaCpp.Main.Builders
{
    public static class ObjExt
    {
        public static bool HasValue(this object obj)
        {
            return obj != null;
        }
    }
}
