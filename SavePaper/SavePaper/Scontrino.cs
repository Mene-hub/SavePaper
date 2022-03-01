using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePaper
{
    //classe contenente i dati dello scontrino
    public class Scontrino
    {
        //lista di prodotti nello scontrino
        public List<SingolaSpesa> scontrino;

        //nome dello scontrino il motivo della spesa
        public string movente;

        //luogo o venditore presso la quale viene fatto l'acquisto
        public string venditore;

        //data dell'acquisto
        public DateTime dataAcquisto;

        //id dell'acquisto, (useless?)
        public int id;

        //variabile nel caso si vogla inserire un budget
        public static double budget;

        //costruttore
        public Scontrino(string movente_, string venditore_, DateTime data_,int id_)
        {
            scontrino = new List<SingolaSpesa>();
            movente = movente_;
            venditore = venditore_;
            dataAcquisto = data_;
            id = id_;
        }

        //costruttore vuoto per la serializzazione della classe in XML
        public Scontrino() { }

        //metodo per l'aggiunta di un prodotto nello scontrino
        public void addSpesa(string nome, double costo)
        {
            scontrino.Add(new SingolaSpesa(costo, nome));
        }

        //metodo per l'aggiunta di una lista di prodotti nello scontrino
        public void addSpesa(List<SingolaSpesa> spese)
        {
            foreach (var item in spese)
            {
                scontrino.Add(item);
            }
        }

        //metodo per il calcolo del totale speso in uno scontrino
        public double totCost()
        {

            double tot = 0;
            foreach (var item in scontrino)
            {
                tot += item.costo;
            }

            return tot;
        }
    }
}
