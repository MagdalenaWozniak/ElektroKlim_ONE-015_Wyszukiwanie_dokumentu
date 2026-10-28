using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TYMnaFIN
{
    class TYM
    {
        public struct ListaTYM
        {
            public int GidTyp{ set; get; }
            public int GidNumer { set; get; }

            
            public string Nazwa { set; get; }

            public string Kwota { set; get; }
            public DateTime Data { set; get; }
        }
    }
}
