using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace my_windowsMediaPlayer
{
    public enum Type
    {
        Sound,
        Video,
        Image
    }
    class MyFile
    {
        private int    _index;
        private string _name;
        private string _path;

        private string _artiste;
        private string _album;
        private string _year;
        private string _comment;
        
        private string _rate;
        private string _runtime;
        private string _genre;
        private string _director;
        private string _langue;
        private string _actors;
        private string _writer;

        private int _nbPlay = 0;

        private Type _type;
        private String _details;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }
        public int NbPlay
        {
            get { return _nbPlay; }
            set {_nbPlay = value; }
        }
        public string Writer
        {
            get { return _writer; }
            set
            {
                _writer = value;
            }
        }
        public string Rate
        {
            get { return _rate; }
            set
            {
                _rate = value;
            }
        }
        public string Actors
        {
            get { return _actors; }
            set
            {
                _actors = value;
            }
        }
        public string Runtime
        {
            get { return _runtime; }
            set
            {
                _runtime = value;
            }
        }
        public string Genre
        {
            get { return _genre; }
            set
            {
                _genre = value;
            }
        }
        public string Director
        {
            get { return _director; }
            set
            {
                _director = value;
            }
        }
        public string Langue
        {
            get { return _langue; }
            set
            {
                _langue = value;
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
            }
        }
        public string Artiste
        {
            get { return _artiste; }
            set
            {
                _artiste = value;
            }
        }
        public string Album
        {
            get { return _album; }
            set
            {
                _album = value;
            }
        }
        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
            }
        }
        public Type Type
        {
            get { return _type; }
            set
            {
                _type = value;
            }
        }
        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
            }
        }
        public String Year
        {
            get { return _year; }
            set
            {
                _year = value;
            }
        }
        public String Details
        {
            get { return "https://www.youtube.com?search_query=" + _name; }
         }
            
        public MyFile(string __Path, int __idx, Type __type)
        {
            char[] delimfile = { '\\' };
            string[] cleanname = __Path.Split(delimfile);
            _path = __Path;

            _index = __idx;
            _type = __type;            
           _name = cleanname[cleanname.Count() - 1];
        }
    }
}
