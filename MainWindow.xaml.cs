using DocumentFormat.OpenXml.Math;
using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottable;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace SumAppMaster
{
    public partial class MainWindow : Window
    {
        private object txtFilename;

        public List<Podatak> podaci = new List<Podatak>();

        dynamic backLine;

        dynamic frontLine;


        DataTable dt;

        public List<double> time= new List<double>();
        public List<double> value = new List<double>();




        public MainWindow()
        {
            InitializeComponent();
            dt = new DataTable();
            dt.Columns.Add("Vreme (s)");
            dt.Columns.Add("Potrošnja (l/min)");
            dataGridView1.ItemsSource = dt.DefaultView;
            txtTime1.Text = "0";
            txtTime2.Text = "0";
            



        }


        public class Podatak
        {
            public string Vreme { get; set; }
            public string Vrednost { get; set; }
        }
       
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");

            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));

        }


        public void DrawGraph(List<double> t1, List<double> v1)
        {
            double[] t = new double[t1.Count];
            double[] v = new double[v1.Count];

            for (int i = 0; i < t1.Count; i++)
            {
                t[i] = t1[i];
                v[i] = v1[i];

            }

            plogGraph.Plot.AddScatter(t, v);

            frontLine = plogGraph.Plot.AddVerticalLine(t[0]);
            frontLine.LineWidth = 2;
            frontLine.DragEnabled = true;
            frontLine.DragLimitMin = 0;
            frontLine.DragLimitMax = t[t.Length - 1];
            frontLine.PositionLabel = true;
            frontLine.Label = "Početno vreme";


            backLine = plogGraph.Plot.AddVerticalLine(t[t.Length-1]);
            backLine.DragEnabled = true;
            backLine.LineWidth = 2;
            backLine.DragLimitMin = 0;
            backLine.DragLimitMax = t[t.Length - 1];
            backLine.PositionLabel = true;
            backLine.Label = "Krajnje vreme";
            plogGraph.Plot.Legend(true);

            plogGraph.Plot.Title("Grafik potrošnje");
            plogGraph.Plot.XLabel("Vreme(sec)");
            plogGraph.Plot.YLabel("Potrošnja\n (l/min)");


            plogGraph.Refresh();
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {

            if (lblFileName.Content == "")        
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
               
                openFileDialog.ShowDialog();
                if (openFileDialog.FileName != "")
                {
                    lblFileName.Content = "Ruta: " + openFileDialog.FileName;
                    SLDocument doc = new SLDocument(openFileDialog.FileName, "Sheet1");

                    for (int i = 2; ; i++)
                    {
                        if (doc.GetCellValueAsString(i, 1) == "")
                        {
                            break;
                        }
                        podaci.Add(new Podatak()
                        {
                            Vreme = doc.GetCellValueAsString(i, 1),
                            Vrednost = doc.GetCellValueAsString(i, 2),
                        });

                        time.Add(double.Parse(podaci[i - 2].Vreme));
                        value.Add(double.Parse(podaci[i - 2].Vrednost));

                        MaterialDesignThemes.Wpf.ButtonProgressAssist.SetIsIndeterminate(btnUpload, false);

                    }
                    dataGridView1.ItemsSource= podaci;

                    DrawGraph(time, value);
                    
                }
                else
                {
                    lblFileName.Content = "Greska";

                }
            }
            else
            {
                lblFileName.Content = "Fajl je već učitan";
            }
        }

        private void btnPotvrda_Click(object sender, RoutedEventArgs e)
        {

            if (txtTime1.Text == "" || txtTime2.Text == "")
            {
                MessageBox.Show("Popunite oba polja!");
            }

            if (txtTime1.Text != "" && txtTime2.Text != "")
            {

                double Start = double.Parse(txtTime1.Text);
                double End = double.Parse(txtTime2.Text);

                if (Start > End)
                {
                    MessageBox.Show("Početna vrednost mora biti manja od krajnje!");

                }


                double q = 0;
                for (int i = 1; i < podaci.Count; i++)
                {
                    if (Start <= double.Parse(podaci[i].Vreme) && End >= double.Parse(podaci[i].Vreme))
                    {
                        double t = double.Parse(podaci[i].Vreme) - double.Parse(podaci[i - 1].Vreme);
                        double v = double.Parse(podaci[i].Vrednost) + double.Parse(podaci[i - 1].Vrednost);
                        q += (t * v) / 2;
                        q = Math.Round(q, 2);

                    }
                }
                lblResault.Content = q.ToString();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            dataGridView1.DataContext="";
            
            podaci.Clear();
            time.Clear();
            value.Clear();
            dataGridView1.ItemsSource = "";
            MaterialDesignThemes.Wpf.ButtonProgressAssist.SetIsIndeterminate(btnUpload, true);

            plogGraph.Plot.Clear();
            plogGraph.DataContext = null;
            plogGraph.Refresh();

            lblFileName.Content = "";
            lblResault.Content = "";
            txtTime1.Text = "";
            txtTime2.Text = "";
        }

        private void txtTime1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");

            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));

          
        }

        private void txtTime2_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("^[.][0-9]+$|^[0-9]*[.]{0,1}[0-9]*$");

            e.Handled = !regex.IsMatch((sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, e.Text));

           
        }


        private void dataGridView1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Podatak selected = (Podatak)dataGridView1.SelectedItem;


            if (double.Parse(txtTime1.Text) ==0)
            {
                txtTime1.Text = selected.Vreme;
                
            } else if (double.Parse(txtTime1.Text)< double.Parse(selected.Vreme)) 
            {
                txtTime2.Text = selected.Vreme;
            }
            else if (double.Parse(txtTime1.Text) > double.Parse(selected.Vreme))
            {
                txtTime1.Text = selected.Vreme;
               

            }
            UpdateGraph();


        }


        private void plogGraph_PlottableDragged(object sender, EventArgs e)
        {
           

            double tb = backLine.X;

            double tf = frontLine.X;

            if (tb < tf)
            {
                frontLine.X = tf-0.01;
                backLine.X=tf;

                plogGraph.Refresh();
            }


            txtTime1.Text = frontLine.X.ToString("0.###");
            txtTime2.Text =  backLine.X.ToString("0.###");


        }

        private void txtTime1_KeyUp(object sender, KeyEventArgs e)
        {
            if (time.Count>0)
            {
                if (!string.IsNullOrEmpty(txtTime1.Text) && double.Parse(txtTime1.Text) < backLine.X)
                {
                    frontLine.X = double.Parse(txtTime1.Text);
                    plogGraph.Refresh();
                }
            }
          
        }

        public void UpdateGraph()
        {
            if (!string.IsNullOrEmpty(txtTime1.Text) && double.Parse(txtTime1.Text) < backLine.X)
            {
                frontLine.X = double.Parse(txtTime1.Text);
                
            }
            if (!string.IsNullOrEmpty(txtTime2.Text) && frontLine.X < double.Parse(txtTime2.Text))
            {
                backLine.X = double.Parse(txtTime2.Text);
               
            }
            plogGraph.Refresh();
        }

        private void txtTime2_KeyUp(object sender, KeyEventArgs e)
        {
            if (time.Count > 0)
            {
                if (!string.IsNullOrEmpty(txtTime2.Text) && frontLine.X < double.Parse(txtTime2.Text))
                {
                    backLine.X = double.Parse(txtTime2.Text);
                    plogGraph.Refresh();
                }
            }
        }
    }
}
