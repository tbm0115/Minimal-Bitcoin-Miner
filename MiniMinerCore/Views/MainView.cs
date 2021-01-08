using ConsoulLibrary.Views;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniMinerCore.Views
{
    public class MainView : StaticView
    {
        public MainView() : base () {
            Title = (new ConsoulLibrary.BannerEntry($"MiniMiner Core")).Message;
        }

        [ViewOption("Start Miner")]
        public void StartMiner() {
            var view = new MinerView();
            view.Run();
        }
    }
}
