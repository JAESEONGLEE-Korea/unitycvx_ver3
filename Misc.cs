using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices; //for using inline

namespace NFP
{
    public class M //Misc. Functions - 독립적인 코드만 작성할 것(System이외 다른 제작된 코드들을 Ref.하지 않도록)
    {
        public static int GetQuadrant(double Xref, double Yref, double X, double Y)
        {
            if(Yref <= Y)
            {
                if( Xref <= X) return 1;
                else           return 2;
            }
            else
            {
                if( Xref > X) return 3;
                else          return 4;
            }
        }
        public const double PI = 3.1415926535897932;
        public const double PIhalf = PI/2.0;
        public static void print(in string str){ Console.WriteLine(str);}
        public static double Rad2Deg(in double radian) { return radian * 57.2957795130823; }
        public static double Deg2Rad(in double degree) { return degree * 0.0174532925199433; }
        public static bool IsZero(in double value) { return ((-0.0000001 < value) && (value < 0.0000001)); }
        public static void PrintTime(in Stopwatch T, in string msg = "", in bool IsRestart = true)
        {
            string ElapsedTimeStr = GetElapsedTimeStr(T, IsRestart);

            if (msg != "") Console.WriteLine(msg + ":" + ElapsedTimeStr);
            else Console.WriteLine("RunTime:" + ElapsedTimeStr);
            if (IsRestart) T.Restart();
        }
        public static double GetElapsedTimeDouble(in Stopwatch T, in bool IsRestart = true)
        {
            return T.Elapsed.TotalMilliseconds;
        }
        public static string GetElapsedTimeStr(in Stopwatch T, in bool IsRestart = true)
        {
            TimeSpan ts = T.Elapsed;
            string elapsedTime;
            if (ts.Hours > 0) elapsedTime = String.Format("{0:00}h {1:00}m {2:00}s {3:0000}ms", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            else if (ts.Minutes > 0) elapsedTime = String.Format("{0:00}m {1:00}s {2:0000}ms", ts.Minutes, ts.Seconds, ts.Milliseconds);
            else elapsedTime = String.Format("{0}s", ts.TotalMilliseconds / 1000);
            if (IsRestart) T.Restart();
            return elapsedTime;
        }
        public static bool IsSameFiles(in string filepath1, in string filepath2)
        {
            string f1 = System.IO.File.ReadAllText(filepath1);
            string f2 = System.IO.File.ReadAllText(filepath2);
            return (f1 == f2);
        }
        public static bool IsVBetwinAB(double A, double B, double V) //NBE = No Both End
        {
            //return ((V-A)*(B-V) > 0); //양수면 V값은 A와 B값 사이에 있음. (양끝불포함)
            return ((A<V)&&(V<B));
        }
        public static bool IsVinsideAB(double A, double B, double V)
        {
            //return ((V-A)*(B-V) > -POLY_POINT.TOL_A); //양수면 V값은 A와 B값 사이에 있음. (양끝포함)
            return ((A<=V)&&(V<=B));
        }
        public static bool IsVinsideAB_tol(double A, double B, double V)
        {
            //return ((V-A)*(B-V) > -POLY_POINT.TOL_A); //양수면 V값은 A와 B값 사이에 있음. (양끝포함)
            return ((A-POLY_POINT.TOL_A<V)&&(V<B+POLY_POINT.TOL_A));
        }
    }
    public class D //DT에서 제작된 Class/Struct들만을 위한 함수들
    {   
        public static double GetAngle3pts(in POLY_POINT Ctr, in POLY_POINT P1, in POLY_POINT P2)
        {
            double Ax = P2.x - Ctr.x;
            double Ay = P2.y - Ctr.y;
            double Bx = P1.x - Ctr.x;
            double By = P1.y - Ctr.y;
            double Asize = Math.Sqrt(Ax*Ax + Ay*Ay);
            double Bsize = Math.Sqrt(Bx*Bx + By*By);
            double Prod  = Ax*Bx + Ay*By;
            double theta = Math.Acos(Prod/(Asize*Bsize));
            return theta;
        }
        public static bool CheckSelfContact_forLastIdx_New(ref List<POLY_POINT> pol)
        {
            int NPts = pol.Count() - 1;
            int idx = NPts; //Last Index
            double TanDet;
            double MTOL = -POLY_POINT.TOL_A;
            double TOL  = POLY_POINT.TOL_A;
            for(int iA=0, iB=1; iB<NPts; ++iA, ++iB)
            {
                double dx1 = pol[idx].x - pol[iA].x;
                double dx2 = pol[iB].x - pol[idx].x;
                if( dx1 * dx2 >= MTOL ) //Check Px in Ax~Bx
                {
                    double dy1 = pol[idx].y - pol[iA].y;
                    double dy2 = pol[iB].y - pol[idx].y;
                    if( dy1 * dy2 >= MTOL )//Check Py in Ay~By
                    {
                        if((MTOL<dx1)&&(dx1<TOL)) if((MTOL<dy1)&&(dy1<TOL)) return true;//Check P is A
                        if((MTOL<dx2)&&(dx2<TOL)) if((MTOL<dy2)&&(dy2<TOL)) return true;//Check P is B
                        TanDet = dy1*dx2 - dy2*dx1;
                        if((MTOL<TanDet)&&(TanDet<TOL)) //Check Grad(AP) & Grad(PB)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static bool CheckSelfContact_forLastIdx(ref List<POLY_POINT> pol)
        {
            int NPts = pol.Count();
            int idx = NPts - 1; //Last Index
            for(int i=1; i<NPts-1; ++i)
            {
                double AB = Math.Sqrt(Math.Pow(pol[i-1].x - pol[i  ].x, 2.0) + Math.Pow(pol[i-1].y - pol[i  ].y, 2.0));
                double AP = Math.Sqrt(Math.Pow(pol[i-1].x - pol[idx].x, 2.0) + Math.Pow(pol[i-1].y - pol[idx].y, 2.0));
                double PB = Math.Sqrt(Math.Pow(pol[idx].x - pol[i  ].x, 2.0) + Math.Pow(pol[idx].y - pol[i  ].y, 2.0));
                if((AP + PB) - AB < POLY_POINT.TOL_A)
                {
                    return true;
                }
            }
            return false;
        }
        public static void RoundCutOff(ref POLY_POINT[] pts, double RD = 1.0E4)
        {
            double NPts = pts.Count();
            for(int i=0; i<NPts; ++i)
            {
                pts[i].x = Math.Truncate(pts[i].x * RD) / RD;
            }
        }
        public static void RoundCutOff(ref POLYGON pol, double RD = 1.0E10)
        {
            for(int i=0; i<pol.NPts; ++i)
            {
                pol.Pts[i].x = Math.Truncate(pol.Pts[i].x * RD) / RD;
            }
        }
        public static double DabNorml (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
        {
            double NN = Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y);
            return ((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) / NN;
        }
        public static POLY_POINT GetDirVector(in POLY_POINT A, in POLY_POINT B)
        {
            return new POLY_POINT(B.x - A.x, B.y - A.y, A.A);
        }
        public static bool IsLeft (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
        { 
            return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) > 0); 
        }
        public static bool IsRight(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
        {
            return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) < 0); 
        }
        public static bool IsLeftAndOn (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
        { 
            return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) >= 0); 
        }
        public static bool IsRightAndOn(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
        {
            return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) <= 0); 
        }
        public static bool IsLeft (in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p)
        {
            return (((p1.x - p2.x) * (p1.y - p.y) - (p1.y - p2.y) * (p1.x - p.x)) > 0); 
        }
        public static bool IsRight(in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p)
        {
            return (((p1.x - p2.x) * (p1.y - p.y) - (p1.y - p2.y) * (p1.x - p.x)) < 0); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLeft (in POLY_POINT p1, in POLY_POINT p2, in double px, in double py)
        {
            return (((p1.x - p2.x) * (p1.y - py) - (p1.y - p2.y) * (p1.x - px)) > 0); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRight(in POLY_POINT p1, in POLY_POINT p2, in double px, in double py)
        {
            return (((p1.x - p2.x) * (p1.y - py) - (p1.y - p2.y) * (p1.x - px)) < 0); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dab (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
        {
            return ((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px));
        }
    }
}
