using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SavePaper
{
    //classe statica per la gestione dei file per la lettura e la scrittura delle liste di scontrini
    public static class FileManager
    {
        //path totale basato sulla posizione dell'eseguibile
        public static string path = Assembly.GetExecutingAssembly().Location + "/../spese/";

        //estensione dei file
        public static string extension = ".xml";

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
            fileName += extension;

            if (File.ReadAllBytes(path + fileName).Length == 0)
                return new GruppoSpese(new List<Scontrino>());

            XmlSerializer serializer = new XmlSerializer(typeof(GruppoSpese));
            GruppoSpese temp;
            using (Stream reader = new FileStream(path + fileName, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                temp = (GruppoSpese)serializer.Deserialize(reader);
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
        public static void deleteFile(string fileName)
        {
            string myfile = path + fileName + extension;
            if (File.Exists(myfile))
                File.Delete(myfile);

            ExcelManager.deleteFile(path + fileName);
        }

        //metodo per il salvataggio del file excel (useless)
        public static void writeExcel(List<Scontrino> scontrini, string fileName)
        {
            ExcelManager.saveExcel(scontrini, path, fileName);
        }

    }
}
