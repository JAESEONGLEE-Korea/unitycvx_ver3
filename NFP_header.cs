using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading; //for using sleep
using System.Threading.Tasks;
using System.Diagnostics; //for using assert
using System.Runtime.CompilerServices; //for using inline

namespace NFP
{
    /// ▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣
    /// ▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣
    /// ▣▣▣▣                                                                                                  ▣▣▣▣
    /// ▣▣▣▣                                                                                                  ▣▣▣▣
    /// ▣▣▣▣                                                                                                  ▣▣▣▣
    /// ▣▣▣▣                                                                                                  ▣▣▣▣
    /// * 알고리즘 완성 후 할일 *
    /// Nfp.Count() 상수로 대체
    /// Abnormal Check Case 해제
    /// Reduce Vertex조건 재생성
    /// 

    public partial class NFP
    {
        // Debug
            private bool isDrawXpider;
        // Main Member Variables
            POLYGON PolS;
            POLYGON PolM;
        // Constant - Compile
            public const double  TOL  =  1.0E-12;
            public const double MTOL  = -1.0E-12;
            public const double  TOLP =  1.0E-06;
            public const double MTOLP = -1.0E-06;
            public const double  TOLD =  1.0E-4;
            public const double MTOLD = -1.0E-4;
        // Constant - Instance
            readonly int NvS; // Number of vertexes of S
            readonly int NvM; // Number of vertexes of M
            readonly int MaxLoop;
            readonly int[] BagSidx; //Next Index Ref Variable
            readonly int[] BagMidx; //Next Index Ref Variable
        // Polygon Specific Variables - will be constant after this.StateInitialize_BasicRule()
            private int Sidx0; // S-Index of Initial Contatc Point
            private int Midx0; // M-Index of Initial Contatc Point
        // Orbital State Variables
            //Current Index
                private int Sidx; // Current Index of Stationary Polygon
                private int Midx; // Current Index of Moving Polygon
            //Sliding
                private bool IsRawSlideMPolEdge;
                private POLY_POINT SldVec; // Sliding Vector. Arc가 들어가야 하니 POLY_POINT로 지정함.
            //Touch
                bool IsTchSpotSidxEdge; //Touch Status
                bool IsTchSpotMidxEdge; //Touch Status
                POLY_POINT Tpoint = new POLY_POINT(0,0,0); //Touch Coord - related to Sidx, Midx
            //Arc
                bool IsSidxArc;
                bool IsMidxArc;
                POLY_POINT[] CxyS;
                POLY_POINT[] CxyM;
            //Terminate
                //private int  OrbitalState = 0;
        // Orbital Duplicate State Variables
            private bool   IsTouchEdge = false;//<-----------------------------------DupCheck 및 전반적으로 IsTchSpotSidx~ 로 변경할 것
            private bool   IsTouchVtxM = false; // !M !S = Edge to Edge <- 곡선의 경우만 본 조건 고려
            private bool   IsTouchVtxS = false; // M S = Vertex to Vertex
            private int    TouchMidx = -1;
            private int    TouchSidx = -1;
            private int    TouchIchk = -1;
            private double Xdup=0;
            private double Ydup=0;
            private double PenetRatio; //No Collision == -1.0E100
        // Orbital Historical Variables
            private int  DupCase = 0; //Reset, DupCase = -1:오류, 0:NoDup, 1:선상침투, 2:시점침투, 3:종점침투, 4:외부침투
            private bool IsDupOccured;
        // Orbital Temporary
            private double NfpAngle=0;

        //=============
        // Constructor
        //=============
        public NFP(ref POLYGON InpPolygonS, ref POLYGON InpPolygonM)
        {
            this.PolS = new POLYGON(InpPolygonS);
            this.PolM = new POLYGON(InpPolygonM);
            NvS     = this.PolS.NPts; // Number of vertexes of S
            NvM     = this.PolM.NPts; // Number of vertexes of M
            MaxLoop = (this.NvS + this.NvM) * 3;
            this.BagSidx = new int[this.NvS * 2]; //Next Index Ref Variable
            this.BagMidx = new int[this.NvM * 2]; //Next Index Ref Variable
            int NvS2 = this.NvS * 2; for (int i = 0; i < NvS2; ++i) this.BagSidx[i] = i % this.NvS;
            int NvM2 = this.NvM * 2; for (int i = 0; i < NvM2; ++i) this.BagMidx[i] = i % this.NvM;
            
        }
    }
}