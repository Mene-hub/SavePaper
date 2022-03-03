using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePaper
{
    public class GruppoSpese
    {

        //nome del gruppo spese e nome del file
        public string nome;

        //budget del gruppo spese
        public double budget;

        //per sapere se il budget è stato settato o no
        public bool budgetSetted;

        //lista scontrini
        public List<Scontrino> spese;

        //costruttore
        public GruppoSpese(string nome_, List<Scontrino> spese_)
        {
            nome = nome_;
            spese = spese_;
        }

        //costruttore vuoto per la serializzazione della clase in XML
        public GruppoSpese(){}

        //costruttore vuoto per la serializzazione della clase in XML
        public GruppoSpese(List<Scontrino> spese_) 
        {
            spese = spese_;
            budget = 0;
            budgetSetted = false;
        }

        //metodo per aggiungere e settare il budget
        public void setBudget(double budget_, bool prev)
        {
            budget = budget_;

            if (!prev)
            {
                budget += speseTotali();
            }

            budgetSetted = true;
        }

        //metodo per il calcolo della spesa totale
        public double speseTotali()
        {
            double totSpesa = 0;
            if(spese != null)
            foreach (var item in spese)
            {
                totSpesa += item.totCost();
            }

            return totSpesa;
        }

    }
}