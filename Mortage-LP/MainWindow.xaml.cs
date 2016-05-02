using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Solvers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace Mortage_LP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SolverContext context;
        public MainWindow()
        {
            InitializeComponent();
            context = SolverContext.GetContext();
        }
        String TargetFormula;
        List<String> listOfStringContrains = new List<string>();
        List<List<double>> ListOfContrains = new List<List<double>>();
        private void Image1_png_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var data = new Invester { Line = dataGridVars.Items.Count , Name = NameTB.Text, MaxValue = MaxTB.Text, Interest = InterestTB.Text };
            if(!dataGridVars.Items.Contains(data))
                dataGridVars.Items.Add(data);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            context.ClearModel();
            Model model = context.CreateModel();

            List<Decision> decisions = new List<Decision>();

            var formula = "";
            string con = "";
            foreach (var item in dataGridVars.Items)
            {
                string li = "L" + (item as Invester).Line;
                double maxValue = Double.Parse((item as Invester).MaxValue) * Double.Parse(Value.Text) /100f;
                String contrain = li + " <= " + maxValue;
                ParseContrain(contrain);
                con += " " + li + " +"; 
                Decision dc = new Decision(Domain.RealRange(0,maxValue), li);
                decisions.Add(dc);
                model.AddDecision(dc);
            }
            ParseContrain(con.Substring(0, con.Length - 1) + " >= " + Value.Text);
            foreach (var item in dataGridVars.Items)
            {
                formula += " " + Double.Parse((item as Invester).Interest) + " * L" + (item as Invester).Line;
                if ((item as Invester).Line <= dataGridVars.Items.Count - 1)
                    formula += " +";
                String contrain = "L" + (item as Invester).Line + " >= 0 ";
                ParseContrain(contrain);
            }
            TargetFormula = formula.Substring(1, formula.Length - 2);

            FormulaTB.Text = TargetFormula;

            foreach (var list in ListOfContrains)
            {
                string c = "";
                for (int i = 0; i < list.Capacity; i++)
                {
                    if (i != 0 && i < list.Capacity - 1)
                        c += list[i] >= 0 ? " + " : " - ";
                    if (i == list.Capacity - 1)
                        c += " " + listOfStringContrains[ListOfContrains.IndexOf(list)] + " " + list[i];
                    else
                        c += Math.Abs(list[i]) + " * L" + i;
                   
                }
                model.AddConstraint("C" + ListOfContrains.IndexOf(list), c);

                Console.WriteLine(c);
            }
            model.AddGoal("Goal", GoalKind.Minimize, TargetFormula);
            var directive = new SimplexDirective()
            {
                IterationLimit = -1,
                TimeLimit = -1,
                Arithmetic = Arithmetic.Exact,
                GetSensitivity = true
            };
            Solution solution = context.Solve(directive);
            Quality.Text = solution.Quality.ToString();
            Console.WriteLine(solution.GetReport().ToString());
            if (solution.Quality != SolverQuality.Optimal)
                return;

            context.PropagateDecisions();
            
            for (int i = 0; i < decisions.Count; i++)
            {
                var old = (dataGridVars.Items[i] as Invester);
                old.SelectedValue = decisions[i].ToDouble().ToString();
                dataGridVars.Items[i] = old;
            }
            dataGridVars.Items.Refresh();
            dataGridVars.UpdateLayout();
        }

        private void AddCntBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ParseContrain(ContrainTB.Text);
        }
        private void ParseContrain(String contrain)
        {
            int resVar = dataGridVars.Items.Count + 1;

            List<double> contrains = new List<double>(resVar);
            for (int i = 0; i < resVar; i++)
            {
                contrains.Add(0);
            }
            resVar--;
            var text = contrain;
            text = text.Replace("+", " + ");
            text = text.Replace("-", " - ");
            text = text.Replace("<=", " <= ");
            text = text.Replace(">=", " >= ");
            var line = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                for (int i = 0; i < line.Length; i++)
                {
                    double mult = 1;
                    var pos = -1;
                    int index = -1;
                    switch (line[i])
                    {
                        case "-":
                            pos = line[i + 1].IndexOf("L", StringComparison.CurrentCultureIgnoreCase);
                            index = int.Parse(line[i + 1].Substring(pos + 1));
                            if (pos != 0)
                                mult *= double.Parse(line[i + 1].Substring(0, pos));
                            contrains[index] = -mult;
                            i++;
                            break;
                        case "+":
                            pos = line[i + 1].IndexOf("L", StringComparison.CurrentCultureIgnoreCase);
                            index = int.Parse(line[i + 1].Substring(pos + 1));
                            if (pos != 0)
                                mult *= double.Parse(line[i + 1].Substring(0, pos));
                            contrains[index] = mult;
                            i++;
                            break;
                        case "<=":
                            contrains[resVar] = double.Parse(line[i + 1]);
                            listOfStringContrains.Add("<=");
                            i++;
                            break;
                        case ">=":
                            contrains[resVar] = double.Parse(line[i + 1]);
                            listOfStringContrains.Add(">=");
                            i++;
                            break;
                        default:
                            pos = line[i].IndexOf("L", StringComparison.CurrentCultureIgnoreCase);
                            index = int.Parse(line[i].Substring(pos + 1));
                            if (pos != 0)
                                mult *= double.Parse(line[i].Substring(0, pos));
                            contrains[index] = mult;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                ContrainTB.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }
            ContrainTB.BorderBrush = System.Windows.Media.Brushes.Gray;
            text = "";
            foreach (var item in line)
            {
                text += item;
                if (item != line[line.Length - 1])
                    text += " ";
                else
                    text += "\n";
            }

            ContrainsTB.Text += text;
            ListOfContrains.Add(contrains);
        }

        private void buttonCL_Click(object sender, RoutedEventArgs e)
        {
            ContrainsTB.Clear();
            TargetFormula = "";
            FormulaTB.Text = "";
            dataGridVars.Items.Clear();
            ListOfContrains.Clear();

        }
        private void Test1()
        {
            buttonCL_Click(null, null);
            dataGridVars.Items.Add( new Invester { Line = 0, Name = "Bank", MaxValue = "100", Interest = "0.15", SelectedValue = "" });
            dataGridVars.Items.Add( new Invester { Line = 1, Name = "Gov", MaxValue = "90", Interest = "0.08", SelectedValue = "" });
            ParseContrain( "2L0 - L1 >= 0");
        }

        private void Test2()
        {
            buttonCL_Click(null, null);
            dataGridVars.Items.Add(new Invester { Line = 0, Name = "BankA", MaxValue = "50", Interest = "0.15", SelectedValue = "" });
            dataGridVars.Items.Add(new Invester { Line = 1, Name = "BankB", MaxValue = "40", Interest = "0.80", SelectedValue = "" });
            dataGridVars.Items.Add(new Invester { Line = 2, Name = "BankC", MaxValue = "33", Interest = "0.50", SelectedValue = "" });
            dataGridVars.Items.Add(new Invester { Line = 3, Name = "BankD", MaxValue = "20", Interest = "0.01", SelectedValue = "" });
            dataGridVars.Items.Add(new Invester { Line = 4, Name = "BankE", MaxValue = "100", Interest = "0.9", SelectedValue = "" });

            ParseContrain("L0 - L1 >= 0");
            ParseContrain("L0 + L1 - 3L2 >= 0");
            ParseContrain("L1 + 2L2 - L3 >= 0");
            ParseContrain("L2 - L3 >= 0");
        }
        private void Test3()
        {
            buttonCL_Click(null, null);
            dataGridVars.Items.Add(new Invester { Line = 0, Name = "Bank", MaxValue = "100", Interest = "0.15", SelectedValue = "" });
            dataGridVars.Items.Add(new Invester { Line = 1, Name = "Gov", MaxValue = "90", Interest = "0.08", SelectedValue = "" });
            dataGridVars.Items.Add(new Invester { Line = 2, Name = "Gov2", MaxValue = "50", Interest = "0.07", SelectedValue = "" });
            ParseContrain("L0  >= 1000000");
            ParseContrain("L1  >= 1000000");
            ParseContrain("L2  >= 1000000");

        }

        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            var btn = (e.Source as System.Windows.Controls.Button);
            switch (btn.Name)
            {
                case "buttonTest1":
                    Test1();
                    break;
                case "buttonTest2":
                    Test2();
                    break;
                case "buttonTest3":
                    Test3();
                    break;

                default:
                    break;
            }
        }

        private void SeeCnts_Click(object sender, RoutedEventArgs e)
        {
            String text ="";
            for (int itemIndex = 0; itemIndex < ListOfContrains.Count; itemIndex++)
            {
                var list = ListOfContrains[itemIndex];
                string c = "";
                for (int i = 0; i < list.Capacity; i++)
                {
                    if (i != 0 && i < list.Capacity - 1)
                        c += list[i] >= 0 ? " + " : " - ";
                    if (i == list.Capacity - 1)
                        c += " " + listOfStringContrains[ListOfContrains.IndexOf(list)] + " " + list[i];
                    else
                        c += Math.Abs(list[i]) + " * L" + i;
                }
                text += "Constrain " + itemIndex + ":\t" + c + "\n";
            }
            MessageBox.Show(text, "Constrains");
        }
    }
    public class Invester
    {
        public int Line { get; set; }
        public string Name{ get; set; }
        public string MaxValue{ get; set; }
        public string Interest { get; set; }
        public string SelectedValue { get; set; }
    }
}
