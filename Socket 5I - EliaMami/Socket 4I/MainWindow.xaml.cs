using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace Socket_4I
{
    public partial class MainWindow : Window
    {
        Task messageControll = null;
        Socket socket = null;
        List<Contact> contacts = null;
        int recvPort = 10000;

        public MainWindow()
        {
            InitializeComponent();

            //inizializzo la classe socket e la mia lista contacts
            contacts = new List<Contact>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //creo il local_endpoint
            IPAddress local_address = IPAddress.Any;
            IPEndPoint local_endpoint = new IPEndPoint(local_address, recvPort);

            socket.Bind(local_endpoint);

            socket.Blocking = false;
            socket.EnableBroadcast = true;

            //Creo la task con il metodo updating_message
            messageControll = new Task(updating_message);
            //La inizio
            messageControll.Start();
        }

        private void updating_message()
        {
            int nBytes = 0;

            //Continua a verificare continuamente se arrivano messaggi
            while(true)
            {
                if((nBytes = socket.Available) > 0)
                {
                    //ricezione dei caratteri in attesa
                    byte[] buffer = new byte[nBytes];

                    EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    //riceve i dati
                    nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);

                    //prendo l'indirizzo del mittente
                    string from = ((IPEndPoint)remoteEndPoint).Address.ToString();

                    //decodo da bytes a string di nuovo
                    string received = Encoding.UTF8.GetString(buffer, 0, nBytes);

                    //riprendo le informazioni dal messaggio per formattarle come preferisco
                    string name = received.Split('~')[0];
                    string port = received.Split('~')[1];
                    string message = received.Split('~')[2];

                    //aggiungo il messaggio alla lstMessage, mi serve un invoke in quanto la lstMessage è gestita da un altro 
                    //thread, perciò non posso andarci a lavorare direttamente con questo

                    lstMessages.Dispatcher.Invoke(() =>
                    {
                        lstMessages.Items.Add(name + "-" + port + ": " + message);
                    });
                }
            }
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //prendo l'indirizzo ip
                IPAddress remote_address = IPAddress.Parse(txtTo.Text);

                //formatto il messaggio con le informazioni necessarie
                string message = txtName.Text + "~" + recvPort + "~" + txtMessaggio.Text;

                //creo l'endpoint con ip e porta
                IPEndPoint remote_endpoint = new IPEndPoint(remote_address, int.Parse(txtPort.Text));

                //per spedirla mi serve una bytestring, perciò converto il messaggio
                byte[] messageToSend = Encoding.UTF8.GetBytes(message);

                //mando con il SendTo il messaggio all'endpoint
                socket.SendTo(messageToSend, remote_endpoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //aggiungo alla rubrica il contatto
                Contact c = new Contact(txtName.Text, int.Parse(txtPort.Text), IPAddress.Parse(txtTo.Text));
                contacts.Add(c);
                cbxRubrica.Items.Add(c);
                cbxRubrica.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Per cambiare il porta che sto ascoltando prendo la porta del txt
                recvPort = int.Parse(txtRecvPort.Text);

                //Ricreo la socket
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress local_address = IPAddress.Any;
                IPEndPoint local_endpoint = new IPEndPoint(local_address, recvPort);

                Console.WriteLine(recvPort);

                socket.Bind(local_endpoint);

                socket.Blocking = false;
                socket.EnableBroadcast = true;

                //Non mi è molto chiaro però come dovrei chiuderle, in quanto quando provo a mettere 
                //una socket che avevo già inserito in precedenza mi dice che 
                //di norma è consentito l'utilizzo di una sola socket, ho provato con il close, ma mi dava altri problemi
                //per "sviare" la cosa ho messo tutto in un try catch per evitare crash del programma
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Grafica
        private void cbxRubrica_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Quando cambio elemento, mi cambia anche le scritte nelle txt            
            Contact c = cbxRubrica.SelectedItem as Contact;
            txtName.Text = c.Name;
            txtPort.Text = c.Port.ToString();
            txtTo.Text = c.Address.ToString();
        }

        //Appena il txtBox prende il focus del mouse si cancella
        private void txtTo_GotFocus(object sender, RoutedEventArgs e)
        {
            txtTo.Text = "";
        }

        private void txtPort_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPort.Text = "";
        }

        private void txtName_GotFocus(object sender, RoutedEventArgs e)
        {
            txtName.Text = "";
        }
        private void txtRecvPort_GotFocus(object sender, RoutedEventArgs e)
        {
            txtRecvPort.Text = "";
        }

        //Appena per il focus, se il txt è vuoto si riempie con le informazioni di default
        private void txtRecvPort_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(txtRecvPort.Text))
                txtRecvPort.Text = "11000";
        }

        private void txtName_LostFocus(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(txtName.Text))
                txtName.Text = "Default";
        }

        private void txtTo_LostFocus(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(txtTo.Text))
                txtTo.Text = "127.0.0.1";
        }

        private void txtPort_LostFocus(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(txtPort.Text))
                txtPort.Text = "10000";
        }
    }
}
