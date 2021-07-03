﻿using System;
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
using WPF_GalacticRunSolver.Model;
using WPF_GalacticRunSolver.ViewModel;
using CLI;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using WPF_GalacticRunSolver.Utility;
using System.ComponentModel;
using System.Diagnostics;

namespace WPF_GalacticRunSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class MyWPFLogger : IManagedLogger
    {
        public MyWPFLogger(TextBox log)
        {
            Debug.WriteLine("The WPF Logger was created.");
            _log = log;
        }

        private TextBox _log;

        private void LogOnUiThread(string input)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input,
                    new Action(() => {
                        _log.Text += Environment.NewLine + input; 
                    }));
        }

        private void LogOnCurrentThread(string input)
        {
            _log.Text += Environment.NewLine + input;
        }

        public void Log(string input)
        {
            LogOnUiThread(string.Copy(input));
        }

        ~MyWPFLogger()
        {
            Debug.WriteLine("The WPF Logger was deleted.");
        }
    }




    public partial class MainWindow : Window
    {
        public MainWindow(MapViewModel mapvm)
        {
            InitializeComponent();

            _Map = mapvm;

            this.Map.DataContext = _Map;

            m_Logger = new MyWPFLogger(DisplayTextBox);
        }

        MyWPFLogger m_Logger;

        public MapViewModel _Map = new MapViewModel(16);

        ManagedMap _managedMap;

        ManagedSolver _managedSolver;

        private void Load_Map(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text File | *.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                ClearSolverDisplay();
                _Map = new MapViewModel(openFileDialog.FileName);
                this.Map.DataContext = _Map;
            }
        }

        private void Load_Map_From_URL(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text;
            if (Utilities.IsValidMapUrl(url))
            {
                _Map = new MapViewModel(Utilities.GetMapFromWeburl(url));
                this.Map.DataContext = _Map;
            }  
        }

        private void Save_Map(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File | *.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                _Map.SaveMap(saveFileDialog.FileName);
                MessageBox.Show("Map was saved", "Map was saved...", MessageBoxButton.OK);
            }
        }

        private void Reset_Map(object sender, RoutedEventArgs e)
        {
            _Map.Reset();
        }

        private void Clear_Map(object sender, RoutedEventArgs e)
        {
            _Map = new MapViewModel(16);
            this.Map.DataContext = _Map;
        }

        private void OnMouseClickMapUrl(object sender, RoutedEventArgs e)
        {
            UrlTextBox.SelectAll();
        }

        private void OnKeyDownMapUrl(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Load_Map_From_URL(sender, e);
            }
        }


        private void ClearSolverDisplay()
        {
            DisplayTextBox.Text = "";
            SolutionTextBox.Text = "";
            this.Solutions.DataContext = new SolutionsViewModel(new List<ManagedSolution>(), _Map);
        }

        private List<ManagedSolution> SolveMap()
        {
            _managedSolver = new ManagedSolver(30, m_Logger);
            //Save map to temp file that will be deleted
            string tempFileName = System.IO.Path.GetTempFileName();
            _Map.SaveMap(tempFileName);
            _managedMap = new ManagedMap(tempFileName);
            var solutions = _managedSolver.GetAllSolutions(_managedMap);
            File.Delete(tempFileName);
            return solutions;
        }

        private void DisplaySolutions(List<ManagedSolution> solutions)
        {
            if (solutions.Count > 0)
            {
                var solution = solutions.First();
                SolutionTextBox.Text = solutions.Count.ToString() + " solution" + (solutions.Count == 1 ? "" : "s");
                SolutionTextBox.Text += " in " + solution.Moves.Count.ToString() + " moves exist.";
                this.Solutions.DataContext = new SolutionsViewModel(solutions, _Map);
            }
            else
            {
                SolutionTextBox.Text = "No solutions where found in under 8 moves...";
            }
        }

        private void Solve_Map(object sender, RoutedEventArgs e)
        {
            _Map._InitialMap = new Map(_Map._Map);
            ClearSolverDisplay();

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var solutions = SolveMap();
            e.Result = solutions;
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DisplaySolutions((List<ManagedSolution>)e.Result);
        }
    }
}