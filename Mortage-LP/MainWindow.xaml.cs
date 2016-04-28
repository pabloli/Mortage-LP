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

namespace Mortage_LP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        String TargetFormula;
        List<String> listOfContrains = new List<string>();
        List<Double> listOfMinimums = new List<double>();

        private void Image1_png_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var data = new Invester { Name = NameTB.Text, MaxValue = MaxTB.Text, Interest = InterestTB.Text };
            if(!dataGrid.Items.Contains(data))
                dataGrid.Items.Add(data);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            listOfContrains.Clear();
            listOfMinimums.Clear();
            TargetFormula = "MIN Z="; 
            foreach (var item in dataGrid.Items)
            {
                double min = Double.Parse((item as Invester).MaxValue) * Double.Parse(Value.Text);
                listOfMinimums.Add(min);
                String contrain = (item as Invester).Name + " <= " + min;
                listOfContrains.Add(contrain);
            }
            foreach (var item in dataGrid.Items)
            {
                TargetFormula += Double.Parse((item as Invester).Interest) + "*" + (item as Invester).Name + " + ";
                String contrain = (item as Invester).Name + " >= 0 ";
                listOfContrains.Add(contrain);
            }
            Console.WriteLine(TargetFormula);
            foreach (var item in listOfContrains)
            {
                Console.WriteLine(item);
            }

        }
    }
    public class Invester
    {
        public string Name{ get; set; }
        public string MaxValue{ get; set; }
        public string Interest { get; set; }
    }
}
