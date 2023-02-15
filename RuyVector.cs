using System;

namespace BlockAlign.Numeric
{
    public class RuyVector
    {
        double mX, mY, mZ;
        public double X { get { return mX; } }
        public double Y { get { return mY; } }
        public double Z { get { return mZ; } }

        public double getValue(int index)
        {
            double result = 0.0;
            switch (index)
            {
                case 0:
                    result = mX; break;
                case 1:
                    result = mY; break;
                case 2:
                    result = mZ; break;
                default:
                    break;
            }
            return result;
        }

        public RuyVector(double x, double y, double z)
        {
            mX = x; mY = y; mZ = z;
        }
        public RuyVector(double[] vec)
        {
            try
            {
                mX = vec[0]; mY = vec[1]; mZ = vec[2];
            }
            catch
            {
                throw new ArgumentOutOfRangeException("index parameter is out of range. @RuyVector(double[])");
            }
        }

        public void Translate(double dx, double dy, double dz)
        {
            mX += dx; mY += dy; mZ += dz;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="angle"></param> angle with radian
        /// <param name="u"></param> direction vector
        /// <param name="v"></param>
        /// <param name="w"></param>
        /// <param name="x"></param> go throught point 
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void RotationAround(double angle, double u, double v, double w, double a, double b, double c)
        {
            double L = (u * u + v * v + w * w);
            double u2 = u * u;
            double v2 = v * v;
            double w2 = w * w;

            if (Math.Abs(L) < 1.0E-10) throw new Exception();

            double xx, yy, zz;
            xx = ((a * (v * v + w * w) - u * (b * v + c * w - u * mX - v * mY - w * mZ)) * (1 - Math.Cos(angle)) + L * mX * Math.Cos(angle) + Math.Sqrt(L) * (-c * v + b * w - w * mY + v * mZ) * Math.Sin(angle)) / L;
            yy = ((b * (u * u + w * w) - v * (a * u + c * w - u * mX - v * mY - w * mZ)) * (1 - Math.Cos(angle)) + L * mY * Math.Cos(angle) + Math.Sqrt(L) * (c * u - a * w + w * mX - u * mZ) * Math.Sin(angle)) / L;
            zz = ((c * (u * u + v * v) - w * (a * u + b * v - u * mX - v * mY - w * mZ)) * (1 - Math.Cos(angle)) + L * mZ * Math.Cos(angle) + Math.Sqrt(L) * (-b * u + a * v - v * mX + u * mY) * Math.Sin(angle)) / L;

            mX = xx;
            mY = yy;
            mZ = zz;
        }
        public static double Inner(RuyVector a, RuyVector b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public double GetMagnitude()
        {
            return Math.Sqrt(Inner(this, this));
        }

        public RuyVector normalize()
        {
            double length = GetMagnitude();
            return new RuyVector(X / length, Y / length, Z / length);
        }
        public static double getAngle(RuyVector a, RuyVector b)
        {
            double value = Inner(a, b) / a.GetMagnitude() / b.GetMagnitude();
            if (value > 1.0) value = 1.0;
            else if (value < -1.0) value = -1.0;

            if (IsRight(a, b)) return 2 * Math.PI - Math.Acos(value);
            else return Math.Acos(value);
        }
        public static bool IsRight(RuyVector a, RuyVector b)
        {
            return ((a.X * b.Y - a.Y * b.X) / (Math.Abs(a.X) + Math.Abs(a.Y))) <= 0; //동일선에 있거나 오른쪽이면 true
        }
        public static double DistanceBtwTwoPoints(RuyVector a, RuyVector b)
        {
            RuyVector temp = a - b;
            return temp.GetMagnitude();
        }
        public static RuyVector operator-(RuyVector a, RuyVector b)
        {
            RuyVector result = new RuyVector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            return result;
        }

        public static RuyVector GetCenterPoint(RuyVector a, RuyVector b)
        {
            RuyVector result = new RuyVector((a.X + b.X)/2.0, (a.Y + b.Y) / 2.0, (a.Z + b.Z) / 2.0);
            return result;
        }

        public static RuyVector operator +(RuyVector a, RuyVector b)
        {
            RuyVector result = new RuyVector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            return result;
        }

        public static RuyVector operator *(RuyVector a, RuyVector b)
        {
            double[] c = Accord.Math.Matrix.Cross(a.ToArray(), b.ToArray());

            return new RuyVector(c);
        }

        public static RuyVector operator *(double a, RuyVector b)
        {
            double[] c = new double[3] { a * b.X, a * b.Y, a * b.Z };

            return new RuyVector(c);
        }
        public double[] ToArray()
        {
            return new double[3] { mX, mY, mZ };
        }
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", mX, mY, mZ);
        }
    }
}
