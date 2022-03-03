using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SavePaper
{
    public partial class MainWindow : Window
    {
        //lista degli scontrini aggiunti
        GruppoSpese spese;

        string[] myPapers;

        //costruttore della window
        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists("SavePaperUpdater.exe"))
                if (File.Exists("newSavePaperUpdater.exe"))
                {
                    System.Diagnostics.Process.Start("newSavePaperUpdater.exe");
                    this.Close();
                }
            //FileManager.startpath = @"C:\Users\Claudio\Desktop\2022 - Copia.sp";
            if (FileManager.startpath!=null)
            {
                PapersList.ItemsSource = myPapers;
                PapersList.Items.Add(System.IO.Path.GetFileName(FileManager.startpath.Replace(FileManager.extension, "")));
                PapersList.SelectedIndex = 0;
                updateListBox();
                return;
            }

            FileManager.filecheck();
            CheckVersion();
            //controllo se ci sono dei gruppi di scontrini già salvati
            //altrimenti visualizzo il popup per l'inserimento
            if (FileManager.getPapersList().Length == 0)
                AddPapersList.Visibility = Visibility.Visible;
            else
            {
                spese = new GruppoSpese(new List<Scontrino>());
                myPapers = FileManager.getPapersList();
                PapersList.ItemsSource = myPapers;
                updateListBox();
            }
            PapersList.SelectedIndex = 0;
        }

        //metodo che in base alla lista di scontrini (spese) svuota e riempie la list box con gli contrini aggiornati
        public void updateListBox()
        {

            ListBoxItem labelItem;
            PaperList.Items.Clear();
            updateBudget();
            foreach (var item in spese.spese)
            {
                labelItem = new ListBoxItem();
                labelItem.Selected += LabelItem_Selected;
                labelItem.Background = Brushes.White;
                labelItem.Foreground = Brushes.Black;
                labelItem.Height = 40;
                labelItem.VerticalAlignment = VerticalAlignment.Center;
                labelItem.HorizontalAlignment = HorizontalAlignment.Center;
                labelItem.Width = 519;
                labelItem.Margin = new Thickness(5, 5, 5, 0);
                labelItem.Content = item.movente + ", data: " + item.dataAcquisto.ToString().Split(' ')[0];

                PaperList.Items.Add(labelItem);
            }
        }

        //evento causato dalla selezione di uno scontrino dalla list box
        //aggiorna l'anteprima dello scontrino sulla view a destra
        private void LabelItem_Selected(object sender, RoutedEventArgs e)
        {
            //prendo il movente
            int id = PaperList.Items.IndexOf(((ListBoxItem)sender));
            PaperName.Content = spese.spese[id].movente;

            //faccio il conto del prezzo totale dello scontrino
            totPrice.Content = "Tot: " + (spese.spese[id].totCost()) + "€";

            Label spesa;
            PaperInList.Children.Clear();

            //stampo la lista dello scontrino
            foreach (var item in spese.spese[id].scontrino)
            {
                spesa = new Label();
                spesa.Content = item.nome + ":      " + item.costo + "€";
                PaperInList.Children.Add(spesa);
            }
            
        }
        
        //evento richiamato per l'aggiunta di un nuovo scontrino nel gruppo selezionato
        //rende visibile il popup per l'inserimento dei dati
        private void NewPaper_BT_Click(object sender, RoutedEventArgs e)
        {
            if (PapersList.Items.Count > 0 && PapersList.SelectedIndex >= 0)
                AddPaper.Visibility = Visibility.Visible;
        }

        //evento richiamato da entrambi i bottoni nel popup per il nuovo scontrino
        private void closeNewPaper(object sender, RoutedEventArgs e)
        {
            //rimpiazzo gli spazi per evitare che il nome sia composto da soli caratteri "vuoti"
            string venditore = TB_Venditore.Text;
            venditore = venditore.Replace(" ", "");

            string movente = TB_Movente.Text;
            movente = movente.Replace(" ", "");

            string lista = TB_listaSpesa.Text;

            //esempio per fix: sacchetto di pasta 5€; pomodori 10€;
            //faccio tutti i controlli per lasciare visibile il popup o il salvataggio dei file
            if (venditore != "" && movente != "" && lista != "" && ((Button)sender).Equals(AcceptPaperBT))
            {
                if (PapersList.Items.Count > 0 && PapersList.SelectedIndex >= 0) {
                    try
                    {
                        //"de serializzo" i dati inseriti nel form per creare un istanza di Scontrino
                        List<SingolaSpesa> spese_ = new List<SingolaSpesa>();

                        //divido i singoli prodotti con i prezzi
                        foreach (var prodotti in lista.Split(';'))
                        {
                            if (prodotti.Length > 3)
                            {
                                string nome = "";
                                double prezzo = 0;
                                foreach (var prodotto in prodotti.Split(' '))
                                {
                                    /*if (prodotto.ToCharArray()[0] == ' ')
                                        prodotto.Remove(0);*/

                                    if (!prodotto.Contains('€'))
                                    {
                                        if (prodotto.Length > 0)
                                            nome += prodotto + " ";
                                    }else
                                    {
                                        prezzo = double.Parse(prodotto.Replace("€", ""));
                                        break;
                                    }
                                }

                                if (prezzo == 0)
                                {
                                    MessageBox.Show("inserisci correttamente i prezzi");
                                    return;
                                }
                                spese_.Add(new SingolaSpesa(prezzo, nome.Remove(nome.Length - 1)));
                            }
                        }


                        //divido il prodotto dal suo prezzo e lo aggiungo alla lista di SingolaSpesa dell scontrino
                        /*for (int i = 0; i < lista.Split(' ').Length; i += 2)
                        {
                            if (lista.Split(' ')[i + 1].Contains('€'))
                            {
                                spese_.Add(new SingolaSpesa(double.Parse(lista.Split(' ')[i + 1].Replace("€", "")), lista.Split(' ')[i]));
                                updateBudget();
                            }
                            else
                                throw new Exception("errore nell'inserimento dei prezzi");
                        }*/

                        //aggiungo lo scontrino
                        spese.spese.Add(new Scontrino(TB_Movente.Text, TB_Venditore.Text, DP_Date.SelectedDate.Value, spese.spese.Count));
                        
                        //aggiungo all'ultimo scontrino aggiunto la lista di prodotti
                        spese.spese.Last().addSpesa(spese_);
                        updateListBox();
                        AddPaper.Visibility = Visibility.Hidden;
                    }
                    catch (Exception E) { MessageBox.Show("errore nell'inserimento dei dati: " + E.Message); return; }

                }
                else
                if (((Button)sender).Equals(AcceptPaperBT))
                    return;

                clearPaper();
                FileManager.ExportGroup(spese, spese.current_path);
            }
            if (((Button)sender).Equals(AcceptPaperBT))
                return;

            AddPaper.Visibility = Visibility.Hidden;
        }

        //evento per la visualizzazione del popup, che visualizza il totale speso del gruppo scontrini
        private void SpesaTotale_BT_Click(object sender, RoutedEventArgs e)
        {
            if (PapersList.Items.Count > 0 && PapersList.SelectedIndex >= 0)
            {
                double totSpesa = 0;
                foreach (var item in spese.spese)
                {
                    totSpesa += item.totCost();
                }
                MessageBox.Show("spesa totale: " + totSpesa + "€");
            }
        }

        //evento richiamato dal pulsante verde
        private void openWithExcel(object sender, RoutedEventArgs e)
        {

            //richiama il metodo per il salvataggio del file, la sua formattazione il salvataggio
            FileManager.writeExcel(spese.spese, PapersList.SelectedItem.ToString());

            //controllo se è sensata l'apertura del file
            if (PapersList.Items.Count > 0 && PapersList.SelectedIndex >= 0) {
                if (spese.spese.Count > 0)
                {
                    //ricreo il path completo del file per la sua apertura
                    string fileName = FileManager.path + PapersList.SelectedItem.ToString() + ".xlsx";
                    //apro il file salvato con excel
                    try
                    {
                        //new ExcelOpener(fileName).ShowDialog();
                        Process.Start(fileName);
                    }
                    catch (Exception E) {
                        if (MessageBox.Show("impossibile aprire il file, vuoi provare a visualizzarlo qui?", "Attenzione", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            new ExcelOpener(fileName).ShowDialog();
                    }
                }
                else
                    MessageBox.Show("non ci sono scontrini da visualizzare", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //evento richiamato dalla 
        private void updatePaperlist(object sender, SelectionChangedEventArgs e)
        {
            spese = new GruppoSpese(new List<Scontrino>());

            string mypath;

            if (PapersList.SelectedItem != null)
            {
                if (FileManager.startpath != null)
                    mypath = FileManager.startpath;
                else
                    mypath = FileManager.path + PapersList.SelectedItem.ToString() + FileManager.extension;

                spese.nome = PapersList.SelectedItem.ToString();
                spese = FileManager.loadScontrini(mypath);
                spese.spese.Sort((x, y) => x.dataAcquisto.CompareTo(y.dataAcquisto));
            }

            updateListBox();
            clearPaper();
            PaperList.SelectedIndex = -1;
            clearPaperView();

        }

        //metodo che pulisce il form per la creazione del nuovo scontrino
        private void clearPaper()
        {
            TB_Movente.Text = "";
            TB_Venditore.Text = "";
            DP_Date.SelectedDate = null;
            TB_listaSpesa.Text = "";

            AddPaper.Visibility = Visibility.Hidden;
        }

        //metodo che pulisce l'anteprima dello scontrino selezionato
        private void clearPaperView()
        {
            PaperName.Content = "Nome Scontrino";
            totPrice.Content = "Tot: 0€";
            PaperInList.Children.Clear();
        }

        //metodo per la visualizzazione del form per la creazione del nuovo gruppo scontrini
        private void newPapersBT_Click(object sender, RoutedEventArgs e)
        {
            AddPapersList.Visibility = Visibility.Visible;
        }

        //evento utilizzato da entrambi i pulsanti nel popup
        private void closeNewPapersList(object sender, RoutedEventArgs e)
        {
            //setto la variabile per il controllo che non ci siano solo spazi
            string newName = NewPapersListNameTB.Text;
            newName = newName.Replace(" ", "");

            //controllo se il nome inserito è accettabile
            if (newName != "" && ((Button)sender).Equals(AcceptNewPapersBT))
            {
                //controllo e creo il nuovo file
                FileManager.filecheck(NewPapersListNameTB.Text);

                //svuoto la Textbox, nascondo il popup e aggiorno la lista di gruppi nella combobox
                NewPapersListNameTB.Text = "";
                AddPapersList.Visibility = Visibility.Hidden;
                myPapers = FileManager.getPapersList();
                PapersList.ItemsSource = myPapers;
                return;
            }

            //nel caso non ci siano gruppi di scontrini non voglio che il popup si chiuda, è obbligato a mettere almeno un gruppo con un nome != vuoto
            if (PapersList.Items.Count == 0 && newName == "")
            {
                AddPapersList.Visibility = Visibility.Visible;
                return;
            }

            //nel caso non ci siano gruppi di scontrini non voglio che il popup si chiuda, è obbligato a mettere almeno un gruppo quindi non è possibile premere X
            if (PapersList.Items.Count == 0 && newName != "" && ((Button)sender).Equals(CloseNewPapersBT))
            {
                AddPapersList.Visibility = Visibility.Visible;
                return;
            }

            //per esclusione se ci sono gruppi di scontrini è possibile annullare la creazione di un gruppo
            if (newName == "" || ((Button)sender).Equals(CloseNewPapersBT))
                AddPapersList.Visibility = Visibility.Hidden;

            //svuota la TextBox se la creazion viene annullata
            if(((Button)sender).Equals(CloseNewPapersBT) && PapersList.Items.Count > 0)
                NewPapersListNameTB.Text = "";
        }

        //evento per la rimozione di uno scontrino
        private void RemovePaper(object sender, RoutedEventArgs e)
        {
            //popup son la conferma SI/NO
            if (MessageBox.Show("sei sicuro di voler cancellare lo scontrino?", "Attenzione", MessageBoxButton.YesNo, MessageBoxImage.Warning)==MessageBoxResult.Yes) {
                if (PapersList.Items.Count > 0 && PapersList.SelectedIndex >= 0)
                {
                    //rimuovo dalla lista di scontrini l'istanza riferita allo scontrino selezionato
                    int idRemoved = PaperList.Items.IndexOf(((ListBoxItem)PaperList.SelectedItem));
                    spese.spese.RemoveAt(idRemoved);

                    //aggiorno la listBox in base alla lista scontrini aggiornata
                    updateListBox();

                    //salvo la lista scontrini aggiornata
                    FileManager.ExportGroup(spese, spese.current_path);

                    //svuoto l'anteprima dello scontrino sulla destra
                    clearPaperView();
                }
            }
        }

        //evento per la rimozione di un gruppo di scontrini
        private void DeletePapersBT_Click(object sender, RoutedEventArgs e)
        {
            //popup son la conferma SI/NO
            if (MessageBox.Show("sei sicuro di voler cancellare il gruppo scontrini?", "Attenzione", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {

                if (PapersList.Items.Count > 0)
                {
                    FileManager.deleteFile(spese.current_path);
                    myPapers = FileManager.getPapersList();
                    //updateListBox();
                    PapersList.ItemsSource = myPapers;
                    PapersList.SelectedIndex = 0;

                    if (PapersList.Items.Count == 0)
                        AddPapersList.Visibility = Visibility.Visible;
                }
            }
        }

        //evento per la visualizzazione del form dei settaggi
        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Visibility = Visibility.Visible;

            CB_ExportFile.ItemsSource = myPapers;

            if(spese.budgetSetted)
                TB_Budget.Text = spese.budget + "€";
        }

        //evento per la chiusura/salvataggio dei settings
        private void closeSettings(object sender, RoutedEventArgs e)
        {
            string budget = TB_Budget.Text;
            budget = budget.Replace(" ", "");
            budget = budget.Replace("€", "");
            budget = budget.Replace(".",",");

            if (budget!="" && double.Parse(budget) > 0 && ((Button)sender).Equals(AccepSettings))
            {
                spese.setBudget(double.Parse(budget), !(bool)Toggle_Budget.IsChecked);
                updateBudget();
                FileManager.salvaScontrini(spese, PapersList.SelectedItem.ToString());
                clearSettings();
            }
            else
                clearSettings();
        }

        //metodo per la cancellazione dei dati dal form settings
        private void clearSettings()
        {
            TB_Budget.Text = "";
            Toggle_Budget.IsChecked = false;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        //metodo per il controllo della versione attuale e se disponibile un nuovo aggiornamento
        private async void CheckVersion()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "C# console program");

            var content = await client.GetStringAsync("https://api.github.com/repos/Mene-hub/SavePaper/releases");
            //string tmp = content.Split(':')[11].Split('"')[1];

            string tmp = (JsonConvert.DeserializeObject<dynamic>(content)[0].tag_name);

            string projectVersion = Assembly.GetExecutingAssembly().GetName().ToString().Split('=')[1].Split(',')[0];
            projectVersion = projectVersion.Split('.')[0] + "." + projectVersion.Split('.')[1];

            if (projectVersion != tmp)
            {
                var Downloadoption = System.Windows.MessageBox.Show("C'è una nuova versione!\nvuoi scaricarla?", "Update " + tmp, MessageBoxButton.YesNo);

                if (Downloadoption == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("newSavePaperUpdater.exe");
                    this.Close();
                }
            }
        }

        //metodo usato per aggiornare il budget all'aggiunta di uno scontrino
        private void updateBudget()
        {
            BudgetView.Visibility = Visibility.Hidden;
            if (spese.budgetSetted)
            {
                BudgetLB.Content = "Budget: " + (spese.budget - spese.speseTotali()) + "€";
                BudgetView.Visibility = Visibility.Visible;

            }
        }

        //metodo per il esportare il file selezionato
        private void ExportSpFile(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CB_ExportFile.SelectedItem.ToString()))
            {
                FileManager.ExportSp(CB_ExportFile.SelectedItem.ToString());
                clearSettings();
            }
            else
                MessageBox.Show("Seleziona un Gruppo scontrini da esportare", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
