using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Xml.Linq;
using System.IO;
using System.Drawing;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Web;
using System.Xml;

namespace my_windowsMediaPlayer
{

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    
public partial class Mywindow : Window
    {
        #region Constructor

        List<MyFile> _allfiles = new List<MyFile>();
        List<MyFile> _biblifiles = new List<MyFile>();
        String       _username = Environment.UserName;
        double        PauseTime = 0.00;
        int          _playMoy;
        string       _PlaylistPath;
 
        public ICollectionView asong { get; private set; }
        public ICollectionView Groupedsong { get; private set; }
        DispatcherTimer timer;
        bool Draging = false;

        int current = 0;
        // 0 : sans vidéo
        // 1 : vidéo
        // 2 : image
        private int mode = 0;

        public Mywindow()
        {
            InitializeComponent();
            InitializePropertyValues();
            IsPlaying(false);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(timer_Timeline);
         }
        #endregion

        #region IsPlaying(bool)
        private void IsPlaying(bool bValue)
        {
            btnStop.IsEnabled = bValue;
            btnMoveBackward.IsEnabled = bValue;
            btnMoveForward.IsEnabled = bValue;
            btnPlay.IsEnabled = bValue;
        }
        #endregion
        #region Timer
        void timer_Timeline(Object sender, EventArgs e)
        {
            String time;
            if (!Draging && btnPlay.Content.ToString() == "Pause")
            {
                timelineSlid.Value += MediaEL.Position.TotalSeconds;
                TimeSpan ts;
                ts = MediaEL.Position;
                if (ts.Hours > 0)
                    time = ts.Hours.ToString() + ":" + ts.Minutes.ToString() + ":" + ts.Seconds.ToString();
                else
                    time = ts.Minutes.ToString() + ":" + ts.Seconds.ToString();
                textTimer.Text = time;
            }
        }
        void    reset_Timer(double val)
        {            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(timer_Timeline);
            timelineSlid.Value = val;
        }
        #endregion
        #region Play and Pause
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            IsPlaying(true);
            if (btnPlay.Content.ToString() == "Play")
            {
                if (MediaEL.NaturalDuration.HasTimeSpan)
                {
                    PauseTime = timelineSlid.Value;
     /*                   reset_Timer(PauseTime);
                        TimeSpan ts = MediaEL.NaturalDuration.TimeSpan;
                        timelineSlid.Maximum = ts.TotalSeconds;
                        timelineSlid.SmallChange = 1;
                        timelineSlid.LargeChange = Math.Min(10, ts.Seconds / 10);*/
                    Draging = false;
                }
                timer.Start();
                if (MediaEL.HasVideo)
                    MediaEL.Play();
                else
                {
                    mediaELFAKE = MediaEL;
                    mediaELFAKE.Play();
                }
                btnPlay.Content = "Pause";
            }
            else
            {
                MediaEL.Pause();
                PauseTime = timelineSlid.Value;
                btnPlay.Content = "Play";                
            }
        }
        #endregion
        #region Stop
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            MediaEL.Stop();
            btnPlay.Content = "Play";
            IsPlaying(false);
            btnPlay.IsEnabled = true;

        }
        #endregion
        #region Back and Forward
        private void btnMoveForward_Click(object sender, RoutedEventArgs e)
        {
            if (MediaEL.Position < TimeSpan.FromSeconds(7))
                if (current - 1 >= 0)
                    play_file(current - 1);
                else
                    MediaEL.Position = TimeSpan.FromSeconds(0);
            else
                MediaEL.Position = TimeSpan.FromSeconds(0);
        }

        private void btnMoveBackward_Click(object sender, RoutedEventArgs e)
        {
            next_song();
        }
        #endregion
        #region Open Media
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            char[] delimfile = { '\\' };
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String name in ofd.FileNames)
                {
                    if (Check_ext(name, 1) || Check_ext(name, 0))
                    {
                        #region Vidéo - Sound
                    if (_allfiles.Count == 0)
                        MediaEL.Source = new Uri(name);

                    _allfiles.Add(new MyFile(name, _allfiles.Count(), Type.Sound));
                    listsongs.Items.Add(_allfiles[_allfiles.Count - 1].Name);

                    if (MediaEL.HasVideo)
                        mode = 1;
                    else
                        mode = 0;
                    getMetaMov(_allfiles.Count - 1, 0);
                    btnPlay.IsEnabled = true;
                    #endregion
                    }
                    else if (Check_ext(name, 2))
                      {
                        #region Image Visualiser
                        btnPlay.IsEnabled = false;
                        IsPlaying(false);
                        btnPlay.Content = "Image";
                        BitmapImage src = new BitmapImage();
                        ImageMetadata data;
                        src.BeginInit();
                        src.UriSource = new Uri(name);
                        src.DecodePixelWidth = 200;
//                        data = src.Metadata;                        
                        src.EndInit();

                        imgvisualizer.Source = src;
                        mode = 2;
  //                      getMetaImg(data);
 
//                        _songs.Add(name);
                        _allfiles.Add(new MyFile(name, _allfiles.Count(), Type.Image));
                        listsongs.Items.Add(_allfiles[_allfiles.Count - 1].Name);
                        #endregion
                     }
                }
            }
        }
        private void Element_MediaOpened(object sender, EventArgs e)
        {
            timelineSlid.Maximum = MediaEL.NaturalDuration.TimeSpan.TotalMilliseconds;
        }
        #endregion
        #region Volume
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            //            if (MediaEL.IsMuted)
            // setcontent au text : Muted
            MediaEL.Volume = (double)volumeSlider.Value;
        }
        #endregion
        #region Speed
        private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            MediaEL.SpeedRatio = (double)speedRatioSlider.Value;
        }
        #endregion
        #region Position
   //     private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
  //      {
   //  
   //         int SliderValue = (int)timelineSlid.Value;
    //        TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
     //       MediaEL.Position = ts;
       // }
        private void Timeline_SeekStart(object sender, DragStartedEventArgs e) { Draging = true; }

        private void Timeline_SeekEnd(object sender, DragCompletedEventArgs e)
        {
            Draging = false;
            int SliderValue = (int)timelineSlid.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, SliderValue);
            MediaEL.Position = ts;
        }
   
        #endregion
        #region Initialize
        void InitializePropertyValues()
        {
            MediaEL.Volume = (double)volumeSlider.Value;
            MediaEL.SpeedRatio = (double)speedRatioSlider.Value;
            listsongs.SelectionMode = SelectionMode.Single;
            feed_bibli();
            ICollectionView cvTasks = CollectionViewSource.GetDefaultView(_biblifiles);
            if (cvTasks != null && cvTasks.CanGroup == true)
            {
                cvTasks.GroupDescriptions.Clear();
                cvTasks.GroupDescriptions.Add(new PropertyGroupDescription("Type"));
            }
            bibli.ItemsSource = cvTasks;
        }
        #endregion
        #region Media End
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            MediaEL.Stop();
            if (current + 1 <= _allfiles.Count())
                next_song();
        }
        #endregion

        #region Playlist
        private void listsongs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listsongs.SelectedIndex != -1)
                play_file(listsongs.SelectedIndex);
        }

        private void    play_file(int idx)
        {
            current = idx;
            if (Check_ext(_allfiles[current].Path, 1) || Check_ext(_allfiles[current].Path, 0))
            {
                #region Vidéo - Sound
                reset_Timer(0.00);
                MediaEL.Source = new Uri(_allfiles[current].Path);
                timer.Start();
                if (MediaEL.HasVideo) // trix de sons
                    MediaEL.Play();
                else
                {
                    mediaELFAKE = MediaEL;
                    mediaELFAKE.Play();
                }
                if (timelineSlid.Value < 0.5)
                    _allfiles[current].NbPlay++;
                btnPlay.Content = "Pause";
                mode = 0;
                
                IsPlaying(true);

                #endregion
            }
            else if (Check_ext(_allfiles[current].Path, 2))
            {
                #region Image Visualiser

                BitmapImage src = new BitmapImage();
                ImageMetadata data;
                src.BeginInit();
                src.UriSource = new Uri(_allfiles[current].Path);
                src.DecodePixelWidth = 200;
  //              data = src.Metadata;
                src.EndInit();

                imgvisualizer.Source = src;
                mode = 2;
                btnPlay.Content = "Image";
                btnPlay.IsEnabled = false;
//                getMetaImg(data);
                IsPlaying(false);

                #endregion
            }
        }
        private void    Save_Playlist(object sender, EventArgs e)
        {
            int i = 0;

            XDocument document = new XDocument(
             new XElement("Playlist"));
            while (i < _allfiles.Count)
            {
                document.Element("Playlist").Add(new XElement("Song", new XAttribute("NbPlay", _allfiles[i].NbPlay), new XAttribute("Path", _allfiles[i].Path)));
                i++;
            }
            System.Windows.Forms.SaveFileDialog ofd = new System.Windows.Forms.SaveFileDialog();
            ofd.DefaultExt = ".xml";
            ofd.AddExtension = true;
            if (ListName.Text != "Playlist sans nom")
                ofd.FileName = ListName.Text;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                document.Save(ofd.FileName);
                char[] delimfile = { '\\' };
                string[] cleanname = ofd.FileName.Split(delimfile);
                ListName.Text = cleanname[cleanname.Count() - 1];
            }
        }
 
        private void    Import_Playlist(object sender, EventArgs e)
        {
            current = 0;
            _playMoy = 0;
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.DefaultExt = ".xml";
            ofd.AddExtension = true;
            MediaEL.Stop();
            IsPlaying(false);
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                char[] delimiterChars = { '>', '<' };
                char[] delimfile = { '\\' };

                if (!Check_ext(ofd.FileName, 3))
                    return ;
                XElement xelement = XElement.Load(ofd.FileName);
                IEnumerable<XElement> playlist = xelement.Elements();
                
                string[] cleanname = ofd.FileName.Split(delimfile);
                ListName.Text = cleanname[cleanname.Count() - 1];
                listsongs.Items.Clear();
                _allfiles.Clear();
                XElement doc = XElement.Parse(xelement.ToString());
                IEnumerable<XElement> infos =
                from el in doc.Elements("Song")
                select el;
                foreach (XElement t in infos)
                {
                    string path =  (string)t.Attribute("Path");
                    if (File.Exists(path))
                    {
                        _allfiles.Add(new MyFile(path, _allfiles.Count(), Type.Sound));
                        _allfiles[_allfiles.Count() - 1].NbPlay = int.Parse((string)t.Attribute("NbPlay"));
                        _playMoy += int.Parse((string)t.Attribute("NbPlay"));
                        listsongs.Items.Add(_allfiles[_allfiles.Count() - 1].Name);
                        if (Check_ext(path, 0))
                            getMetaMov(_allfiles.Count() - 1, 0);
                       else if (Check_ext(path, 1))
                           getMetaMov(_allfiles.Count() - 1, 0);
//                        else if (Check_ext(clean[2], 2))
//                            getMetaImg()
                    }
                    else
                    {
                        string msg = "Le fichier " + cleanname[cleanname.Count() - 1] + " n'a pas pus être trouvé";
                        MessageBox.Show(msg);
                    }
                }
                if (_allfiles.Count > 0)
                    _playMoy = _playMoy / _allfiles.Count();
            }
        }
        private void    clean_playlist(object sender, EventArgs e)
        {
            _allfiles.Clear();
            ListName.Text = "Playlist sans nom";
            IsPlaying(false);
            mode = 0;
            listsongs.Items.Clear();
        }
        private void    next_song()
        {
            if (current + 1 < _allfiles.Count())
            {
                if (mode == 1)
                    while (_allfiles[current].NbPlay <= _playMoy && current < _allfiles.Count())
                        current++;
                else if (mode == 2)
                    while (_allfiles[current].NbPlay >= _playMoy && current < _allfiles.Count())
                        current++;
                else
                    current++;
                play_file(current);
            }
            else
                MediaEL.Stop();
        }

        #endregion
        #region Image Visualizer
        private void    initImgMode()
        {


        }
        private void    initSVMode()
        {

        }
        #endregion

        private bool    Check_ext(string path, int mod)
        {
            if (mod == 0)
            {
                if (path.IndexOf(".wmv") != -1 || path.IndexOf(".avi") != -1 || path.IndexOf(".mov") != -1)
                    return (true);
                else               
                    return (false);
            }
            else if (mod == 1)
            {
                if (path.IndexOf(".mpg") != -1 || path.IndexOf(".mpa") != -1 || path.IndexOf(".wma") != -1 || path.IndexOf(".mp3") != -1 || path.IndexOf(".vob") != -1 || path.IndexOf(".flac") != -1 || path.IndexOf(".cda") != -1 || path.IndexOf(".wav") != -1 || path.IndexOf(".mid") != -1 || path.IndexOf(".ogg") != -1)
                    return (true);
                else
                    return (false);
            }   
            else if (mod == 2)
            {
                if (path.IndexOf(".jpg") != -1 || path.IndexOf(".jpeg") != -1 || path.IndexOf(".bmp") != -1 || path.IndexOf(".tiff") != -1 || path.IndexOf(".gif") != -1 || path.IndexOf(".rif") != -1 || path.IndexOf(".bpg") != -1 || path.IndexOf(".png") != -1)
                    return (true);
                else
                    return (false);
            }
            if (mod == 3)
            {
                if (path.IndexOf(".xml") != -1)
                    return (true);
                else
                    return (false);
            }
            return (false);
        }

        #region Meta Data
        #region Sound - Video Meta
        #region By-Bytes
        private void    getMetaMov(int idx, int mod)
        {

                byte[] b = new byte[128];

                string sTitle;
                string sSinger;
                string sAlbum;
                string sYear;
                string sComm;
                FileStream fs;

                try
                {
                    if (mod == 0)
                        fs = new FileStream(_allfiles[idx].Path, FileMode.Open);
                    else
                        fs = new FileStream(_biblifiles[idx].Path, FileMode.Open);
                }
                catch
                {
                    return;
                }
                fs.Seek(-128, SeekOrigin.End);
                fs.Read(b, 0, 128);
                bool isSet = false;
                String sFlag = System.Text.Encoding.Default.GetString(b, 0, 3);
                if (sFlag.CompareTo("TAG") == 0)
                {
                    isSet = true;
                }

                if (isSet)
                {
                    //get   title   of   song; 
                    if ((sTitle = System.Text.Encoding.Default.GetString(b, 3, 30)) == null)
                        sTitle = "n/a";

                    //get   singer; 
                    if ((sSinger = System.Text.Encoding.Default.GetString(b, 33, 30)) == null)
                        sSinger = "n/a";

                    //get   album; 
                    if ((sAlbum = System.Text.Encoding.Default.GetString(b, 63, 30)) == null)
                        sAlbum = "n/a";
                    //get   Year   of   publish; 
                    if ((sYear = System.Text.Encoding.Default.GetString(b, 93, 4)) == null)
                        sYear = "n/a";

                    //get   Comment; 
                    if ((sComm = System.Text.Encoding.Default.GetString(b, 97, 30)) == null)
                        sComm = "n/a";
                    if (mod == 0)
                    {
                        _allfiles[idx].Name = sTitle;
                        _allfiles[idx].Comment = sComm;
                        _allfiles[idx].Year = sYear;
                        _allfiles[idx].Album = sAlbum;
                        _allfiles[idx].Artiste = sSinger;
                    }
                    else
                    {
                        _biblifiles[idx].Name = sTitle;
                        _biblifiles[idx].Comment = sComm;
                        _biblifiles[idx].Year = sYear;
                        _biblifiles[idx].Album = sAlbum;
                        _biblifiles[idx].Artiste = sSinger;
                    }
                }
                fs.Close();
        }
        private void see_meta(object sender, EventArgs e)
        {
            feed_SongInfo(current, 1);
        }
        #endregion
        #region API IMDB
        private void    feed_SongInfo(int idx, int mod)
        {
            String rep;

            char[] delimfile = { '.', ',', '-', '_' };
            string[] cleanname = _allfiles[idx].Name.Split(delimfile);

            MyWebRequest req = new MyWebRequest("http://www.omdbapi.com/?t=" + cleanname[0] + "&apikey=b245d105&r=xml");
            rep = req.GetResponse();
            XElement doc = XElement.Parse(rep);
            IEnumerable<XElement> infos =
            from el in doc.Elements("movie")
            select el;
            foreach (XElement t in infos)
            {

                _biblifiles[idx].Name = (string)t.Attribute("title");
                _biblifiles[idx].Year = (string)t.Attribute("year");
                _biblifiles[idx].Rate = (string)t.Attribute("rated");
                _biblifiles[idx].Runtime = (string)t.Attribute("runtime");
                _biblifiles[idx].Langue = (string)t.Attribute("language");
                _biblifiles[idx].Actors = (string)t.Attribute("actors");
                _biblifiles[idx].Director = (string)t.Attribute("director");
                _biblifiles[idx].Artiste = (string)t.Attribute("director");
                _biblifiles[idx].Writer = (string)t.Attribute("writer");
                _biblifiles[idx].Comment = (string)t.Attribute("plot");

                _allfiles[idx].Name = (string)t.Attribute("title");
                _allfiles[idx].Year = (string)t.Attribute("year");
                _allfiles[idx].Rate = (string)t.Attribute("rated");
                _allfiles[idx].Runtime = (string)t.Attribute("runtime");
                _allfiles[idx].Langue = (string)t.Attribute("language");
                _allfiles[idx].Actors = (string)t.Attribute("actors");
                _allfiles[idx].Director = (string)t.Attribute("director");
                _allfiles[idx].Artiste = (string)t.Attribute("director");
                _allfiles[idx].Writer = (string)t.Attribute("writer");
                _allfiles[idx].Comment = (string)t.Attribute("plot");

            }
            if (mod == 1 && _biblifiles[idx].Comment != null)
                MessageBox.Show("Movie info : " + _biblifiles[idx].Name + Environment.NewLine +
                    Environment.NewLine +
                    "Name : " + _biblifiles[idx].Name + Environment.NewLine +
                    "rate : " + _biblifiles[idx].Rate + Environment.NewLine +
                    "Année de production : " + _biblifiles[idx].Year + Environment.NewLine +
                    "Durée : " + _biblifiles[idx].Runtime + Environment.NewLine +
                    "Langue : " + _biblifiles[idx].Langue + Environment.NewLine +
                    "Acteurs : " + _biblifiles[idx].Actors + Environment.NewLine +
                    "Directeur(s) : " + _biblifiles[idx].Director + Environment.NewLine +
                    "Ecrivain(s) : " + _biblifiles[idx].Writer + Environment.NewLine
                    );
            else
                MessageBox.Show("Nous n'avons pus trouvé d'information concernant cette vidéo. Veuillez vérifier le nom du fichier.");
        }
        #endregion
        #endregion
        #region Image
     /*   private void    getMetaImg(ImageMetadata img)
        {
            List<string> arrHeaders = new List<string>();

            Shell32.Shell shell = new Shell32.Shell();
            Shell32.Folder objFolder;

            objFolder = shell.NameSpace(@"C:\temp\testprop");

            for (int i = 0; i < short.MaxValue; i++)
            {
                string header = objFolder.GetDetailsOf(null, i);
                if (String.IsNullOrEmpty(header))
                    break;
                arrHeaders.Add(header);
            }

            foreach (Shell32.FolderItem2 item in objFolder.Items())
            {
                for (int i = 0; i < arrHeaders.Count; i++)
                {
                    Console.WriteLine("{0}\t{1}: {2}", i, arrHeaders[i], objFolder.GetDetailsOf(item, i));
                }
            }
//            MessageBox.Show("The Description metadata of this image is: " + img("/Text/Description").ToString());

        }*/
        #endregion
        #endregion
        #region Bibliotheque
        private void feed_bibli()
        {
            List<String> _allPath = new List<string>();
            string[] dirs;
            _allPath.Add("c:\\Users\\" + _username + "\\Music\\");
            _allPath.Add("c:\\Users\\Public\\Music");
            _allPath.Add("c:\\Users\\" + _username + "\\Pictures\\");
            _allPath.Add("c:\\Users\\Public\\Pictures\\");
            _allPath.Add("c:\\Users\\" + _username + "\\Videos\\");
            _allPath.Add("c:\\Users\\Public\\Videos\\");
            _allPath.Add("c:\\Users\\" + _username + "\\Documents\\");
            _allPath.Add("c:\\Users\\Public\\Documents\\");

            try
            {
                foreach (string p in _allPath)
                {
                    dirs = Directory.GetFiles(p, "*");
                    foreach (string dir in dirs)
                    {
                        if (Check_ext(dir, 0))
                        {
                            _biblifiles.Add(new MyFile(dir, _biblifiles.Count - 1, Type.Video));
                            getMetaMov(_biblifiles.Count - 1, 1);
                        }
                        if (Check_ext(dir, 1))
                        {
                            _biblifiles.Add(new MyFile(dir, _biblifiles.Count - 1, Type.Sound));
                            getMetaMov(_biblifiles.Count - 1, 1);
                        }
                        else if (Check_ext(dir, 2))
                        {
                            _biblifiles.Add(new MyFile(dir, _biblifiles.Count - 1, Type.Image));
                            getMetaMov(_biblifiles.Count - 1, 1);
                        }
                    }                    
                }
            }
            catch (Exception e)
            {
               MessageBox.Show("Error when try to feed bibliotheque");
            }

        }
        #endregion
        #region playlist gestion
        private void    Starred_playlist(object sender, EventArgs e)
        {
            current = 0;
            while (_allfiles[current].NbPlay <= _playMoy && current < _allfiles.Count)
                current++;
            mode = 1;
            play_file(current);
        }
        private void    UnStarred_playlist(object sender, EventArgs e)
        {
            current = 0;
            while (_allfiles[current].NbPlay >= _playMoy && current < _allfiles.Count)
                current++;
            mode = 2;
            play_file(current);
        }
        private void normal_lecture(object sender, EventArgs e)
        {
            current = 0;
            mode = 0;
            play_file(current);
        }
        #endregion
        private void    close_app(Object sender, EventArgs e)
        {
            MessageBox.Show("Good Bye " + Environment.UserName);
        }
    }
}