using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace lightlauncher
{
    public class Game
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string executablePath { get; set; }
        public string imagePath { get; set; }
        //public Image image { get; set; }
        public Game(int ID, int number, string name, string executablePath, string imagePath)
        {
            this.ID = ID;
            this.name = name;
            this.executablePath = executablePath;
            this.imagePath = imagePath;
        }
        public Game()
        {
        }
    }
}
