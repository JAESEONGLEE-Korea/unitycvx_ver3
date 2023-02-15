using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; //for using assert

namespace NFP
{
    public class STEEL
    { 
        //Emulator 구조 Copy함
        public string Id; //<--------------------------------------
        public string MatNo;
        public int Qty;
        public double Breadth;
        public double Length;
        public POLYGON OutContour;
    }
    public class PART 
    { 
        //Member Variables
            //Emulator 구조 Copy함
            public string Id; //<--------------------------------------
            public string Name;
            public bool IsPortBase;
            public int QtyP;
            public int QtyS;
            public bool SuspendRotate;
            public bool SuspendTurnover;
            public double MinLengthOnRotate;
            public POLYGON OutContour;               
            public POLYGON[] Holes;
            public string Vertex1; //<--------------------------------------
            public string Vertex2; //<--------------------------------------
        // MODIFY
            public void Shift(in double dx, in double dy)
            {
                this.OutContour.Shift(dx, dy);
                for(int i=0; i<this.Holes.Count(); ++i) Holes[i].Shift(dx, dy);
            }
            public void Shift(in POLY_POINT DirVector)
            {
                this.OutContour.Shift(DirVector.x, DirVector.y);
                for(int i=0; i<this.Holes.Count(); ++i) Holes[i].Shift(DirVector.x, DirVector.y);
            }
            public void Move(in int RefOutcontourIdx, in double NewX, in double NewY)
            {
                double dx = NewX - this.OutContour.Pts[RefOutcontourIdx].x;
                double dy = NewX - this.OutContour.Pts[RefOutcontourIdx].y;
                this.Shift(dx, dy);
            }
            public void Move(in int RefOutcontourIdx, in POLY_POINT NewP)
            {
                double dx = NewP.x - this.OutContour.Pts[RefOutcontourIdx].x;
                double dy = NewP.y - this.OutContour.Pts[RefOutcontourIdx].y;
                this.Shift(dx, dy);
            }
            public void MoveToBegNFP(in PART TargetPart)
            {
                int VidxTarg = TargetPart.OutContour.GetIdx_VertexMinY();
                int VidxThis = this.OutContour.GetIdx_VertexMaxY();
                this.Move(VidxThis, TargetPart.OutContour.Pts[VidxTarg]);
                //시작 지점이 arc 중간인 경우는 별도 처리해야 함
            }
            public void Rotate(in int RefOutcontourIdx, in double radian)
            {
                this.OutContour.Rotate(this.OutContour.Pts[RefOutcontourIdx], radian);
                for(int i=0; i<this.Holes.Count(); ++i) Holes[i].Rotate(this.OutContour.Pts[RefOutcontourIdx], radian);
            }
            public void Rotate(in POLY_POINT RefPoint, in double radian)
            {
                this.OutContour.Rotate(RefPoint, radian);
                for(int i=0; i<this.Holes.Count(); ++i) Holes[i].Rotate(RefPoint, radian);
            }
    }
    public class NestingObjects
    {
        // Member Variables - INPUT
            public List<STEEL> Steels = new List<STEEL>(); //Emulator 구조 Copy함
            public List<PART> Parts = new List<PART>(); //Emulator 구조 Copy함
        // Get 1. Information
            public string GetSummary_Sample()
            {
                //Init.
                    List<string> RV = new List<string>();
                    int NSteel = this.Steels.Count();
                    int NPart = this.Parts.Count();
                    int sumSt = 0, sumP = 0, sumS = 0;
                //Steels
                    for (int i = 0; i < this.Steels.Count(); ++i) sumSt += this.Steels[i].Qty;
                    RV.Add("========= Summary of Steels =========");
                    RV.Add("");
                    RV.Add($"Number of Steel Types = {NSteel}");
                    for (int i = 0; i < NSteel; ++i)
                        RV.Add($"\t{this.Steels[i].MatNo}: ({this.Steels[i].Length} x {this.Steels[i].Breadth}) x {this.Steels[i].Qty}");
                    RV.Add("");
                    RV.Add($"\tTotal Number of Steel = {sumSt}");
                    RV.Add("");
                //Parts
                    for (int i = 0; i < this.Parts.Count(); ++i) sumP += this.Parts[i].QtyP;
                    for (int i = 0; i < this.Parts.Count(); ++i) sumS += this.Parts[i].QtyS;
                    RV.Add("========= Summary of Parts =========");
                    RV.Add("");
                    RV.Add($"Number of Part Types  = {NPart}");
                    for (int i = 0; i < NPart; ++i)
                        RV.Add($"\t{this.Parts[i].Name}: Port {this.Parts[i].QtyP}, Stbd {this.Parts[i].QtyS}");
                    RV.Add("");
                    RV.Add($"\tTotal Number of Part  = {sumP + sumS} (Port {sumP}, Stbd {sumS})");
                    RV.Add("");
                //Return
                    return string.Join("\n", RV); ;
            }
        // File I/O
            public NestingObjects(in string SmpFilePath, bool IsIgnoreArc = false)
            {
                //File to String
                    string AllTextStr = System.IO.File.ReadAllText(SmpFilePath);
                    Debug.Assert(AllTextStr.Count() > 0);
                //Divide Grp
                    string[] TxtGrp = AllTextStr.Split('*');
                //Parsing each Grp
                    int NGrp = TxtGrp.Count();
                    for (int iGrp = 0; iGrp < NGrp; ++iGrp)
                    {
                        //Split String to Lines
                            string[] stringSeparators = new string[] { "\r\n" };
                            string[] Lines = TxtGrp[iGrp].Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                            int NLine = Lines.Count();
                            if (NLine == 0) continue;
                        // Parsing
                            string[] words = Lines[0].Split('\t');
                            int NW = words.Count();
                            if (words[0] == "STEEL")
                            {
                                //Parse Single Values
                                    Debug.Assert(NW == 7);
                                    STEEL tmpSteel = new STEEL();
                                    tmpSteel.Id = words[1]; //<--------------------------------------
                                    tmpSteel.MatNo = words[2];
                                    tmpSteel.Qty = int.Parse(words[3]);
                                    tmpSteel.Breadth = double.Parse(words[4]);
                                    tmpSteel.Length = double.Parse(words[5]);
                                    int NVertexs = int.Parse(words[6]);
                                    Debug.Assert(NVertexs == NLine - 1); //-1 for first line
                                //Parse Out Contour
                                    POLY_POINT[] tmpPoints = new POLY_POINT[NVertexs];
                                    for (int iLine = 1; iLine < NLine; ++iLine)
                                    {
                                        string[] XYA = Lines[iLine].Split('\t');
                                        Debug.Assert(XYA.Length == 3);
                                        double x = double.Parse(XYA[0]);
                                        double y = double.Parse(XYA[1]);
                                        double a = double.Parse(XYA[2]);
                                        if(IsIgnoreArc) a=0;
                                        Debug.Assert(iLine - 1 < NVertexs);
                                        tmpPoints[iLine - 1] = new POLY_POINT(x, y, a);
                                    }
                                    tmpSteel.OutContour = new POLYGON(tmpPoints);
                                //Update
                                    this.Steels.Add(tmpSteel);
                            }
                            else if (words[0] == "PART")
                            {
                                //Parse Single Values
                                    Debug.Assert(NW == 13);
                                    PART tmpPart = new PART();
                                    tmpPart.Id = words[1]; //<--------------------------------------
                                    tmpPart.Name = words[2];
                                    tmpPart.IsPortBase = bool.Parse(words[3]);//P,S한개만 가지고 있을 땐 가지고있는것을 Base, 둘다가질떈 T or F.
                                    tmpPart.QtyP = int.Parse(words[4]);
                                    tmpPart.QtyS = int.Parse(words[5]);
                                    tmpPart.SuspendRotate = bool.Parse(words[6]);
                                    tmpPart.SuspendTurnover = bool.Parse(words[7]);
                                    tmpPart.MinLengthOnRotate = double.Parse(words[8]);
                                    int NOutVertexs = int.Parse(words[9]);
                                    int NHoles = int.Parse(words[10]);
                                    tmpPart.Vertex1 = words[11];//<--------------------------------------
                                    tmpPart.Vertex2 = words[12];//<--------------------------------------
                                    //ErrCheck
                                    Debug.Assert(tmpPart.QtyP + tmpPart.QtyS > 0);
                                    Debug.Assert(NOutVertexs > 1);
                                //Parse Out Contour
                                    POLY_POINT[] tmpOutPoints = new POLY_POINT[NOutVertexs];
                                    int iLineRef = 0;
                                    for (int vtxCnt = 0, iLine = 1; vtxCnt < NOutVertexs; ++vtxCnt, ++iLine)
                                    {
                                        string[] XYA = Lines[iLine].Split('\t');
                                        Debug.Assert(XYA.Length == 3);
                                        double x = double.Parse(XYA[0]);
                                        double y = double.Parse(XYA[1]);
                                        double a = double.Parse(XYA[2]);
                                        if(IsIgnoreArc) a = 0;
                                        tmpOutPoints[vtxCnt] = new POLY_POINT(x, y, a);
                                        iLineRef = iLine;
                                    }
                                    tmpPart.OutContour = new POLYGON(tmpOutPoints);
                                //Parse Hole Contour
                                    tmpPart.Holes = new POLYGON[NHoles];//Declare
                                    for (int ihole = 0, iLine = iLineRef + 1; ihole < NHoles; ++ihole, ++iLine)
                                    {
                                        //Get Number of Vertexes of ihole
                                            string[] NvtxInHoleStr = Lines[iLine].Split('\t');
                                            Debug.Assert(NvtxInHoleStr.Count() == 1);//Hole개수를 나타내는 곳이 아닌지 체크
                                            int NHoleVtxs = int.Parse(NvtxInHoleStr[0]);
                                            Debug.Assert(NHoleVtxs > 1); //Circle도 최소 2개 이상의 Arc로 구현한다고 함.
                                        //Make HolePoints Ary
                                            POLY_POINT[] tmpHolePoints = new POLY_POINT[NHoleVtxs];
                                            for (int vtxCnt = 0; vtxCnt < NHoleVtxs; ++vtxCnt)
                                            {
                                                ++iLine; //due to Aleady Read
                                                string[] XYA = Lines[iLine].Split('\t');
                                                Debug.Assert(XYA.Length == 3);
                                                double x = double.Parse(XYA[0]);
                                                double y = double.Parse(XYA[1]);
                                                double a = double.Parse(XYA[2]);
                                                if(IsIgnoreArc) a = 0;
                                                tmpHolePoints[vtxCnt] = new POLY_POINT(x, y, a);
                                            }
                                            tmpPart.Holes[ihole] = new POLYGON(tmpHolePoints);
                                    }
                                //Update
                                    this.Parts.Add(tmpPart);
                            }
                            else if (words[0] == "END")
                            {
                                Debug.Assert(iGrp == NGrp - 1);
                            }
                            else
                            {
                                Console.WriteLine("--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR");
                                Console.WriteLine($"[{words[0]}]");
                                Console.WriteLine("--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR");
                                Console.WriteLine(TxtGrp[iGrp]);
                                Console.WriteLine("--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR--ERROR");
                                Console.WriteLine($"iGrp={iGrp}, Words={words[0]}");
                                Debug.Assert(false);
                            }
                    }
            }
            public void WriteSmp(in string OutFilePath)
            {
                using (System.IO.StreamWriter outstream = new System.IO.StreamWriter(OutFilePath, false))
                {
                    for (int i = 0; i < this.Steels.Count(); ++i)
                    {
                        outstream.WriteLine($"*STEEL\t{this.Steels[i].Id}\t{this.Steels[i].MatNo}\t{this.Steels[i].Qty}\t{this.Steels[i].Breadth}\t{this.Steels[i].Length}\t{this.Steels[i].OutContour.NPts}");
                        for(int j=0; j< this.Steels[i].OutContour.NPts; ++j)
                        {
                            outstream.WriteLine($"{this.Steels[i].OutContour.Pts[j].x}\t{this.Steels[i].OutContour.Pts[j].y}\t{this.Steels[i].OutContour.Pts[j].A}");
                        }
                    }
                    for (int i=0; i< this.Parts.Count(); ++i)
                    {
                        outstream.WriteLine($"*PART\t{this.Parts[i].Id}\t{this.Parts[i].Name}\t{this.Parts[i].IsPortBase}\t{this.Parts[i].QtyP}\t{this.Parts[i].QtyS}\t{this.Parts[i].SuspendRotate}\t{this.Parts[i].SuspendTurnover}\t{this.Parts[i].MinLengthOnRotate}\t{this.Parts[i].OutContour.Pts.Count()}\t{this.Parts[i].Holes.Count()}\t{this.Parts[i].Vertex1}\t{this.Parts[i].Vertex2}");
                        for (int j = 0; j <this.Parts[i].OutContour.NPts; ++j)//Write OutContour Vertexes
                        {
                            outstream.WriteLine($"{this.Parts[i].OutContour.Pts[j].x}\t{this.Parts[i].OutContour.Pts[j].y}\t{this.Parts[i].OutContour.Pts[j].A}");
                        }
                        for (int j = 0; j < this.Parts[i].Holes.Count(); ++j)//Write Hole Vertexes
                        {
                            outstream.WriteLine($"{this.Parts[i].Holes.Count()}");//Write Hole Count
                            for (int ivtx = 0; ivtx < this.Parts[i].Holes[j].NPts; ++ivtx)
                            {
                                outstream.WriteLine($"{this.Parts[i].Holes[j].Pts[ivtx].x}\t{this.Parts[i].Holes[j].Pts[ivtx].y}\t{this.Parts[i].Holes[j].Pts[ivtx].A}");
                            }
                        }
                    }
                    outstream.WriteLine("*END");
                }
            }
    }//End Class NestingObjects
}//End namespace SHI.DT.NESTOBJ

