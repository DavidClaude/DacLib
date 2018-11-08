using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DacLib.Generic;

namespace DacLib.U3D.Archits
{
	public class GOPoolElem : MonoBehaviour
	{
		public event NoneForVoid_Handler onRequest;
		public event NoneForVoid_Handler onCollect;

		public void OnRequest ()
		{
			if (onRequest == null)
				return;
			onRequest ();
		}

		public void OnCollect ()
		{
			if (onCollect == null)
				return;
			onCollect ();
		}

	}
}