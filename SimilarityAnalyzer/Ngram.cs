using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication2
{
    class Ngram
    {
        public int n;
        public List<List<string>> ngramSet = new List<List<string>>();

        public Ngram()
        {

        }

        public Ngram(List<string> mnemonics, int n)
        {
            this.n = n;
            for(int i=0; i<mnemonics.Count()-n+1; i++)
            {
                List<string> ngram = new List<string>();
                for (int j=i; j<n+i; j++)
                {
                    ngram.Add(mnemonics[j].ToString());
                    //mnemonics.CopyTo(mnemonic, i, i+n);
                }
                ngramSet.Add(ngram);
            }
        }

        public double compare(List<List<string>> P, List<List<string>> Q, double threshold)
        {
            int count = 0;
            foreach (List<string> ngram1 in P)
                foreach (List<string> ngram2 in Q)
                    if (match(ngram1, ngram2, threshold))
                    {
                        count += 1;
                        break;
                    }
            double similarity = count * 2 / (P.Count + Q.Count);
            return similarity;
        }

        private bool match(List<string> ngram1, List<string> ngram2, double threshold)
        {
            int common_lenth = lcs(ngram1, ngram2);
            double match_score = (common_lenth * 2 / (ngram1.Count + ngram2.Count));
            if (match_score > threshold)
                return true;
            else
                return false;
        }

        public int lcs(List<string> ngram1, List<string> ngram2)
        {
            int[,] lengths = new int[ngram1.Count+1, ngram2.Count+1];
            lengths.Initialize();
            for (int i = 0; i < ngram1.Count; i++)
                for (int j = 0; j < ngram2.Count; j++)
                {
                    //Console.WriteLine(ngram1[i].ToString() == ngram2[j].ToString());
                    if (ngram1[i].ToString() == ngram2[j].ToString())
                        lengths[i + 1, j + 1] = lengths[i, j] + 1;
                    else
                        lengths[i + 1, j + 1] = Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
                }
            int mlcs = lengths[ngram1.Count, ngram2.Count];
            return mlcs;
        }
    }
}
