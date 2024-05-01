using System.IO.Compression;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace copiaSeguridad
{
    public class copiaUSB
    {
        string destino = @"u:\copias\";
        public async Task lanzaCopia()
        {
            List<Ficheros.Fichero> listaFicheros = Ficheros.obtenerFicheros();
            if (File.Exists(Path.Combine(destino, "copia.txt"))) //Controla si esta introducida la llave usb
            {
                Stopwatch timerUsb = Stopwatch.StartNew();
                foreach (var archivo in listaFicheros)
                {
                    int clase = archivo.Clase;
                    if (clase == 1)
                    {
                        string nombre = archivo.Nombre;
                        string ruta = archivo.Ruta;
                        string rutaZip = Path.Combine(destino, $"{nombre}.zip");
                        string rutaZipTmp = Path.Combine(Path.GetTempPath(), $"{nombre}.zip");

                        Console.WriteLine($"\nComprimiendo ficheros de {nombre}. Espere por favor ...");
                        Stopwatch timer = Stopwatch.StartNew();
                        //Creamos la tarea a ejecutar
                        Task tarea = Task.Run(async () =>
                        {
                            try
                            {
                                if (File.Exists(rutaZip))
                                {
                                    Console.WriteLine($"Abriendo fichero {rutaZip}...");
                                    using (var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Update))
                                    {
                                        Console.WriteLine("Leyendo ficheros a grabar...");
                                        var archivosZip = zip.Entries.Select(entry => entry.FullName).ToList();
                                        string[] archivosActuales = Directory.GetFiles(ruta, "*", SearchOption.AllDirectories);

                                        Console.WriteLine("Actualizando ficheros...");
                                        // Agregar nuevos archivos y actualizar los existentes
                                        foreach (var archivoActual in archivosActuales)
                                        {
                                            string nombreArchivo = Path.GetFileName(archivoActual);

                                            //Si no existe el fichero se crea
                                            if (!archivosZip.Contains(nombreArchivo))
                                            {
                                                zip.CreateEntryFromFile(archivoActual, nombreArchivo);
                                            }
                                            else
                                            {
                                                //Si existe el fichero se compara la fecha de modificacion
                                                var infoArchivo = new FileInfo(archivoActual);
                                                var archivoZip = zip.GetEntry(nombreArchivo);

                                                if (archivoZip != null)
                                                {
                                                    // Obtener la fecha de modificación del archivo en el ZIP
                                                    var fechaArchivoZip = archivoZip.LastWriteTime.DateTime;

                                                    //Si la fecha del archivo es mas reciente que en el zip, se actualiza
                                                    if (infoArchivo.LastWriteTime.Date > fechaArchivoZip.Date)
                                                    {
                                                        //Se borra el fichero del zip, y se copia el actual
                                                        archivoZip.Delete();
                                                        zip.CreateEntryFromFile(archivoActual, nombreArchivo);
                                                    }
                                                }
                                            }
                                        }

                                        // Eliminar archivos que ya no existen
                                        foreach (var archivoZip in archivosZip)
                                        {
                                            if (!archivosActuales.Any(archivo => Path.GetFileName(archivo) == archivoZip))
                                            {
                                                zip.GetEntry(archivoZip)?.Delete();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //Si no existe el fichero.zip se crea de nuevo
                                    using (var zip = ZipFile.Open(rutaZip, ZipArchiveMode.Create))
                                    {
                                        string[] ficheros = Directory.GetFiles(ruta, "*", SearchOption.AllDirectories);

                                        foreach (var fichero in ficheros)
                                        {
                                            try
                                            {
                                                zip.CreateEntryFromFile(fichero, Path.GetFileName(fichero));
                                            }
                                            catch (IOException ex)
                                            {
                                                Program.log += $"Error de copia en el USB del fichero {nombre}.\n\t- {ex.Message}\n";
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Program.log += $"Error en el proceso de copia USB.\n\t- {ex.Message}";
                            }
                        });

                        await tarea;
                        timer.Stop();
                        Console.WriteLine($"Copia de {archivo.Nombre} finalizada en {(int)timer.Elapsed.TotalMinutes} minutos");
                    }
                }

                timerUsb.Stop();
                string fecha = DateTime.Now.ToShortDateString();
                string hora = DateTime.Now.ToShortTimeString();
                string controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}. Duracion de la copia: {(int)timerUsb.Elapsed.TotalMinutes} minutos";
                File.WriteAllText(Path.Combine(destino, "copia.txt"), controlCopia);
            }
            else
            {
                ConsoleColor color = Console.BackgroundColor;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("No esta insertada la llave USB.");
                System.Threading.Thread.Sleep(5000);
                Console.BackgroundColor = color;

            }
        }
    }
}

