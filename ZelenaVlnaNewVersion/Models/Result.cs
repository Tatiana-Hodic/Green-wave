using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;

namespace ZelenaVlnaNewVersion.Models
{
    public class Result
    {
        public Result()
        {

        }
        //fields - privátní proměnné pro práci s daty vlastností třídy
        private List<Cross> _trackState = new List<Cross>();
        private List<double> _timeTableA = new List<double>();
        private List<double> _timeTableB = new List<double>();
        private List<double> _valuesOfh = new List<double>();
        private Exception _errorMessage;
        //Stav daného plánu. V kolekci je uložen stav řešené sady křižovatek dle vlastností objektu typu Cross
        public List<Cross> TrackState { 
            get { return _trackState; }
            set 
            {
                foreach(Cross c in value)
                _trackState.Add(c); 
            } 
        }
        //Vlastnost reprezentuje chybovou zprávu, vzniklou během výpočtu
        public Exception ErrorMessage 
        {
            get 
            {
                if (_errorMessage == null || _errorMessage == new Exception("")) return new Exception("OK"); 
                //Pokud není chyba, vypíše OK
                return _errorMessage; 
            }
            set 
            {
                _errorMessage = new Exception(value.Message);
            }
        }

        //Rozvrh křižovatek pro auto A
        public List<double> TimeTableA
        {
            get { return _timeTableA; }
            set
            {
                foreach (double d in value)
                    _timeTableA.Add(d);
            }
        }
        //Rozvrh křižovatek pro auto B
        public List<double> TimeTableB
        {
            get { return _timeTableB; }
            set
            {
                foreach (double d in value)
                    _timeTableB.Add(d);
            }
        }
        //spočtené hodnoty proměnné h pro jednotlivé křižovatky
        public List<double> ValuesOfh 
        {
            get {return _valuesOfh; }
            set
            {
                foreach (double d in value)
                    _valuesOfh.Add(d);
            }
        }
        //Hodnota minimalizované funkce
        public double ResultValue { get; set; }
        //Hluboká kopie objektu, používaná při přenášení objektů např. do kolekcí
        public static Result DeepCopy(Result item)
        {
            Result result = new Result();
            result.ResultValue = item.ResultValue;
            result.TimeTableA = item.TimeTableA.Select(o => o).ToList();
            result.TimeTableB = item.TimeTableB.Select(o => o).ToList();
            result.TrackState = new List<Cross>();
            result.TrackState = item.TrackState.Select(
                c => new Cross(c.Id, c.Position, c.PreviousIntervalLength, c.BelongsToI, c.BelongsToJ)
                ).ToList();
            result.ValuesOfh = item.ValuesOfh.Select(
                h => h
                ).ToList();
            result.ErrorMessage = item.ErrorMessage;
            return result;
        }
    }
}
