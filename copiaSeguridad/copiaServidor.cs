using System.Diagnostics;

namespace copiaSeguridad
{
    public class copiaServidor
    {
        private string destino1 = @"\\SERVIDOR\copias_usb\";
        private string destino2 = @"\\SERVIDOR\copias_disco\";
        private String destinoCopia = string.Empty;

        public copiaServidor()
        {
            string fecha = DateTime.Now.ToShortDateString();
            string hora = DateTime.Now.ToShortTimeString();
            string controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}";
            if (File.Exists(@"d:\copias\copiadisco.txt"))
            {
                destinoCopia = destino1;
                Program.destinoLog = destino1;
                File.Delete(@"d:\copias\copiadisco.txt");
                File.WriteAllText(@"d:\copias\copiausb.txt", controlCopia);
            }
            else if (File.Exists(@"d:\copias\copiausb.txt"))
            {
                destinoCopia = destino2;
                Program.destinoLog = destino2;
                File.Delete(@"d:\copias\copiausb.txt");
                File.WriteAllText(@"d:\copias\copiadisco.txt", controlCopia);
            }
        }

        public async Task lanzaCopia()
        {
            List<Ficheros.Fichero> listaFicheros = Ficheros.obtenerFicheros();
            foreach (var archivo in listaFicheros)
            {
                int clase = archivo.Clase;
                if (clase == 2)
                {
                    string nombre = archivo.Nombre;
                    string origen = archivo.Ruta;
                    string destino = Path.Combine(destinoCopia, nombre);

                    Console.WriteLine($"\nCopiando ficheros de {nombre} al servidor. Espere por favor ...");
                    //Creamos la tarea a ejecutar
                    string argumentos = $" {origen} {destino} /MIR /MT:36 /NP /r:3";
                    Process robocopy = new Process();
                    robocopy.StartInfo.FileName = "robocopy";
                    robocopy.StartInfo.Arguments = argumentos;
                    robocopy.StartInfo.RedirectStandardOutput = true;
                    robocopy.StartInfo.UseShellExecute = false;

                    Task tarea = Task.Run(() =>
                    {
                        try
                        {
                            robocopy.Start();
                            while (!robocopy.StandardOutput.EndOfStream)
                            {
                                string linea = robocopy.StandardOutput.ReadLine();
                                Console.WriteLine(linea);
                            }
                            robocopy.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            Program.log += $"Error al comprimir archivos: {ex.Message}";
                        }
                    });
                    await tarea;
                }
            }

            string fecha = DateTime.Now.ToShortDateString();
            string hora = DateTime.Now.ToShortTimeString();
            string controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}";
            File.WriteAllText(Path.Combine(destinoCopia, "copia.txt"), controlCopia);

        }
    }
}
