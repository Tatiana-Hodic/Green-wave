using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZelenaVlnaNewVersion.Models
{
    public class Vector
    {
        //Vektor jako pole čísel
        double[] _vs;
        public double[] Vs 
        { 
            get
            {
                return _vs;
            }

            set
            {
                _vs = value;
            }
        }
        public Vector()
        {

        }
        //Vektor je pole double
        public Vector(params double[] components)
        {
            _vs = components;
        }

        public Vector(int vectorLength)
        {
            _vs = new double[vectorLength];
        }
        //Sčítání vektorů
        public Vector Sum(Vector a, Vector b)
        {
            Vector c = a;
            int l = a.Vs.Length;
            for (int i = 0; i <= l - 1; i++)
            {
                c.Vs[i] = a.Vs[i] + b.Vs[i];
            }
            return c;
        }
        //Násobení vektoru číslem
        public Vector ScalProd(double b, Vector a)
        {
            Vector c = a;
            int l = a.Vs.Length;
            for (int i = 0; i <= l - 1; i++)
            {
                c.Vs[i] = b * a.Vs[i];
            }
            return c;
        }
        //Odčítání vektorů
        public Vector Sub(Vector a, Vector b)
        {
            Vector c = a;
            int l = a.Vs.Length;
            for (int i = 0; i <= l - 1; i++)
            {
                c.Vs[i] = a.Vs[i] - b.Vs[i];
            }
            return c;
        }
        //Skalární součin
        public double ScalarProduct(Vector a, Vector b)
        {
            double c = 0;
            int l = a.Vs.Length;
            for (int i = 0; i <= l - 1; i++)
            {
                c += a.Vs[i] * b.Vs[i];
            }

            return c;
        }
    }
}
