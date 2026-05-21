using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace UP17Anisimov
{
    internal class Core
    {
        public static UpAnisimovEntities2 Context = new UpAnisimovEntities2();
        public static Users CurrentUser { get; set; }
    }
}
