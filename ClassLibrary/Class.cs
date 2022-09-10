using System.Collections.ObjectModel;
using System.Runtime.InteropServices;


namespace ClassLibrary
{
    public enum VMf
    {
        vmdTan,
        vmdErfInv
    }

    [Serializable] public class VMGrid 
    { 
        public int N { get; set; }
        public double[] Ends { get; set; }
        public VMf Func { get; set; }
        public VMGrid(int n = 2, double first = 0, double last = 0, VMf vmf = VMf.vmdTan)
        {
            N = n;
            Ends = new double[2];
            Ends[0] = first;
            Ends[1] = last;
            Func = vmf;
        }

        public double Step
        {
            get { return (Ends[1] - Ends[0]) / (N - 1); }
        }
    }

    [Serializable] public struct VMTime
    {
        public VMGrid Grid { get; set; }
        public int HA_time { get; set; }
        public int EP_time { get; set; }
        public int C_time { get; set; }
        public VMTime(VMGrid grid, int ha, int ep, int c)
        {
            Grid = grid;
            HA_time = ha;
            EP_time = ep;
            C_time = c;
        }

        public double HA_over_C
        {
            get
            {
                if (C_time == 0)
                    return 0;
                else
                    return (double)HA_time / C_time;
            }
        }

        public double EP_over_C
        {
            get
            {
                if (C_time == 0)
                    return 0;
                else
                    return (double)EP_time / C_time;
            }
        }

        public override string ToString()
        {
            return $"N = {Grid.N}, Min = {Grid.Ends[0]}, Max = {Grid.Ends[1]}, " +
                $"Step = {Grid.Step}, func = {Grid.Func}, Time_HA = {HA_time}, " +
                $"Time_EP = {EP_time}, Time_base = {C_time}, Time_HA / Time_base = {HA_over_C}, " +
                $"Time_EP / Time_base = {EP_over_C}";
        }
    }

    [Serializable] public struct VMAccuracy
    {
        public VMGrid Grid { get; set; }
        public double Max_diff_HA { get; set; }
        public double Max_diff_EP { get; set; }
        public double Max_diff_arg { get; set; }
        public VMAccuracy(VMGrid grid, double diffha, double diffep, double diffarg)
        {
            Grid = grid;
            Max_diff_HA = diffha;
            Max_diff_EP = diffep;
            Max_diff_arg = diffarg;
        }

        public double Max_diff
        {
            get
            {
                return Math.Abs(Max_diff_HA - Max_diff_EP);
            }
        }

        public override string ToString()
        {
            return $"N = {Grid.N}, Min = {Grid.Ends[0]}, Max = {Grid.Ends[1]}, " +
                $"Step = {Grid.Step}, func = {Grid.Func}, Max_diff = {Max_diff}, " +
                $"Max_diff_arg = {Max_diff_arg}, Value VML_HA = {Max_diff_HA}, Value VML_EP = {Max_diff_EP}";
        }
    }

    [Serializable] public class VMBenchmark
    {
        [DllImport(@"C:\Users\imsuv\source\repos\Lab1_V2\x64\Debug\Dll.dll")]
        static extern void vmd(int n, double min, double max, int vmf_num, int[] time, double[] acc);
        public ObservableCollection<VMTime> VMTimes { get; set; }
        public ObservableCollection<VMAccuracy> VMAccuracies { get; set; }
        public VMBenchmark()
        {
            VMTimes = new ObservableCollection<VMTime>();
            VMAccuracies = new ObservableCollection<VMAccuracy>();
        }

        public void AddVMTime(VMGrid grid)
        {
            VMGrid copyGrid = new(grid.N, grid.Ends[0], grid.Ends[1], grid.Func);
            int[] timeData = new int[3];
            double[] accuracyData = new double[3];

            vmd(copyGrid.N, copyGrid.Ends[0], copyGrid.Ends[1], (int)copyGrid.Func, timeData, accuracyData);
            var newVMTime = new VMTime(copyGrid, timeData[0], timeData[1], timeData[2]);
            VMTimes.Add(newVMTime);
        }

        public void AddVMAccuracy(VMGrid grid)
        {
            VMGrid copyGrid = new(grid.N, grid.Ends[0], grid.Ends[1], grid.Func);
            int[] timeData = new int[3];
            double[] accuracyData = new double[3];

            vmd(copyGrid.N, copyGrid.Ends[0], copyGrid.Ends[1], (int)copyGrid.Func, timeData, accuracyData);
            var newVMAccuracy = new VMAccuracy(copyGrid, accuracyData[0], accuracyData[1], accuracyData[2]);
            VMAccuracies.Add(newVMAccuracy);
        }
        
        public double Min_HA_over_C
        {
            get
            {
                var numDataItem = from i in VMTimes select i;
                if (numDataItem != null && numDataItem.Any())
                    return (numDataItem.Aggregate((i1, i2) => i1.HA_over_C < i2.HA_over_C ? i1 : i2)).HA_over_C;
                else
                    return -1;
            }
        }

        public double Max_HA_over_C
        {
            get
            {
                var numDataItem = from i in VMTimes select i;
                if (numDataItem != null && numDataItem.Any())
                    return (numDataItem.Aggregate((i1, i2) => i1.EP_over_C > i2.EP_over_C ? i1 : i2)).HA_over_C;
                else
                    return -1;
            }
        }
    }
}