using Newtonsoft.Json;


namespace copiaSeguridad
{
    public class Ficheros
    {
        string rutaJson = @"configuracion.json";
        
        // Lista estática para almacenar los ficheros
        public static List<Fichero> listaFicheros = new List<Fichero>();

        //Diccionario para almacenar los datos de los ficheros por defecto
        public List<Dictionary<string, object>> Valores { get; set; }

        // Clase para representar un fichero con nombre y ruta
        public class Fichero
        {
            public string Nombre { get; set; }
            public string Ruta { get; set; }
            public int Clase { get; set; } //Indica si se usara para la copia en usb (1) o en el servidor (2)
        }

        //Constructor de la clase Ficheros
        public Ficheros()
        {
            leerFicheros();
        }

        public static List<Fichero> obtenerFicheros()
        {
            return listaFicheros;
        }

        // Método para cargar los ficheros desde el archivo JSON
        public void leerFicheros()
        {
            if (!File.Exists(rutaJson))
            {
                //Si no existe el fichero.json crea uno por defecto
                generarFichero();
            }
            string json = File.ReadAllText(rutaJson);
            listaFicheros = JsonConvert.DeserializeObject<List<Fichero>>(json);
        }

        public void generarFichero()
        {
            //Crea un nuevo diccionario para añadir los valores por defecto al ficheros.json
            Valores = new List<Dictionary<string, object>>();

            //Añade los valores por defecto
            agregarValoresDiccionario(1, "dspi", @"c:\dspi");
            agregarValoresDiccionario(1, "db_asesoria", @"d:\dropbox\asesoria");
            agregarValoresDiccionario(1, "db_diagram", @"d:\oficina_ds\diagram");
            agregarValoresDiccionario(1, "programacion", @"D:\Programacion\C#");
            agregarValoresDiccionario(2, @"copia_dspi\dspi", @"c:\dspi");
            agregarValoresDiccionario(2, "copia_dropbox", @"d:\dropbox");
            agregarValoresDiccionario(2, "copia_oficina_ds", @"d:\oficina_ds");
            agregarValoresDiccionario(2, "copia_asesoria_2", @"d:\asesoria (datos sin sincronizar)");
            agregarValoresDiccionario(2, "copia_carlos", @"d:\carlos");
            agregarValoresDiccionario(2, "copia_copias", @"d:\copias");
            agregarValoresDiccionario(2, "copia_programacion", @"d:\programacion\c#");

            string json = JsonConvert.SerializeObject(Valores, Formatting.Indented);
            File.WriteAllText(rutaJson, json);
        }

        private void agregarValoresDiccionario(int clase, string nombre, string ruta)
        {
            //Metodo para ir agregando cada uno de los elementos al diccionario
            Dictionary<string, object> nuevoValor = new Dictionary<string, object>
            {
                { "Clase", clase },
                { "Nombre", nombre },
                { "Ruta", ruta }
            };

            Valores.Add(nuevoValor);
        }
    }
}
