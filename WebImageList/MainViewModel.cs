using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebImageList
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            using (var sr = new StreamReader("list.txt"))
            {
                this._Images = new List<String>();
                while (!sr.EndOfStream)
                {
                    this._Images.Add(sr.ReadLine());
                }
            }
        }
        private List<String> _Images;

        public List<String> Images
        {
            get { return _Images; }
            set { _Images = value; }
        }

    }
}
