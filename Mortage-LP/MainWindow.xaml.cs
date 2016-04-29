using Microsoft.SolverFoundation.Services;
using Microsoft.SolverFoundation.Solvers;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

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
            var data = new Invester { Line = 0, Name = "Bank", MaxValue = "100", Interest = "0.15" };
            if (!dataGridVars.Items.Contains(data))
                dataGridVars.Items.Add(data);
            data = new Invester { Line = 1, Name = "Gov", MaxValue = "90", Interest = "0.08" };
            if (!dataGridVars.Items.Contains(data))
                dataGridVars.Items.Add(data);
            ContrainTB.Text = "2L0 - L1 >= 0";

        }
        String TargetFormula;
        List<String> listOfStringContrains = new List<string>();
        List<Double> listOfMinimums = new List<double>();
        List<List<double>> ListOfContrains = new List<List<double>>();
        List<String> ListOfTypeContrains = new List<String>();
        private void Image1_png_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var data = new Invester { Line = dataGridVars.Items.Count , Name = NameTB.Text, MaxValue = MaxTB.Text, Interest = InterestTB.Text };
            if(!dataGridVars.Items.Contains(data))
                dataGridVars.Items.Add(data);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();
            List<Decision> decisions = new List<Decision>();

            var formula = "";
            string con = "";
            foreach (var item in dataGridVars.Items)
            {
                string li = "L" + (item as Invester).Line;
                double maxValue = Double.Parse((item as Invester).MaxValue) * Double.Parse(Value.Text);
                String contrain = li + " <= " + maxValue;
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

            Console.WriteLine(TargetFormula);

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
            // Solve the problem

            Solution solution = context.Solve(new SimplexDirective());
            // Propogate decisions tells the context to write the solution directly to
            // the database
            context.PropagateDecisions();
            // Display the results
            Report report = solution.GetReport();
            ResultTB.Text = report.ToString();
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
            text.Replace("+", " + ");
            text.Replace("-", " - ");
            text.Replace("<=", " <= ");
            text.Replace("<=", " >= ");
            var line = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
            ListOfContrains.Add(contrains);
        }
    }
    public class Invester
    {
        public int Line { get; set; }
        public string Name{ get; set; }
        public string MaxValue{ get; set; }
        public string Interest { get; set; }
    }
}
