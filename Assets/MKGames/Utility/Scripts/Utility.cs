using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MkGames
{
	public static class Utility
	{
		/// <summary>
		///     Tenta encontrar um objeto com o tipo especificado.
		/// </summary>
		/// <returns>O objeto encontrado ou null se não encontrado</returns>
		/// <typeparam name="T">O tipo a procurar</typeparam>
		public static T TryToFind<T>() where T : MonoBehaviour
		{
			var returnObject = Object.FindObjectOfType<T>();
			if (returnObject == null) Debug.Log("Objeto do tipo " + typeof(T).Name + " não encontrado");
			return returnObject;
		}

		/// <summary>
		///     Alternativa ao Invoke fornecido, utiliza uma função e não uma string.
		/// </summary>
		/// <param name="monoBehaviour">
		///     A classe MonoBehavior onde se encontra a função ("this" para a classe atual).
		/// </param>
		/// <param name="action">A função a ser chamada.</param>
		/// <param name="time">Delay em segundos até que a função seja chamada</param>
		public static Coroutine Invoke(MonoBehaviour monoBehaviour, Action action, float time)
		{
			return monoBehaviour.StartCoroutine(InvokeImpl(action, time));
		}

		/// <summary>
		///     Tenta encontrar o Componente especificado.
		/// </summary>
		/// <returns>Retorna o Componente ou null se não encontrado</returns>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetRequiredComponent<T>(GameObject obj) where T : Component
		{
			var component = obj.GetComponent<T>();

			if (component == null)
				Debug.LogError("Componente do tipo "
				               + typeof(T) + " não encontrado", obj);

			return component;
		}

		/// <summary>
		///     Verifica se o elemento não é nulo.
		/// </summary>
		/// <param name="obj"> Objeto sendo procurado.</param>
		public static void NotNull(Component obj)
		{
			if (obj == null) Debug.Log("Objeto do tipo " + obj.name + " não encontrado");
		}
		
		/// <summary>
		///     Verifica se o elemento não é nulo.
		/// </summary>
		/// <param name="obj"> Objeto sendo procurado.</param>
		public static void NotNullException(Component obj)
		{
			if (obj == null) 
				new UnityException("Objeto do tipo " + obj.name + " não encontrado");
		}

		private static IEnumerator InvokeImpl(Action action, float time)
		{
			yield return new WaitForSeconds(time);

			action();
		}

		/// <summary>
		///     Um Timer para uso geral.
		/// </summary>
		public class Timer
		{
			private bool countdownExpired;
			private float countdownExpireTime;

			private float countdownTime;
			private bool isPaused;
			private float time;

			/// <summary>
			///     Initializes a new instance of the <see cref="MkGames.Utility+Timer" /> class.
			///     Obs: Não se esqueça de chamar Update regularmente para que o timer atualize.
			/// </summary>
			public Timer()
			{
				countdownExpired = false;
				CountdownStarted = false;
			}

			/// <summary>
			///     Gets a value indicating whether this <see cref="MkGames.Utility+Timer" /> countdown started.
			/// </summary>
			/// <value><c>true</c> if countdown started; otherwise, <c>false</c>.</value>
			public bool CountdownStarted { get; private set; }

			/// <summary>
			///     Gets a value indicating whether this <see cref="MkGames.Utility+Timer" /> countdown expired.
			/// </summary>
			/// <value><c>true</c> if countdown expired; otherwise, <c>false</c>.</value>
			public bool CountdownExpired
			{
				get
				{
					if (countdownExpireTime == 0) Debug.Log("Countdown nunca setado");
					return countdownExpired;
				}
			}

			/// <summary>
			///     Atualiza o tempo interno do timer (Necessário para o timer funcionar).
			/// </summary>
			/// <param name="deltaTime">Delta time.</param>
			public void Update(float deltaTime)
			{
				if (!isPaused)
				{
					time += deltaTime;
					if (CountdownStarted && time > countdownExpireTime)
					{
						countdownExpired = true;
						CountdownStarted = false;
					}
				}
			}

			/// <summary>
			///     Sets the countdown.
			/// </summary>
			/// <param name="timeInSeconds">Time in seconds.</param>
			public void SetCountdown(float timeInSeconds)
			{
				countdownTime = timeInSeconds;
				countdownExpireTime = time + timeInSeconds;
				countdownExpired = false;
				CountdownStarted = true;
			}

			/// <summary>
			///     Resets the count down.
			/// </summary>
			public void ResetCountDown()
			{
				countdownExpireTime = time + countdownTime;
				countdownExpired = false;
				CountdownStarted = true;
			}

			/// <summary>
			///     Pausa o timer.
			/// </summary>
			public void Pause()
			{
				isPaused = true;
			}

			/// <summary>
			///     Roda o timer, ou o mantém rodando.
			/// </summary>
			public void Run()
			{
				isPaused = false;
			}
		}
	}
}