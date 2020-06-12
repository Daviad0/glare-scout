using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LightScout.Models
{
    public class MatchViewModel : INotifyPropertyChanged
    {
        private static ObservableCollection<TeamMatchViewItem> matches = new ObservableCollection<TeamMatchViewItem>();
        //private static HttpClient client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TeamMatchViewItem> Matches
        {
            get { return matches; }
            set
            {

                matches = value;
            }
        }

        public async Task Update()
        {
            Matches.Clear();
            Matches.Add(new TeamMatchViewItem() { MatchNumber = 1, TeamName = "Lightning Robotics", TeamNumber = 862 });
            Matches.Add(new TeamMatchViewItem() { MatchNumber = 2, TeamName = "Cheesy Poofs", TeamNumber = 254 });
            Matches.Add(new TeamMatchViewItem() { MatchNumber = 3, TeamName = "Robonauts?", TeamNumber = 114 });
            Matches.Add(new TeamMatchViewItem() { MatchNumber = 4, TeamName = "Lightning Robotics 2", TeamNumber = 8622 });
        }
    }
}
