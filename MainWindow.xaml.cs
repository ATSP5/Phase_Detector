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
using LiveCharts;
using LiveCharts.Wpf;

namespace PhaseDetector
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Oscylator 1",
                    Values = new ChartValues<double> { 0}
                },
                new LineSeries
                {
                    Title = "Oscylator 2",
                    Values = new ChartValues<double> { 0 },
                },
                new LineSeries
                {
                    Title = "Wyjście Detektora",
                    Values = new ChartValues<double> { 0 },
                },
                new LineSeries
                {
                    Title = "Wyjście przerzutnika",
                    Values = new ChartValues<double> { 0 },
                }
            };

            DataContext = this;
        }
        
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Program obrazujący działanie przerzutnikowego detektora fazy. \n Autorzy: Adam Prukała, Meggi Głowacki. \n Copyrigth: 2020");
        }

        List<double> Osc1Val = new List<double>();
        List<double> Osc2Val = new List<double>();
        List<double> RSValue = new List<double>();
        List<double> PhDetVal = new List<double>();

        double Osc1Freq = 100;
        double Osc2Freq = 100;
        int NumberOfPoints = 0;
        int Phase1 = 0;
        int Phase2 = 0;
        double dt = 1000;
        List<double> time=new List<double>();
        List<string> times = new List<string>();
        private void ShowGraph_Click(object sender, RoutedEventArgs e)
        {
            //Update mathematical values:
            Osc1Val.Clear();
            Osc2Val.Clear();
            RSValue.Clear();

            SeriesCollection[0].Values.Clear();
            SeriesCollection[1].Values.Clear();
            SeriesCollection[2].Values.Clear();
            SeriesCollection[3].Values.Clear();

            try
            {
                Osc1Freq = int.Parse(Freq1TB.Text);
            }
            catch
            {
                MessageBox.Show("Niewłaściwy format danych. Nie używaj wartości po przecinku! Wartość w oknie została zignorowana! Częstotliwość oscylatora 1 wynosi 100Hz");
                Osc1Freq = 100;
            }
            try
            {
                Osc2Freq = int.Parse(Freq2TB.Text);
            }
            catch
            {
                MessageBox.Show("Niewłaściwy format danych. Nie używaj wartości po przecinku! Wartość w oknie została zignorowana! Częstotliwość oscylatora 2 wynosi 100Hz");
                Osc2Freq = 100;
            }

            UpdateNumberOfPoints();

            try
            {
                Calculate_dt(int.Parse(SampleFrequencyTB.Text));
            }
            catch
            {
                MessageBox.Show("Niewłaściwy format danych. Nie używaj wartości po przecinku! Wartość w oknie została zignorowana! Częstotliwość próbkowania wynosi 10kHz");
                Calculate_dt(10000);
            }
            UpdateTimeList();

            
            ///
            //Calculate oscilators values and plot:
            for (int i=0; i<NumberOfPoints; i++)
            {
                Osc1Val.Add(Math.Sin(2*Math.PI* Osc1Freq*time[i] + Phase1 * Math.PI / 180));
                if(OSC1_CB.IsChecked==true)
                {
                    SeriesCollection[0].Values.Add(Osc1Val[i]);
                }
                
                Osc2Val.Add(Math.Sin(2 * Math.PI * Osc2Freq * time[i] + Phase2*Math.PI/180));

                if (OSC2_CB.IsChecked == true)
                {
                    SeriesCollection[1].Values.Add(Osc2Val[i]);
                }
                    
                ///Calculate RS flip flop:
                if (Osc1Val[i] > 0.0 && Osc2Val[i] < 0.0)
                {
                    RSValue.Add(1.0);
                }
                else
                {
                    RSValue.Add(0.0);
                }
                if(RS_CB.IsChecked==true)
                {
                    SeriesCollection[3].Values.Add(RSValue[i]);
                }

            }
            ////
            //Calculate Phase Detector signal and plot:
            if(PH_Det_CB.IsChecked==true)
            {
                CalculateCounter();
                for (int i = 0; i < NumberOfPoints; i++)
                {
                    SeriesCollection[2].Values.Add(PhDetVal[i]);
                }
            }

        }

        private void Osc1Silider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Phase1 = (int)Osc1Silider.Value;
        }

        private void Osc2Silider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Phase2 = (int)Osc2Silider.Value;
        }
        private void Calculate_dt(int Sampl_Freq)
        {
            dt = (double)1/Sampl_Freq;
        }
        private void UpdateTimeList()
        {
            time.Clear();
            times.Clear();
            time.Add(0.0);
            for(int i=1; i< NumberOfPoints; i++)
            {
                time.Add( time[i - 1] + dt);
                times.Add(time[i].ToString());
            }
        }

        private void UpdateNumberOfPoints()
        {
            int RefCycleNumber=4;
            int SampleFrequency=10000;
            try
            {
                RefCycleNumber = int.Parse(RefCycleNum.Text);
            }
            catch
            {
                RefCycleNumber = 4;
                MessageBox.Show("Niewłaściwy format danych. Nie używaj wartości po przecinku! Wartość w oknie została zignorowana! Ilość widocznych cykli wynosi 4");
            }
            try
            {
                SampleFrequency = int.Parse(SampleFrequencyTB.Text);
            }
            catch
            {
                SampleFrequency = 10000;
            }
            NumberOfPoints = (int) RefCycleNumber * SampleFrequency / (int)Osc1Freq;
        }

        private void CalculateCounter()
        {
            bool reset = false;
            PhDetVal.Clear();
            int addStep = 10;
            try
            {
                addStep = int.Parse(DetCounterStep.Text);
            }
            catch
            {
                addStep = 10;
                MessageBox.Show("Niewłaściwy format danych. Nie używaj wartości po przecinku! Wartość w oknie została zignorowana! Krok licznika detektora wynosi 10");
            }
            double dStep = (double)addStep / NumberOfPoints;
            PhDetVal.Add(0.0);
            for(int i=1; i<NumberOfPoints; i++)
            {
                if (RSValue[i - 1] == 0.0 && RSValue[i] > 0.0)
                {
                    reset = true;
                }
                if (RSValue[i]>0.0)
                {
                    if(reset==true)
                    {
                        reset = false;
                        PhDetVal.Add(0.0);
                    }
                    else
                    {
                        PhDetVal.Add(PhDetVal[i - 1] + dStep);
                    }
                    
                }
                else
                {
                    PhDetVal.Add(PhDetVal[i - 1]);
                }
                
            }
        }
    }
    
}
