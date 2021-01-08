using ConsoulLibrary;
using ConsoulLibrary.Views;
using MiniMiner;
using MiniMiner.Model;
using MiniMinerCore.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniMinerCore.Views
{
    public class MinerView : DynamicView<MinerViewModel>
    {
        private int MaxAttempts = 5;
        private int CurrentAttempt = 0;

        public MinerView() {
            Title = (new ConsoulLibrary.BannerEntry($"Mining - MiniMiner Core")).Message;

            Source = new MinerViewModel();
        }

        private string _selectPoolMessage() => $"Select Pool{(Source.Pool != null ? $" ({Source.Pool})" : "")}";
        private ConsoleColor _selectPoolColor() => Source.Pool != null ? ConsoleColor.Green : ConsoleColor.Yellow;
        [DynamicViewOption(nameof(_selectPoolMessage), nameof(_selectPoolColor))]
        public void SelectPool() {
            string login; // "lithander_2:foo@btcguild.com:8332"
            Regex reg = new Regex("");
            do {
                login = Consoul.Input($"Enter a mining pool login. For example: \r\nuser:password@url:port\r\n");
            } while (string.IsNullOrEmpty(login) || !reg.IsMatch(login));
            Source.Pool = new Pool(login);

            Consoul.Write($"Requesting Work from Pool...");
            Consoul.Write($"\tServer URL: {Source.Pool.Url}\r\n\tUser: {Source.Pool.User}\r\n\tPassword: {Source.Pool.Password}", ConsoleColor.DarkGray);
            Consoul.Wait();
        }

        private string _mineMessage() => $"Start Mining{(CurrentAttempt >= MaxAttempts ? $" (Timed Out)" : (Source.Pool != null ? $" (Ready)" : $" (Not Ready)"))}";
        private ConsoleColor _mineColor() => Source.Pool != null ? ConsoleColor.Yellow : ConsoleColor.Red;
        [DynamicViewOption(nameof(_mineMessage), nameof(_mineColor))]
        public void Mine() {
            Stopwatch sw = new Stopwatch();
            while (CurrentAttempt < MaxAttempts) {
                try {
                    while (true) {
                        Work work = Source.Pool.GetWork();
                        uint nonce = 0;
                        if (work == null || work.Age > Source.MaxAgeTicks) {
                            work = Source.Pool.GetWork();
                            sw.Restart();
                        }

                        if (work.FindShare(ref nonce, Source.BatchSize)) {
                            Console.Clear();
                            Consoul.Write("Found valid share:", ConsoleColor.Green);
                            Consoul.Write($"\tShare: {Utils.ToString(work.Current)}\r\n\tNonce: {Utils.ToString(nonce)}\r\n\tHash: {Utils.ToString(work.Hash)}", ConsoleColor.DarkGray);
                            Consoul.Write("Sending Share to Pool...", ConsoleColor.Yellow);
                            if (Source.Pool.SendShare(work.Current)) {
                                Consoul.Write("\tAccepted!", ConsoleColor.Green);
                            } else {
                                Consoul.Write("\tDeclined!", ConsoleColor.Red);
                            }

                            work = null;
                        } else {
                            Console.Clear();
                            Consoul.Write($"Data: {Utils.ToString(work.Data)}", ConsoleColor.DarkGray);
                            string current = Utils.ToString(nonce);
                            string max = Utils.ToString(uint.MaxValue);
                            double progress = ((double)nonce / uint.MaxValue) * 100;
                            Consoul.Write($"Nonce: {current}/{max} {progress.ToString("F2")}%", ConsoleColor.DarkGray);
                            Consoul.Write($"Hash: {Utils.ToString(work.Hash)}", ConsoleColor.DarkGray);
                            Consoul.Write($"Speed: {(int)((Source.BatchSize / 1000) / sw.Elapsed.TotalSeconds)}Kh/s", ConsoleColor.DarkGray);
                        }
                        CurrentAttempt = 0;
                    }
                } catch (Exception e) {
                    Consoul.Write($"An error occurred while getting work: \r\n{e.Message}", ConsoleColor.Red);
                    CurrentAttempt++;
                }
                System.Threading.Thread.Sleep(1000);
            }
            Consoul.Wait();
        }
    }
}
