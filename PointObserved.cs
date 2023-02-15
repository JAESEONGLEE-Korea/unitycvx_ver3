using Accord.MachineLearning;
using Accord.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;



namespace BlockAlign.Numeric
{
    public class PointObserved
    {
        List<RuyVector> mPoints;

        public PointObserved()
        {
            mPoints = new List<RuyVector>();
        }
        public int getNumOfPoints()
        {
            return mPoints.Count;
        }
        public double getValue(int index, int indexXYZ)
        {
            if (indexXYZ == 0) return getPointX(index);
            else if(indexXYZ == 1) return getPointY(index);
            else return getPointZ(index);
        }
        public double getPointX(int n)
        {
            return mPoints[n].X;
        }
        public double getPointY(int n)
        {
            return mPoints[n].Y;
        }
        public double getPointZ(int n)
        {
            return mPoints[n].Z;
        }

        public static bool ParseText(string tstring, out double x, out double y, out double z)
        {
            bool result = false; x = y = z = 0.0;
            string[] ds = tstring.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (ds.Length == 2)
            {
                string[] ds1 = ds[1].Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if(ds1.Length == 3)
                {
                    x = double.Parse(ds1[0]); y = double.Parse(ds1[1]); z = double.Parse(ds1[2]);
                    result = true;
                }
            }
            return result;
        }
        private void ReadMatrix(double[,] data, bool insert = false)
        {
            try
            {
                mPoints = new List<RuyVector>();

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    mPoints.Add(new RuyVector(data[i, 0], data[i, 1], data[i, 2]));
                }
            }
            catch (Exception)
            {
                throw new Exception("PointObserved double[,]");
            }

            if(insert == true)
            {
                List<RuyVector> aPoints = new List<RuyVector>();
                for (int i = 0; i < mPoints.Count; i++)
                {
                    double length;
                    if (i != mPoints.Count - 1)
                        length = RuyVector.DistanceBtwTwoPoints(mPoints[i], mPoints[i + 1]);
                    else
                        length = RuyVector.DistanceBtwTwoPoints(mPoints[i], mPoints[0]);

                    aPoints.Add(mPoints[i]);
                    if(length > 7000.0)
                    {
                        if (i != mPoints.Count - 1)
                            aPoints.Add(RuyVector.GetCenterPoint(mPoints[i], mPoints[i + 1]));
                        else
                            aPoints.Add(RuyVector.GetCenterPoint(mPoints[i], mPoints[0]));
                    }

                }
                mPoints = aPoints;
            }
        }

        public PointObserved(double[,] data, bool insert = false)
        {
            ReadMatrix(data, insert);
        }

        //public PointObserved(string fileName, bool insert = false)
        //{
        //    if (System.IO.Path.GetExtension(fileName).ToUpper() == ".XLSX")
        //    {
        //        DataTable table = new Accord.IO.ExcelReader(fileName).GetWorksheet(0);

        //        double[,] matrix = Accord.Math.Matrix.ToMatrix(table);
        //        ReadMatrix(matrix, insert);
        //    }
        //    else
        //    {
        //        try
        //        {
        //            Accord.IO.CsvReader aReader = new Accord.IO.CsvReader(fileName, false);
        //            double[,] matrix = aReader.ToMatrix<double>();
        //            ReadMatrix(matrix, insert);
        //        }
        //        catch (Exception)
        //        {
        //            throw new Exception();
        //        }
                
        //    }
        //}

        public void TranslateAllPoint(double x, double y, double z)
        {
            for(int i=0; i<mPoints.Count; i++)
            {
                mPoints[i].Translate(x, y, z);
            }
        }
        public void TranslateAllPoint(double[] x)
        {
           TranslateAllPoint(x[0], x[1], x[2]);
           
        }

        public void ToFile(string fileName)
        {
            StreamWriter sw = new StreamWriter(fileName);
            for (int i = 0; i < mPoints.Count; i++)
            {
                sw.WriteLine(mPoints[i].ToString());
            }
            sw.Close();
        }

        public void RotateAllPoint(double angle, double u, double v, double w, double a, double b, double c)
        {
            for (int i = 0; i < mPoints.Count; i++)
            {
                mPoints[i].RotationAround(angle, u, v, w, a, b, c);
            }
        }

        public void RotateAllPoint(double angle, double[] u, double[] a)
        {
            RotateAllPoint(angle, u[0], u[1], u[2], a[0], a[1], a[2]);
        }

        public void Step1(int n, double x, double y, double z)
        {
            // 지정된 점을 어떤 지점에 고정시킨다. // n번째 점을 중심으로 xyz좌표에 놓아둔다.
            // 단순한 Translattion으로 대처한다.
            RuyVector designatedPont = mPoints[n];
            RuyVector targetPoint = new RuyVector(x, y, z);

            RuyVector aVec = targetPoint - designatedPont;

            TranslateAllPoint(aVec.ToArray());
        }

        public void Step2(int m, int n, double x, double y, double z)
        {
            // Step1 점(m)을 고정시키고 n점을 m과 x점의 벡터 상위에 올려놓는다. alignment를 진행한다.
            // 회전을 위한 기준 벡터를 정한다. n-m 벡터와 x-m 벡터를 외적해서 기준 벡터를 정한다.
            // 해당 m점과 기준벡터를 이용하여 정규화된 n-m벡터와 x-m 벡터의 내적값 만큼 회전시킨다.
            RuyVector nm = mPoints[n] - mPoints[m];
            RuyVector xm = new RuyVector(x, y, z) - mPoints[m];

            RuyVector rotationbase = nm * xm; // 외적을 구함.
            // 각도를 구하자.
            double theta = Math.Acos(RuyVector.Inner(nm.normalize(), xm.normalize()));
            //// 준비완료
            RotateAllPoint(theta, rotationbase.ToArray(), mPoints[m].ToArray());
        }

        public void Step3(int m, int n, int l, double x, double y, double z)
        {
            // 직선 nm (m을 지난다)을 기준으로 돌린다. 
            // nm를 기준으로 몇도를 돌려야지 가장 최소의 rework이 소비되는지를 결정한다.
            // 혹은 주어진 각도에서의 비용을 산출한다.
            RuyVector mn = mPoints[m] - mPoints[n];RuyVector b = mPoints[n];RuyVector X = mPoints[l];
            // 우선 l과 가장 가까운 nm위에서의 점을 구한다. 이를 위해서 t값을 설정  
            // (t*mn+b - X)*(mn) = 0 그러므로 t = (X * mn - b * mn)/|mn|^2         
            double t = (RuyVector.Inner(mn, X) - RuyVector.Inner(mn, b)) / Math.Pow(mn.GetMagnitude(), 2.0);
            RuyVector aVectorOnNM = t * mn + b;
            double theta = Math.Acos(RuyVector.Inner(aVectorOnNM - X, aVectorOnNM - new RuyVector(x, y, z)));
            RotateAllPoint(theta, mn.ToArray(), mPoints[m].ToArray());
        }

        public void getRelatedData(int n, out double[,] x, out double [,] y, out double[,] z, bool IsClosed)
        {
            double length = 0;

            for (int i = n; i < mPoints.Count; i++)
            { 
                if(i != mPoints.Count -1)
                    length += RuyVector.DistanceBtwTwoPoints(mPoints[i], mPoints[i + 1]);
                
                if(i == mPoints.Count-1 && IsClosed)
                {
                        length += RuyVector.DistanceBtwTwoPoints(mPoints[i], mPoints[0]);
                }
             }
            int add = IsClosed == true ? 1 : 0;
            x = new double[mPoints.Count + add, 3];
            y = new double[mPoints.Count + add, 3];
            z = new double[mPoints.Count + add, 3];

            double tleng = 0.0; /// 0점도 추가해야 한다.
            for (int i = n; i < mPoints.Count; i++)
            {
                if (i == n)
                {
                    x[i, 0] = tleng / length; x[i, 1] = 0.0; x[i, 2] = mPoints[i].X;
                    y[i, 0] = tleng / length; y[i, 1] = 0.0; y[i, 2] = mPoints[i].Y;
                    z[i, 0] = tleng / length; z[i, 1] = 0.0; z[i, 2] = mPoints[i].Z;
                }
                else
                {
                    tleng += RuyVector.DistanceBtwTwoPoints(mPoints[i - 1], mPoints[i]);
                    x[i, 0] = tleng / length; x[i, 1] = 0.0; x[i, 2] = mPoints[i].X;
                    y[i, 0] = tleng / length; y[i, 1] = 0.0; y[i, 2] = mPoints[i].Y;
                    z[i, 0] = tleng / length; z[i, 1] = 0.0; z[i, 2] = mPoints[i].Z;
                }

                if (i == mPoints.Count - 1 && IsClosed)
                {
                    tleng += RuyVector.DistanceBtwTwoPoints(mPoints[i], mPoints[0]);
                    x[i + 1, 0] = tleng / length; x[i + 1, 1] = 0.0; x[i + 1, 2] = mPoints[0].X;
                    y[i + 1, 0] = tleng / length; y[i + 1, 1] = 0.0; y[i + 1, 2] = mPoints[0].Y;
                    z[i + 1, 0] = tleng / length; z[i + 1, 1] = 0.0; z[i + 1, 2] = mPoints[0].Z;
                }

            }

            length = 0.0;
            for (int i = n; i > 0; i--)
            {
                length += RuyVector.DistanceBtwTwoPoints(mPoints[i], mPoints[i - 1]);
            }


            tleng = 0.0; /// 0점도 추가해야 한다.
            for (int i = n-1; i >= 0; i--)
            {
                tleng += RuyVector.DistanceBtwTwoPoints(mPoints[i+1], mPoints[i]);
                x[i, 0] = -tleng / length; x[i, 1] = 0.0; x[i, 2] = mPoints[i].X;
                y[i, 0] = -tleng / length; y[i, 1] = 0.0; y[i, 2] = mPoints[i].Y;
                z[i, 0] = -tleng / length; z[i, 1] = 0.0; z[i, 2] = mPoints[i].Z;
            }
        }

        public void FittingCurves(int n, int refN, out double[,] points,
                                  out double[] distanceX, bool IsAkima,
                                  double rbase, int nlayer, double reg, bool isclosed = true)
        {

            double[,] x, y, z;

            getRelatedData(refN, out x, out y, out z, isclosed);
            

            distanceX = new double[x.GetLength(0)];
            points = new double[n, 3];
            for (int i = 0; i < x.GetLength(0); i++)
                distanceX[i] = x[i, 0];

            if (IsAkima)
            {
                double[] xOnly, yOnly, zOnly;
                xOnly = new double[x.GetLength(0)];
                yOnly = new double[y.GetLength(0)];
                zOnly = new double[z.GetLength(0)];
                for(int i=0; i<x.GetLength(0); i++)
                {
                    xOnly[i] = x[i, 2];
                    yOnly[i] = y[i, 2];
                    zOnly[i] = z[i, 2];
                }

                Algorithm.AkimaAlgorithm aAkimaX = new Algorithm.AkimaAlgorithm(distanceX, xOnly);
                Algorithm.AkimaAlgorithm aAkimaY = new Algorithm.AkimaAlgorithm(distanceX, yOnly);
                Algorithm.AkimaAlgorithm aAkimaZ = new Algorithm.AkimaAlgorithm(distanceX, zOnly);
                aAkimaX.LearnProcess();
                aAkimaY.LearnProcess();
                aAkimaZ.LearnProcess();

                for (int i = 0; i < n; i++)
                {
                    double xvalue = distanceX[0] + i * (distanceX[distanceX.Length - 1] - distanceX[0]) / n;
                    points[i, 0] = aAkimaX.Calculated(xvalue);
                    points[i, 1] = aAkimaY.Calculated(xvalue);
                    points[i, 2] = aAkimaZ.Calculated(xvalue);
                }
            }
            else
            {
                alglib.rbfmodel modelx, modely, modelz;
                alglib.rbfreport rep;

                alglib.rbfcreate(2, 1, out modelx);
                alglib.rbfsetpoints(modelx, x);
                alglib.rbfsetalgomultilayer(modelx, rbase, nlayer, reg);
                alglib.rbfbuildmodel(modelx, out rep);

                alglib.rbfcreate(2, 1, out modely);
                alglib.rbfsetpoints(modely, y);
                alglib.rbfsetalgomultilayer(modely, rbase, nlayer, reg);
                alglib.rbfbuildmodel(modely, out rep);

                alglib.rbfcreate(2, 1, out modelz);
                alglib.rbfsetpoints(modelz, z);
                alglib.rbfsetalgomultilayer(modelz, rbase, nlayer, reg);
                alglib.rbfbuildmodel(modelz, out rep);

                for (int i = 0; i < n; i++)
                {
                    double xvalue = distanceX[0] + i * (distanceX[distanceX.Length - 1] - distanceX[0]) / n;
                    points[i, 0] = alglib.rbfcalc2(modelx, xvalue, 0.0);
                    points[i, 1] = alglib.rbfcalc2(modely, xvalue, 0.0);
                    points[i, 2] = alglib.rbfcalc2(modelz, xvalue, 0.0);
                }
            }
        }
        // obsoleted!!
        //public int ClassificationPoints(int index)
        //{
        //    int result = 0;
        //    double[][] observation = new double[mPoints.Count][];
        //    double[] xAxis = new double[mPoints.Count];
        //    double[] yAxis = new double[mPoints.Count];

        //    for (int i=0;i<mPoints.Count; i++)
        //    {
        //        observation[i] = new double[1] { mPoints[i].getValue(index) };
        //        xAxis[i] = mPoints[i].getValue(index);
        //    }

        //    KMeans kmeans = new KMeans(2);

        //    int[] ret = kmeans.Compute(observation); // obsoleted!!
        //    //KMeansClusterCollection km = kmeans.Learn(observation)
        //    for (int i = 0; i < ret.Length; i++)
        //        yAxis[i] = ret[i];

        //    //ScatterplotBox.Show("aa", xAxis, yAxis);

        //    List<RuyVector> tempList = new List<RuyVector>();
        //    for(int i=0; i<ret.Length; i++)
        //    {
        //        //if()
        //    }


        //    return result;
        //}
    }
}
