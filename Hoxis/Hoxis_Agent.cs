using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.Hoxis
{
	public class Hoxis_Agent : MonoBehaviour
	{
		/// <summary>
		/// Hoxis类型
		/// </summary>
		public AgentType agentType {get; private set;}

		public Hoxis_ID hoxisId { get; private set;}



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
		public void CoFunc (AgentType agentTypeArg, Hoxis_ID hoxisIdArg)
		{
			agentType = agentTypeArg;
			hoxisId = hoxisIdArg;
		}
	
		// Update is called once per frame
		void Update ()
		{
		
		}
	}
}