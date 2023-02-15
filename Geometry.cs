using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; //for using assert
using System.Runtime.CompilerServices; //for using inline

namespace NFP
{
    public class POLY_POINT //POLYGON의 좌표계 및 Sweep Angle
    {
        //Constant
            public const double TOL_A = 1.0E-7;//Must Plus
        //Member Variables
            public double x; // x
            public double y; // y
            public double A; // Sweep Angle (Rad)
            public bool   IsArc;
        //Constructor
            public POLY_POINT(in double x, in double y, in double A = 0)
            {
                this.x = x;
                this.y = y;
                if((-POLY_POINT.TOL_A < A) && (A < POLY_POINT.TOL_A)){this.A = 0; this.IsArc = false;}
                else                                                 {this.A = A; this.IsArc = true;}
            }
            public POLY_POINT(in POLY_POINT pt)
            {
                this.x = pt.x;
                this.y = pt.y;
                if((-POLY_POINT.TOL_A < A) && (A < POLY_POINT.TOL_A)){this.A = 0; this.IsArc = false;}
                else                                                 {this.A = pt.A; this.IsArc = true;}
            }
            //public POLY_POINT(in Vertex vtx)
            //{
            //    this.x = vtx.X;
            //    this.y = vtx.Y;
            //    if((-POLY_POINT.TOL_A < A) && (A < POLY_POINT.TOL_A)){this.A = 0; this.IsArc = false;}
            //    else                                                 {this.A = vtx.SweepAngle; this.IsArc = true;}
            //}
        //Modify
            public void Shift(in double dx, in double dy) { this.x += dx; this.y += dy; }
            public void Rotate(in double Xref, in double Yref, in double cosR, in double sinR)
            {
                double NewX = (this.x - Xref) * cosR - (this.y - Yref) * sinR + Xref;
                double NewY = (this.x - Xref) * sinR + (this.y - Yref) * cosR + Yref;
                this.x = NewX;
                this.y = NewY;
            }
            public void SetA(in double A)
            {
                if((-POLY_POINT.TOL_A < this.A) && (this.A < POLY_POINT.TOL_A))
                {
                    this.A = A;
                    this.IsArc = false;
                }
                else
                {
                    this.IsArc = true;
                }
            }
        //Get
            public bool IsLine()
            {
                if((-POLY_POINT.TOL_A < this.A) && (this.A < POLY_POINT.TOL_A))
                {
                    this.A = 0;
                    this.IsArc = false;
                    return true;
                }
                else
                {
                    this.IsArc = true;
                    return false;
                }
            }
            public bool IsSameXY(in POLY_POINT p)
            { 
                return ((this.x == p.x) && (this.y == p.y));
            }
            public bool IsSameXY_tol(in POLY_POINT p)
            {
                return (Math.Abs(this.x - p.x) + Math.Abs(this.y - p.y) < POLY_POINT.TOL_A);
            }
            public bool IsSameXY_tol(in double x, in double y)
            { 
                return (Math.Abs(this.x - x) + Math.Abs(this.y - y) < POLY_POINT.TOL_A);
            }
            public bool IsSameXYA_tol(in POLY_POINT p)
            { 
                return (Math.Abs(this.x - p.x) + Math.Abs(this.y - p.y) + Math.Abs(this.A - p.A) < POLY_POINT.TOL_A);
            }
            public POLY_POINT GetnTowardDirVector(in POLY_POINT TargetPoint)
            {
                return new POLY_POINT(TargetPoint.x - this.x, TargetPoint.y - this.y, 0);
            }
        //Print
            public string GetCoord2String(){return $"({this.x},{this.y},{this.A})";}
    }
    public class ARC2D //곡 정보. 두 Point 이상에서 의미가 있으므로 POLY_POINT와 합치지 않음.
    {
        //Description
            // Sweep Angle이 있을 때, 관련된 Circle에 관한 정보
            // WARNING: Member Variable 추가 금지
        //Member Variables
            public double dCx; //Vertex 시점 x로 부터 Arc Center까지 떨어진 거리 dx
            public double dCy; //Vertex 시점 y로 부터 Arc Center까지 떨어진 거리 dy
            public double R;
        //Constructor
            public ARC2D(in POLY_POINT A, in POLY_POINT B)
            {
                if (A.IsLine())
                {
                    this.dCx = 0;
                    this.dCy = 0;
                    this.R = 0;
                }
                else if (A.A > 0)
                {
                    double fac = Math.Tan(1.5707963267949 - A.A / 2.0);
                    this.dCx = 0.5 * ((A.x + B.x) + (A.y - B.y) * fac);
                    this.dCy = 0.5 * ((A.y + B.y) + (B.x - A.x) * fac);
                    //SHI.CadDev.Common.Diagnostics.Trace.WriteMsg(Level.Low, $"B.x={B.x}, A.x={A.x}, B.y={B.y}, A.y={A.y}, fac={fac}");
                    this.R = Math.Sqrt(Math.Pow(A.x - this.dCx, 2) + Math.Pow(A.y - this.dCy, 2));
                    this.dCx = this.dCx - A.x; //이동량으로 변환
                    this.dCy = this.dCy - A.y; //이동량으로 변환
                }
                else if (A.A < 0)
                {
                    double fac = Math.Tan(1.5707963267949 + A.A / 2.0); 
                    this.dCx = 0.5 * ((B.x + A.x) + (B.y - A.y) * fac);
                    this.dCy = 0.5 * ((B.y + A.y) + (A.x - B.x) * fac);
                    //SHI.CadDev.Common.Diagnostics.Trace.WriteMsg(Level.Low, $"B.x={B.x}, A.x={A.x}, B.y={B.y}, A.y={A.y}, fac={fac}");
                    this.R = Math.Sqrt(Math.Pow(B.x - this.dCx, 2) + Math.Pow(B.y - this.dCy, 2));
                    this.dCx = this.dCx - A.x; //이동량으로 변환
                    this.dCy = this.dCy - A.y; //이동량으로 변환
                }
                else
                {
                    this.dCx = 0;
                    this.dCy = 0;
                    this.R  = 0;
                    Debug.Assert(false);
                }
            }
        //Get
        //MODIFY
            public void Rotate(in double cosR, in double sinR)
            {
                double NewX = (this.dCx) * cosR - (this.dCy) * sinR;
                double NewY = (this.dCx) * sinR + (this.dCy) * cosR;
                this.dCx = NewX;
                this.dCy = NewY;
            }
    }
    public class POLYGON
    {
        //Member Variables
            public          POLY_POINT[] Pts;   //각 Segment 시작점의 Point위치 및 Sweep Angle
            public readonly POLY_POINT[] PtX;   //각 Segment 끝점의 Point위치 및 "시점"의 Sweep Angle <- Pts 참조복사 이용함.
            public          POLY_POINT[] Pdir;  //각 Vertex에서 다음 Vertex로의 방향 Vector
            public          POLY_POINT[] PdirR; //각 Vertex에서 다음 Vertex로의 방향 역Vector
            public          POLY_POINT[] PNdir; //Pdir의 크기를 1로 변환한 방향 Vector
            //public          POLY_POINT[] ArcCxy{get;}  //<---------개발 후 Get해제할것. 속도느리더라.
        //Member Variables - Private
            private          POLY_POINT[] ArcCxy;  //<---------개발 후 Get해제할것. 속도느리더라.
            private          ARC2D[]      ArcInfo; //Pts의 Arc정보(Circle Center+R), (Line이라면 R < 0)
            private bool         ArcCxyNeedUpdate = true;
        //Member Variables - Read Only
            public readonly bool[] IsArcIdx;  //Ex) IsArcIdx[idx] == false 라면, idx는 직선segment이다
            public readonly int NPts;         // <- 배열크기 수정시 항시 같이 Update되어야 함
        //Constructor
            public POLYGON()
            {
                //본 생성자 사용 금지
                    Debug.Assert(false);
            }
            public POLYGON(in POLYGON O)
            {
                int N = O.NPts;
                Pts = new POLY_POINT[N];   //각 Segment 시작점의 Point위치 및 Sweep Angle
                PtX = new POLY_POINT[N];   //각 Segment 끝점의 Point위치 및 "시점"의 Sweep Angle <- Pts 참조복사 이용함.
                Pdir = new POLY_POINT[N];  //각 Vertex에서 다음 Vertex로의 방향 Vector
                PdirR = new POLY_POINT[N]; //각 Vertex에서 다음 Vertex로의 방향 역Vector
                PNdir = new POLY_POINT[N]; //Pdir의 크기를 1로 변환한 방향 Vector
                ArcCxy = new POLY_POINT[N];  //<---------개발 후 Get해제할것. 속도느리더라.
                ArcInfo = new ARC2D[N]; //Pts의 Arc정보(Circle Center+R), (Line이라면 R < 0)
                IsArcIdx = new bool[N];
                ArcCxyNeedUpdate = O.ArcCxyNeedUpdate;
                for(int i=0; i<O.IsArcIdx.Count(); ++i)
                {
                    Pts      = O.Pts;
                    PtX      = O.PtX;
                    Pdir     = O.Pdir;
                    PdirR    = O.PdirR;
                    PNdir    = O.PNdir;
                    ArcCxy   = O.ArcCxy;
                    ArcInfo  = O.ArcInfo;
                    IsArcIdx = O.IsArcIdx;
                    IsArcIdx[i] = O.IsArcIdx[i];
                }
                NPts=O.NPts;         // <- 배열크기 수정시 항시 같이 Update되어야 함
            }
            public POLYGON(in POLY_POINT[] oriPts)
            {
                //Set Info.
                    int N = oriPts.Count(); Debug.Assert(N > 0);
                    this.NPts = N;
                    this.Pts = new POLY_POINT[N];
                    this.IsArcIdx = new bool[N];
                //Copy
                    for (int i = 0; i < N; ++i)
                    {
                        //Copy
                            this.Pts[i] = new POLY_POINT(oriPts[i]);
                        //Correct Sweep Angle
                            if ((-POLY_POINT.TOL_A < oriPts[i].A) && (oriPts[i].A < POLY_POINT.TOL_A))
                            {
                                this.IsArcIdx[i] = false;
                                this.Pts[i].A = 0;
                            }
                            else
                            {
                                this.IsArcIdx[i] = true;
                                this.Pts[i].A = oriPts[i].A;
                                Debug.Assert(this.Pts[i].A <= M.PI);
                            }
                    }
                // Next Point 생성
                    this.PtX = new POLY_POINT[N];
                    for (int i = 0; i < N; ++i) this.PtX[i] = this.Pts[(i + 1) % N];
                // Arc 정보 생성
                    this.ArcInfo = new ARC2D[N];
                    this.ArcCxy = new POLY_POINT[N];
                    for (int i = 0; i < N; ++i)
                    {
                        this.ArcInfo[i] = new ARC2D(this.Pts[i], this.PtX[i]);
                        this.ArcCxy[i] = new POLY_POINT(this.Pts[i].x + this.ArcInfo[i].dCx, this.Pts[i].y + this.ArcInfo[i].dCy);
                    }
                // 방향 Vector 생성
                    this.Pdir  = new POLY_POINT[N];
                    this.PdirR = new POLY_POINT[N];
                    this.PNdir = new POLY_POINT[N];
                    //D.RoundCutOff(ref this.Pts);
                    double length;
                    for(int i = 0; i< N; ++i)
                    {
                        this.Pdir[i]  = new POLY_POINT(this.PtX[i].x - this.Pts[i].x, this.PtX[i].y - this.Pts[i].y, this.Pts[i].A);
                        this.PdirR[i] = new POLY_POINT(-this.Pdir[i].x, -this.Pdir[i].y, -this.Pdir[i].A);
                        length = Math.Sqrt(this.Pts[i].x*this.Pts[i].x + this.Pts[i].y*this.Pts[i].y);
                        this.PNdir[i] = new POLY_POINT(this.Pdir[i].x/length, this.Pdir[i].y/length, 0);
                    }
            }
            public POLYGON(in List<POLY_POINT> oriPts)
            {
                //Set Info.
                    int N = oriPts.Count(); Debug.Assert(N > 0);
                    this.NPts = N;
                    this.Pts = new POLY_POINT[N];
                    this.IsArcIdx = new bool[N];
                //Copy
                    for (int i = 0; i < N; ++i)
                    {
                        //Copy
                            this.Pts[i] = new POLY_POINT(oriPts[i]);
                        //Correct Sweep Angle
                            if ((-POLY_POINT.TOL_A < oriPts[i].A) && (oriPts[i].A < POLY_POINT.TOL_A))
                            {
                                this.IsArcIdx[i] = false;
                                this.Pts[i].A = 0;
                            }
                            else
                            {
                                this.IsArcIdx[i] = true;
                                this.Pts[i].A = oriPts[i].A;
                            }
                    }
                // Next Point 생성
                    this.PtX = new POLY_POINT[N];
                    for (int i = 0; i < N; ++i) this.PtX[i] = this.Pts[(i + 1) % N];
                // Arc 정보 생성
                    this.ArcCxy = new POLY_POINT[N];
                    for (int i = 0; i < N; ++i)
                    {
                        //this.Arc[i] = new ARC2D(this.Pts[i], this.PtX[i]);
                        //this.ArcCxy[i] = new POLY_POINT(this.Pts[i].x + this.Arc[i].dCx, this.Pts[i].y + this.Arc[i].dCy);
                    }
                // 방향 Vector 생성
                    this.Pdir  = new POLY_POINT[N];
                    this.PdirR = new POLY_POINT[N];
                    this.PNdir = new POLY_POINT[N];
                    double length;
                    for(int i = 0; i< N; ++i)
                    {
                        this.Pdir[i]  = new POLY_POINT(this.PtX[i].x - this.Pts[i].x, this.PtX[i].y - this.Pts[i].y, this.Pts[i].A);
                        this.PdirR[i] = new POLY_POINT(-this.Pdir[i].x, -this.Pdir[i].y, -this.Pdir[i].A);
                        length = Math.Sqrt(this.Pts[i].x*this.Pts[i].x + this.Pts[i].y*this.Pts[i].y);
                        this.PNdir[i] = new POLY_POINT(this.Pdir[i].x/length, this.Pdir[i].y/length, 0);
                    }
            }
            //public POLYGON(in IEnumerable<Vertex> Vertexs)
            //{
            //    //Set Info.
            //        int N = Vertexs.Count(); Debug.Assert(N > 0);
            //        this.NPts = N;
            //        this.Pts = new POLY_POINT[N];
            //        this.IsArcIdx = new bool[N];
            //    //Copy
            //        int idx = 0; //iterator
            //        foreach (Vertex vtx in Vertexs)
            //        {
            //            //Copy
            //                this.Pts[idx] = new POLY_POINT(vtx);
            //            //Correct Sweep Angle
            //                if ((-POLY_POINT.TOL_A < vtx.SweepAngle) && (vtx.SweepAngle < POLY_POINT.TOL_A))
            //                { this.IsArcIdx[idx] = false; this.Pts[idx].A = 0; }
            //                else { this.IsArcIdx[idx] = true; this.Pts[idx].A = vtx.SweepAngle; }
            //                idx++;
            //        }
            //    // Next Point 생성
            //        this.PtX = new POLY_POINT[N];
            //        for (int i = 0; i < N; ++i) this.PtX[i] = this.Pts[(i + 1) % N];
            //    // Arc 정보 생성
            //        this.ArcCxy = new POLY_POINT[N];
            //        for (int i = 0; i < N; ++i)
            //        {
            //            this.ArcInfo[i] = new ARC2D(this.Pts[i], this.PtX[i]);
            //            this.ArcCxy[i] = new POLY_POINT(this.Pts[i].x + this.ArcInfo[i].dCx, this.Pts[i].y + this.ArcInfo[i].dCy);
            //        }
            //    // 방향 Vector 생성
            //        this.Pdir  = new POLY_POINT[N];
            //        this.PdirR = new POLY_POINT[N];
            //        this.PNdir = new POLY_POINT[N];
            //        double length;
            //        for(int i = 0; i< N; ++i)
            //        {
            //            this.Pdir[i]  = new POLY_POINT(this.PtX[i].x - this.Pts[i].x, this.PtX[i].y - this.Pts[i].y, this.Pts[i].A);
            //            this.PdirR[i] = new POLY_POINT(-this.Pdir[i].x, -this.Pdir[i].y, -this.Pdir[i].A);
            //            length = Math.Sqrt(this.Pts[i].x*this.Pts[i].x + this.Pts[i].y*this.Pts[i].y);
            //            this.PNdir[i] = new POLY_POINT(this.Pdir[i].x/length, this.Pdir[i].y/length, 0);
            //        }
            //}
        // Get 0 - Misc.
            public int GetIndexLengthCountClock(in int BegIdx, in int EndIdx)
            {
                int Length;
                if(BegIdx <= EndIdx) Length = EndIdx - BegIdx;               //Ex) N=4, B=2, E=3 -> 3-2   = 1
                else                 Length = (EndIdx + this.NPts) - BegIdx; //Ex) N=4, B=2, E=1 -> 1+4-2 = 3
                return Length;
            }
            public bool CheckSelfContact(int idx)
            {
                for(int i=1; i<this.NPts; ++i)
                {
                    double AB = Math.Sqrt(Math.Pow(this.Pts[i-1].x - this.Pts[i  ].x, 2.0) + Math.Pow(this.Pts[i-1].y - this.Pts[i  ].y, 2.0));
                    double AP = Math.Sqrt(Math.Pow(this.Pts[i-1].x - this.Pts[idx].x, 2.0) + Math.Pow(this.Pts[i-1].y - this.Pts[idx].y, 2.0));
                    double PB = Math.Sqrt(Math.Pow(this.Pts[idx].x - this.Pts[i+0].x, 2.0) + Math.Pow(this.Pts[idx].y - this.Pts[i+0].y, 2.0));
                    if((AP + PB) - AB < POLY_POINT.TOL_A) return true;
                }
                return false;
            }
        // Get 1 - relationship with point
            public double GetMidPt_X(in int idx1, in int idx2)
            {
                return (this.Pts[idx1].x + this.Pts[idx2].x)/2.0;
            }
            public double GetMidPt_Y(in int idx1, in int idx2)
            {
                return (this.Pts[idx1].y + this.Pts[idx2].y)/2.0;
            }
            public double Dab (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            {
                return ((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px));
            }
            public double DabNorml (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            {
                //double NN = (Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y));
                //return ((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) / NN;
                return ((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) / (Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y));
            }
            public double Dab (in int idx, in double px, in double py)
            {
                return this.Dab(this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }
            public double DabNorml (in int idx, in double px, in double py)
            {
                return this.DabNorml(this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }
            public double DabDir(in double DirX, in double DirY, in double px, in double py)
            {
                //double L1 = Math.Abs(this.Pdir[idx].x) + Math.Abs(this.Pdir[idx].y);
                //double L2 = Math.Abs(px) + Math.Abs(py);
                //double L3 = L1*L2;
                //return (this.Pdir[idx].x * py - this.Pdir[idx].y * px) / L3;
                return (DirX * py - DirY * px);
                //return this.Dab(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, px, py); 
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public double DabDir(in int idx, in double px, in double py)
            {
                //double L1 = Math.Abs(this.Pdir[idx].x) + Math.Abs(this.Pdir[idx].y);
                //double L2 = Math.Abs(px) + Math.Abs(py);
                //double L3 = L1 * L2;
                //return (this.Pdir[idx].x * py - this.Pdir[idx].y * px) / L3;

                double size = Math.Abs(this.Pdir[idx].x - px) + Math.Abs(this.Pdir[idx].y - py);
                return (this.Pdir[idx].x * py - this.Pdir[idx].y * px)/size;
                //return this.Dab(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, px, py); 
            }
            public bool IsLeft (in int idx, in double px, in double py) // 속도체크요
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeft (this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }
            public bool IsRight (in int idx, in double px, in double py) // 속도체크요
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRight (this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }
            public bool IsLeftAndOn (in int idx, in double px, in double py) // 속도체크요
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeftAndOn_tol (this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }
            public bool IsRightAndOn (in int idx, in double px, in double py) // 속도체크요
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRightAndOn_tol (this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }
            public bool IsOnInfLine (in int idx, in double px, in double py) // 속도체크요
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsOnInfLine_tol (this.Pts[idx].x, this.Pts[idx].y, this.PtX[idx].x, this.PtX[idx].y, px, py); 
            }

            public bool IsLeft (in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeft (this.Pts[idx], this.PtX[idx], p); 
            }
            public bool IsRight(in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRight(this.Pts[idx], this.PtX[idx], p); 
            }
            public bool IsLeftAndOn (in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeftAndOn_tol (this.Pts[idx], this.PtX[idx], p); 
            }
            public bool IsRightAndOn(in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRightAndOn_tol(this.Pts[idx], this.PtX[idx], p); 
            }
            public bool IsDirLeft (in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeft (0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirLeft (in int idx, in double x, double y)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeft (0, 0, this.Pdir[idx].x, this.Pdir[idx].y, x, y); 
            }
            public bool IsDirRight(in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRight(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirLeftWithTol (in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeftWithTol (0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirRightWithTol(in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRightWithTol(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirLeftReverseTol (in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeftReverseTol (0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirRightReverseTol(in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRightReverseTol(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirRightReverseTol(in int idx, in double x, double y)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRightReverseTol(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, x, y); 
            }
            public bool IsDirLeftAndOn (in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsLeftAndOn_tol (0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsDirRightAndOn(in int idx, in POLY_POINT p)
            { 
                Debug.Assert(!this.IsArcIdx[idx]); 
                return IsRightAndOn_tol(0, 0, this.Pdir[idx].x, this.Pdir[idx].y, p.x, p.y); 
            }
            public bool IsLeft (in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p)
            {
                Debug.Assert(!p1.IsArc); Debug.Assert(!p2.IsArc);
                return (((p1.x - p2.x) * (p1.y - p.y) - (p1.y - p2.y) * (p1.x - p.x)) > 0); 
            }
            public bool IsRight(in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p)
            {
                Debug.Assert(!p1.IsArc); Debug.Assert(!p2.IsArc);
                return (((p1.x - p2.x) * (p1.y - p.y) - (p1.y - p2.y) * (p1.x - p.x)) < 0); 
            }
            public bool IsLeftAndOn_tol (in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p)
            {
                Debug.Assert(!p1.IsArc); Debug.Assert(!p2.IsArc);
                return (((p1.x - p2.x) * (p1.y - p.y) - (p1.y - p2.y) * (p1.x - p.x)) > -POLY_POINT.TOL_A); 
            }
            public bool IsRightAndOn_tol(in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p)
            {
                Debug.Assert(!p1.IsArc); Debug.Assert(!p2.IsArc);
                return (((p1.x - p2.x) * (p1.y - p.y) - (p1.y - p2.y) * (p1.x - p.x)) < POLY_POINT.TOL_A); 
            }
            public bool IsLeft (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            { 
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) > 0); 
            }
            public bool IsRight(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            {
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) < 0); 
            }
            public bool IsLeftWithTol (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py, double tol = POLY_POINT.TOL_A)
            { 
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) > -tol); 
            }
            public bool IsRightWithTol(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py, double tol = POLY_POINT.TOL_A)
            {
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) < tol); 
            }
            public bool IsLeftReverseTol (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py, double tol = POLY_POINT.TOL_A)
            { 
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) > tol); 
            }
            public bool IsRightReverseTol(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py, double tol = POLY_POINT.TOL_A)
            {
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) < -tol); 
            }
            public bool IsLeftAndOn_tol (in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            { 
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) > -POLY_POINT.TOL_A); 
            }
            public bool IsRightAndOn_tol(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            {
                return (((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) < POLY_POINT.TOL_A); 
            }
            public bool IsOnInfLine_tol(in double p1x, in double p1y, in double p2x, in double p2y, in double px, in double py)
            {
                return (Math.Abs((p1x - p2x) * (p1y - py) - (p1y - p2y) * (p1x - px)) < POLY_POINT.TOL_A); 
            }
            public bool IsOnSegment(in int idx, in POLY_POINT p, bool IsOpenBdry = false)
            {
                //Description
                    //this.Pts[idx]를 시점으로하는 segment에 p가 놓여있는지 여부 검사.
                    //속도 문제있으면 업글 ㄱㄱ
                //Code
                    Debug.Assert(!this.IsArcIdx[idx]);
                    double d1 = Math.Pow(Math.Pow(this.Pts[idx].x - p.x, 2) + Math.Pow(this.Pts[idx].y - p.y, 2), 0.5);
                    double d2 = Math.Pow(Math.Pow(this.PtX[idx].x - p.x, 2) + Math.Pow(this.PtX[idx].y - p.y, 2), 0.5);
                    double dt = Math.Pow(Math.Pow(this.PtX[idx].x - this.Pts[idx].x, 2) + Math.Pow(this.PtX[idx].y - this.Pts[idx].y, 2), 0.5);
                    if (IsOpenBdry) if ((d1 == 0.000001) || (d2 == 0.000001)) return false;
                    if (Math.Abs(d1 + d2 - dt) < 0.000001) return true;
                    else return false;
            }
            public bool IsOnSegment(in POLY_POINT p1, in POLY_POINT p2, in POLY_POINT p, bool IsOpenBdry = false)
            {
                //Description
                    //this.Pts[idx]를 시점으로하는 segment에 p가 놓여있는지 여부 검사.
                    //속도 문제있으면 업글 ㄱㄱ
                //Code
                    Debug.Assert(!p1.IsArc); Debug.Assert(!p2.IsArc);
                    double d1 = Math.Pow(Math.Pow(p1.x - p.x, 2) + Math.Pow(p1.y - p.y, 2), 0.5);
                    double d2 = Math.Pow(Math.Pow(p2.x - p.x, 2) + Math.Pow(p2.y - p.y, 2), 0.5);
                    double dt = Math.Pow(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2), 0.5);
                    if (IsOpenBdry) if ((d1 == 0.000001) || (d2 == 0.000001)) return false;
                    if (Math.Abs(d1 + d2 - dt) < 0.000001) return true;
                    else return false;
            }
            public bool IsPointInsideMe(in POLY_POINT p)
            {
                //Description
                    // [Ref] The Industrial Application of New Irregular Cutting and Packing Algorithms (Robert Hellier, 2013)
                    //       Page 49의 Winding Number Algorithm을 수정하여 사용
                //Code 
                    int cnt = 0;
                    int N   = this.NPts;
                    for(int i=0; i<N; ++i)
                    {
                        if(this.Pts[i].y <= p.y) //시점이 P이하
                        {
                            if (this.PtX[i].y > p.y) //종점이 P위
                                if (this.IsLeft(this.Pts[i].x, this.Pts[i].y, this.PtX[i].x, this.PtX[i].y, p.x, p.y)) ++cnt;
                        }
                        else if(this.Pts[i].y > p.y)
                        {
                            if(this.PtX[i].y <= p.y)
                                if( ! IsLeft(this.Pts[i].x, this.Pts[i].y, this.PtX[i].x, this.PtX[i].y, p.x, p.y)) --cnt;
                        }
                        if (this.IsOnSegment(this.Pts[i], this.PtX[i], p)) 
                            return false;
                    }
                    return (cnt != 0); // cnt==0:외부, cnt!=0:내부
            }
        // Get 2 - Polygon Prop.
            public int GetIdx_VertexMaxY()
            {
                double max = this.Pts[0].y;
                int maxIdx = 0;
                for(int i=0; i<this.NPts; ++i)
                {
                    if(max < this.Pts[i].y)
                    {
                        max = this.Pts[i].y;
                        maxIdx = i;
                    }
                }
                return maxIdx;
            }
            public void GetActualMaxY(out int MinIdx, ref POLY_POINT T, out bool IsOnEdge)
            {//Get Actual Maximum Coord with Arc
                //Init
                    double Ymax = -1.0E100;//dummy
                    MinIdx = -1;//dummy
                    IsOnEdge = false;//dummy
                //Local
                    //int i2;
                    //double Xa, Ya, Xb, Xc, fmax;
                    double Ya;
                for(int i1=0; i1<this.NPts; ++i1)
                {
                    //i2 = (i1+1)%this.NPts;
                    //Xa = this.Pts[i1].x;
                    //Xb = this.Pts[i2].x;
                    Ya = this.Pts[i1].y;
                    //Xc = this.ArcCxy[i1].x;
                    /*else */if    (Ymax < Ya)  {Ymax = Ya;   MinIdx = i1; IsOnEdge = false;}
                }
                if(IsOnEdge)
                {
                    T.x = this.ArcCxy[MinIdx].x;
                    T.y = this.ArcCxy[MinIdx].y;
                    T.A = this.Pts[MinIdx].A;
                }
                else
                {
                    T.x = Pts[MinIdx].x;
                    T.y = Ymax;
                    T.A = this.Pts[MinIdx].A;
                }
            }
            public void GetActualMinY(out int MinIdx, ref POLY_POINT T, out bool IsOnEdge)
            {//Get Actual Minimum Coord with Arc
                //Init
                    double Ymin = 1.0E100;//dummy
                    MinIdx = -1;//dummy
                    IsOnEdge = false;//dummy
                //Local
                    //int i2;
                    //double Xa, Ya, Xb, Xc, fmin;
                    double Ya;
                for(int i1=0; i1<this.NPts; ++i1)
                {
                    //i2 = (i1+1)%this.NPts;
                    //Xa = this.Pts[i1].x;
                    //Xb = this.Pts[i2].x;
                    Ya = this.Pts[i1].y;
                    //Xc = this.ArcCxy[i1].x;

                    //if(this.IsArcIdx[i1])
                    //{
                    //    if((Xa<Xc)&&(Xc<Xb))
                    //    {   
                    //        fmin = this.ArcCxy[i1].y - this.ArcInfo[i1].R;
                    //        if (Ymin > fmin){Ymin = fmin; MinIdx = i1; IsOnEdge = true;}
                    //    }
                    //    else if(Ymin > Ya)  {Ymin = Ya;   MinIdx = i1; IsOnEdge = false;}
                    //}
                    /*else */if    (Ymin > Ya)  {Ymin = Ya;   MinIdx = i1; IsOnEdge = false;}
                }
                if(IsOnEdge)
                {
                    T.x = this.ArcCxy[MinIdx].x;
                    T.y = Ymin;
                    T.A = this.Pts[MinIdx].A;
                }
                else
                {
                    T.x = Pts[MinIdx].x;
                    T.y = Pts[MinIdx].y;
                    T.A = this.Pts[MinIdx].A;
                }
            }
            public int GetIdx_VertexMinY()
            {
                //Get Minimum Vertex
                    double min = this.Pts[0].y;
                    int minIdx = 0;
                    for(int i=0; i<this.NPts; ++i)
                    {
                        if(min > this.Pts[i].y)
                        {
                            min = this.Pts[i].y;
                            minIdx = i;
                        }
                    }
                return minIdx;
            }
            public int GetIdx_VertexMaxX()
            {
                double max = this.Pts[0].x;
                int maxIdx = 0;
                for(int i=0; i<this.NPts; ++i)
                {
                    if(max < this.Pts[i].x)
                    {
                        max = this.Pts[i].x;
                        maxIdx = i;
                    }
                }
                return maxIdx;
            }
            public int GetIdx_VertexMinX()
            {
                double min = this.Pts[0].x;
                int minIdx = 0;
                for(int i=0; i<this.NPts; ++i)
                {
                    if(min > this.Pts[i].x)
                    {
                        min = this.Pts[i].x;
                        minIdx = i;
                    }
                }
                return minIdx;
            }
            public POLY_POINT[] GetArcCenterXY()
            {
                if(this.ArcCxyNeedUpdate)
                {
                    this.UpdateArcCenterXY();
                }
                return this.ArcCxy;
            }
            public POLY_POINT GetArcCenterXY(in int idx)
            {
                Debug.Assert(idx < this.NPts);
                if(this.ArcCxyNeedUpdate)
                {
                    this.UpdateArcCenterXY();
                }
                return this.ArcCxy[idx];
            }
            private void UpdateArcCenterXY()
            {
                if(this.ArcCxyNeedUpdate)
                {
                    Debug.Assert(this.NPts == this.ArcCxy.Count());
                    Debug.Assert(this.NPts == this.ArcInfo.Count());
                    for(int i=0; i<this.ArcInfo.Count(); ++i)
                    {
                        this.ArcCxy[i].x = this.Pts[i].x + this.ArcInfo[i].dCx;
                        this.ArcCxy[i].y = this.Pts[i].y + this.ArcInfo[i].dCy;
                    }
                    this.ArcCxyNeedUpdate = false;
                }
            }
            //public int GetActualMinY()
            //{
            //    int VidxMinY = this.GetIdx_VertexMinY();
            //    double min = this.Pts[VidxMinY].y;
            //    double cx, cy;
            //    for(int i=0; i<this.NPts; ++i)
            //    {
            //        if(this.IsArcIdx[i])
            //        {
            //            cy = this.Arc[i].cy;
            //        }
            //    }
            //}
        // Get 3 - Polygon Components
            


        // MODIFY
            public void Shift(in double dx, in double dy)
            {
                int N = this.NPts;
                for(int i=0; i<N; ++i)
                {
                    this.Pts[i].Shift(dx, dy);
                }
                this.ArcCxyNeedUpdate = true;
            }
            public void Shift(in POLY_POINT DirVector)
            {
                int N = this.NPts;
                for(int i=0; i<N; ++i)
                {
                    this.Pts[i].Shift(DirVector.x, DirVector.y);
                }
                this.ArcCxyNeedUpdate = true;
            }
            public void Move(in int RefIdx, in POLY_POINT NewP)
            {
                Debug.Assert(RefIdx < this.NPts);
                double dx = NewP.x - this.Pts[RefIdx].x;
                double dy = NewP.y - this.Pts[RefIdx].y;
                int N = this.NPts;
                for(int i=0; i<N; ++i)
                {
                    this.Pts[i].Shift(dx, dy);
                }
                this.ArcCxyNeedUpdate = true;
            }
            public void Move(in int RefIdx, in double NewX, in double NewY)
            {
                Debug.Assert(RefIdx < this.NPts);
                int N = this.NPts;
                double dx = NewX - this.Pts[RefIdx].x;
                double dy = NewY - this.Pts[RefIdx].y;
                for(int i=0; i<N; ++i)
                {
                    this.Pts[i].x = this.Pts[i].x + dx;
                    this.Pts[i].y = this.Pts[i].y + dy;
                }
                this.ArcCxyNeedUpdate = true;
            }
            public void Rotate(in POLY_POINT RefPoint, in double radian)
            {
                double cosR = Math.Cos(radian);
                double sinR = Math.Sin(radian);
                double Xref = RefPoint.x;
                double Yref = RefPoint.y;
                int N = this.NPts;
                for(int i=0; i<N; ++i)
                {
                    this.Pts[i].Rotate(Xref, Yref, cosR, sinR);
                    this.Pdir[i].Rotate(0, 0, cosR, sinR);
                    this.PdirR[i].Rotate(0, 0, cosR, sinR);
                }
                this.ArcCxyNeedUpdate = true;
            }

    } // End class POLYGON


} // End namespace SHI.DT.GEOMETRY