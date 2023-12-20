using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel.DataAnnotations;

namespace lightlauncher
{
    public class Game
    {
        [Key]
        public int ID { get; set; }
        public string name { get; set; }
        public string executablePath { get; set; }
        public string imagePath { get; set; }
        public Game(int ID, string name, string executablePath, string imagePath)
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
