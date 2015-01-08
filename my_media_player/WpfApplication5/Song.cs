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
