using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.SqlClient;
using Aspose.Cells;

namespace SavePaper
{
    static class ExcelManager
    {

        //array contenete i nomi dei mesi per una migliore suddivisione nel calcolo su Excel
        private static string[] mesi = { "Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno", "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre", };


        //metodo per il salvataggio e formattazione del fie Excel
        public static void saveExcel(List<Scontrino> spese, string path, string nomeGruppo)
        {
            try
            {
                Workbook wb = new Workbook();
                Worksheet sheet = wb.Worksheets[0];

                //valore y della casella Excel da dove parte il mese
                int monthy = 4;

                Cells cells = sheet.Cells;
                Cell cell;

                cell = cells["A1"];
                cell.PutValue("MOVENTE");

                Aspose.Cells.Style style = sheet.Cells["A1"].GetStyle();
                style.Pattern = BackgroundType.Solid;
                style.ForegroundColor = System.Drawing.Color.White;
                style.Font.Color = Color.White; 

                cell.SetStyle(style);

                cell = cells["B1"];
                cell.PutValue("VENDITORE");
                cell.SetStyle(style);

                cell = cells["C1"];
                cell.PutValue("DATA");
                cell.SetStyle(style);

                cell = cells["D1"];
                cell.PutValue("PRODOTTI");
                cell.SetStyle(style);

                cell = cells["E1"];
                cell.PutValue("COSTO");
                cell.SetStyle(style);
                
                
                
                //ciclo e creo una piccola tabella per ogni mese
                for (int i = 1; i < 13; i++)
                {

                    style.ForegroundColor = System.Drawing.Color.Green;
                    
                    cell = cells["A" + (monthy-1)];
                    cell.PutValue(mesi[i-1] + ":");
                    cell.SetStyle(style);

                    style.ForegroundColor = System.Drawing.Color.Blue;
                    

                    cell = cells["A" + monthy];
                    cell.PutValue("Movente");
                    cell.SetStyle(style);

                    cell = cells["B" + monthy];
                    cell.PutValue("Venditore");
                    cell.SetStyle(style);

                    cell = cells["C" + monthy];
                    cell.PutValue("Data");
                    cell.SetStyle(style);

                    cell = cells["D" + monthy];
                    cell.PutValue("Prodotti");
                    cell.SetStyle(style);

                    cell = cells["E" + monthy];
                    cell.PutValue("Costo");
                    cell.SetStyle(style);

                    //useless
                    int x = 1;

                    //cella y 
                    int y = monthy + 1;

                    //y dove inizia la tabella del mese, utilizzata per il calcolo mensile
                    int currenty = y;
                    
                    //ciclo per la lettura dei dati degli scontrini
                    foreach (var scontrino in spese)
                    {
                        //controllo che siano del mese corretto
                        if (scontrino.dataAcquisto.Month == i) {

                            cell = cells["A" + y];
                            cell.PutValue(scontrino.movente);

                            cell = cells["B" + y];
                            cell.PutValue(scontrino.venditore);

                            //formatto in modo decente la data (useless)
                            string day = scontrino.dataAcquisto.Day + "/" + scontrino.dataAcquisto.Month + "/" + scontrino.dataAcquisto.Year;

                            cell = cells["C" + y];
                            cell.PutValue(day);

                            //ciclo per la lettura dei prodotti per scontrino
                            foreach (var spesa in scontrino.scontrino)
                            {
                                cell = cells["D" + y];
                                cell.PutValue(spesa.nome);

                                cell = cells["E" + y];
                                cell.PutValue(spesa.costo);

                                //setto le celle in valuta €/$
                                style.ForegroundColor = System.Drawing.Color.Orange;
                                style.Number = 5;
                                cell.SetStyle(style);
                                y++;
                            }
                        }
                    }

                    //celle per il totale
                    cell = cells["D" + y];
                    cell.PutValue("Totale:");

                    cell = cells["E" + y];

                    //se la tabella ha almeno una spesa al suo interno fa il calcolo altrimenti evita
                    if(y!=currenty)
                        cell.Formula = "=SUM(E" + currenty + ":E" + (y - 1) + ")";
                    else
                        cell.PutValue(0);

                    //setto la cella in valuta €/$
                    style.ForegroundColor = System.Drawing.Color.OrangeRed;
                    style.Number = 5;
                    cell.SetStyle(style);

                    monthy = y + 4;
                }

                //salvo il file .xlsx
                wb.Save(path + nomeGruppo + ".xlsx", SaveFormat.Xlsx);
            }
            catch (Exception e)
            {
                //nel caso il file sia già aperto in Excel il programma non potrebbe aprirlo perciò viene notificato
                MessageBox.Show("impossibile aprire il file aggiornato perchè già in uso da una altro programma\n" + e.Message);
            }
        }

        //metodo per la cancellazione del file excel nel caso esista
        public static void deleteFile(string fileName)
        {
            string myfile = fileName + ".xlsx";
            if (File.Exists(myfile))
                File.Delete(myfile);
        }
    }
}
