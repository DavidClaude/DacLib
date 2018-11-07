using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DacLib.U3D.Codex
{
	public delegate void PoolHandler ();

	public class GOElem : MonoBehaviour
	{
		public event PoolHandler onRequest;
		public event PoolHandler onCollect;

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