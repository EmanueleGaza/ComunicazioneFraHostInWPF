using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ComunicazioneFraHostInWPF
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            IPEndPoint localendpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 60000); //La variabile "localendpoint" contiente l'indirizzo IP e il numero di porta utilizzato per la trasmissione

            //Creazione del thread e avvio di quest'ultimo
            Thread t1 = new Thread(new ParameterizedThreadStart(SocketReceive));
            t1.Start(localendpoint);
        }

        /// <summary>
        /// Metodo per la gestione del messaggio ricevuto dall'altro host della rete
        /// </summary>
        /// <param name="sourceEndPoint"></param>
        public async void SocketReceive (object sourceEndPoint)
        {
            try
            {
                IPEndPoint sourceEP = (IPEndPoint)sourceEndPoint;
                Socket t = new Socket(sourceEP.AddressFamily, SocketType.Dgram, ProtocolType.Udp); //Recupera informazioni IPv4 necessarie
                t.Bind(sourceEP);
                Byte[] byteRicevuti = new byte[256]; //Massimo byte da ricevere 256
                string message = "";
                int bytes = 0; //Conta quanti byte sono stati ricevuti
                await Task.Run(() =>
                {
                    while (true)
                    {
                        if (t.Available > 0)
                        {
                            message = "";
                            bytes = t.Receive(byteRicevuti, byteRicevuti.Length, 0); //La variabile bytes ottiene i byte corrispondenti al messaggio 
                            message = message + Encoding.ASCII.GetString(byteRicevuti, 0, bytes); //Si ottiene il messaggio completo convertendo in ASCII i byte presenti nella variabile bytes

                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                lblMessaggioRicevuto.Content = message; //Mostra il messaggio ricevuto nella label del programma
                            }));
                        }
                    }
                });
            }
            catch (FormatException ex) //Controllo di errori nella ricezione del messaggio
            {
                MessageBox.Show("Si è verificato un problema nel formato: " + ex.Message, "ERRORE", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Si è verificato un problema: " + ex.Message, "ERRORE", MessageBoxButton.OK);
            }
            
        }

        /// <summary>
        /// Metodo per la gestione del messaggio da inviare all'altro host della rete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtDestinationIP == null)
                {
                    throw new Exception("Non è inserito nessuno indirizzo IP all'interno della textbox");
                }
                IPAddress ipDest = IPAddress.Parse(txtDestinationIP.Text); //Inserisce l'indirizzo IP scritto nella TextBox all'interno della variabile ipDest
                int portDest = int.Parse(txtDestinationPort.Text); //Inserisce il numero di porta scritto nella TextBox adeguata
                IPEndPoint remoteEndPoint = new IPEndPoint(ipDest, portDest);
                Socket s = new Socket(ipDest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                if (txtMessaggioDaInviare.Text == null)
                {
                    throw new Exception("La textbox è vuota, per favore inserisci il messaggio da inviare");
                }
                else
                {
                    Byte[] byteInviati = Encoding.ASCII.GetBytes(txtMessaggioDaInviare.Text); //Conversione del messaggio (scritto nella Textbox) in byte per poter essere inviato
                    s.SendTo(byteInviati, remoteEndPoint); //Invio del messaggio al destinatario
                }
            }
            catch (FormatException ex) //Controllo errori nell'invio del messaggio
            {
                MessageBox.Show("Si è verificato un problema nel formato: " + ex.Message, "ERRORE", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Si è verificato un problema: " + ex.Message, "ERRORE", MessageBoxButton.OK);
            }
            
        }
    }
}
