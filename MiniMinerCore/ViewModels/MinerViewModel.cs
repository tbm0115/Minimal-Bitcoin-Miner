using MiniMiner.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiniMinerCore.ViewModels
{
    public class MinerViewModel
    {
        public Pool Pool { get; set; }

        public long MaxAgeTicks => 20000 * TimeSpan.TicksPerMillisecond;

        public uint BatchSize => 100000;

        public MinerViewModel() {

        }
    }
}
