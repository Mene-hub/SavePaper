using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavePaper
{
    //classe per l'identificazione di un prodotto allinterno di uno scontrino
    public class SingolaSpesa
    {
        //costo del singolo prodotto in uno scontrino
        public double costo;

        //nome del prodotto all'interno dello scontrino
        public string nome;

        //costruttore
        public SingolaSpesa(double costo_, string nome_)
        {
            costo = costo_;
            nome = nome_;
        }

        //costruttore vuoto per la serializzazione della classe in XML
        public SingolaSpesa() { }
    }
}
