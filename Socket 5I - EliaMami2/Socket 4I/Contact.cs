using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    class Contact
    {
        private string _name;
        private int _port;
        private IPAddress _address;
        
        public string Name
        {
            get { return _name; }
            set { _name = String.IsNullOrEmpty(value) ? throw new Exception("Nome non valido.") : value; }
        }
        public int Port
        {
            get { return _port; }
            set { _port = (value <= 0) ? throw new Exception("Port non valido.") : value; }
        }
        public IPAddress Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public Contact(string n, int p, IPAddress a)
        {
            Name = n;
            Port = p;
            Address = a;
        }

        public override string ToString()
        {
            return Name + " - " + Address + ": " + Port;
        }
    }
}
