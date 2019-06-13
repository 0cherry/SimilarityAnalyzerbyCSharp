using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using GraphAlgorithms;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication2
{
    class Program
    {
        private static int[,] generateMatrix(int size)
        {
            var matrix = new int[size, size];

            var rnd = new Random(0);
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    matrix[i, j] = -(rnd.Next() % size);

            return matrix;
        }

        private static void printMatrix(int[,] matrix)
        {
            Console.WriteLine("Matrix:");
            var size = matrix.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    Console.Write("{0,3:0}", matrix[i, j]);
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            // Run similarity analyze
            //SimilarityAnalyzer analyzer = new SimilarityAnalyzer("test.csv");
            //analyzer.run("libeay32_1.0.1f.dll_fninfo.json", "libeay32_1.0.2h.dll_fninfo.json");

            // Run packer identificate
            PackerIdentificator identificator = new PackerIdentificator();
            string dir = "D:\\PackerIdentificator\\EPmnemonics";
            string[] path = Directory.GetFiles(dir);
            for (int i = 10; i < 51; i = i + 5)
            {
                List<string> file_list = new List<string>();
                for (int j=0; j<path.Count(); j++)
                {
                    if (path[j].Contains(i.ToString()))
                    {
                        file_list.Add(path[j]);
                    }
                }
                StreamWriter writer = new StreamWriter("D:\\PackerIdentificator\\report\\" + i + ".csv");
                writer.WriteLine("name1,packer1,name2,packer2,lcs,hungarian,distance");
                for (int j = 0; j < file_list.Count; j++)
                    for (int k = j + 1; k < file_list.Count; k++)
                    {
                        measure m = new measure();
                        m = identificator.run(file_list[j], file_list[k]);
                        writer.WriteLine("{0},{1},{2},{3},{4},{5},{6}", m.name1, m.packer1, m.name2, m.packer2, m.lcs, m.hungarian, m.distance);
                    }
                writer.Close();
            }
        }
    }
}
