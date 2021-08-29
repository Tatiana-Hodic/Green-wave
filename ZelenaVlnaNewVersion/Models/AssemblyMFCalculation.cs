using Combinatorics.Collections;
using Microsoft.SolverFoundation.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZelenaVlnaNewVersion.Models;
using ZelenaVlnaNewVersion.Services;

namespace ZelenaVlnaNewVersion.Models
{
    public class AssemblyMFCalculation
    {
        private double _time;
        private List<double> _timeTableA = new List<double>();
        private List<double> _timeTableB = new List<double>();
        private List<string> _reports = new List<string>();
        //Celkový čas
        public double Time
        {
            get => _time;
        }
        //Rozdíl mezi odjezdem auta A a auta B
        private double _difference;
        public double Difference
        {
            get => _difference;
        }
        //Podíl doby, po kterou na semaforu svítí zelená
        private double _lightPeriodGreenFraction;
        public double LightPeriodGreenFraction
        {
            get => _lightPeriodGreenFraction;
        }
        //Matice vzdáleností
        private Matrix _matrixOfDistances;
        public Matrix MatrixOfDistances
        {
            get => _matrixOfDistances;
        }
        //List objektů typu Cross, reprezentující cestu
        public List<Cross> Track = new List<Cross>();
        //Proměnné u
        private List<Decision> _uVariables = new List<Decision>();
        public Decision[] UVariables
        {
            get
            {
                return _uVariables.ToArray();
            }

            set 
            {
                foreach (Decision d in value)
                {
                    _uVariables.Add(d);
                }
            }
        }
        //Proměnné v
        private List<Decision> _vVariables = new List<Decision>();
        public Decision[] VVariables {
            get
            {
                return _vVariables.ToArray();
            }

            set
            {
                foreach (Decision d in value)
                {
                    _vVariables.Add(d);
                }
            }
        }

        //Řízení výpočtu
        public void Solve(Matrix matrix, double period, double difference)
        {
            //Matice vzdáleností, podíl zelené na semaforu a rozdíl mezi časem vyjetí auta A a B
            _matrixOfDistances = matrix;
            _lightPeriodGreenFraction = period;
            _difference = difference;
            //Generování křižovatek z matice vzdáleností
            GenerateCrosses();
            //Iterace přes plány
            IteratePlans();
        }
        //Nastavení optimalizované funkce
        private Term SetFunction()
        {
            Term function = 0;
            Cross previousCross = Track.FirstOrDefault();

            var lastPrevI = 0;
            try
            {
                //Poslední křižovatka, patřící do I
                lastPrevI = Track.Where(o => o.BelongsToI == true).Select(o => o.Position).LastOrDefault();
            }
            catch (Exception e)
            {
                //Výpis případné chybové hlášky
                Console.WriteLine(e.Message);
            }
            finally
            {
                //Iterace přes křižovatky v cestě
                foreach (Cross c in Track)
                {
                    //Nastavení členů ve funkci pro křižovatky v množině I
                    if (c.BelongsToI)
                    {
                        if (lastPrevI < _matrixOfDistances.Spans - 1)
                        {
                            function += _matrixOfDistances.Elements[previousCross.Position, c.Position] + _uVariables[c.Position] + _matrixOfDistances.Elements[lastPrevI, _matrixOfDistances.Spans - 1] + _matrixOfDistances.Elements[0, 0];
                        }
                    }
                    //Nastavení členů ve funkci pro křižovatky v množině J
                    if (c.BelongsToJ)
                    {
                        Cross prevCrossJ = Track.Last();
                        foreach (Cross k in Track)
                        {
                            if ((k.Position >= c.Position) && c.BelongsToJ && (k.Position < _matrixOfDistances.Spans) && (prevCrossJ.Position < _matrixOfDistances.Spans))
                            {
                                function += _matrixOfDistances.Elements[prevCrossJ.Position, k.Position] + _vVariables[k.Position];
                            }
                            prevCrossJ = Track.Where(o => o.BelongsToJ == true).Select(o => o).FirstOrDefault();
                        }
                    }
                }
            }

            return function;
        }

        //Uložení výsledků
        private List<Result> _results = new List<Result>();
        public List<Result> GetResults { get { return _results; } }
        //Výpis cesty. Jen pro potřeby debuggování
        private void PrintTrack()
        {
            foreach (Cross c in Track)
            {
                Console.WriteLine("{0}; {1}", c.BelongsToI, c.BelongsToJ);
            }
            Console.WriteLine("\\");
        }
        //Výpis reportu do souboru
        private void WriteReport(List<string>lines)
        {
            FileManagement.SaveResult(lines, "Report");
        }
        //Iterace přes plány
        private void IteratePlans()
        {
            //iterace přes všchny variace s opakováním n dvojic true - false
            List<bool> bools = new List<bool> { true, false };

            Variations<bool> variations = new Variations<bool>(bools, Track.Count(), GenerateOption.WithRepetition);
            //řádky souboru výpisu jako kolekce string
            List<string> lines = new List<string>();
            //číslování plánů
            int increment = 0;
            //iterace přes variace
            foreach(var v in variations)
            {
                foreach(var u in variations)
                {
                    //Přiřazování hodnot true - false podle jednotlivých variací
                    for(int i = 0; i<=v.Count() - 1; i++)
                    {
                        Track[i].BelongsToI = v[i];
                        for(int j = 0; j<=u.Count() - 1; j++)
                        {
                            Track[j].BelongsToJ = u[j];
                        }
                        //výpis stavu křižovatek
                        Console.WriteLine(Track[i].BelongsToI + " " + Track[i].BelongsToJ);
                    }
                    increment++;
                    //Zápis čísla plánu
                    Console.WriteLine(increment);
                    //Výpočet daného plánu pomocí lineárního programování
                    Calculate();
                }
            }
            //zápis výsledku do souboru
            WriteReport(_reports);
            Console.WriteLine("Finished");
        }
        //Použití MSF na optimalizaci
        private void Calculate()
        {
            //Proměnné u a v
            _vVariables = new List<Decision>();
            _uVariables = new List<Decision>();

            int delimiter = 0;
            //Inicializace knihovny
            SolverContext solverContext = SolverContext.GetContext();
            solverContext.ClearModel();
            Model model = solverContext.CreateModel();
            //Logování výjimek a tvorba proměnných x a y
            Exception exception = new Exception();
            Term x = 0;
            Term y = _difference;
            //Iterace přes křižovatky při tvorbě omezujících podmínek
            foreach (Cross c in Track)
            {
                //Tvorba rozhodnutí a jejich přidání do modelu
                Decision decisionU = new Decision(Domain.RealNonnegative, "decision" + delimiter.ToString() + "_in_I");
                Decision decisionV = new Decision(Domain.RealNonnegative, "decision" + delimiter.ToString() + "_in_J");
                _uVariables.Add(decisionU);
                _vVariables.Add(decisionV);
                model.AddDecision(decisionU);
                model.AddDecision(decisionV);
                //Tvorba omezení na rozhodnutí
                Term constraintUinI = 1 - _lightPeriodGreenFraction;
                Term constraintUnotInI = _lightPeriodGreenFraction;
                Term constraintUinJ = 1 - _lightPeriodGreenFraction;
                Term constraintVnotInJ = _lightPeriodGreenFraction;
                Cross previousCross = Track.FirstOrDefault();
                int lastPrevI = Track.Where(o => o.BelongsToI == true).Select(o => o.Position).LastOrDefault();
                Cross prevCrossJ = Track.Last();
                //Přidání omezení do modelu
                if (c.BelongsToI)
                {
                    model.AddConstraint("constraint" + delimiter + "_in_I", decisionU <= 1 - _lightPeriodGreenFraction);
                }
                if (!c.BelongsToI) 
                { 
                    model.AddConstraint("constraint" + delimiter + "_not_in_I", decisionU - _lightPeriodGreenFraction <= 0);
                }
                if (c.BelongsToJ) 
                { 
                    model.AddConstraint("constraint" + delimiter + "_in_J", decisionV <= 1 - _lightPeriodGreenFraction);
                }
                if (!c.BelongsToJ) 
                {
                    model.AddConstraint("constraint" + delimiter + "_not_in_J", decisionV - _lightPeriodGreenFraction <= 0);
                }
                //Číslování křižovatek
                delimiter++;
            }
            List<double> decisionValues = new List<double>();
            foreach (Cross c in Track)
            {
                Cross previousCross = Track.FirstOrDefault();
                int lastPrevI = Track.Where(o => o.BelongsToI == true).Select(o => o.Position).LastOrDefault();
                Cross prevCrossJ = Track.Last();

                //Tvorba neznámých x a y
                if (c.BelongsToI)
                {
                    if ((lastPrevI < _matrixOfDistances.Spans - 1) && (c.Position < Track.Count() - 1))
                    {
                        x += _matrixOfDistances.Elements[previousCross.Position, c.Position] + _uVariables[c.Position] + _matrixOfDistances.Elements[lastPrevI, _matrixOfDistances.Spans - 1];
                        //y += _matrixOfDistances.Elements[previousCross.Position, c.Position] + _vVariables[c.Position] + _matrixOfDistances.Elements[lastPrevI, _matrixOfDistances.Spans - 1];
                    }
                }
                if (!c.BelongsToI)
                {
                    if ((lastPrevI < _matrixOfDistances.Spans - 1) && (c.Position < Track.Count() - 1))
                    {
                        x += _matrixOfDistances.Elements[previousCross.Position, c.Position] - _uVariables[c.Position] + _matrixOfDistances.Elements[lastPrevI, _matrixOfDistances.Spans - 1];
                    }
                }
                if (c.BelongsToJ)
                {
                    foreach (Cross k in Track)
                    {
                        if ((k.Position >= c.Position) && c.BelongsToJ && (k.Position < _matrixOfDistances.Spans) && (prevCrossJ.Position < _matrixOfDistances.Spans))
                        {
                            y += _matrixOfDistances.Elements[prevCrossJ.Position, k.Position] + _vVariables[k.Position];
                        }
                        prevCrossJ = Track.Where(o => o.BelongsToJ == true).Select(o => o).FirstOrDefault();
                    }
                }
                if (!c.BelongsToJ)
                {
                    foreach (Cross k in Track)
                    {
                        if ((k.Position >= c.Position) && c.BelongsToJ && (k.Position < _matrixOfDistances.Spans) && (prevCrossJ.Position < _matrixOfDistances.Spans))
                        {
                            y += _matrixOfDistances.Elements[prevCrossJ.Position, k.Position] - _vVariables[k.Position];
                        }
                        prevCrossJ = Track.Where(o => o.BelongsToJ == true).Select(o => o).FirstOrDefault();
                    }
                }
                //Přidání omezení na neznámé x a y, celočíselnost
                int integerXandY = (int)Math.Round(_matrixOfDistances.Elements[_matrixOfDistances.Spans - 1, 0]) + (int)_difference + 2 * _matrixOfDistances.Spans;
                for (int i = integerXandY; i >= 0; i--)
                {
                    Term function = SetFunction();
                    model.AddConstraint("XminusYareEqualTo" + i.ToString() + c.Id.ToString(), x - y == i);
                    model.AddGoal("functionToMinimize", GoalKind.Minimize, function);
                    Solution solution = solverContext.Solve();
                    //Uložení rozhodnutí
                    foreach (Decision d in solution.Decisions)
                    {
                        decisionValues.Add((Math.Abs(d.ToDouble() - Math.Truncate(d.ToDouble()))));
                    }
                    model.RemoveConstraint(model.Constraints.Last());
                    model.RemoveGoal(model.Goals.First());
                }
            }

            double[] decisionArray;
            
            List<double> _oldTimeTableA = new List<double>();
            List<double> _oldTimeTableB = new List<double>();
            //Probíhá řešení
            try
            {
                Solution solution = solverContext.Solve();

                decisionArray = new double[decisionValues.Count()];
                decisionArray = decisionValues.ToArray();
                int rankA = _timeTableA.Count();
                int rankB = _timeTableB.Count();
                _oldTimeTableA = new List<double>(_timeTableA);
                _oldTimeTableB = new List<double>(_timeTableB);
                
                for (int i=0; i<=rankA - 2; i++)
                {
                    decisionArray[i + 1] = Math.Abs((_timeTableA[i] - _timeTableB[i] - Math.Truncate(_timeTableA[i] - _timeTableB[i])));
                }
                decisionArray[0] = 0;
                decisionArray[decisionArray.Length - 1] = Difference;
                //Geneze plánů pro obě auta
                _timeTableA = new List<double>();
                _timeTableB = new List<double>();
                foreach(Cross c in Track)
                {
                    foreach(Cross k in Track)
                    {
                        if(c.Id !=0 && k.Id !=Track.Last().Id && c.Id!=Track.Last().Id && k.Id != 0)
                        {
                            if(_timeTableA.Count() > 0) _timeTableA.Add(_matrixOfDistances.Elements[c.Id, k.Id - 1]);
                            else _timeTableA.Add(_matrixOfDistances.Elements[c.Id, k.Id - 1]);
                            if (_timeTableB.Count() > 0) _timeTableB.Add(_matrixOfDistances.Elements[_matrixOfDistances.Spans - c.Id, _matrixOfDistances.Spans - k.Id - 1]);
                            else _timeTableB.Add(_matrixOfDistances.Elements[_matrixOfDistances.Spans - c.Id, _matrixOfDistances.Spans - k.Id - 1]);
                            //Console.WriteLine(_matrixOfDistances.Elements[c.Id, k.Id].ToString());
                            //Console.WriteLine(_matrixOfDistances.Elements[_matrixOfDistances.Spans - 1 - c.Id, _matrixOfDistances.Spans - 1 - k.Id]);
                        }
                    }
                }
                for (int i = 0; i<= rankA; i++)
                {
                    
                    _timeTableA.Add(decisionArray[i]);
                }
                for (int i = 0; i <= rankB - 1; i++)
                {
                    _timeTableB.Add(decisionArray[rankA + i]);
                }


                decisionValues = new List<double>(decisionArray);

                _time = _matrixOfDistances.Elements[_matrixOfDistances.Spans - 1, 0];
                _reports.Add(_time.ToString());
                exception = new Exception("OK");
                //Console.WriteLine(solution.GetReport().ToString());
            }
            catch(Exception e)
            {
                //throw e;
                exception = new Exception(e.Message);
                return;
            }
            finally
            {
                //Přidání výcledku pro daný plán do kolekce výsledků pro všechny plány
                List<Result> oldResults = new List<Result>();
                foreach (Result r in _results)
                {
                    oldResults.Add(Result.DeepCopy(r));
                }

                Result result = new Result() { TrackState = Track, ResultValue = _time, TimeTableA = _timeTableA, TimeTableB = _timeTableB, ValuesOfh = decisionValues, ErrorMessage = exception};
                oldResults.Add(result);
                _results = new List<Result>(oldResults);
                _timeTableA = new List<double>(_oldTimeTableA);
                _timeTableB = new List<double>(_oldTimeTableB);
            }
        }
        //Určování, zda na semaforu svítí červená nebo zelená
        private bool TrafficLight(double time)
        {
            
            int numberOfblicks = (int)time;
            double toCompare = time - numberOfblicks;
            if (toCompare <= LightPeriodGreenFraction) return true;
            else return false;
        }
        //Geneze křižovatek z matice vzdáleností
        private void GenerateCrosses()
        {
            //Tvorba plánu
            if (Track == null || (Track.Count == 0))
            {
                Track = new List<Cross>();
                Track.Add(new Cross(0, 0, 0, true, true));
            }
            //Příprava proměnných
            Cross previousCross = Track.Last();
            int previousFalseCrossA = 0;
            int previousFalseCrossB = _matrixOfDistances.Spans - 1;
            _time = _difference;
            double _timeA = 0;
            double _timeB = _difference;
            bool[] openedLightsA = new bool[_matrixOfDistances.Spans];
            bool[] openedLightsB = new bool[_matrixOfDistances.Spans];
            //Inicializace prvního plánu
            for(int i = 0; i<= _matrixOfDistances.Spans - 1; i++)
            {
                openedLightsA[i] = true;
                openedLightsB[i] = true;
            }
            openedLightsA[0] = true;
            openedLightsA[_matrixOfDistances.Spans - 1] = true;
            openedLightsB[0] = true;
            openedLightsB[_matrixOfDistances.Spans - 1] = true;
            _timeTableA.Add(_timeA);
            _timeTableB.Add(_timeB);
            //Výpočet vzdáleností mezi křižovatkami na základě matice
            for (int i = 0; i <= _matrixOfDistances.Spans - 2; i++)
            {
                double temporalTime = _timeA + _matrixOfDistances.Elements[previousFalseCrossA, i];
                double temporalTimeB = _timeB + _matrixOfDistances.Elements[previousFalseCrossB, i];
                _timeTableA.Add(temporalTime);
                _timeTableB.Add(temporalTimeB);
                if (!TrafficLight(temporalTime))
                {
                    _timeA += _matrixOfDistances.Elements[previousFalseCrossA, i];
                    previousFalseCrossA = previousCross.Position;
                    openedLightsA[i] = false;
                }
                if (!TrafficLight(temporalTimeB))
                {
                    _timeB += _matrixOfDistances.Elements[previousFalseCrossB, i];
                    previousFalseCrossB = previousCross.Position;
                    openedLightsA[i] = false;
                    _time += _matrixOfDistances.Elements[previousFalseCrossB, i];
                }
                else
                {
                    //Track.Add(new Cross(previousCross.Id + 1, previousCross.Position + 1, true, ));
                }
                temporalTime = 0;
                temporalTimeB = 0;
            }
            if (Track.Count() == 1)
            {
                Track = new List<Cross>();
            }
            for (int i = 0; i <= _matrixOfDistances.Spans - 1; i++)
            {
                Track.Add(new Cross(i+1, i + 1, _matrixOfDistances.Elements[i, i], openedLightsA[i], openedLightsB[_matrixOfDistances.Spans - 1 - i]));
            }
        }
    }
}