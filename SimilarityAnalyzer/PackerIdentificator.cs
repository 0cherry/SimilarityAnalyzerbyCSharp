using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication2
{
    public struct measure
    {
        public string name1;
        public string packer1;
        public string name2;
        public string packer2;
        public double lcs;
        public double hungarian;
        public double distance;

        public measure(string name1, string packer1, string name2, string packer2, double lcs, double hungarian, double distance)
        {
            this.name1 = name1;
            this.packer1 = packer1;
            this.name2 = name2;
            this.packer2 = packer2;
            this.lcs = lcs;
            this.hungarian = hungarian;
            this.distance = distance;
        }
    }

    class PackerIdentificator
    {
        Ngram ngram;

        

        public PackerIdentificator()
        {
            ngram = new Ngram();
        }

        private JObject readJSON(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            string json = sr.ReadToEnd();
            JObject data = JObject.Parse(json);
            return data;
        }

        private List<string> toStringList(List<JToken> mnemonics)
        {
            List<string> string_list = new List<string>();
            for (int i = 0; i < mnemonics.Count(); i++)
            {
                string_list.Add(mnemonics[i].ToString());
            }
            return string_list;
        }

        private double getMnemonicLCS(JToken f1, JToken f2)
        {
            List<string> mnemonics1 = this.toStringList(f1["mnemonics"].ToList());
            List<string> mnemonics2 = this.toStringList(f2["mnemonics"].ToList());

            double max_length = Math.Max(mnemonics1.Count(), mnemonics2.Count());
            double mnemonicLCS = ngram.lcs(mnemonics1, mnemonics2)/max_length;
            return mnemonicLCS;
        }

        private EditDistance.ngram_similarity getNgramEditDistance(JToken f1, JToken f2, int n)
        {
            List<string> mnemonics1 = this.toStringList(f1["mnemonics"].ToList());
            List<string> mnemonics2 = this.toStringList(f2["mnemonics"].ToList());

            int min = Math.Min(mnemonics1.Count(), mnemonics2.Count());
            if (mnemonics1.Count() > min)
                mnemonics1.RemoveRange(min, mnemonics1.Count() - min);
            if (mnemonics2.Count() > min)
                mnemonics2.RemoveRange(min, mnemonics2.Count() - min);
            if (min < n)
                n = min;

            EditDistance ed = new EditDistance();
            ed.ngramset_edit_distance(mnemonics1, mnemonics2, n);

            return ed.similarity;
        }

        public measure run(string input_file1, string input_file2)
        {
            JToken binary1_ep = readJSON(input_file1);
            JToken binary2_ep = readJSON(input_file2);

            string name1 = binary1_ep["name"].ToString();
            string packer1 = binary1_ep["packer"].ToString();
            string name2 = binary2_ep["name"].ToString();
            string packer2 = binary2_ep["packer"].ToString();

            double mnemonic_lcs = getMnemonicLCS(binary1_ep, binary2_ep);
            EditDistance.ngram_similarity ngram_similarity = getNgramEditDistance(binary1_ep, binary2_ep, 8);

            return new measure(name1, packer1, name2, packer2, mnemonic_lcs, ngram_similarity.ngram_edit_distance, ngram_similarity.edit_distance);
        }
    }
}
