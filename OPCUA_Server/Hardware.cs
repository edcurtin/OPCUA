using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Timers;

namespace OPCUA_Server
{
    internal class Hardware
    {
        public delegate void EventHandler(object sender, EventArgs e);
        public event EventHandler MyEvent;
        public Hardware()
        {
            // Create a new timer
            System.Timers.Timer myTimer = new System.Timers.Timer();

            // Set the interval to 5 seconds (5000 milliseconds)
            myTimer.Interval = 5000;

            // Enable the timer
            myTimer.Enabled = true;

            // Add an event handler for the Elapsed event
            myTimer.Elapsed += OnTimedEvent;
        }

        private int count = 0;
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if(count % 2 == 0)
            {
                IsBusy = true;
            }
            else
            {
                IsBusy = false;
            }
            count++;
            MyEvent?.Invoke(this, e);
        }

        public bool IsBusy { get; set; }
    }
}
