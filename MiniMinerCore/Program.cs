using System;
using System.Collections.Generic;
using System.Linq;
using MiniMiner.Model;
using MiniMinerCore.Views;

namespace MiniMiner
{
    class Program
    {
        static void Main(string[] args) {
            // Enable Monitoring for Routines
            ConsoulLibrary.Routines.MonitorInputs = true;
            // Process requested Routines
            ConsoulLibrary.Routines.InitializeRoutine(args);

            var mainView = new MainView();
            mainView.Run();
        }
    }
}
