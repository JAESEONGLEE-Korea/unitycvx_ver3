using System;
using System.Collections.Generic;

namespace BlockAlign.Numeric
{
    public class RuyComparer: IEqualityComparer<RuyVector>
    {
        private double limit = 1.0E-8;
        public bool Equals(RuyVector a, RuyVector b)
        {
            bool result = false;
            if (Math.Abs(a.X - b.X) < limit && Math.Abs(a.Y - b.Y) < limit && Math.Abs(a.Z - b.Z) < limit)
                result = true;
            return result;
        }
        public int GetHashCode(RuyVector obj)
        {
            string s = string.Format("{0}{1}{2}",obj.X,obj.Y,obj.Z);
            return s.GetHashCode();
        }
    }
}
