using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics; //for using assert

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
        public POLYGON GetNFP_Orbital(bool inpIsDrawXpider, ref int returnQuality, ref int RefPtIdx)
        {
            //X.SendMsg("---------------------------------------------------------------------------------------------------------------------------------------------", 3);
            
            //for (int i = 0; i < PolS.Pts.Count(); i++) X.DrawPoint(PolS.Pts[i].x, PolS.Pts[i].y, 3);  s폴리곤 플롯
            
            // for Debug
                this.isDrawXpider = inpIsDrawXpider;
                bool isGoodTerminate = false;
                int  runLoop = 0; //for Debug
            
            // Output
                List<POLY_POINT> nfpPList = new List<POLY_POINT>(); //최종 얻고자 하는 NFP Vertex List
                List<POLY_POINT> nfpPList_Closed = new List<POLY_POINT>(); //Debugging을 위한 NFP
            // Initialize
                this.StateInitialize_BasicRule(ref RefPtIdx); //Move polM to polS //<-------아크수정필요
                //Xprint_InitCond(this.Sidx0, this.Midx0); //debug
            
            //===============================================================
            // Start Orbital Loop
            //===============================================================            
            for (int iLoop = 0; iLoop < this.MaxLoop; ++iLoop)
            {
                //X.DrawPolygon(PolM, 1); //0726_이민수
                //X.DrawPolygon(PolS, 0);
                // Pre-Update State Variables
                this.IsSidxArc = this.PolS.IsArcIdx[Sidx];
                    this.IsMidxArc = this.PolM.IsArcIdx[Midx];
                    //this.CxyS      = this.PolS.GetArcCenterXY(); //<------iLoop마다 계산할 필요없음. 코드정리시 처리할 것.
                    //this.CxyM      = this.PolM.GetArcCenterXY();
                // For Debug
                    runLoop = iLoop; //for Debug
                    //if (this.isDrawXpider) X.SendMsg($" ---- Orbital Step {iLoop}, Sidx={this.Sidx}, Midx={this.Midx}  ----", 2);

                // Define Sliding Vector
                    SetRawSlideDir(); //Find Sliding Edge (Convex Orbital Rule)
                    SetRawSldVec();   //Get Raw Sliding Vector(Raw = Follow Basic Rule)
                    //if (this.isDrawXpider) Xprint_SldVec();
                    CorrectRawSldVec(); // Correct Raw Sliding vector - consider vertexes on edge

                // Check Duplicate
                    CheckDuplicate();

                // Update Sliding Vector
                    if (this.DupCase > 0) //Dup. Occured
                    {
                        //Assert
                        Debug.Assert(this.PenetRatio >= -0.000001);
                        //if (this.isDrawXpider) X.SendMsg($"Final PnetRatio({this.DupCase}) = {this.PenetRatio}, TouchIdx (S,M)=({this.TouchMidx},{this.TouchSidx})", 1);
                        //if (this.isDrawXpider) X.DrawPoint(this.PolS.Pts[this.TouchSidx]);
                        //if (this.isDrawXpider) X.DrawPoint(this.PolM.Pts[this.TouchMidx]);
                        this.SldVec.x *= (1 - this.PenetRatio);
                        this.SldVec.y *= (1 - this.PenetRatio);
                    }
                // Get Next Touch Point & Get Next Index
                    if (this.DupCase > 0) //Dup. Occured
                    {
                        //Get Touch Point
                        Debug.Assert(this.TouchIchk > -1);
                        if (this.TouchIchk == 0)
                        {
                            this.Xdup = this.PolM.Pts[this.TouchMidx].x + this.SldVec.x;
                            this.Ydup = this.PolM.Pts[this.TouchMidx].y + this.SldVec.y;
                        }
                        else
                        {
                            this.Xdup = this.PolS.Pts[this.TouchSidx].x;
                            this.Ydup = this.PolS.Pts[this.TouchSidx].y;
                        }
                        //Update Index
                        this.Sidx = this.TouchSidx;
                        this.Midx = this.TouchMidx;
                        this.Tpoint.x = Xdup;
                        this.Tpoint.y = Ydup;
                    }
                    else //No Dup.
                    {
                        if (this.IsRawSlideMPolEdge)
                        {
                            Tpoint.x = PolM.Pts[Midx].x;//Sliding전 Midx위치는 Sliding후 Midx+1과 동일
                            Tpoint.y = PolM.Pts[Midx].y;//Sliding전 Midx위치는 Sliding후 Midx+1과 동일
                            this.Midx = this.BagMidx[this.Midx + 1];
                        }
                        else
                        {
                            this.Sidx = this.BagSidx[this.Sidx + 1];
                            Tpoint.x = PolS.Pts[Sidx].x;//Sidx+1로 Sliding하므로 SidNext로 저장
                            Tpoint.y = PolS.Pts[Sidx].y;//Sidx+1로 Sliding하므로 SidNext로 저장
                        }
                    }

                // Run Sliding & Update NFP
                    bool isM_Moved = (this.PenetRatio != 1);
                    if (isM_Moved)
                    {
                        this.PolM.Shift(this.SldVec);
                    }

                // Check Terminate Condition
                    bool needUpdateNFP = isM_Moved;
                    if (nfpPList.Count() > 2)
                    {
                        TerminateCheck(ref returnQuality, ref isGoodTerminate, nfpPList, nfpPList_Closed, ref needUpdateNFP);
                    }

                // Update NFP
                    if (needUpdateNFP)
                    {
                        // Declare Temp. Variables
                        bool isHaveSameArc = false;
                        int ia = nfpPList.Count() - 2;
                        int ib = nfpPList.Count() - 1;//LastIdx
                        bool needExtendNFP = false;
                        // Check whether extend the line
                        if (ia >= 0)
                        {
                            double dab = D.DabNorml(nfpPList[ia].x, nfpPList[ia].y,
                                                nfpPList[ib].x, nfpPList[ib].y,
                                                this.PolM.Pts[Midx0].x, this.PolM.Pts[Midx0].y);
                            isHaveSameArc = (Math.Abs(nfpPList[ia].A - nfpPList[ib].A) < NFP.TOL);
                            needExtendNFP = ((Math.Abs(dab) < NFP.TOLD) && isHaveSameArc);
                        }
                        // Update NFP
                        if (needExtendNFP)
                        {
                            nfpPList[ib].x = this.PolM.Pts[Midx0].x; //새 포인트 추가 대신, 기존 포인트 수정
                            nfpPList[ib].y = this.PolM.Pts[Midx0].y;
                            nfpPList_Closed[ib].x = this.PolM.Pts[Midx0].x;
                            nfpPList_Closed[ib].y = this.PolM.Pts[Midx0].y;
                            //if (this.isDrawXpider) X.SendMsg($"Extend NFP ({nfpPList.Last().x},{nfpPList.Last().y})", 1);
                            //if (this.isDrawXpider) X.DrawPoint(nfpPList.Last(), 1);
                            //if (this.isDrawXpider) X.DrawPolygon(this.PolM);
                        }
                        else
                        {
                            nfpPList.Add(new POLY_POINT(this.PolM.Pts[Midx0]));
                            nfpPList_Closed.Add(new POLY_POINT(this.PolM.Pts[Midx0]));
                            //if (this.isDrawXpider) X.SendMsg($"Add NFP ({nfpPList.Last().x},{nfpPList.Last().y})", 1);
                            //if (this.isDrawXpider) X.DrawPoint(nfpPList.Last(), 1);
                            //if (this.isDrawXpider) X.DrawPolygon(this.PolM);
                        }
                    }

                // Check Abnormal Terminate Condition - Exact Fit Case없다는 가정
                    CorrectNfp_SelfContact(ref returnQuality, ref isGoodTerminate, ref nfpPList, iLoop);

                // Do Terminate Condition
                    if (isGoodTerminate) break;
            }//End for(int iLoop = 0; iLoop<MaxLoop; ++iLoop)

            if (!isGoodTerminate)
            {
                ///////////디버그용 임시 추가////////////////////// xpider 추가하기
                //var test = (PolS.DabDir(Sidx, PolM.Pdir[Midx].x, PolM.Pdir[Midx].y) > NFP.TOL);


                //////////////////////////////////
                returnQuality = 3;
                //if (this.isDrawXpider)
                //{
                //    //Console.ReadLine();//PAUSE
                //}
            }
            //=================================
            // Return
            //=================================
            if (nfpPList.Count() < 3)
            {


                returnQuality = 10;
                //if (this.isDrawXpider)
                //{
                //    M.print("ERROR: NFP Curve Not Created");
                //    //Console.ReadLine();//PAUSE
                //}
            }
            //Console.WriteLine($"GapMax = {GapMax}");
            Debug.Assert(nfpPList.Count() > 0);
            return new POLYGON(nfpPList);
        }//End public static POLYGON GetNFP_Orbital_Arc(ref POLYGON polS, ref POLYGON polM, bool isDrawXpider)

        

        
        /// ▣▣▣▣                                                                                                  ▣▣▣▣
        /// ▣▣▣▣                                                                                                  ▣▣▣▣
        /// ▣▣▣▣                                                                                                  ▣▣▣▣
        /// ▣▣▣▣                                                                                                  ▣▣▣▣
        /// ▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣
        /// ▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣▣

    }

}//End Namespace