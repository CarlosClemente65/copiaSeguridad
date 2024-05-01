using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace copiaSeguridad
{
    public class gestionServidor
    {
        private IPAddress ipServidor = IPAddress.Parse("192.168.1.65");
        private int puertoWol = 9;
        private string direccionMAC = "D8CB8AE6B1FC";

        public bool chequeoServidor()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ipServidor);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void encenderServidor()
        {
            byte[] magicPacket = crearPaquete();
            envioPaquete();
            System.Threading.Thread.Sleep(35000); // Espera 15 segundos
        }

        public byte[] crearPaquete() //Crea el Magic Paquet para el envio del WOL
        {
            string macAddress = Regex.Replace(direccionMAC, "[: -]", ""); //Se eliminan de la direccion MAC los simbolos y se deja unicamente los caracteres.
            byte[] macBytes = Convert.FromHexString(macAddress); //Se convierte a hexadecimal la direccion MAC

            IEnumerable<byte> header = Enumerable.Repeat((byte)0xff, 6); //La cabecera son 6 veces 255 (0xff en hexadecimal)
            IEnumerable<byte> data = Enumerable.Repeat(macBytes, 16).SelectMany(m => m); //Se añaden 16 vecs la direccion MAC en hesadecimal
            return header.Concat(data).ToArray(); //Se une la cabecera y los datos para formar el Magic Paquet
        }


        async void envioPaquete()
        {
            try
            {

                PhysicalAddress target = PhysicalAddress.Parse(direccionMAC);
                var header = Enumerable.Repeat(byte.MaxValue, 6);
                var data = Enumerable.Repeat(target.GetAddressBytes(), 16).SelectMany(mac => mac);

                var magicPacket = header.Concat(data).ToArray();

                using var client = new UdpClient();

                await client.SendAsync(magicPacket, magicPacket.Length, new IPEndPoint(IPAddress.Broadcast, puertoWol));
            }

            catch (Exception e)
            {
                Console.WriteLine($"Error al enviar paquete {e.Message}");
            }
        }

        public void apagarServidor()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = $"/s /m \\\\{ipServidor}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process.Start(psi);
                System.Threading.Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se ha podido apagar el servidor. {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}
