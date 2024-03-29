using System.Collections;
using UnityEngine;

namespace HJ.Tools
{
    public class CoroutineRunner : MonoBehaviour
    {
        private IEnumerator _coroutine;
        private CoroutineRunner _self;

        public static Coroutine RunGet(GameObject owner, IEnumerator coroutine)
        {
            CoroutineRunner runner = owner.AddComponent<CoroutineRunner>();
            runner._coroutine = coroutine;
            runner._self = runner;

            return runner.StartCoroutine(runner.RunCoroutine());
        }

        public static CoroutineRunner Run(GameObject owner, IEnumerator coroutine)
        {
            CoroutineRunner runner = owner.AddComponent<CoroutineRunner>();
            runner._coroutine = coroutine;
            runner._self = runner;

            runner.StartCoroutine(runner.RunCoroutine());
            return runner;
        }

        public void Stop()
        {
            StopAllCoroutines();
            Destroy(_self);
        }

        public IEnumerator RunCoroutine()
        {
            yield return _coroutine;
            Destroy(_self);
        }
    }
}