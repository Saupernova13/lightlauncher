//By Sauraav Jayrajh
using System.ComponentModel.DataAnnotations;
namespace lightlauncher
{
    public class Emulator
    {
        [Key]
        public int ID { get; set; }
        public string name { get; set; }
        public string executablePath { get; set; }
    }
}
