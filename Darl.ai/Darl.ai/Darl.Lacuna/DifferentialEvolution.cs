/// <summary>
/// </summary>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Lacuna
{
    /// <summary>
    /// Summary description for DifferentialEvolution.
    /// </summary>
    internal class DifferentialEvolution
    {
        /// <summary>
        /// 
        /// </summary>
        internal DifferentialEvolution()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        /// <summary>
        /// Differential Evolution is available with several different mutation schemes. This identifies them
        /// </summary>
        internal enum Method
        {
            /// <summary>
            /// DE/rand/1/bin
            /// </summary>
            method1,
            /// <summary>
            /// DE/current-to-rand/1
            /// </summary>
            method2,
            /// <summary>
            /// DE/rand/1/exp
            /// </summary>
            method3,
            /// <summary>
            /// DE/current-to-rand/l/bin
            /// </summary>
            method4
        };
        /// <summary>
        /// Used to set the mutation scheme
        /// </summary>
        internal Method method;

        /// <summary>
        /// Number of parameters
        /// </summary>
        internal int D;
        /// <summary>
        /// Population size
        /// </summary>
        internal int NP;
        /// <summary>
        /// Scale factor [0,1]
        /// </summary>
        internal double F;
        /// <summary>
        /// Coefficient of combination [0,1]
        /// </summary>
        internal double K;
        /// <summary>
        /// Crossover control constant [0,1]
        /// </summary>
        internal double CR;
        /// <summary>
        /// maximum number of generations
        /// </summary>
        internal int Gmax;
        /// <summary>
        /// upper parameter bounds
        /// </summary>
        internal double[] hi;
        /// <summary>
        /// lower parameter bounds
        /// </summary>
        internal double[] lo;
        /// <summary>
        /// Scaling factor for sharing 
        /// </summary>
        internal double sigma;
        /// <summary>
        /// The population array
        /// </summary>
        private double[,] x;
        /// <summary>
        /// Array of processed costs.
        /// </summary>
        /// <remarks>These are the confidence figures modified by the sharing algorithm</remarks>
        internal double[] cost;
        /// <summary>
        /// Array of pre-processed costs
        /// </summary>
        /// <remarks>These are the rule confidences</remarks>
        internal double[] rawcost;

        internal static Random random = new Random();
        internal KDTree tree;
        /// <summary>
        /// Differential Evolution, Appendix 6.A, New ideas in optimisation
        /// </summary>
        /// <returns>the population of solutions</returns>
        internal async Task<double[,]> Evolve(LacunaFinder lacfind)
        {
            x = new double[NP, D];
            cost = new double[NP];
            rawcost = new double[NP];
            double[] temp = new double[D];
            int generation = 0;
            for (int i = 0; i < NP; i++)
            {
                for (int j = 0; j < D; j++)
                    x[i, j] = lo[j] + random.NextDouble() * (hi[j] - lo[j]);
                for (int j = 0; j < D; j++)
                    temp[j] = x[i, j];
                cost[i] = await lacfind.Evaluate(temp);
                rawcost[i] = cost[i];
            }
            tree = new KDTree(x);
            for (int i = 0; i < NP; i++)
            {
                this.SharingAlgorithm(x, ref cost, i);
            }

            do
            {
                for (int i = 0; i < NP; i++)     /* Population index, i, begins with 0 */
                {
                    int r1;
                    int r2;
                    int r3;
                    do r1 = (int)(random.NextDouble() * NP); while (r1 == i);
                    do r2 = (int)(random.NextDouble() * NP); while (r2 == i || r2 == r1);
                    do r3 = (int)(random.NextDouble() * NP); while (r3 == i || r3 == r1 || r3 == r2);
                    //different mutation methods
                    switch (method)
                    {
                        case Method.method2:
                            Method1(ref temp, x, i, r1, r2, r3);
                            break;
                        case Method.method3:
                            Method2(ref temp, x, i, r1, r2, r3);
                            break;
                        case Method.method4:
                            Method3(ref temp, x, i, r1, r2, r3);
                            break;
                        default:
                            Method1(ref temp, x, i, r1, r2, r3);
                            break;
                    }
                    double score = await lacfind.Evaluate(temp);
                    if (score <= cost[i])
                    {
                        for (int j = 0; j < D; j++)
                            x[i, j] = temp[j];
                        cost[i] = score;
                        rawcost[i] = score;
                        this.SharingAlgorithm(x, ref cost, i);
                    }
                }
                generation++;
            } while (generation < Gmax);
            return x;
        }
        /// <summary>
        /// DE/rand/1/bin
        /// </summary>
        /// <param name="i">indexes solutions</param>
        /// <param name="r1">is one random solution choice</param>
        /// <param name="r2">is another random solution choice</param>
        /// <param name="r3">is yet another random solution choice</param>
        /// <param name="x">is the set of solutions</param>
        /// <param name="temp">mutated solution</param>
        private void Method1(ref double[] temp, double[,] x, int i, int r1, int r2, int r3)
        {
            int j = (int)(random.NextDouble() * D);
            for (int k = 1; k <= D; k++)
            {
                if (random.NextDouble() < CR || k == D)
                    temp[j] = x[r3, j] + F * (x[r1, j] - x[r2, j]);
                else
                    temp[j] = x[i, j];
                temp[j] = BoundsCheck(temp[j], x[i, j], j);
                j = (j + 1) % D;
            }
        }
        /// <summary>
        /// DE/current-to-rand/1
        /// </summary>
        /// <param name="i">indexes solutions</param>
        /// <param name="r1">is one random solution choice</param>
        /// <param name="r2">is another random solution choice</param>
        /// <param name="r3">is yet another random solution choice</param>
        /// <param name="x">is the set of solutions</param>
        /// <param name="temp">mutated solution</param>
        private void Method2(ref double[] temp, double[,] x, int i, int r1, int r2, int r3)
        {
            for (int j = 0; j < D; j++)
            {
                temp[j] = x[i, j] + K * (x[r3, j] - x[i, j]) + F * (x[r1, j] - x[r2, j]);
                temp[j] = BoundsCheck(temp[j], x[i, j], j);
            }
        }
        /// <summary>
        /// DE/rand/1/exp
        /// </summary>
        /// <param name="i">indexes solutions</param>
        /// <param name="r1">is one random solution choice</param>
        /// <param name="r2">is another random solution choice</param>
        /// <param name="r3">is yet another random solution choice</param>
        /// <param name="x">is the set of solutions</param>
        /// <param name="temp">mutated solution</param>
        private void Method3(ref double[] temp, double[,] x, int i, int r1, int r2, int r3)
        {
            int j = (int)(random.NextDouble() * D);
            bool flag = false;
            for (int k = 1; k <= D; k++)
            {
                if (random.NextDouble() < CR || k == D)
                    flag = true;
                if (flag)
                    temp[j] = x[r3, j] + F * (x[r1, j] - x[r2, j]);
                else
                    temp[j] = x[i, j];
                temp[j] = BoundsCheck(temp[j], x[i, j], j);
                j = (j + 1) % D; /*   % = modulo; index j runs from 0 to D-1    */
            }
        }
        /// <summary>
        /// DE/current-to-rand/l/bin
        /// </summary>
        /// <param name="i">indexes solutions</param>
        /// <param name="r1">is one random solution choice</param>
        /// <param name="r2">is another random solution choice</param>
        /// <param name="r3">is yet another random solution choice</param>
        /// <param name="x">is the set of solutions</param>
        /// <param name="temp">mutated solution</param>
        private void Method4(ref double[] temp, double[,] x, int i, int r1, int r2, int r3)
        {
            int j = (int)(random.NextDouble() * D);
            for (int k = 1; k <= D; k++)
            {
                if (random.NextDouble() < CR || k == D)
                {
                    temp[j] = x[i, j] + K * (x[r3, j] - x[i, j]) + F * (x[r1, j] - x[r2, j]);
                }
                else temp[j] = x[i, j];
                temp[j] = BoundsCheck(temp[j], x[i, j], j);
                j = (j + 1) % D; /*   % = modulo; index j runs from 0 to D-1   */
            }
        }
        private double BoundsCheck(double newVal, double existing, int j)
        {
            if (newVal < lo[j])
                newVal = (existing + lo[j]) / 2;
            if (newVal > hi[j])
                newVal = (existing + hi[j]) / 2;
            return newVal;
        }
        private void SharingAlgorithm(double[,] x, ref double[] cost, int i)
        {
            if (x == null)
            {
                throw new ArgumentNullException(nameof(x));
            }

            double[] vector = new double[D];
            for (int j = 0; j < D; j++)
                vector[j] = x[i, j];
            List<KDTreePoint> distances = new List<KDTreePoint>();
            tree.Search(vector, D + 1, ref distances);
            double scale = 0.0;
            foreach (KDTreePoint element in distances)
            {
                scale += Math.Exp(element.distSquared * -1.0 * sigma);
            }
            scale /= distances.Count;
            cost[i] = cost[i] / scale;
        }

        /// <summary>
        /// De Jong's test functions number 5
        /// </summary>
        /// <remarks>input data range -65.536 to +65.536
        /// otherwise known as Shekel's foxholes</remarks>
        /// <param name="coord">input array of 2 doubles</param>
        /// <returns>fitness</returns>
        private static double DeJongF5(double[] coord)
        {
            double[] xcoefficients = { -32, -16, 0, 16, 32, -32, -16, 0, 16, 32, -32, -16, 0, 16, 32, -32, -16, 0, 16, 32, -32, -16, 0, 16, 32 };
            double[] ycoefficients = { -32, -32, -32, -32, -32, -16, -16, -16, -16, -16, 0, 0, 0, 0, 0, 16, 16, 16, 16, 16, 32, 32, 32, 32, 32 };
            double sum = 0.0;
            for (int n = 0; n < 25; n++)
            {
                double x = coord[0] - xcoefficients[n];
                x = x * x * x * x * x * x;
                double y = coord[1] - ycoefficients[n];
                y = y * y * y * y * y * y;
                sum += 1.0 / (n + x + y);
            }
            return (0.002 + sum);
        }
    }
}
