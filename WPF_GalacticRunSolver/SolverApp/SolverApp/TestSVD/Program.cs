using System;

namespace TestSVD
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TEST2();
        }

        static void TEST2()
        {
            double[,] a = {
                { 0, 0, 0,-98,-86,-1,  0,  0,  0 },
                { 98, 86,  1,  0,  0,  0,  0,  0,  0 },
                { 0,  0,  0,-119, -416, -1,  0,  0,  0},
                {  119,416,  1,  0,  0,  0,-95081, -332384,  -799},
                {  0,  0,  0,-583,-80, -1,349217, 47920,599},
                {  583, 80,  1,  0,  0,  0,  0,  0,  0},
                { 0,  0,  0,-569,-409, -1,340831,244991,599},
                { 569,409,  1,  0,  0,  0 ,-454631, -326791, -799},
            };

            // Output variables for SVD
            double[] w;       // Singular values
            double[,] u;      // Left singular vectors
            double[,] vt;     // Right singular vectors transposed

            // Perform SVD
            // Flags:
            //   true  -> compute U
            //   true  -> compute VT
            //   2     -> algorithm type (0=default, 1=QR, 2=Divide-and-Conquer)
            alglib.rmatrixsvd(a, a.GetLength(0), a.GetLength(1),
                              1, 2, 2, // compute U, VT, algorithm type
                              out w, out u, out vt);

            // Print VT
            Console.WriteLine("\nMatrix VT:");
            PrintMatrix(vt);

            //Computing all different possibilities and see if one works?
            double N = 1;
            double[,] H = new double[3, 3];

            var j = 8; //last row

            N = vt[j, 8];
            for (int i = 0; i < 3; i++)
            {
                H[i, 0] = vt[j, i * 3] / N;
                H[i, 1] = vt[j, i * 3 + 1] / N;
                H[i, 2] = vt[j, i * 3 + 2] / N;
            }
            Console.WriteLine("\n Matrix H :");
            PrintMatrix(H);

            // Find back point using H

            int info;
            alglib.matinvreport rep;
            alglib.rmatrixinverse(ref H,out info, out rep);

            Console.WriteLine("\n Matrix H inversed:");
            PrintMatrix(H);

            var point1 = GetInputPoint(0, 0, H);
            Console.WriteLine(string.Format("\nPoint : {0}   {1}", point1[0], point1[1]));

            var point2 = GetInputPoint(799, 0, H);
            Console.WriteLine(string.Format("\nPoint : {0}   {1}", point2[0], point2[1]));

            var point3 = GetInputPoint(0, 599, H);
            Console.WriteLine(string.Format("\nPoint : {0}   {1}", point3[0], point3[1]));

            var point4 = GetInputPoint(799, 599, H);
            Console.WriteLine(string.Format("\nPoint : {0}   {1}", point4[0], point4[1]));
        }

        static double[] GetInputPoint(double x, double y, double[,] H)
        {
            double[] p = new double[3] { x, y, 1 };
            double[] p2 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                p2[i] = H[i, 0] * p[0] + H[i, 1] * p[1] + H[i, 2] * p[2];
            }
            return new double[2] { (p2[0] / p2[2]), (float)(p2[1] / p2[2]) };
        }


        // Helper method to print a 2D matrix
        static void PrintMatrix(double[,] mat)
        {
            int rows = mat.GetLength(0);
            int cols = mat.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    Console.Write($"{mat[i, j],10:F6} ");
                Console.WriteLine();
            }
        }
    }
}
