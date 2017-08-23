﻿using System.Collections;
using System.Threading;

namespace MkGames
{
	public class ThreadedJob
	{
		private readonly object m_Handle = new object();
		private bool m_IsDone;
		private Thread m_Thread;

		public bool IsDone
		{
			get
			{
				bool tmp;
				lock (m_Handle)
				{
					tmp = m_IsDone;
				}
				return tmp;
			}
			set
			{
				lock (m_Handle)
				{
					m_IsDone = value;
				}
			}
		}

		public virtual void Start()
		{
			m_Thread = new Thread(Run);
			m_Thread.Start();
		}

		public virtual void Abort()
		{
			m_Thread.Abort();
		}

		protected virtual void ThreadFunction()
		{
		}

		protected virtual void OnFinished()
		{
		}

		public virtual bool Update()
		{
			if (IsDone)
			{
				OnFinished();
				return true;
			}
			return false;
		}

		public IEnumerator WaitFor()
		{
			while (!Update())
				yield return null;
		}

		private void Run()
		{
			ThreadFunction();
			IsDone = true;
		}
	}
} //MkGames