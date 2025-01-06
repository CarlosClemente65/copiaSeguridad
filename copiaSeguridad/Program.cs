using copiaSeguridad;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static public string log = string.Empty;
    static public string destinoLog = string.Empty;
    static bool usb = true;
    static bool disco = true;
    public static string logUsb = @"D:\Copias\copiasSeguridad\logcopiausb.txt";
    public static string logDisco = @"D:\Copias\copiasSeguridad\logcopiaservidor.txt";

    static async Task Main(string[] args)
    {

        if(args.Length > 0) //Se puede pasar como argumento el tipo de copia (una 'u' o una 's' o ambas)
        {
            if(!args.Contains("u"))
            {
                if(File.Exists(logUsb))
                {
                    Console.WriteLine();
                    Console.WriteLine("No ha seleccionado hacer la copia en el USB");
                    Console.WriteLine(File.ReadAllText(logUsb));
                    Console.WriteLine();
                    Console.Write("Quieres hacer la copia en el USB (S/N)?");
                    while(true)
                    {
                        ConsoleKeyInfo tecla = Console.ReadKey();
                        if(tecla.Key == ConsoleKey.N)
                        {
                            usb = false;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Console.WriteLine();
                }
            }

            if(!args.Contains("s"))
            {
                if(File.Exists(logDisco))
                {
                    Console.WriteLine();
                    Console.WriteLine("No ha seleccionado la copia en el servidor");
                    Console.WriteLine(File.ReadAllText(logDisco));
                    Console.WriteLine();
                    Console.Write("Quieres hacer la copia en el servidor (S/N)?");
                    while(true)
                    {
                        ConsoleKeyInfo tecla = Console.ReadKey();
                        if(tecla.Key == ConsoleKey.N)
                        {
                            disco = false;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        Ficheros ficheros = new Ficheros(); //Instanciacion de la clase para leer el configuracion.json

        gestionServidor servidor = new gestionServidor();

        Console.Clear();
        if(disco)
        {
            if(!servidor.chequeoServidor()) //Chequeo si esta encendido el servidor para encenderlo
            {
                Console.WriteLine("Encendiendo el Servidor\n");
                servidor.encenderServidor();
            }
            else
            {
                Console.WriteLine("Servidor encendido\n");
            }
        }

        if(usb)
        {
            Console.WriteLine("Iniciando copia USB\n");
            if(File.Exists(@"u:\copias\errores.log"))
            {
                File.Delete(@"u:\copias\errores.log");
            }
            copiaUSB copia = new copiaUSB();
            await copia.lanzaCopia();
            Console.WriteLine("\nCopia en el USB finalizada");
            if(!string.IsNullOrEmpty(log))
            {
                log += $"Errores de la copia realizada el {DateTime.Now.ToShortTimeString}";
                File.WriteAllText(Path.Combine(@"u:\copias", "errores.log"), log);
                log = string.Empty;
            }

            if(disco)
            {
                Console.WriteLine("\nContinuamos con la copia en el servidor");
            }
        }

        if(servidor.chequeoServidor())
        {
            copiaServidor copia = new copiaServidor();
            await copia.lanzaCopia();

            if(!string.IsNullOrEmpty(log))
            {
                log += $"Errores de la copia realizada el {DateTime.Now.ToShortTimeString}";
                File.WriteAllText(Path.Combine(destinoLog, "errores.log"), log);
                log = string.Empty;
            }
            Console.WriteLine("\nCopia en el servidor finalizada. Pulsa una tecla para salir");
            Console.ReadKey();
        }
        else
        {
            Console.WriteLine("El servidor no esta encendido, no se hara esta copia. Pulsa una tecla para salir");
            Console.ReadKey();
        }


        if(servidor.chequeoServidor())
        {
            Console.WriteLine("\nApagando el servidor");
            servidor.apagarServidor();
        }
    }

}
