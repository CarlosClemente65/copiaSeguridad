using System.Diagnostics;

namespace copiaSeguridad
{
    public class copiaServidor
    {
        private string destino1 = @"\\SERVIDOR\copias_usb\";
        private string destino2 = @"\\SERVIDOR\copias_disco\";
        private String destinoCopia = string.Empty;
        private string pathCopia1 = @"d:\copias\copiausb.txt";
        private string pathCopia2 = @"d:\copias\copiadisco.txt";

        public copiaServidor()
        {
            string fecha = DateTime.Now.ToShortDateString();
            string hora = DateTime.Now.ToShortTimeString();
            string controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}";
            if (File.Exists(pathCopia2))
            {
                destinoCopia = destino1;
                Program.destinoLog = destino1;
                File.Delete(pathCopia2);
                File.WriteAllText(pathCopia1, controlCopia);
            }
            else if (File.Exists(pathCopia1))
            {
                destinoCopia = destino2;
                Program.destinoLog = destino2;
                File.Delete(pathCopia1);
                File.WriteAllText(pathCopia2, controlCopia);
            }
        }

        public async Task lanzaCopia()
        {
            Stopwatch timer = Stopwatch.StartNew();
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

            timer.Stop();
            string fecha = DateTime.Now.ToShortDateString();
            string hora = DateTime.Now.ToShortTimeString();
            string pathLogCopia = @"d:\copias\logcopiaservidor.txt";
            string controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}. Duracion de la copia: {(int)timer.Elapsed.TotalMinutes} minutos";
            File.WriteAllText(Path.Combine(destinoCopia, "copia.txt"), controlCopia);
            File.WriteAllText(pathLogCopia, controlCopia);

        }
    }
}
