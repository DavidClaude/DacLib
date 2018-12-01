using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DacLib.Generic;

namespace DacLib.Hoxis.Server
{
    public class HoxisUser
    {
        public HoxisUser()
        {

        }




        #region reflection functions

        private void LoadUserData(Dictionary<string,string> args)
        {
            long uid = FormatFunc.StringToLong(args["uid"]);
        }

        //private void 

        #endregion

    }
}
