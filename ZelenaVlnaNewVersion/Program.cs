using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZelenaVlnaNewVersion.Models;
using ZelenaVlnaNewVersion.Services;

namespace ZelenaVlnaNewVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Vitejte v programu Zelena Vlna");
            Console.WriteLine("Nastavte jméno souboru pro vstup:");
            Console.WriteLine("Nastavte jméno souboru pro vystup:");
            //Inicializace objektu typu InsertData, který spravuje interakci mezi programem a uživatelem
            InsertData data = new InsertData();
            //Nastavení cest k ukládání a nahrání souboru pro vstup a výstup
            data.SetPath(Console.ReadLine(), Console.ReadLine());
            //Vložení matice ručně do konzole
            data.InsertMatrix();
            //Uložení matice
            data.SaveMatrix();
            //Vložení zlomku pro zelenou na semaforu
            data.InsertGreenFraction();
            //uložení matice vzdáleností do stringu kvůli kontrole v konzoli
            Matrix matrix = data.MatrixOfDistances;
            matrix.ToString();
            //Výpis výsledků
            data.ReturnTrack();
            Console.ReadKey();
        }
    }
}
