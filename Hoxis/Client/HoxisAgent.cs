using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis.Client
{
	public class HoxisAgent : MonoBehaviour
	{
   		/// <summary>
		/// Hoxis类型
		/// </summary>
		public HoxisType hoxisType {get; private set;}

		/// <summary>
		/// Hoxis编号
		/// </summary>
		/// <value>The hoxis identifier.</value>
		public HoxisID hoxisId { get; private set;}

		/// <summary>
		/// 自动同步
		/// 周期性矫正关键信息：位置、朝向、状态等
		/// </summary>
		/// <value><c>true</c> if auto syn; otherwise, <c>false</c>.</value>
		public bool autoSyn { get; set;}

        /// <summary>
        /// 是否为Host
        /// 该GameObject是否为Player
        /// </summary>
        public bool isPlayer { get; private set; }

		// Use this for initialization
		void Start ()
		{
			
		}

		/// <summary>
		/// Mono构造方法
		/// 手动调用
		/// </summary>
		/// <param name="tp"> Hoxis类型 </param>
		/// <param name="id"> Hoxis编号 </param>
		public void CoFunc (HoxisType hoxisTypeArg, HoxisID hoxisIdArg, bool autoSynArg = true)
		{
            hoxisType = hoxisTypeArg;
			hoxisId = hoxisIdArg;
			autoSyn = autoSynArg;
            isPlayer = (hoxisType == HoxisType.Host ? true : false);
		}
	
		// Update is called once per frame
		void Update ()
		{
		
		}
	}
}