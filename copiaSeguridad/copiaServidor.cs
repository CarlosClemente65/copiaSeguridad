using System.Diagnostics;

namespace copiaSeguridad
{
    public class copiaServidor
    {
        private string destino_usb = @"\\SERVIDOR\copias_usb\";
        private string destino_disco = @"\\SERVIDOR\copias_disco\";
        private string destinoCopia = string.Empty;
        private string pathCopia_usb = @"D:\Copias\copiasSeguridad\controlCopiaUsb.txt";
        private string pathCopia_disco = @"D:\Copias\copiasSeguridad\controlCopiaDisco.txt";
        
        private string fecha;
        private string hora;
        private string controlCopia;
        private string ultimaCopia = string.Empty;

        public copiaServidor()
        {
            fecha = DateTime.Now.ToShortDateString();
            hora = DateTime.Now.ToShortTimeString();
            controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}";
            //Comprueba si la ultima copia se ha hecho en el disco del servidor, para hacerla en el usb
            if(File.Exists(pathCopia_disco))
            {
                ultimaCopia = " en el usb del servidor";
                controlCopia += ultimaCopia;
                destinoCopia = destino_usb;
                Program.destinoLog = destino_usb;
                File.Delete(pathCopia_disco);
                File.WriteAllText(pathCopia_usb, controlCopia);
            }
            else
            {
                controlCopia += " en el disco del servidor";
                destinoCopia = destino_disco;
                Program.destinoLog = destino_disco;
                if(File.Exists(pathCopia_usb))
                {
                    File.Delete(pathCopia_usb);
                }
                File.WriteAllText(pathCopia_disco, controlCopia);
            }
        }

        public async Task lanzaCopia()
        {
            Stopwatch timer = Stopwatch.StartNew();
            List<Ficheros.Fichero> listaFicheros = Ficheros.obtenerFicheros();
            foreach(var archivo in listaFicheros)
            {
                int clase = archivo.Clase;
                if(clase == 2)
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
                            while(!robocopy.StandardOutput.EndOfStream)
                            {
                                string linea = robocopy.StandardOutput.ReadLine();
                                Console.WriteLine(linea);
                            }
                            robocopy.WaitForExit();
                        }
                        catch(Exception ex)
                        {
                            Program.log += $"Error al comprimir archivos: {ex.Message}";
                        }
                    });
                    await tarea;
                }
            }

            timer.Stop();
            controlCopia += $". Duracion de la copia: {(int)timer.Elapsed.TotalMinutes} minutos";
            File.WriteAllText(Path.Combine(destinoCopia, "copia.txt"), controlCopia);
            File.WriteAllText(Program.logDisco, controlCopia);

        }
    }
}
