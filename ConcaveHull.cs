using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;

namespace BlockAlign.Numeric
{
    class ConcaveHull
    {
        private List<RuyVector> pointsOrigin;
        private List<RuyVector> points;
        private int mK;
        private int mDirection;

        public ConcaveHull(List<RuyVector> pts, int direction, int k = 8)
        {
            // direction x = 1// direction y = 2// direction z = 3
            pointsOrigin = pts;
            mK = k;
            mDirection = direction;
            points = new List<RuyVector>();

            foreach (var pnt in pointsOrigin)
            {
                if (direction == 1) points.Add(new RuyVector(pnt.Y, pnt.Z, 0.0));
                else if (direction == 2) points.Add(new RuyVector(pnt.Z, pnt.X, 0.0));
                else if (direction == 3) points.Add(new RuyVector(pnt.X, pnt.Y, 0.0));
                else throw new ArgumentException("The direction is required to be a value among 1~3", "direction");
            }
        }

        public List<RuyVector> Run()
        {
            List<RuyVector> result_hull = new List<RuyVector>();
            var hull = ConcaveHullRun(points, mK);
            int idx;

            foreach(var pnt in hull)
            {
                if(mDirection == 1)
                {
                    idx = pointsOrigin.FindIndex(a => Math.Abs(a.Y - pnt.X) < 1.0E-8 && Math.Abs(a.Z - pnt.Y) < 1.0E-8);
                    result_hull.Add(new RuyVector(pointsOrigin[idx].X, pnt.X, pnt.Y));
                }
                else if (mDirection == 2)
                {
                    idx = pointsOrigin.FindIndex(a => Math.Abs(a.Z - pnt.X) < 1.0E-8 && Math.Abs(a.X - pnt.Y) < 1.0E-8);
                    result_hull.Add(new RuyVector(pnt.Y, pointsOrigin[idx].Y, pnt.X));
                }
                else
                {
                    idx = pointsOrigin.FindIndex(a => Math.Abs(a.X - pnt.X) < 1.0E-8 && Math.Abs(a.Y - pnt.Y) < 1.0E-8);
                    result_hull.Add(new RuyVector(pnt.X, pnt.Y, pointsOrigin[idx].Z));
                }
            }
            return result_hull;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"> 들어오는 pts는 모두 Z 부분이 0으로 세팅되어야 한다.</param>
        /// <param name="k"></param>
        /// <returns></returns>
        private List<RuyVector> ConcaveHullRun(List<RuyVector> pts, int k = 8)
        {
            points = pts; mK = k;
            if (mK < 3)
                throw new ArgumentException("K is required to be 3 or more", "k");
            List<RuyVector> hull = new List<RuyVector>();
            //Clean first 
            List<RuyVector> clean = RemoveDuplicates(points);

            if (clean.Count < 3)
                throw new ArgumentException("At least 3 dissimilar points reqired", "points");
            if (clean.Count == 3) return clean;
            if (clean.Count < mK)
                throw new ArgumentException("K must be equal to or smaller then the amount of dissimilar points", "points");

            int first_idx = FindMinYPointIndex(clean);
            if (first_idx < 0 || first_idx > clean.Count - 1)
                throw new OutOfMemoryException("FindMinYPointIndex Error");

            RuyVector firstPoint = clean[first_idx]; //TODO find mid point
            hull.Add(firstPoint);
            RuyVector currentPoint = firstPoint;
            List<RuyVector> dataset = RemoveIndex(clean, first_idx);

            int step = 2;

            while (((step == 2)||!((new RuyComparer()).Equals(firstPoint, currentPoint))) && dataset.Count >0)
            {
                if (step == 5)                
                    dataset.Add(firstPoint);

                List<RuyVector> kNearestPoints = getNearstPoints(dataset, currentPoint, mK);
                List<RuyVector> cPoints = sortByAngle(kNearestPoints, currentPoint, hull);

                bool its = true;
                int i = 0;
                while (its == true && cPoints.Count > i)
                {
                    i++;
                    int lastPoint = 0;
                    if ((new RuyComparer()).Equals(cPoints[0], firstPoint))
                        lastPoint = 1;

                    int j = 0;
                    its = false;
                    while( !its && j<hull.Count - lastPoint-1)
                    {
                        its = IsIntersectLine(currentPoint, cPoints[i-1], hull[j], hull[j+1]);
                        j++;
                    }
                }
                if (its) return ConcaveHullRun(points, mK + 1);

                currentPoint = cPoints[i-1];
                hull.Add(currentPoint);
                int datasetIdx = dataset.FindIndex(a => Math.Abs(a.X - currentPoint.X) < 1.0E-8 && Math.Abs(a.Y - currentPoint.Y) < 1.0E-8);
                dataset = RemoveIndex(dataset, datasetIdx);
                Check(hull);
                step++;
            }

            bool allInside = true;
            int idx = dataset.Count;

            //while (allInside && idx > 0)
            //{
            //    allInside = IsInPolygon(hull, dataset[idx - 1]);
            //    idx--;
            //}

            if (allInside == false)
                return ConcaveHullRun(points, mK + 1);
 
            return hull;
        }
        private bool IsIntersectLine(RuyVector sV1, RuyVector eV1, RuyVector sV2, RuyVector eV2)
        {
            // a + tb
            RuyVector a = sV1; RuyVector b = eV1 - sV1;
            RuyVector c = sV2; RuyVector d = eV2 - sV2;

            double[,] tMatrix = new double[2, 2] { { b.X, -d.X }, { b.Y, -d.Y } };
            double[] f = new double[2] { c.X - a.X, c.Y - a.Y };

            if (tMatrix.IsSingular() == true) return false; // parallel type

            double[] solved = tMatrix.Solve(f);
            if (solved[0] > 0.0 && solved[0] < 1.0 && solved[1] > 0.0 && solved[1] < 1.0)
            {
                return true;
            }
            else
                return false;
        }
        private void Check(List<RuyVector> hull)
        {
            int n = hull.Count;
            if (n > 3)
            {
                RuyVector p1 = hull[n - 1];
                RuyVector p2 = hull[n - 2];
                RuyVector p3 = hull[n - 3];

                if (IsPointOnLine(p1, p2, p3))
                {
                    hull.RemoveAt(n - 3);
                }
            }
        }
        private bool IsPointOnLine(RuyVector p1, RuyVector p2, RuyVector p3)
        {
            RuyVector v1 = p3 - p1;
            RuyVector v2 = p2 - p1;

            RuyVector cross = v1 * v2; ;

            if (cross.Z < 1.0E-8 && cross.Z > -1.0E-8)
            {
                double dot1 = RuyVector.Inner(v1, v1);
                double dot2 = RuyVector.Inner(v1, v2);

                if (dot1 <= dot2 && dot2 >= 0)
                {
                    return true;
                }
            }
            return false;
        }
        // kNearestPoints를 입력 받아서 쏘팅을 한다. 가장 큰 Angle를 가지는 순서로 Sorting
        private List<RuyVector> sortByAngle(List<RuyVector> kNearestPoints, RuyVector currentPoint, List<RuyVector> hull)
        {
            RuyVector reference;
            RuyVector evalLine;
            if (hull.Count == 1) reference = new RuyVector(-1.0E10, currentPoint.Y, 0.0);
            else reference = hull[hull.Count - 2];

            int idx = 0;
            List<RuyVector> tmpList = new List<RuyVector>();
            foreach (RuyVector aPnt in kNearestPoints)
            {
                RuyVector refer = reference - currentPoint;
                evalLine = aPnt - currentPoint;
                tmpList.Add(new RuyVector(aPnt.X, aPnt.Y, ((int)(RuyVector.getAngle(refer, evalLine)*10000))/10000.0));
                idx++;
            }

            ////////////////////////원래코드//////////////////////////////
            tmpList = (from bPnt in tmpList
                       orderby bPnt.Z ascending/*descending*/
                       select bPnt).ToList();
            var tmpList_only = (from cPnt in tmpList
                                where Math.Abs(tmpList[0].Z - cPnt.Z) < 1.0E-8
                                orderby (cPnt - currentPoint).GetMagnitude() ascending
                                select cPnt).ToList();
            List<RuyVector> resultList = new List<RuyVector>();
            for (int i = 0; i < tmpList.Count; i++)
            {
                if (i < tmpList_only.Count)
                    resultList.Add(new RuyVector(tmpList_only[i].X, tmpList_only[i].Y, 0.0));
                else
                    resultList.Add(new RuyVector(tmpList[i].X, tmpList[i].Y, 0.0));
            }

            ////////////////////////수정코드/////////////////////
            //tmpList = (from bPnt in tmpList
            //           //where Math.Abs(tmpList[0].Z - bPnt.Z) < 1.0E-8
            //           orderby bPnt.Z ascending, (bPnt - currentPoint).GetMagnitude() ascending
            //           select bPnt).ToList();

            ////////////////////////////////////////////////////
            return resultList;
        }

        private List<RuyVector> getNearstPoints(List<RuyVector> dataset, RuyVector currentPoint, int mK)
        {
            List<RuyVector> tmpList = new List<RuyVector>();
            foreach (RuyVector pts in dataset)
                tmpList.Add(pts - currentPoint);

            tmpList = (from RuyVector pts in tmpList
                         orderby pts.X * pts.X + pts.Y * pts.Y ascending
                         select pts + currentPoint).ToList();
            List<RuyVector> result = new List<RuyVector>();
            for (int i = 0; i < (new int[2] { tmpList.Count, mK }).Min() ; i++)
                result.Add(tmpList[i]);
            return result;
        }

        private List<RuyVector> RemoveDuplicates(List<RuyVector> vs)
        {
            List<RuyVector> clean = new List<RuyVector>();
            RuyComparer vc = new RuyComparer();
            foreach (RuyVector v in vs)
            {
                if (!clean.Contains(v,vc))
                    clean.Add(v);
            }
            /// vs에서도 제거를 하자. 원본 보관이 의미가 없으며
            return clean;
        }

        private List<RuyVector> RemoveIndex(List<RuyVector> vs, int index)
        {
            List<RuyVector> removed = new List<RuyVector>();
            for (int i = 0; i < vs.Count; i++)
                if (i != index)
                    removed.Add(vs[i]);
            return removed;
        }

        // after RemoveDuplication
        private int FindMinYPointIndex(List<RuyVector> vs)
        {
            var result =    (from RuyVector pts in vs
                            orderby pts.Y ascending
                            select pts).ToList();
            double yValue = (result[0] as RuyVector).Y;
            
            result = (from RuyVector pts in result
                      where Math.Abs(pts.Y - (result[0] as RuyVector).Y)<1.0E-8
                      orderby pts.X ascending
                      select pts).ToList();

            return vs.FindIndex(a => Math.Abs(a.X - result[0].X) < 1.0E-8 && Math.Abs(a.Y - result[0].Y) < 1.0E-8);
        }
        private bool IsInPolygon(List<RuyVector> poly, RuyVector point)
        {
            List<double> coef = new List<double>();

            for(int i=1; i<poly.Count; i++)
            {
                RuyVector a = poly[i - 1] - point;
                RuyVector b = poly[i] - point;
                RuyVector c = a * b;

                coef.Add(c.Z);
            }

            if (coef.Any(p => p == 0))
                return true;

            for (int i = 1; i < coef.Count(); i++)
            {
                if (coef[i] * coef[i - 1] < 0)
                    return false;
            }
            return true;
        }
    }
 }
