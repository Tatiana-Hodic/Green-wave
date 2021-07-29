using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZelenaVlnaNewVersion.Models;

namespace ZelenaVlnaNewVersion.Services
{
    public static class FileManagement
    {
        //Ukládá matici do souboru standardními nástroji knihovny StreamWriter
        public static void SaveMatrixToFile(Matrix matrix, string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (Vector v in matrix.MxSpans)
                {
                    List<string> components = new List<string>();
                    foreach (double d in v.Vs)
                    {
                        components.Add(d.ToString());
                    }

                    string line = String.Join(";", components);
                    sw.WriteLine(line);
                }
                sw.Flush();
            }
        }
        //Metoda nahraje matici ze souboru standardními metodami knihovny StreamReader. Blok using zaručuje,
        //že posléze dojde k vyprázdnění dočasné paměti a že data nebudou poškozena, pokud například program během načítání spadne.
        public static Matrix LoadMatrixFromFile(string path)
        {
            string line;
            List<Vector> spans = new List<Vector>();
            using(StreamReader sr = new StreamReader(path))
            {
                while((line = sr.ReadLine()) != null)
                {
                    string[] components = line.Split(';');
                    components = line.Split(';');
                    List<double> compDouble = new List<double>();
                    foreach(string s in components)
                    {
                        compDouble.Add(double.Parse(s));
                    }
                    Vector v = new Vector(compDouble.ToArray());
                    spans.Add(v);
                }
            }
            Matrix matrix = new Matrix(true, spans.ToArray());
            return matrix;
        }
        //metoda vezme kolekci řádků jako List stringů a uloží je do souboru. 
        //Použití bloku using zajišťuje atomicitu celého procesu a následné vymazání paměti.
        public static void SaveResult(List<string>lines, string path)
        {
            using(StreamWriter sw = new StreamWriter(path))
            {
                foreach(string line in lines)
                {
                    sw.WriteLine(line);
                    Console.WriteLine(line);
                }
                sw.Flush();
            }
        }
    }
}
