using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace my_windowsMediaPlayer
{
    /// <summary>
    /// Logique d'interaction pour Bibliotheque.xaml
    /// </summary>
    public partial class Bibliotheque : Window
    {
        List<string> _listSongs;

        public Bibliotheque(List<string> _songs)
        {
            InitializeComponent();
            this._listSongs = _songs;
            foreach (var song in _listSongs)
            {
              //  songlist.Items.Add(song);
            }
         }

    }
}
