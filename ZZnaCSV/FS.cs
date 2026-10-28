using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TYMnaFIN
{
    public class FS
    {
        
             public int GIDNumer { set; get; }
             public int GIDTyp { set; get; }
             public int GIDFirma { set; get; }
             public short GIDLp { set; get; }
            // public int KntNumer { set; get; }

        

        public FS(int GidNumer, int GidFirma, short GidLp, int GidTyp)
        {
            this.GIDNumer = GidNumer;
            this.GIDFirma = GidFirma;
            this.GIDLp = GidLp;
            this.GIDTyp = GidTyp;
           // this.KntNumer = kntNumer;
        }

        public FS()
        {
            this.GIDNumer = 0;
            this.GIDFirma = 0;
            this.GIDLp = 0;
            this.GIDTyp = 0;
            //this.KntNumer = 0;
        }
    }
}
