using System.IO.Compression;

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
                foreach (var archivo in listaFicheros)
                {
                    int clase = archivo.Clase;
                    if (clase == 1)
                    {
                        string nombre = archivo.Nombre;
                        string ruta = archivo.Ruta;
                        string rutaZip = Path.Combine(destino, $"{nombre}.zip");
                        string rutaZipTmp = Path.Combine(Path.GetTempPath(), $"{nombre}.zip");

                        if (File.Exists(rutaZipTmp))
                        {
                            File.Delete(rutaZipTmp);
                        }
                        if (File.Exists(rutaZip))
                        {
                            File.Delete(rutaZip);
                        }

                        Console.WriteLine($"\nComprimiendo ficheros de {nombre}. Espere por favor ...");
                        //Creamos la tarea a ejecutar
                        Task tarea = Task.Run(async () =>
                        {
                            try
                            {
                                using (var zip = ZipFile.Open(rutaZipTmp, ZipArchiveMode.Create))
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

                                Console.WriteLine($"Copiando fichero {nombre} al disco USB. Espere por favor ...");
                                await Task.Run(() => File.Copy(rutaZipTmp, rutaZip, true));
                                File.Delete(rutaZipTmp);
                            }
                            catch (Exception ex)
                            {
                                Program.log += $"Error en el proceso de copia USB.\n\t- {ex.Message}";
                            }
                        });
                        await tarea;
                    }
                }

                string fecha = DateTime.Now.ToShortDateString();
                string hora = DateTime.Now.ToShortTimeString();
                string controlCopia = $"Ultima copia realizada el dia {fecha} a las {hora}";
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

