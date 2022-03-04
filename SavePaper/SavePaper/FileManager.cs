using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SavePaper
{
    //classe statica per la gestione dei file per la lettura e la scrittura delle liste di scontrini
    public static class FileManager
    {
        public static string startpath;

        //path totale basato sulla posizione dell'eseguibile
        public static string path = Assembly.GetExecutingAssembly().Location + "/../spese/";

        //estensione dei file
        public static string extension = ".sp";

        //metodo per il controlle dell'esistenza e crazione dei file e directory
        public static void filecheck(string fileName)
        {
            fileName += extension;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (!File.Exists(path + fileName))
                File.Create(path + fileName).Close();
        }

        public static void filecheck()
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        //metodo per serializzare e salvare su file le lista di scontrini
        public static void salvaScontrini(GruppoSpese scontrini, string fileName)
        {
            fileName += extension;
            XmlSerializer serializer = new XmlSerializer(typeof(GruppoSpese));
            TextWriter writer = new StreamWriter(path + fileName);

            serializer.Serialize(writer, scontrini);
            writer.Close();
        }

        //metodo per de serializzare e ritornare la lista di scontrini caricata su una lista
        public static GruppoSpese loadScontrini(string fileName)
        {
            GruppoSpese temp = new GruppoSpese(new List<Scontrino>());
            temp.current_path = fileName;
            if (File.ReadAllBytes(fileName).Length == 0)
                return temp;

            XmlSerializer serializer = new XmlSerializer(typeof(GruppoSpese));
            
            using (Stream reader = new FileStream(fileName, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                temp = (GruppoSpese)serializer.Deserialize(reader);
                temp.current_path = fileName;
            }

            return temp;
        }

        //metedo per caricare la lista di file (gruppi di scontrini)
        public static string[] getPapersList()
        {
            List<string> files = Directory.GetFiles(path).ToList();
            List<string> returner = new List<string>();
            foreach (var item in files)
            {
                if (item.Contains(extension))
                    returner.Add(Path.GetFileName(item).Split('.')[0]);

            }

            return returner.ToArray();
        }

        //metodo per la cancellazione del file contenente il gruppo di scontrini
        public static void deleteFile(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            //da fixare
            //ExcelManager.deleteFile(path + filePath);
        }

        //metodo per il salvataggio del file excel (useless)
        public static void writeExcel(List<Scontrino> scontrini, string fileName)
        {
            ExcelManager.saveExcel(scontrini, path, fileName);
        }

        public static void ExportSp(string filePath)
        {
            GruppoSpese tmp = loadScontrini(filePath);
            using (var fbs = new FolderBrowserDialog())
            {
                DialogResult result = fbs.ShowDialog();
                if(result == DialogResult.OK)
                {
                    string fileName = Path.GetFileName(filePath);
                    string ExportFile = fbs.SelectedPath + "\\" + filePath;
                    ExportGroup(tmp, ExportFile);
                }
            }
        }

        public static void ExportGroup(GruppoSpese scontrini, string filepath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(GruppoSpese));
            TextWriter writer = new StreamWriter(filepath);

            serializer.Serialize(writer, scontrini);
            writer.Close();
        }
    }
}
