using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

//read from json, write to csv
namespace ConsoleApplication2
{
    class SimilarityAnalyzer
    {
        ConcurrentQueue<JToken[]> queue;
        Object thisLock = new object();
        Ngram ngram;
        Stopwatch timer;

        public SimilarityAnalyzer(string output_file_name)
        {
            ngram = new Ngram();
            timer = new Stopwatch();
        }

        private JObject readJSON(string file_path)
        {
            StreamReader sr = new StreamReader(file_path);
            string json = sr.ReadToEnd();
            JObject data = JObject.Parse(json);
            return data;
        }

        private void initQueue(string file1, string file2)
        {
            queue = new ConcurrentQueue<JToken[]>();

            JToken fninfo1 = readJSON(file1)["functions"];
            JToken fninfo2 = readJSON(file2)["functions"];

            Console.WriteLine(file1 + " functions : " + fninfo1.Count());
            Console.WriteLine(file2 + " functions : " + fninfo2.Count());
            Console.WriteLine("#Queue is setting...");

            DateTime start = DateTime.Now;
            for (int i = 0; i < fninfo1.Count(); i++)
                for (int j = 0; j < fninfo2.Count(); j++)
                    if (this.isCandidate(fninfo1[i], fninfo2[j]))
                    {
                        JToken[] couple = { fninfo1[i], fninfo2[j] };
                        queue.Enqueue(couple);
                    }
            DateTime end = DateTime.Now;
            Console.WriteLine("Filtering Time : " + (end - start));
            Console.WriteLine("#Complete.  queue size is " + queue.Count());
        }

        private bool isCandidate(JToken f1, JToken f2)
        {
            int f1_size = f1["mnemonics"].Count();
            int f2_size = f2["mnemonics"].Count();

            string ananymous_function = @"sub_[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9]";
            if (!Regex.IsMatch(f1["name"].ToString(), ananymous_function) && !Regex.IsMatch(f2["name"].ToString(), ananymous_function))
                //if (getCosineSimilarity(f1, f2) >= 0.7)
                //if (50 < f1_size && f1_size < 500 && 50 < f2_size && f2_size < 500)
                    //if(f1_size < 40 && f2_size < 40)
                    //if (f1["name"].ToString() == f2["name"].ToString())
                        return true;
            return false;
        }

        private void analyzeThread()
        {
            //multi-thread
            int NumOfThread = 30;
            Thread[] threads = new Thread[NumOfThread];
            StreamWriter writer = new StreamWriter("test.csv");
            writer.WriteLine("srcName,srcNumofMne,dstName,dstNumofMne,cosine,lcs,ngram,slope1,equal_slope2,equal_slope3,authenticity");
            writer.Close();

            for (int i = 0; i < NumOfThread; i++)
            {
                threads[i] = new Thread(new ThreadStart(delegate () { target(i); }));
                threads[i].Start();
            }
            //Process currentProcess = Process.GetCurrentProcess();
            //foreach (ProcessThread pThread in currentProcess.Threads)
            //    pThread.ProcessorAffinity = currentProcess.ProcessorAffinity;
            for (int i = 0; i < NumOfThread; i++)
            {
                threads[i].Join();
            }
        }

        private void target(int i)
        {
            JToken[] couple = new JToken[2];
            
            while (true)
            {
                lock (thisLock)
                {
                    if (queue.IsEmpty)
                        break;
                    queue.TryDequeue(out couple);
                }
                
                string srcName = couple[0]["name"].ToString();
                int srcAddr = (int)couple[0]["addr"];
                int srcNumOfMne = couple[0]["mnemonics"].Count();
                string dstName = couple[1]["name"].ToString();
                int dstAddr = (int)couple[1]["addr"];
                int dstNumOfMne = couple[1]["mnemonics"].Count();
                double cosine_similarity = getCosineSimilarity(couple[0], couple[1]);
                string cosine_time = timer.Elapsed.ToString();
                double mnemonic_similarity = getMnemonicLCS(couple[0], couple[1]);
                EditDistance.ngram_similarity ngram_similarity = getNgramEditDistance(couple[0], couple[1], 8);
                string ngram_time = timer.Elapsed.ToString();
                bool authenticity = (couple[0]["name"].ToString() == couple[1]["name"].ToString());

                lock (thisLock)
                {
                    StreamWriter writer = File.AppendText("test.csv");
                    writer.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", srcName, srcNumOfMne, dstName, dstNumOfMne, cosine_similarity, mnemonic_similarity, ngram_similarity.ngram_edit_distance, ngram_similarity.slope1_ratio, ngram_similarity.continuous_equal_slope2, ngram_similarity.continuous_equal_slope3, authenticity);
                    writer.Close();
                }
            }
        }

        private double getCosineSimilarity(JToken f1, JToken f2)
        {
            timer.Reset();
            timer.Start();
            string name1, name2;
            int blocks1, blocks2, edges1, edges2, calls1, calls2, cmps1, cmps2;
            name1 = f1["name"].ToString(); name2 = f2["name"].ToString();
            blocks1 = (int)f1["blocks"]; blocks2 = (int)f2["blocks"];
            edges1 = (int)f1["edges"]; edges2 = (int)f2["edges"];
            calls1 = (int)f1["calls"]; calls2 = (int)f2["calls"];
            cmps1 = (int)f1["cmps"]; cmps2 = (int)f2["cmps"];

            int a = cmps1 * cmps2 + blocks1 * blocks2 + calls1 * calls2 + edges1 * edges2;
            double b = Math.Sqrt(cmps1 * cmps1 + blocks1 * blocks1 + calls1 * calls1 + edges1 * edges1);
            double c = Math.Sqrt(cmps2 * cmps2 + blocks2 * blocks2 + calls2 * calls2 + edges2 * edges2);

            double cosine_similarity = a / (b * c) * Math.Min(b, c) / Math.Max(b, c);
            timer.Stop();
            return cosine_similarity;
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
            timer.Reset();
            timer.Start();
            List<string> mnemonics1 = this.toStringList(f1["mnemonics"].ToList());
            List<string> mnemonics2 = this.toStringList(f2["mnemonics"].ToList());

            int min = Math.Min(mnemonics1.Count(), mnemonics2.Count());
            if(mnemonics1.Count() > min)
                mnemonics1.RemoveRange(min, mnemonics1.Count() - min);
            if(mnemonics2.Count() > min)
                mnemonics2.RemoveRange(min, mnemonics2.Count() - min);
            if (min < n)
                n = min;

            /*
            int count1 = mnemonics1.Count();
            int count2 = mnemonics2.Count();
            int max = Math.Max(count1, count2);
            if (mnemonics1.Count() < max)
                for (int i = 0; i < max - count1; i++)
                    mnemonics1.Add("");
            if (mnemonics2.Count() < max)
                for (int i = 0; i < max - count2; i++)
                    mnemonics2.Add("");
            if (max < n)
                n = max;
            */

            EditDistance ed = new EditDistance();
            ed.ngramset_edit_distance(mnemonics1, mnemonics2, n);
            timer.Stop();

            return ed.similarity;
        }

        public void run(string input_file1, string input_file2)
        {
            DateTime start = DateTime.Now;
            initQueue(input_file1, input_file2);
            analyzeThread();
            DateTime end = DateTime.Now;
            Console.WriteLine("execution time : " + (end-start));
        }
    }
}
