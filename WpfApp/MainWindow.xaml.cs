using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class ViewData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public VMGrid Vmgrid { get; set; }
        public VMBenchmark Vmbenchmark { get; set; }
        private bool DataChanged = false;
        
        public ViewData()
        {
            Vmgrid = new VMGrid();
            Vmbenchmark = new VMBenchmark(); 
        }

        public ViewData(VMBenchmark vmb)
        {
            Vmgrid = new VMGrid();
            Vmbenchmark = vmb;
            Vmbenchmark.VMTimes.CollectionChanged += TimeCollectionChanged;
            Vmbenchmark.VMAccuracies.CollectionChanged += AccuracyCollectionChanged;
        }

        public void AddVMTime(VMGrid grid)
        {
            try
            { 
                Vmbenchmark.AddVMTime(grid);
                OnPropertyChanged("Max_HA_over_C");
                OnPropertyChanged("Min_HA_over_C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occured:\n" + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void AddVMAccuracy(VMGrid grid)
        {
            try 
            { Vmbenchmark.AddVMAccuracy(grid); }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occured:\n" + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Save(string filename)
        {
            BinaryFormatter Formatter = new();
            FileStream? fs = null;
            try
            {
                using (fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    Formatter.Serialize(fs, Vmbenchmark);
                    //Console.WriteLine("\nSerialization Complete");
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Exception occured:\n" + ex.Message); }
            finally
            { if (fs != null) fs.Close(); }
        }

        public void Load(string filename)
        {
            BinaryFormatter Formatter = new();
            FileStream? fs = null;
            try
            {
                using (fs = new FileStream(filename, FileMode.Open))
                {
                    VMBenchmark LoadedBenchmark = (VMBenchmark)Formatter.Deserialize(fs);
                    for (int i = 0; i < LoadedBenchmark.VMTimes.Count; i++)
                        Vmbenchmark.VMTimes.Add(LoadedBenchmark.VMTimes[i]);
                    for (int i = 0; i < LoadedBenchmark.VMAccuracies.Count; i++)
                        Vmbenchmark.VMAccuracies.Add(LoadedBenchmark.VMAccuracies[i]);
                    //Console.WriteLine("\nDeserialization Complete");
                    OnPropertyChanged("Max_HA_over_C");
                    OnPropertyChanged("Min_HA_over_C");
                }
            }
            catch (Exception ex)
            { MessageBox.Show("Exception occured:\n" + ex.Message); }
            finally
            { if (fs != null) fs.Close(); }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        void TimeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Vmbenchmark.VMTimes");
        }
        void AccuracyCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Vmbenchmark.VMAccuracies");
        }

        public bool IsChanged
        {
            get 
            { 
                return DataChanged; 
            }
            set 
            { 
                DataChanged = value;
                OnPropertyChanged("DataChanged");
            }
        }

        public double Min_HA_over_C
        {
            get
            {
                return Vmbenchmark.Min_HA_over_C;
            }
        }
        
        public double Max_HA_over_C
        {
            get
            {
                return Vmbenchmark.Max_HA_over_C;
            }
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private object selected;

        public object Selected { get => selected; set => SetProperty(ref selected, value); }
    }

    public partial class MainWindow : Window
    {
        public ViewData? Viewdata;
        Microsoft.Win32.SaveFileDialog Save;
        string Name = "file";
        string Ext = ".txt";
        string SaveMessage = "All unsaved data will be lost.\nDo you want to save?";

        public MainWindow()
        {
            Save = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Name,
                DefaultExt = Ext
            };

            InitializeComponent();
            VMBenchmark benchmark = new();
            Viewdata = new ViewData(benchmark);
            this.DataContext = Viewdata;
            comboBoxIn.ItemsSource = Enum.GetValues(typeof(VMf));
        }

        private void NewClick(object sender, RoutedEventArgs e)
        {
            if (Viewdata.IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(SaveMessage, "Save?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    if (Save.ShowDialog() == true)
                    {
                        if (Viewdata != null)
                            Viewdata.Save(Save.FileName);
                    }
                }
                Viewdata.IsChanged = false;
            }
            Viewdata.Vmbenchmark.VMTimes.Clear();
            Viewdata.Vmbenchmark.VMAccuracies.Clear();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            if (Save.ShowDialog() == true)
            {
                if (Viewdata != null)
                    Viewdata.Save(Save.FileName);
            }
            Viewdata.IsChanged = false;
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            if (Viewdata.IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(SaveMessage, "Save?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    Microsoft.Win32.SaveFileDialog new_save = new()
                    {
                        FileName = Name,
                        DefaultExt = ".txt"
                    };

                    if (new_save.ShowDialog() == true)
                    {
                        if (Viewdata != null)
                            Viewdata.Save(new_save.FileName);
                    }
                }
                Viewdata.IsChanged = false;
            }
            Viewdata.Vmbenchmark.VMTimes.Clear();
            Viewdata.Vmbenchmark.VMAccuracies.Clear();

            Microsoft.Win32.OpenFileDialog open = new Microsoft.Win32.OpenFileDialog();
            open.FileName = Name;
            open.DefaultExt = ".txt";

            if (open.ShowDialog() == true)
                Viewdata.Load(open.FileName);
        }

        private void AddVMTimeClick(object sender, RoutedEventArgs e)
        {
            Viewdata.AddVMTime(Viewdata.Vmgrid);
            Viewdata.IsChanged = true;
        }

        private void AddVMAccuracyClick(object sender, RoutedEventArgs e)
        {
            Viewdata.AddVMAccuracy(Viewdata.Vmgrid);
            Viewdata.IsChanged = true;
        }

        void WpfAppClosing(object sender, CancelEventArgs e)
        {
            if (Viewdata.IsChanged)
            {
                MessageBoxResult result = MessageBox.Show(SaveMessage, "Save?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                if (result == MessageBoxResult.Yes)
                {
                    if (Save.ShowDialog() == true)
                    {
                        if (Viewdata != null)
                            Viewdata.Save(Save.FileName);
                    }
                }
                Viewdata.IsChanged = false;
            }
        }
    }

    public class GridConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string N = value[0].ToString();
                string End0 = value[1].ToString();
                string End1 = value[2].ToString();
                return N + ";" + End0 + ";" + End1;
            }
            catch
            { return "Exception"; }
        }
        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string str = value as string;
                string[] s = str.Split(';', StringSplitOptions.RemoveEmptyEntries);
                double min = double.Parse(s[1]);
                double max = double.Parse(s[2]);
                if (min > max)
                    throw (new Exception());
                return new object[] { Int32.Parse(s[0]), min, max };
            }
            catch
            {
                return new object[3];
            }
        }
    }

    public class DoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double val = (double)value;
                return val.ToString();
            }
            catch
            { return "Exception"; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class BoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value) return "Данные изменены"; else return "Данные не изменены";
            }
            catch
            { return "Exception"; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class MaxConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string str;
                if ((double)value == -1) str = "не определено";
                else str = "= " + value.ToString();
                return "Максимальное значение отношения " + str;
            }
            catch
            { return "Exception"; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class MinConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string str;
                if ((double)value == -1) str = "не определено";
                else str = "= " + value.ToString();
                return "Минимальное значение отношения " + str;
            }
            catch
            { return "Exception"; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}