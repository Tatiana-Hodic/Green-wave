using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ZelenaVlnaNewVersion.Models;

namespace ZelenaVlnaNewVersion.Services
{
    public class InsertData
    {
        private string _path;
        private string _suffix;
        //Přípona souboru
        public string Suffix
        {
            get { return _suffix; }
            set { _suffix = value; }
        }
        //Cesta pro vstup
        public string Path 
        {
            get
            {
                if (_path != null)
                    return _path;
                else throw new Exception("Path is empty");
            }
        }
        //Cesta pro výstup
        private string _pathOutput;
        public string PathOutput
        {
            get
            {
                if (_pathOutput != null)
                    return _pathOutput;
                else throw new Exception("Path is empty");
            }
        }

        private List<Cross> _track = new List<Cross>();

        //Vrátí informace o řešeném dopravním problému
        public void ReturnTrack()
        {
            //Provede definici a založí list stringů pro pozdější uložení do dokumentu
            AssemblyMFCalculation calculation = new AssemblyMFCalculation();
            List<string> lines = new List<string>();
            //Vyřeší zadaný problém
            calculation.Solve(_matrixOfDistances, _greenFraction, _difference);
            //Uloží stav cesty jakožto list objektů typu Cross
            _track = calculation.Track;
            //Skládání dokumentu pro výpis výsledků
            for(int i = 0; i<= calculation.GetResults.Count() - 1; i++)
            {
                //Výpis stavu křižovatek po výpočtu
                foreach(Cross c in calculation.GetResults[i].TrackState)
                {
                    lines.Add("Stav krizovatky pri vypoctu: " + c.BelongsToI + ", " + c.BelongsToJ);
                }
                //Jízdní řády aut
                lines.Add("Jizdni rady aut A prijezd/odjezd, B prijezd/odjezd a hodnot h:");
                int length = calculation.GetResults[i].TimeTableA.Count()-2;
                for(int j=0; j<=length - 1; j+=2)
                {
                    //Výpis výsledků
                    lines.Add(calculation.GetResults[i].TimeTableA[j].ToString() 
                        + ";   " + calculation.GetResults[i].TimeTableA[j + 1].ToString()
                        + ";   " + calculation.GetResults[i].TimeTableB[j].ToString()
                        + ";   " + calculation.GetResults[i].TimeTableB[j + 1].ToString()
                        + ";   " + (calculation.GetResults[i].ValuesOfh[j] + calculation.GetResults[i].ValuesOfh[j + 1]).ToString()
                        );
                }
                lines.Add("////");
                foreach (double h in calculation.GetResults[i].TimeTableA)
                {
                    lines.Add(h.ToString() + ";");
                }
                lines.Add("////");
                foreach (double h in calculation.GetResults[i].TimeTableB)
                {
                    lines.Add(h.ToString() + ";");
                }
                lines.Add("////");
                lines.Add("Vypocet cislo"+ i.ToString());
                foreach(double h in calculation.GetResults[i].ValuesOfh)
                {
                    lines.Add(h.ToString() + ",");
                }
                lines.Add("////");
                lines.Add("Vysledek: " + calculation.GetResults[i].ResultValue);
                lines.Add("Chybova hlaska:" + calculation.GetResults[i].ErrorMessage.Message);
                _suffix = i.ToString();
                
            }
            //Připravený soubor ve formě listu stringů se uloží do souboru
            WriteSolution(lines);
        }

        //Matice vzdáleností
        private Matrix _matrixOfDistances;
        public Matrix MatrixOfDistances 
        {
            get
            {
                return _matrixOfDistances; 
            } 
        }
        //Interval mezi odjezdem auta A a auta B
        private double _difference;
        public double Difference
        {
            get
            {
                return _difference;
            }
        }

        //Podíl intervalu semaforu, na kterém svítí zelená.
        private double _greenFraction;
        public double GreenFraction 
        { 
            get
            {
                return _greenFraction;
            }
        }
        public InsertData()
        {

        }
        //Vložení matice z konzole
        public void InsertMatrix()
        {
            //Zadání velikosti matice
            Console.WriteLine("Vlozte rozmer matice:");
            int dimension = int.Parse(Console.ReadLine());
            //Vložení jednotlivých elementů horní trojúhelníkové matice
            Console.WriteLine("Vkladejte jednotlive elementy matice, po radcich:");
            double[][] elementsToSet = new double[dimension][];
            for(int i = 0; i<= dimension - 1; i++)
            {
                double[] vs = new double[dimension];
                for (int j = 0; j <= dimension - 1; j++)
                {
                    if (i < j)
                    {
                        vs[j] = double.Parse(Console.ReadLine());
                    }
                    //nulová diagonála
                    else if (i == j)
                    {
                        vs[j] = 0;
                    }
                    else vs[j] = 0;
                }
                elementsToSet[i] = vs;
            }
            //symetricita
            for (int i = 0; i <= dimension - 1; i++)
            {
                for(int j = 0; j<=dimension - 1; j++)
                {
                    elementsToSet[j][i] = elementsToSet[i][j];
                }
            }
            //Vezme vytvořené pole čísel a vytvoří matici
            Matrix matrix = new Matrix(elementsToSet);
            _matrixOfDistances = matrix;
            //kontrola, zda matice vzdáleností dává smysl, tedy že auto se zastávkou na prostředním semaforu jede déle než bez ní
            if (CheckInterpoints(matrix))
            {
                Console.WriteLine("Zadal jste matici:");
                matrix.ToString();
                Console.WriteLine("Matice ok");
            }
            else
            {
                Console.WriteLine("Chybná data!");
            }
        }
        //Uložení matice
        public void SaveMatrix()
        {
            FileManagement.SaveMatrixToFile(_matrixOfDistances, _path);
        }
        //Nahrání matice
        public void LoadMatrix()
        {
            _matrixOfDistances =  FileManagement.LoadMatrixFromFile(_path);
        }
        //Nastavení cest pro vstup a výstup
        public void SetPath(string path, string pathOutput)
        {
            _path = path + ".txt";
            _pathOutput = pathOutput + ".txt";
        }
        //Vložení podílu intervalu na semaforu, po kterou je zelený
        public void InsertGreenFraction()
        {
            Console.WriteLine("Vlozte cislo od 0 do 1, jak velkou cast periody sviti zelena:");
            _greenFraction = double.Parse(Console.ReadLine());
            if((0 <= _greenFraction) && (_greenFraction <= 1))
            {
                Console.WriteLine("Zlomek pro zelenou ok");
            }
            else { Console.WriteLine("Chybna data na zlomku pro zelenou"); }
        }
        //Vezme list stringů a vytiskne soubor
        private void WriteSolution(List<string> lines)
        {
            Console.WriteLine("Vypocet dava reseni:\n");
            FileManagement.SaveResult(lines, _pathOutput);
        }
        //Kontroluje správnost zadané matice. Součet dob, po kterou jede auto přes 
        //prostřední semafor se zastávkou musí být vždy delší, než přímá jízda.        
        //Funkce v jednotlivých cyklech:
        private bool CheckInterpoints(Matrix matrix)
        {
            //iteruje řádky
            for (int i = 0; i <= matrix.Rows - 1; i++)
            {
                //iteruje sloupce
                for (int j = 0; j <= matrix.Spans - 1; j++)
                {
                    //Omezí se na horní trojúhelníkovou matici
                    if(j > i)
                    {
                        for (int k = i; k <= j; k++)
                        {
                            for (int l = j; l >= i; l--)
                            {
                                //Kontrola validity elementů matice vzdáleností
                                if (matrix.Elements[k, j] <= matrix.Elements[i, j])
                                {
                                    continue;
                                }
                                if (matrix.Elements[i, l] <= matrix.Elements[i, j])
                                {
                                    continue;
                                }
                                else return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
