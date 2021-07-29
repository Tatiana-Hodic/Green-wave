using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelenaVlnaNewVersion.Models
{
    public class Matrix
    {
        //Matice jako pole sloupcových vektorů
        private Vector[] _mxSpans;
        public Vector[] MxSpans 
        {
            get
            {
                if(_mxSpans == null)
                {
                    _mxSpans = new Vector[_elements.GetLength(1)];
                    if (_elements != null)
                    {
                        for(int i = 0; i <= _elements.GetLength(1) - 1; i++)
                        {
                            List<double> components = new List<double>();
                            for (int j = 0; j <= _elements.GetLength(0) - 1; j++)
                            {
                                components.Add(_elements[j, i]);
                            }
                            _mxSpans[i] = new Vector(components.ToArray());
                        }
                        return _mxSpans;
                    }
                    else throw new Exception("Matrix is empty");
                }
                else
                return _mxSpans; 
            } 
        }
        //Matice jako pole řádkových vektorů

        private Vector[] _mxRows;
        public Vector[] MxRows 
        {
            get
            {
                if (_mxRows == null)
                {
                    _mxRows = new Vector[_elements.GetLength(0)];
                    if (_elements != null)
                    {
                        for (int i = 0; i <= _elements.GetLength(0) - 1; i++)
                        {
                            List<double> components = new List<double>();
                            for (int j = 0; j <= _elements.GetLength(j) - 1; j++)
                            {
                                components.Add(_elements[j, i]);
                            }
                            _mxRows[i] = new Vector(components.ToArray());
                        }
                        return _mxRows;
                    }
                    else throw new Exception("Matrix is empty");
                }
                else
                    return _mxRows;
            } 
        }
        //Matice jako pole čísel

        private double[,] _elements;
        public double[,] Elements
        {
            get
            {
                return _elements;
            }
            set
            {
                _elements = value;
            }
        }
        //Počet sloupců
        public int Spans
        {
            get { return FindFirstLongest(MxSpans).Vs.Length; }
        }
        //Počet řádků
        public int Rows
        {
            get { return MxSpans.Length; }
        }

        public Matrix()
        {

        }

        public Matrix(double numberToFill, int rows, int spans)
        {
            Vector[] vectors = new Vector[spans];
            double[] components = new double[rows];
            for(int i = 0; i<= rows - 1; i++)
            {
                components[i] = numberToFill;
            }
            Vector vector = new Vector(components);
            for (int i = 0; i<= rows - 1; i++)
            {
                vectors[i] = vector;
            }
            new Matrix(true, vectors);
        }

        //Konstruktor matice - sloupcove vektory pro isSpan = true, pro false radkove
        public Matrix(bool isSpan, params Vector[] vectors)
        {
            if (isSpan)
            {
                _mxSpans = vectors;
                _elements = new double[Spans, Rows];
                for (int j = 0; j <= Spans - 1; j++)
                {
                    for (int i = 0; i <= Rows - 1; i++)
                    {
                        _elements[i, j] = MxSpans[i].Vs[j];
                    }
                }
            }

            else
            {
                _mxRows = vectors;
                for (int j = 0; j <= Rows - 1; j++)
                {
                    for (int i = 0; i <= Spans - 1; i++)
                    {
                        _elements[i, j] = MxRows[i].Vs[j];
                    }
                }
            }
        }

        //Konstruktor matice - pole cisel

        public Matrix(params double[][] values)
        {
            _elements = new double[values.Length, values[0].Length];
            for (int i = 0; i <= values.Length - 1; i++)
            {
                for (int j = 0; j <= values.Length - 1; j++)
                {
                    _elements[i, j] = values[i][j];
                }
            }

            for (int i = 0; i <= Rows - 1; i++)
            {
                for (int j = 0; j <= Spans - 1; j++)
                {
                    MxSpans[i].Vs[j] = _elements[i, j];
                }
            }
        }
        //Určování kvadratičnosti matice, pro kvadratickou vrátí true
        public bool IsQuadrate(Matrix a)
        {
            return a.Spans == a.Rows;
        }
        //Sledování, zda jsou matice stejných rozměrů
        public bool IsSameDimension(Matrix a, Matrix b)
        {
            return ((a.Spans == b.Spans) && (a.Rows == b.Rows));
        }
        //sčítání matic po elementech
        public Matrix Sum(Matrix a, Matrix b)
        {
            Matrix c = a;
            try
            {
                if (IsSameDimension(a, b))
                {
                    for (int i = 0; i <= MxSpans.Length - 1; i++)
                    {
                        c.MxSpans[i] = c.MxSpans[i].Sum(c.MxSpans[i], c.MxSpans[i]);
                    }
                }
                else
                {
                    c = null;
                }
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return c;
        }
        //Součin matic. Využívá skalárního součinu pro vektory.
        public Matrix Product(Matrix a, Matrix b)
        {
            Matrix matrix = a;
            for (int i = 0; i <= a.Spans - 1; i++)
            {
                for (int j = 0; j <= b.Rows - 1; j++)
                {
                    matrix.Elements[i, j] = a.MxRows[i].ScalarProduct(a.MxRows[i], b.MxSpans[j]);
                }
            }

            return matrix;
        }
        //Metoda vybere prvni vektor z nejdelsich. 
        public Vector FindFirstLongest(params Vector[] vectors)
        {
            Vector vector = vectors[0];
            foreach (Vector v in vectors)
            {
                if (vector.Vs.Length <= v.Vs.Length)
                {
                    vector = v;
                }
            }
            return vector;
        }
        //Vrátí matici jako sadu sloupcových vektorů ve tvaru {{1;2;8};{0;9;5};{7;8;3}} a vypíše do konzole.
        public override string ToString()
        {
            string matrixToWrite = "{ ";
            Console.WriteLine("{");
            for (int i = 0; i <= this.Spans - 1; i++)
            {
                string s = "{ ";
                for (int j = 0; j <= this.Rows - 1; j++)
                {
                    s += this.Elements[i, j].ToString() + ";";
                }
                s += " }";
                Console.WriteLine(s);
                matrixToWrite += s;
            }
            Console.WriteLine("}");
            matrixToWrite += " }";            
            //Console.ReadLine();
            return matrixToWrite;
        }
    }
}
