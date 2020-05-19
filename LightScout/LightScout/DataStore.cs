using LightScout.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LightScout
{
    public interface DataStore
    {
        void SaveData(string competitionfile, TeamMatch datatosave);
        string LoadData(string competitionfile);
    }
}
