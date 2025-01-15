using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualHFT.Helpers
{
    public class HelperMath
    {

        public static double CalculateStdDev(IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 0)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }




        ///<summary>
        ///The Cumulative Normal Distribution function CND(double) returns values of N(x) to within double-precision accuracy
        ///This is a numerical approximation to the normal distribution.
        ///</summary>
        ///<param name="X">number with a double precision</param>
        public static double CND(double X)
        {
            double y = 0.0;
            double t = 0.0;
            double dCND = 0.0;

            const double p = 0.2316419;
            const double b1 = 0.31938153;
            const double b2 = -0.356563782;
            const double b3 = 1.781477937;
            const double b4 = -1.821255978;
            const double b5 = 1.330274429;

            /*
            L = Math.Abs(X);
            K = 1.0 / (1.0 + 0.2316419 * L);
            dCND = 1.0 - 1.0 / Math.Sqrt(2 * Convert.ToDouble(Math.PI.ToString())) * Math.Exp(-L * L / 2.0) * (a1 * K + a2 * K * K + a3 * Math.Pow(K, 3.0) + a4 * Math.Pow(K, 4.0) + a5 * Math.Pow(K, 5.0));
            */

            y = Math.Abs(X);
            double z = Math.Exp(-y * y / 2) / Math.Sqrt(2 * Math.PI);
            t = 1 / (1 + p * y);
            dCND = 1 - z * (b1 * t + b2 * t * t + b3 * t * t * t + b4 * t * t * t * t + b5 * t * t * t * t * t);
            if (X < 0)
                dCND = 1 - dCND;
            return dCND;

        }



        ///<summary>
        ///Standar Normal Density function
        ///</summary>
        ///<param name="X">number with a double precision</param>
        public static double n(double z)
        {  // normal distribution function    
            return (1.0 / Math.Sqrt(2 * Math.PI)) * Math.Exp(-0.5 * z * z);
        }





        public static double Max(params double[] values)
        {
            double dRet = 0;
            dRet = values.ToList().Max();
            return dRet;
        }
        public static double Min(params double[] values)
        {
            double dRet = 0;
            dRet = values.ToList().Min();
            return dRet;
        }

        ///<summary>
        ///Falling values: values in the array are falling in order
        /// - 1st element to last
        ///</summary>
        ///<param name="value">double array</param>
        public static bool FallingValues(params double[] values)
        {
            double dValue = double.MaxValue;
            foreach (double d in values)
            {
                if (d > dValue)
                    return false;
                dValue = d;
            }
            return true;
        }
        ///<summary>
        ///Growing values: values in the array are growing in order
        /// - 1st element to last
        ///</summary>
        ///<param name="value">double array</param>
        public static bool GrowingValues(params double[] values)
        {
            double dValue = double.MinValue;
            foreach (double d in values)
            {
                if (d < dValue)
                    return false;
                dValue = d;
            }
            return true;
        }

        public static List<System.Drawing.PointF> LinearRegression(List<System.Drawing.PointF> points)
        {
            // The PointsCollection class is some random class
            // (or it could be any appropriate data-structure)
            // that can contain your points.  YMMV, so figure out what
            // works best and build/use it.

            double sumX = 0D;  // Sum of x values
            double sumY = 0D;  // Sum of y values
            double sumX2 = 0D; // Sum of x-squared values
            double sumXY = 0D; // Sum of the products of x*y
            double m = 0D, b = 0D;

            // Since I'm assuming PointsCollection is a standard
            // collection class, get the count of the points that
            //  its InnerList contains.

            int pointCount = points.Count;

            if (pointCount == 0)
                return null;

            // Iterate through all of the points in the PointsCollections structure.
            foreach (System.Drawing.PointF p in points)
            {
                sumX += p.X;
                sumY += p.Y;
                sumX2 += (p.X * p.X);
                sumXY += (p.X * p.Y);
            }

            m = ((pointCount * sumXY) - (sumX * sumY)) / ((pointCount * sumX2) - (sumX * sumX));   // Slope
            b = (sumY - (m * sumX)) / pointCount;  //Intercept

            // This implementation just returns a string representation of the best fit line in the standard y = mx + b format..
            // You can return whatever you want instead.  
            //return "y = " + Convert.ToString(m) + "x" + " + " + Convert.ToString(b);

            List<System.Drawing.PointF> aRet = new List<System.Drawing.PointF>();
            System.Drawing.PointF point1 = new System.Drawing.PointF();
            System.Drawing.PointF point2 = new System.Drawing.PointF();
            point1.X = (float)points.Min(x => x.X);
            point1.Y = (float)(m * point1.X + b);
            point2.X = (float)points.Max(x => x.X);
            point2.Y = (float)(m * point2.X + b);

            aRet.Add(point1);
            aRet.Add(point2);
            return aRet;
        }


    }
}
