using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockAlign.Algorithm
{
    class AkimaAlgorithm
    {
        private double[] mX;
        private double[] mY;

        private double[] mM;
        private double[,] mPara;
        private double[] tL;
        private double[] tR;
        private double[] alp;

        private bool IsClosed;

        public AkimaAlgorithm(double[] x, double[] y)
        {
            mX = x;
            mY = y;

            if (mX.Length < 5)
                throw new Exception("AkimaAlgorithm data must be more that 5!");

            if (Math.Abs(mY[0] - mY[mY.Length - 1]) < 1.0E-8)
                IsClosed = true;

            MakingMArray();
        }

        private void MakingMArray() // Only for non-periodic case
        {
            mM = new double[mX.Length + 3];

            for(int i=0; i<mX.Length-1; i++)
            {
                mM[i + 2] = (mY[i + 1] - mY[i]) / (mX[i + 1] - mX[i]);
                // mM[mX.Length]까지만 존재하는데
            }

            if (IsClosed)
            {
                mM[0] = mM[mX.Length - 1];
                mM[1] = mM[mX.Length];
                mM[mX.Length + 1] = mM[2];
                mM[mX.Length + 2] = mM[3];

            }
            else
            {
                mM[0] = 3.0 * mM[2] - 2.0 * mM[3];
                mM[1] = 2.0 * mM[2] - mM[3];
                mM[mX.Length + 1] = 2.0 * mM[mX.Length] - mM[mX.Length - 1];
                mM[mX.Length + 2] = 3.0 * mM[mX.Length] - 2.0 * mM[mX.Length - 1];
            }
        }

        public void LearnProcess()
        {
            tL = new double[mX.Length];
            tR = new double[mX.Length];
            alp = new double[mX.Length];

            for (int i=0; i<mX.Length; i++)
            {
                double NE = Math.Abs(mM[i + 3] - mM[i+2]) + Math.Abs(mM[i + 1] - mM[i]);

                if(NE > 0)
                {
                    alp[i] = Math.Abs(mM[i + 1] - mM[i]) / NE;
                    tL[i] = (1 - alp[i]) * mM[i + 1] + alp[i] * mM[i + 2];
                    tR[i] = tL[i]; 
                }
                else
                {
                    tL[i] = mM[i + 1];
                    tR[i] = mM[i + 2];
                }
            }

            mPara = new double[mX.Length-1, 4];

            for(int i=0; i<mX.Length-1; i++)
            {
                mPara[i, 0] = mY[i];
                mPara[i, 1] = tR[i];
                mPara[i, 2] = (3.0 * mM[i + 2] - 2.0 * tR[i] - tL[i + 1])/(mX[i+1]-mX[i]);
                mPara[i, 3] = (tR[i] + tL[i + 1] - 2.0*mM[i+2]) / Math.Pow(mX[i + 1] - mX[i], 2.0);
            }
        }

        public double Calculated(double x)
        {
            int v_idx = -1;

            for(int i=0; i<mX.Length-1; i++)
            {
                if(x>=mX[i] && x< mX[i+1])
                {
                    v_idx = i; break;
                }
            }

            if (v_idx == -1)
                throw new Exception("Akima Calculation Error - index Error!");
            double X = (x - mX[v_idx]);
            return mPara[v_idx, 0] + mPara[v_idx, 1] * X + mPara[v_idx, 2] * X * X + mPara[v_idx, 3] * X * X * X;
        }
    }
}
