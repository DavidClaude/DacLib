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

        private void LoadUserData(HoxisProtocolArgs args)
        {
            long uid = FormatFunc.StringToLong(args.kv["uid"]);
            
            //访问数据库，获取UserData
            //
        }

        //private void 

        #endregion

    }
}
