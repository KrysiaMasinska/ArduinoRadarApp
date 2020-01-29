﻿using MySql.Data.MySqlClient;
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
        private int _angle;
        private int _distance;
        private int _id;
        private int _count;
        private int _i;

        #endregion

        #region Public variable
        public int Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }
        public int Distance
        {
            get { return _distance; }
            set { _distance = value; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public int Count
        {
            get { return _count= int.Parse(ConfigurationManager.AppSettings["Count"]); }
            set { _count = value; }
        }

        public int MyInit
        {
            get { return _i; }
            set { _i = value; }
        }

        public int Second
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["Second"]);
            }
        }

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

                if (_dispatcherTimer.IsEnabled == true)
                {
                    _dispatcherTimer.IsEnabled = false;
                    ReciveButton.IsEnabled = true;
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
            StopSendButton.IsEnabled = false;
            ReciveButton.IsEnabled = true;
            _dispatcherTimer.Stop();
        }

        /// <summary>
        /// Start Recive data - enable timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReciveSend_Click(object sender, RoutedEventArgs e)
        {
            StopSendButton.IsEnabled = true;
            ReciveButton.IsEnabled = false;
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(Second);//timer = 1 second
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.IsEnabled = true;
            _dispatcherTimer.Start();
            //RecivAndSend();
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
        /// <summary>
        /// 
        /// </summary>
        private void RecivAndSend()
        {
            try
            {
                if (_serialPort.IsOpen)//for test
                {

                    //string text = "76 34.32 34.56 78.";//for test
                    char space = ' ';
                    char dot = '.';
                    TextBox.Text = _serialPort.ReadExisting();
                    string[] wiersze = TextBox.Text.Split(dot);
                    //string[] wiersze = text.Split(dot);
                    _i = 1;
                    foreach (var wiersz in wiersze)
                    {
                        if (_i < Count)
                        {
                            string[] ad = wiersz.Split(space);
                            if (ad.Length == 2)
                            {
                                if (ad[0] != "" && ad[1] != "")
                                {
                                    Angle = Convert.ToInt32(ad[0]);
                                    Distance = Convert.ToInt32(ad[1]);

                                    QueryExectuded(Angle, Distance, _i);
                                    Angle = 0;
                                    Distance = 0;
                                }
                            }
                            _i++;
                        }
                        else
                        {
                            _i = 0;
                        }

                    }
                    TextBox.Clear();
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
        private void QueryExectuded(int angle, int distance, int id)
        {
            try
            {
                _cmd = _mySqlConnection.CreateCommand();
                // _cmd.CommandText = "insert into arduino(Angle,Distance) values(@Angle,@Distance)";
                _cmd.CommandText = "UPDATE arduino SET Angle=@Angle,Distance=@Distance WHERE id =@Id";
                _cmd.Parameters.AddWithValue("@Angle", Angle);
                _cmd.Parameters.AddWithValue("@Distance", Distance);
                _cmd.Parameters.AddWithValue("@Id", MyInit);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            if (_mySqlConnection.State == ConnectionState.Open)
            {
                _mySqlConnection.Close();
            }
        }
    }
}
