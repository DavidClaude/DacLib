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
        /// <summary>
        /// Event of post protocols
        /// Should be registered by superior HoxisConnection
        /// </summary>
        public event ProtocolHandler onPost;

        protected Dictionary<string, ActionArgsHandler> businessTable = new Dictionary<string, ActionArgsHandler>();

        public HoxisUser()
        {
            #region register reflection table
            businessTable.Add("load_user_data", LoadUserData);
            businessTable.Add("save_user_data", SaveUserData);
            #endregion
        }




        #region reflection functions

        private void LoadUserData(HoxisProtocolArgs args)
        {
            //解析uid
            long uid = FormatFunc.StringToLong(args.kv["uid"]);

            //访问数据库，获取UserData

            //将UserData转为json

            //将UserData打包成协议
            HoxisProtocol proto = new HoxisProtocol
            {
                type = ProtocolType.Response,
                handle = "",
                rcvr = HoxisProtocolReceiver.undef,
                sndr = HoxisProtocolSender.undef,
                action = new HoxisProtocolAction
                {
                    method = "",
                    args = new HoxisProtocolArgs
                    {
                        kv = new Dictionary<string, string> {
                            {"data","" },
                        }
                    }
                },
                desc = "",
            };

            onPost(proto);
        }

        private void SaveUserData(HoxisProtocolArgs args)
        {

        }


        #endregion

    }
}
