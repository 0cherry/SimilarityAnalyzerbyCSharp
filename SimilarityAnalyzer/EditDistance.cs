using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using GraphAlgorithms;

namespace ConsoleApplication2
{
    class EditDistance
    {
        public ngram_similarity similarity;

        public struct ngram_similarity {
            public double ngram_edit_distance;
            public double edit_distance;
            public double slope1_ratio;
            public double continuous_equal_slope2;
            public double continuous_equal_slope3;

            public ngram_similarity(double ngram_edit_distance, double edit_distance, double slope1_ratio, double equal_slope2, double equal_slope3)
            {
                this.ngram_edit_distance = ngram_edit_distance;
                this.edit_distance = edit_distance;
                this.slope1_ratio = slope1_ratio;
                this.continuous_equal_slope2 = equal_slope2;
                this.continuous_equal_slope3 = equal_slope3;
            }
        }

        public EditDistance()
        {

        }

        private void printMatrix(int[,] matrix)
        {
            Console.WriteLine("Matrix:");
            var size = matrix.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    Console.Write("{0,2:0}", matrix[i, j]);
                Console.WriteLine();
            }
        }

        private void printMaxValue(int[,] matrix, int[] array)
        {
            Console.WriteLine("MaxValue:");
            for (int i = 0; i < array.Length; i++)
                Console.Write(matrix[i, array[i]] + " ");
            Console.WriteLine();
        }

        private double getTotal(int[,] matrix, int[] array)
        {
            int standard = array.Length / 2;
            double total = 0;
            for (int i = 0; i < array.Length; i++)
                total = total + (2*standard)-matrix[i, array[i]];
            return total;
        }

        private int[,] ngram_matrix(List<List<string>> set1, List<List<string>> set2)
        {
            int standard = set1.Count()/2;
            int[,] matrix = new int[set1.Count(), set2.Count()];
            Ngram ngram = new Ngram();

            double count = set1[0].Count();
            for (int i = 0; i < set1.Count(); i++)
            {
                for (int j = 0; j < set2.Count(); j++)
                {
                    int lcs = ngram.lcs(set1[i], set2[j]);
                    if (lcs < (count * 0.7))
                        lcs = 0;
                    matrix[i, j] = (2*standard)-lcs;
                }
            }
            return matrix;
        }

        private double calc_edit_distance(List<string> mnemonics1, List<string> mnemonics2)
        {
            int n = mnemonics1.Count;
            int m = mnemonics2.Count;
            int[,] distance = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; distance[i, 0] = i++) ;
            for (int i = 0; i <= m; distance[0, i] = i++) ;

            for(int i=1; i<=n; i++)
                for(int j=1; j<=m; j++)
                {
                    int cost = (mnemonics1[j - 1] == mnemonics2[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }

            return distance[n, m]/(float)n;
        }

        public void ngramset_edit_distance(List<string> mnemonics1, List<string> mnemonics2, int n)
        {
            Ngram ngram1 = new Ngram(mnemonics1, n);
            Ngram ngram2 = new Ngram(mnemonics2, n);

            int set_size = ngram1.ngramSet.Count;
            //int reduced_set_size = set_size  - Math.Max(mnemonics1.FindIndex(delegate (string data) { return data == ""; }), mnemonics2.FindIndex(delegate (string data) { return data == ""; }));
            int ngram_size = n;

            int[,] matrix = ngram_matrix(ngram1.ngramSet, ngram2.ngramSet);
            var hungarian = new HungarianAlgorithm(matrix);
            int[] hungarian_indexes = hungarian.Run();
            
            double ngram_edit_distance = getTotal(matrix, hungarian_indexes)/set_size/ngram_size;
            double edit_distance = calc_edit_distance(mnemonics1, mnemonics2);
            double slope1_ratio = calculate_slope1_ratio(hungarian_indexes);
            double index_similarity2 = calculate_continuous_equal_slope(hungarian_indexes, 2);
            double index_similarity3 = calculate_continuous_equal_slope(hungarian_indexes, 3);

            this.similarity = new ngram_similarity(ngram_edit_distance, edit_distance, slope1_ratio, index_similarity2, index_similarity3);
        }

        private double calculate_continuous_equal_slope(int[] indexes, int level)
        {
            double indexes_slope;
            double total = indexes.Length - level;
            int continuous_count = 0;

            for (int i=0; i<total; i++)
            {
                int count = 0;
                for(int j=i; j<i+level; j++)
                {
                    if ( (indexes[j+1] - indexes[j] ) == 1 )
                        count++;
                }
                continuous_count += count / level;
            }
            indexes_slope = continuous_count / total;
            return indexes_slope;
        }

        private double calculate_slope1_ratio(int[] indexes)
        {
            double slope1_ratio;
            int slope1_count = 0;
            double total = indexes.Length;

            for (int i=0; i<total; i++)
            {
                if (i == indexes[i])
                    slope1_count++;
            }

            slope1_ratio = slope1_count / total;
            return slope1_ratio;
        }
    }
}
