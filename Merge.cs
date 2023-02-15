using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NFP
{
    public class VERTEX
    {
        public double X;
        public double Y;
        public double SweepAngle = 0;

        public VERTEX(double x, double y, double A)
        {
            this.X = x;
            this.Y = y;
            this.SweepAngle = A;
        }
        public VERTEX(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.SweepAngle = 0;
        }
        public VERTEX(VERTEX other)
        {
            this.X = other.X;
            this.Y = other.Y;
            this.SweepAngle = other.SweepAngle;
        }
        public void Move(double dx, double dy)
        {
            this.X += dx;
            this.Y += dy;
        }
        public static VERTEX operator +(VERTEX a, VERTEX b)
        {
            VERTEX result = new VERTEX(a.X + b.X, a.Y + b.Y);
            return result;
        }
        public static VERTEX operator -(VERTEX a, VERTEX b)
        {
            VERTEX result = new VERTEX(a.X - b.X, a.Y - b.Y);
            return result;
        }
    }
    public class New__Vertex__ : VERTEX
    {
        public New__Vertex__(bool chk, VERTEX other) : base(other)
        {
            this.isCrossPoint = chk;
        }
        public New__Vertex__(bool chk, double x, double y, double sweepAngle = 0) : base(x, y, sweepAngle)
        {
            this.isCrossPoint = chk;
        }

        public bool isCrossPoint;
    }

    public class Merge
    {
        public Merge(List<VERTEX> station, List<VERTEX> orbital)
        {
            foreach (var q1 in orbital)  //나중에 없어도되면 여기 카피는 지우기
            {
                double q1X = q1.X;
                double q1Y = q1.Y;
                double q1A = q1.SweepAngle;

                orbitalFix.Add(new VERTEX(q1X, q1Y, q1A));
            }
            foreach (var w1 in station)
            {
                double w1X = w1.X;
                double w1Y = w1.Y;
                double w1A = w1.SweepAngle;

                stationFix.Add(new VERTEX(w1X, w1Y, w1A));
            }

            sPoly.Clear();
            oPoly.Clear();
            mergedPolygon.Clear();
            stationFix.Add(stationFix[0]);
            orbitalFix.Add(orbitalFix[0]);
            MakeSPoly(stationFix, orbitalFix);
            MakeOPoly(stationFix, orbitalFix);

            GetMergedPolygon(sPoly, oPoly);
        }


        List<VERTEX> FindCrossPoint(Tuple<VERTEX, VERTEX> sEdge, Tuple<VERTEX, VERTEX> oEdge/*, bool IsMakeSPoly*/)
        {
            if (sEdge.Item1.SweepAngle == 0 && oEdge.Item1.SweepAngle == 0)
            {
                return LineToLine(sEdge, oEdge/*, IsMakeSPoly*/);
            }
            else
            {
                Exception e = new Exception();
                throw (e);
            }
        }

        public List<VERTEX> GetMerge()
        {
            return mergedPolygon;
        }


        /// <summary>
        /// sPoly작성 시 station이 매개변수 앞으로오게, oPoly작성시 orbital이 앞으로 오게해야 함
        /// </summary>
        /// <param name="sEdge"></param>
        /// <param name="oEdge"></param>
        /// <returns></returns>
        private List<VERTEX> LineToLine(Tuple<VERTEX, VERTEX> sEdge, Tuple<VERTEX, VERTEX> oEdge/*, bool IsMakeSPoly*/) //line과 line의 경우 sEdge와 OEdge사이에서 교점은 하나만 생성되야 한다
        {
            double a = sEdge.Item1.Y - sEdge.Item2.Y;
            double b = sEdge.Item2.X - sEdge.Item1.X;
            double c = -sEdge.Item1.Y * (sEdge.Item2.X - sEdge.Item1.X) + sEdge.Item1.X * (sEdge.Item2.Y - sEdge.Item1.Y);
            double d1;
            double d2;

            List<VERTEX> pointList = new List<VERTEX>();

            d1 = (a * oEdge.Item1.X + b * oEdge.Item1.Y + c) / (Math.Sqrt(a * a + b * b)); //oEdge.Item1와 sEdge사이의 거리
            d2 = (a * oEdge.Item2.X + b * oEdge.Item2.Y + c) / (Math.Sqrt(a * a + b * b)); //oEdge.Item2와 sEdge사이의 거리

            d1 = Math.Round(d1, 4);
            d2 = Math.Round(d2, 4);

            if (Math.Abs(d1) <= epsilon)  //oEdge.Item1가 접할때
            {
                double x = (sEdge.Item1.X - oEdge.Item1.X) * (sEdge.Item2.X - oEdge.Item1.X);
                double y = (sEdge.Item1.Y - oEdge.Item1.Y) * (sEdge.Item2.Y - oEdge.Item1.Y);

                if ((Math.Abs(sEdge.Item1.X - oEdge.Item1.X) < epsilon) && (Math.Abs(sEdge.Item1.Y - oEdge.Item1.Y) < epsilon))  //sEdge의 시작점과 일치하는 oEdge.Item1
                {
                    pointList.Add(oEdge.Item1);
                }
                else if ((Math.Abs(sEdge.Item2.X - oEdge.Item1.X) < epsilon) && (Math.Abs(sEdge.Item2.Y - oEdge.Item1.Y) < epsilon))  //sEdge의 끝점과 일치하는 oEdge.Item1
                {
                    pointList.Add(oEdge.Item1);
                }
                else if (x < epsilon && y < epsilon)  //벡터의 양 끝점이 아닌 oEdge.Item1
                {
                    pointList.Add(oEdge.Item1);
                }
            }

            if (Math.Abs(d2) <= epsilon)  //base의 oEdge.Item2가 접할때
            {
                double x = (sEdge.Item1.X - oEdge.Item2.X) * (sEdge.Item2.X - oEdge.Item2.X);
                double y = (sEdge.Item1.Y - oEdge.Item2.Y) * (sEdge.Item2.Y - oEdge.Item2.Y);

                if ((Math.Abs(sEdge.Item1.X - oEdge.Item2.X) < epsilon) && (Math.Abs(sEdge.Item1.Y - oEdge.Item2.Y) < epsilon))
                {
                    pointList.Add(oEdge.Item2);
                }
                else if ((Math.Abs(sEdge.Item2.X - oEdge.Item2.X) < epsilon) && (Math.Abs(sEdge.Item2.Y - oEdge.Item2.Y) < epsilon))
                {
                    pointList.Add(oEdge.Item2);
                }
                else if (x < epsilon && y < epsilon)  //벡터의 양 끝점이 아닌 접점
                {
                    pointList.Add(oEdge.Item2);
                }
            }

            /////////////////////////////////////


            double a2 = oEdge.Item1.Y - oEdge.Item2.Y;
            double b2 = oEdge.Item2.X - oEdge.Item1.X;
            double c2 = -oEdge.Item1.Y * (oEdge.Item2.X - oEdge.Item1.X) + oEdge.Item1.X * (oEdge.Item2.Y - oEdge.Item1.Y);
            double d3;
            double d4;

            d3 = (a2 * sEdge.Item1.X + b2 * sEdge.Item1.Y + c2) / (Math.Sqrt(a2 * a2 + b2 * b2)); //sEdge.Item1와 oEdge사이의 거리
            d4 = (a2 * sEdge.Item2.X + b2 * sEdge.Item2.Y + c2) / (Math.Sqrt(a2 * a2 + b2 * b2)); //sEdge.Item2와 oEdge사이의 거리

            d3 = Math.Round(d3, 4);
            d4 = Math.Round(d4, 4);

            if (Math.Abs(d3) <= epsilon)  //oEdge.Item1가 접할때
            {
                double x = (oEdge.Item1.X - sEdge.Item1.X) * (oEdge.Item2.X - sEdge.Item1.X);
                double y = (oEdge.Item1.Y - sEdge.Item1.Y) * (oEdge.Item2.Y - sEdge.Item1.Y);

                if ((Math.Abs(oEdge.Item1.X - sEdge.Item1.X) < epsilon) && (Math.Abs(oEdge.Item1.Y - sEdge.Item1.Y) < epsilon))  //sEdge의 시작점과 일치하는 oEdge.Item1
                {
                    pointList.Add(sEdge.Item1);
                }
                else if ((Math.Abs(oEdge.Item2.X - sEdge.Item1.X) < epsilon) && (Math.Abs(oEdge.Item2.Y - sEdge.Item1.Y) < epsilon))  //sEdge의 끝점과 일치하는 oEdge.Item1
                {
                    pointList.Add(sEdge.Item1);
                }
                else if (x < epsilon && y < epsilon)  //벡터의 양 끝점이 아닌 oEdge.Item1
                {
                    pointList.Add(sEdge.Item1);
                }
            }

            if (Math.Abs(d4) <= epsilon)  //base의 oEdge.Item2가 접할때
            {
                double x = (oEdge.Item1.X - sEdge.Item2.X) * (oEdge.Item2.X - sEdge.Item2.X);
                double y = (oEdge.Item1.Y - sEdge.Item2.Y) * (oEdge.Item2.Y - sEdge.Item2.Y);

                bool condition1 = EqualTo2(oEdge.Item1, sEdge.Item2);  //sEdge의 시작점과 일치하는 oEdge.Item2
                bool condition2 = EqualTo2(oEdge.Item2, sEdge.Item2);  //sEdge의 끝점과 일치하는 oEdge.Item2

                if ((Math.Abs(oEdge.Item1.X - sEdge.Item2.X) < epsilon) && (Math.Abs(oEdge.Item1.Y - sEdge.Item2.Y) < epsilon))
                {
                    pointList.Add(sEdge.Item2);
                }
                else if ((Math.Abs(oEdge.Item2.X - sEdge.Item2.X) < epsilon) && (Math.Abs(oEdge.Item2.Y - sEdge.Item2.Y) < epsilon))
                {
                    pointList.Add(sEdge.Item2);
                }
                else if (x < epsilon && y < epsilon)  //벡터의 양 끝점이 아닌 접점
                {
                    pointList.Add(sEdge.Item2);
                }
            }


            for (int u = 0; u < pointList.Count; u++)
            {
                if (EqualTo2(sEdge.Item2, pointList[u]))  //pointList의 요소중  sEdge의 끝점에 해당하는 나머지 요소들 중에서 같은 요소를 지운다;
                {
                    pointList.Remove(pointList[u]);
                    u -= 1;
                }
            }

            for (int u = 1; u < pointList.Count; u++)
            {
                if (EqualTo2(pointList[0], pointList[u]))  //pointList의 요소중 [0]을기준으로 같은 값을 지운다
                {
                    pointList.Remove(pointList[u]);
                    u -= 1;
                }
            }

            //////////////////points에 중복된 값을 지운다
            ///
            ModPointList(pointList);

            //Debug.Assert(pointList.Count <= 2);  //sEdge 하나와 oEdge 하나의 검사에서, pointList의 첫요소를 기준으로 나머지 요소들 중에서 다른 점이 존재한다면 에러이다

            return pointList;
        }

        List<VERTEX> points = new List<VERTEX>();
        void MakeSPoly(List<VERTEX> station, List<VERTEX> orbital)
        {
            for (int i = 0; i < station.Count - 1; i++)
            {
                points.Clear();
                ////////sEdge에 대해서 orbital의 모든 Edge를 돌려서 교차점을 찾는다 => 자연스럽게 모든 교차점은 sEdge상에 놓인다
                for (int j = 0; j < orbital.Count - 1; j++)
                {
                    var sEdge = new Tuple<VERTEX, VERTEX>(station[i], station[i + 1]);
                    var oEdge = new Tuple<VERTEX, VERTEX>(orbital[j], orbital[j + 1]);

                    if (FindCrossPoint(sEdge, oEdge/*, true*/).Count > 0)
                    {
                        points.AddRange(FindCrossPoint(sEdge, oEdge/*, true*/));
                    }
                }

                //////////////////points에 중복된 값을 지운다
                ///
                ModPointList(points);

                ////////////////sEdge상의 모든 교차점에 대한 조사가 끝난 후 sPoly에 값을 저장한다
                ///
                if (points.Count > 1) //sEdge상에서 교점이 1개보다 많을 때
                {
                    var sorted = points.OrderBy(x => DistanceTo2(x, station[i])).ToList();   //교점을 sEdge와의 거리순으로 정렬; 가장 가까운순서로 앞에서부터 정렬

                    ///////sorted된 points중 첫번째 요소만 따로 sEdge의 시작점과 같은지 검사한다
                    if (EqualTo2(sorted[0], station[i]))
                    {
                        var pt = new New__Vertex__(true, station[i]);
                        sPoly.Add(pt);
                    }
                    else if (EqualTo2(sorted[0], station[i + 1])) { Debug.Assert(false); }  //교점이 여러개인데 sorted의 첫 요소가 끝점일 경우 에러
                    else  //sEdge의 중간에서 만나는 교점
                    {
                        var pt1 = new New__Vertex__(false, station[i]);
                        var pt2 = new New__Vertex__(true, sorted[0]);
                        sPoly.Add(pt1);  //sEdge의 시작점 부터 입력
                        sPoly.Add(pt2);  //sEdge의 시작점 다음에 교차점이 위치하므로 시작점 뒤에 입력
                    }

                    ///////첫번째 요소를 제외한 sorted의 나머지 요소들을 검사한다
                    for (int k = 1; k < sorted.Count(); k++)
                    {
                        if (EqualTo2(sorted[k], station[i])) { Debug.Assert(false); } //sorted[0]을 위에서 검사했으므로 그 다음 교차점이 sEdge의 시작점과 같을 수 없음
                        else if (EqualTo2(sorted[k], station[i + 1])) { }  //교점이 끝점일 경우 패스
                        else  //sEdge의 중간에서 만나는 교점
                        {
                            var pt = new New__Vertex__(true, sorted[k]);

                            foreach (var item in sPoly)
                            {
                                if (item.isCrossPoint)
                                {
                                    if (Math.Abs(item.X - pt.X) < 1.0E-8 && Math.Abs(item.Y - pt.Y) < 1.0E-8) pt.isCrossPoint = false;
                                }
                            }
                            sPoly.Add(pt);  //교차점 입력
                        }
                    }
                }
                else if (points.Count == 1) //sEdge상에서 교점이 1개일 때
                {
                    if (EqualTo2(points[0], station[i]))
                    {
                        var pt = new New__Vertex__(true, station[i]);
                        foreach (var item in sPoly)
                        {
                            if (item.isCrossPoint)
                            {
                                if (Math.Abs(item.X - pt.X) < 1.0E-8 && Math.Abs(item.Y - pt.Y) < 1.0E-8) pt.isCrossPoint = false;
                            }
                        }
                        sPoly.Add(pt);
                    }
                    else if (EqualTo2(points[0], station[i + 1])) { }  //교점이 끝점일 경우 패스
                    else  //sEdge의 중간에서 만나는 교점
                    {
                        var pt1 = new New__Vertex__(false, station[i]);
                        var pt2 = new New__Vertex__(true, points[0]);
                        sPoly.Add(pt1);  //sEdge의 시작점 부터 입력
                        sPoly.Add(pt2);  //sEdge의 시작점 다음에 교차점이 위치하므로 시작점 뒤에 입력
                    }
                }
                else  //교점이 없을 때
                {
                    var pt1 = new New__Vertex__(false, station[i]);
                    sPoly.Add(pt1);
                }
            }

            //station의 마지막점이 교차점인 경ㄴ우도 포함하기 + oPoly생성과정에서 Debug.Assert메세지 박스 뜨는 부분 검사하기
        }

        void MakeOPoly(List<VERTEX> station, List<VERTEX> orbital)
        {
            for (int i = 0; i < orbital.Count - 1; i++)
            {
                points.Clear();
                ////////oEdge에 대해서 station의 모든 Edge를 돌려서 교차점을 찾는다 => 자연스럽게 모든 교차점은 oEdge상에 놓인다
                for (int j = 0; j < station.Count - 1; j++)
                {
                    var oEdge = new Tuple<VERTEX, VERTEX>(orbital[i], orbital[i + 1]);
                    var sEdge = new Tuple<VERTEX, VERTEX>(station[j], station[j + 1]);

                    if (FindCrossPoint(oEdge, sEdge/*, false*/).Count > 0)
                    {
                        points.AddRange(FindCrossPoint(oEdge, sEdge/*, false*/));
                    }
                }

                //////////////////points에 중복된 값을 지운다
                ///
                ModPointList(points);

                ////////////////oEdge상의 모든 교차점에 대한 조사가 끝난 후 oPoly에 값을 저장한다
                ///
                if (points.Count > 1) //sEdge상에서 교점이 1개보다 많을 때
                {
                    var sorted = points.OrderBy(x => DistanceTo2(x, orbital[i])).ToList();   //교점을 sEdge와의 거리순으로 정렬; 가장 가까운순서로 앞에서부터 정렬

                    ///////sorted된 points중 첫번째 요소만 따로 oEdge의 시작점과 같은지 검사한다
                    if (EqualTo2(sorted[0], orbital[i]))
                    {
                        var pt = new New__Vertex__(true, orbital[i]);
                        oPoly.Add(pt);
                    }
                    else if (EqualTo2(sorted[0], orbital[i + 1])) { Debug.Assert(false); }  //교점이 여러개인데 sorted의 첫 요소가 끝점일 경우 에러
                    else  //oEdge의 중간에서 만나는 교점
                    {
                        var pt1 = new New__Vertex__(false, orbital[i]);
                        var pt2 = new New__Vertex__(true, sorted[0]);
                        oPoly.Add(pt1);  //oEdge의 시작점 부터 입력
                        oPoly.Add(pt2);  //oEdge의 시작점 다음에 교차점이 위치하므로 시작점 뒤에 입력
                    }

                    ///////첫번째 요소를 제외한 sorted의 나머지 요소들을 검사한다
                    for (int k = 1; k < sorted.Count(); k++)
                    {
                        if (EqualTo2(sorted[k], orbital[i])) { Debug.Assert(false); } //sorted[0]을 위에서 검사했으므로 그 다음 교차점이 oEdge의 시작점과 같을 수 없음
                        else if (EqualTo2(sorted[k], orbital[i + 1])) { }  //교점이 끝점일 경우 패스
                        else  //oEdge의 중간에서 만나는 교점
                        {
                            var pt = new New__Vertex__(true, sorted[k]);

                            foreach (var item in oPoly)
                            {
                                if (item.isCrossPoint)
                                {
                                    if (Math.Abs(item.X - pt.X) < 1.0E-8 && Math.Abs(item.Y - pt.Y) < 1.0E-8) pt.isCrossPoint = false;
                                }
                            }

                            oPoly.Add(pt);  //교차점 입력
                        }
                    }
                }
                else if (points.Count == 1) //oEdge상에서 교점이 1개일 때
                {
                    if (EqualTo2(points[0], orbital[i]))
                    {
                        var pt = new New__Vertex__(true, orbital[i]);

                        foreach (var item in oPoly)
                        {
                            if (item.isCrossPoint)
                            {
                                if (Math.Abs(item.X - pt.X) < 1.0E-8 && Math.Abs(item.Y - pt.Y) < 1.0E-8) pt.isCrossPoint = false;
                            }
                        }
                        oPoly.Add(pt);
                    }
                    else if (EqualTo2(points[0], orbital[i + 1])) { }  //교점이 끝점일 경우 패스
                    else  //oEdge의 중간에서 만나는 교점
                    {
                        var pt1 = new New__Vertex__(false, orbital[i]);
                        var pt2 = new New__Vertex__(true, points[0]);
                        oPoly.Add(pt1);  //oEdge의 시작점 부터 입력
                        oPoly.Add(pt2);  //oEdge의 시작점 다음에 교차점이 위치하므로 시작점 뒤에 입력
                    }
                }
                else  //교점이 없을 때
                {
                    var pt1 = new New__Vertex__(false, orbital[i]);
                    oPoly.Add(pt1);
                }
            }
        }


        static void ModPointList(List<VERTEX> pointList)
        {
            for (int u = 0; u < pointList.Count - 1; u++)
            {
                for (int r = u + 1; r < pointList.Count; r++)
                {
                    if (EqualTo2(pointList[u], pointList[r]))  //pointList의 요소중 중복된 값을 지운다
                    {
                        pointList.RemoveAt(r);

                        r = r - 1;
                    }
                }
            }
        }

        public static bool EqualTo2(VERTEX a, VERTEX b)
        {
            double dis;

            dis = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
            dis = Math.Round(dis, 2);
            if (dis <= 2.0E-2) { return true; }
            else { return false; }
        }

        public static double DistanceTo2(VERTEX a, VERTEX b)
        {
            double dis;

            dis = Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));

            return dis;
        }

        void GetMergedPolygon(List<New__Vertex__> sPoly, List<New__Vertex__> oPoly)
        {
            int num = 0;
            foreach (var n in sPoly)
            {
                if (n.isCrossPoint)
                {
                    num++;
                }
            }

            if (num > 1) { MergeOption1(sPoly, oPoly); }
            else if (num == 1) { MergeOption2(sPoly, oPoly); }
            //else { Debug.Assert(false); }
        }


        void MergeOption1(List<New__Vertex__> sPoly, List<New__Vertex__> oPoly)
        {
            List<New__Vertex__> result = new List<New__Vertex__>();
            mergedPolygonList.Clear();
            sNum = 0;
            oNum = 0;
            bool condition = false;
            List<int> idx = new List<int>();
            var timeUp = (sPoly.Count + oPoly.Count) * 2;
            bool IsOver = false;

            //타블릿 merge파일에 step 적음

            //step1 : sPoly의 마지막 교차점의 index 검색
            idx = StartIndex2(sPoly);

            foreach (var index in idx)
            {
                mergedPolygon.Clear();
                result.Clear();
                sNum = index;
                ////step2 : 마지막 교차점으로 부터 sPoly 끝까지 리스트에 저장
                //result.AddRange(sPoly.GetRange(index, sPoly.Count() - index/*1*/));

                //step5
                while (true)
                {
                    //step3 : sPoly의 시작점부터 다음 교차점까지 리스트에 저장
                    while (true)
                    {
                        if (sNum == sPoly.Count) { sNum = 0; }
                        if (sPoly[sNum].isCrossPoint && result.Count != 0)  //교차점일 경우 + 처음 시작점이 아닐 때
                        {
                            if (EqualTo2(sPoly[sNum], result[0])) { condition = true; } //step4에서의 결과가 result의 첫 요소와 같은지 비교해보고 맞으면 종료, 아니면 속행

                            break;
                        }
                        else  //교차점이 아닐경우
                        {
                            result.Add(sPoly[sNum]);
                            sNum++;
                        }
                    }

                    if (condition) { break; }

                    //step4 : oPoly의 시작점부터 다음 교차점까지 리스트에 저장
                    oNum = FindOPolyIndex(oPoly, sPoly[sNum]);
                    result.Add(oPoly[oNum]);
                    oNum++;
                    if (oNum == oPoly.Count) { oNum = 0; }
                    while (true)
                    {
                        if (oPoly[oNum].isCrossPoint)  //교차점일 경우
                        {
                            //condition3 = false;
                            break;
                        }
                        else  //교차점이 아닐경우
                        {
                            result.Add(oPoly[oNum]);
                            oNum++;
                            if (oNum == oPoly.Count) { oNum = 0; }
                        }
                    }

                    sNum = FindSPolyIndex(sPoly, oPoly[oNum]);  //oPoly의 다음 교차점이 sPoly의 어떤 교차점과 닿아있는지 검색

                    if (EqualTo2(sPoly[sNum], result[0])) { break; } //step4에서의 결과가 result의 첫 요소와 같은지 비교해보고 맞으면 종료, 아니면 속행
                    else
                    {
                        result.Add(sPoly[sNum]);
                        sNum++;
                    }

                    if (result.Count > timeUp) { IsOver = true; break; }
                }

                if(IsOver) { IsOver = false; result.Clear();}
                if (result.Count > 2)
                {
                    foreach (var v in result)
                    {
                        var ve = new VERTEX(v.X, v.Y, v.SweepAngle);
                        mergedPolygon.Add(ve);
                    }
                }
                mergedPolygonList.Add(new List<VERTEX>(mergedPolygon));
            }

            for (int i = 0; i < mergedPolygonList.Count; i++)
            {
                if (mergedPolygonList[i].Count > 0)
                {
                    y = (from pt in mergedPolygonList[i]
                         orderby pt.X ascending
                         select pt).ToList();
                    u = (from pt in mergedPolygonList[i]
                         orderby pt.Y ascending
                         select pt).ToList();
                    g = (y.Last().X - y[0].X) + (u.Last().Y - u[0].Y);

                    if (g > old_g)
                    {
                        old_g = g;
                        recIdx = i;
                    }
                }
            }

            mergedPolygon = mergedPolygonList[recIdx];
        }

        void MergeOption2(List<New__Vertex__> sPoly, List<New__Vertex__> oPoly)
        {
            List<New__Vertex__> result = new List<New__Vertex__>();
            int sNum = 0;
            int oNum = 0;
            bool condition = false;
            //bool condition2 = true;
            //bool condition3 = true;


            //타블릿 merge파일에 step 적음

            //step1 : sPoly의 마지막 교차점의 index 검색
            int idx = StartIndex(sPoly);

            //step2 : 마지막 교차점으로 부터 sPoly 끝까지 리스트에 저장
            result.AddRange(sPoly.GetRange(idx, sPoly.Count() - idx/*1*/));


            //step5
            while (true)
            {
                //step3 : sPoly의 시작점부터 다음 교차점까지 리스트에 저장
                while (true)
                {
                    if (sPoly[sNum].isCrossPoint) { break; }  //교차점일 경우
                    else  //교차점이 아닐경우
                    {
                        result.Add(sPoly[sNum]);
                        sNum++;
                    }
                }

                //step4 : oPoly의 시작점부터 다음 교차점까지 리스트에 저장
                oNum = FindOPolyIndex(oPoly, sPoly[sNum]);
                result.Add(oPoly[oNum]);
                oNum++;
                if (oNum == oPoly.Count) { oNum = 0; }
                while (true)
                {
                    if (oPoly[oNum].isCrossPoint)  //교차점일 경우
                    {
                        condition = true;
                        break;
                    }
                    else  //교차점이 아닐경우
                    {
                        result.Add(oPoly[oNum]);
                        oNum++;
                        if (oNum == oPoly.Count) { oNum = 0; }
                    }
                }

                if (condition) { break; }


                sNum = FindSPolyIndex(sPoly, oPoly[oNum]);  //oPoly의 다음 교차점이 sPoly의 어떤 교차점과 닿아있는지 검색

                if (EqualTo2(sPoly[sNum], result[0])) { break; } //step4에서의 결과가 result의 첫 요소와 같은지 비교해보고 맞으면 종료, 아니면 속행
                else
                {
                    result.Add(sPoly[sNum]);
                    sNum++;
                }
            }

            foreach (var v in result)
            {
                var ve = new VERTEX(v.X, v.Y, v.SweepAngle);
                mergedPolygon.Add(ve);
            }
        }

        int StartIndex(List<New__Vertex__> sPoly)
        {
            int result = 0;

            foreach (var pt in sPoly)
            {
                if (pt.isCrossPoint)
                {
                    int num = sPoly.IndexOf(pt);

                    if (num > result)
                    {
                        result = num;
                    }
                }
            }

            return result;
        }

        List<int> StartIndex2(List<New__Vertex__> sPoly)
        {
            List<int> result = new List<int>();

            foreach (var pt in sPoly) { if (pt.isCrossPoint) result.Add(sPoly.IndexOf(pt)); }
            return result;
        }

        int FindOPolyIndex(List<New__Vertex__> oPoly, New__Vertex__ sPoint)
        {
            int result = -1;

            foreach (var oPt in oPoly)
            {
                if (oPt.isCrossPoint)
                {
                    if (EqualTo2(oPt, sPoint))
                    {
                        result = oPoly.IndexOf(oPt);
                    }
                }
                else { /*Debug.Assert(false);*/ }
            }

            Debug.Assert(result >= 0);

            return result;
        }

        int FindSPolyIndex(List<New__Vertex__> sPoly, New__Vertex__ oPoint)
        {
            int result = -1;

            foreach (var sPt in sPoly)
            {
                if (sPt.isCrossPoint)
                {
                    if (EqualTo2(sPt, oPoint))
                    {
                        result = sPoly.IndexOf(sPt);
                    }
                }
                else { /*Debug.Assert(false);*/ }
            }

            Debug.Assert(result >= 0);

            return result;
        }


        private List<VERTEX> mergedPolygon = new List<VERTEX>();
        List<VERTEX> y;
        List<VERTEX> u;
        private List<List<VERTEX>> mergedPolygonList = new List<List<VERTEX>>();
        public List<New__Vertex__> sPoly = new List<New__Vertex__>(); //교차점 정보를 포함한 station에 대한 리스트
        public List<New__Vertex__> oPoly = new List<New__Vertex__>(); //교차점 정보를 포함한 orbital에 대한 리스트
        List<VERTEX> stationFix = new List<VERTEX>();
        List<VERTEX> orbitalFix = new List<VERTEX>();
        const double epsilon = 2.0E-2;
        double g; double old_g = int.MinValue; int recIdx; int sNum; int oNum;
    }
}
