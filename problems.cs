using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace problemSolver.Model
{
    public class Problems
    {
        public string PhoneId { get; set; }
        public string SearchWord { get; set; }
        public string HtmlTitle { get; set; }
        public string Url { get; set; }
        public string Nice { get; set; }
        public string Key { get; set; }
        public bool Del { get; set; }
        public Color ListColor { get; set; }
        public GridLength DelWidth1 { get; set; }
        public GridLength DelWidth2 { get; set; }
        //public int PersonId { get; set; }
        //public string Name { get; set; }

    }
}
