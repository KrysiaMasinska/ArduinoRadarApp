using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;

namespace ArduionoRadar
{
    /// <summary>
    /// Interaction logic for Radar.xaml
    /// </summary>
    public partial class Radar : Window
    {
        #region Private variable
        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private SerialPort _serialPort;
        private static string _connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        private MySqlConnection _mySqlConnection = new MySqlConnection(_connectionString);
        private MySqlCommand _cmd;
        #endregion

        #region Public variable
        public object Angle { get; private set; } = 0;
        public object Distance { get; private set; } = 0;
        #endregion

        public Radar()
        {
            InitializeComponent();
            GetPorts();
        }
        /// <summary>
        /// Get all avaliable ports and list them into combobox
        /// </summary>
        private void GetPorts()
        {
            ComboPorts.Items.Clear();
            _serialPort = new SerialPort();
            string[] ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                ComboPorts.Items.Add(port);
            }
            
        }
        
        /// <summary>
        /// Close all serial ports
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisConnectedDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DisConnectDBButton.IsEnabled = false;
                ConnectButton.IsEnabled = true;
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            catch (Exception es)
            {
                MessageBox.Show("Error: " + es.Message, "ERROR");
            }
        }

        /// <summary>
        /// Stop send data into database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopSendDB_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
        }

        /// <summary>
        /// Start Recive data - enable timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReciveSend_Click(object sender, RoutedEventArgs e)
        {

            _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);//timer = 1 second
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Start();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            RecivAndSend();
        }

        /// <summary>
        /// Connect to serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            DisConnectDBButton.IsEnabled = true;
            ConnectButton.IsEnabled = false;
            try
            {
                _serialPort.PortName = ComboPorts.Text;
                _serialPort.Open();
            }
            catch (Exception es)
            {
                MessageBox.Show("Error: " + es.Message, "ERROR");
            }
        }

        private void RecivAndSend()
        {
            try
            {
                if (!_serialPort.IsOpen)//for test
                {
                    string text = "76 34.32 34.56 78.";//for test
                    char space = ' ';
                    char dot = '.';
                    Richtextbox.AppendText(text);
                    //Richtextbox.AppendText(_serialPort.ReadExisting());
                    //string[] wiersze = TextBox.Text.Split(dot);
                    string[] wiersze = text.Split(dot);
                    foreach (var wiersz in wiersze)
                    {
                        string[] ad = wiersz.Split(space);
                        if (ad.Length == 2)
                        {
                            if (ad[0] != "" && ad[1] != "")
                            {
                                Angle = Convert.ToInt32(ad[0]);
                                Distance = Convert.ToInt32(ad[1]);

                                QueryExectuded();
                                Angle = 0;
                                Distance = 0;
                            }
                        }
                    }
                    //for (int i = 0; i < wiersze.Length; i++)
                    //{
                    //    string[] ad = wiersze[i].Split(space);
                    //    if (ad.Length == 2)
                    //    {
                    //        if (ad[0] != "" && ad[1] != "")
                    //        {
                    //            Angle = Convert.ToInt32(ad[0]);
                    //            Distance = Convert.ToInt32(ad[1]);

                    //            QueryExectuded();
                    //            Angle = 0;
                    //            Distance = 0;
                    //        }
                    //    }
                    //}
                    Richtextbox.Document.Blocks.Clear();
                }
            }
            catch (Exception es)
            {
                MessageBox.Show("Error: " + es.Message, "ERROR");
            }
        }

        private void GetConnection()
        {
            try
            {
                _mySqlConnection.Open();
                if (_mySqlConnection.State == ConnectionState.Open)
                {

                    ConnectLabel.Foreground = System.Windows.Media.Brushes.Green;
                    ConnectLabel.Content = "Connected to database.";
                }

            }
            catch (MySqlException)
            {
                ConnectLabel.Foreground = System.Windows.Media.Brushes.Red;
                ConnectLabel.Content = "NOT CONNECTED TO DATABASE!!!";
            }
        }
        /// <summary>
        /// Execute command into database
        /// </summary>
        private void QueryExectuded()
        {
            try
            {
                _cmd = _mySqlConnection.CreateCommand();
                _cmd.CommandText = "insert into arduino(Angle,Distance) values(@Angle,@Distance)";
                _cmd.Parameters.AddWithValue("@Angle", Angle);
                _cmd.Parameters.AddWithValue("@Distance", Distance);
                _cmd.ExecuteNonQuery();
            }
            catch (MySqlException es)
            {

                MessageBox.Show("ErrorSQL: " + es.Message + es.Code, "ERORSQL");
            }

        }

        /// <summary>
        /// Refresh open ports in computer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetPorts();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetConnection();
            DisConnectDBButton.IsEnabled = false;
            ConnectButton.IsEnabled = true;
        }
    }
}
