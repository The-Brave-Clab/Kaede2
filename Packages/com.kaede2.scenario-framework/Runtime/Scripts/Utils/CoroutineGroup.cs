using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.Scenario.Framework.Utils
{
    public class CoroutineGroup
    {
        private class CoroutineStatus
        {
            public Coroutine coroutine;
            public MonoBehaviour owner;
            public bool isFinished;
        }

        private readonly List<CoroutineStatus> coroutineStatus = new();

        public CoroutineGroup()
        {
        }

        public CoroutineGroup(IEnumerable<IEnumerator> coroutines, MonoBehaviour owner = null)
        {
            foreach (var coroutine in coroutines)
            {
                Add(coroutine, owner);
            }
        }

        public Coroutine Add(IEnumerator coroutine, MonoBehaviour owner = null)
        {
            if (owner == null)
                owner = CoroutineProxy.Instance;

            CoroutineStatus status = new CoroutineStatus
            {
                isFinished = false,
                owner = owner
            };
            status.coroutine = owner.StartCoroutine(SetStatusAfterCoroutine(coroutine, status));
            coroutineStatus.Add(status);

            return status.coroutine;
        }

        public void Stop(Coroutine coroutine)
        {
            if (coroutine == null)
                return;

            CoroutineStatus status = coroutineStatus.Find(c => c.coroutine == coroutine);
            if (status == null) return;

            if (status.owner != null)
                status.owner.StopCoroutine(status.coroutine);
            coroutineStatus.Remove(status);
        }

        public void StopAll()
        {
            foreach (var status in coroutineStatus)
            {
                if (status.owner != null)
                    status.owner.StopCoroutine(status.coroutine);
            }
            coroutineStatus.Clear();
        }

        public IEnumerator WaitForAll()
        {
            coroutineStatus.RemoveAll(c => c.owner == null);
            while (coroutineStatus.Exists(c => !c.isFinished))
            {
                yield return null;
            }
            coroutineStatus.Clear();
        }

        private IEnumerator SetStatusAfterCoroutine(IEnumerator c, CoroutineStatus s)
        {
            while (c.MoveNext())
            {
                yield return c.Current;
            }
            s.isFinished = true;
        }
    }
}