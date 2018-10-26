using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.perroelectrico.flip.util {

    /// <summary>
    /// A queue for managing serial events: they are executed when there are no more events
    /// in the queue. Execution is FIFO, and external event managers have to signal when
    /// the event is finished.
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueuedSerialEvents<T> {
        private Queue<T> queue = new Queue<T>();
        private T current;
        private bool active = false;

        public delegate void EventListener(T evt);
        public event EventListener FireEvent;

        /// <summary>
        /// Enqueue a new event. if none was previously in the queue, will fire now. If not, will fire when
        /// all the remaining in the queue have finished
        /// </summary>
        /// <param name="evt"></param>
        public void NewEvent(T evt) {
            Debug.LogFormat("NewEvent: {0}", evt);
            lock (queue) {
                queue.Enqueue(evt);
                CheckQueue();
            }
        }

        private void CheckQueue() {
            Debug.LogFormat("CheckQueue, active: {0}", active);
            lock (queue) {
                if (queue.Count == 0 || active) {
                    return;
                }
                current = queue.Dequeue();
                if (FireEvent != null) {
                    active = true;
                    FireEvent(current);
                }
            }
        }

        internal void Clear() {
            Debug.Log("Clear");
            lock (queue) {
                queue.Clear();
                active = false;
            }
        }

        /// <summary>
        /// Call this after the event ended to invoke the next one queued (if any)
        /// </summary>
        public void EndEvent() {
            Debug.LogFormat("EndEvent, queue remaining: {0}", queue.Count);
            lock(queue) {
                active = false;
                CheckQueue();
            }
        }
    }
}
