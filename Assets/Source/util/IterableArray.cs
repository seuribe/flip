using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.perroelectrico.flip.util {

    public class IterableArray<T> {
        private IEnumerable<T> data;
        private int index;
        private bool wrap = false;

        public IterableArray(IEnumerable<T> data, bool wrap = true) {
            this.data = data;
            this.wrap = wrap;
            index = 0;
        }

        public int Index {
            get {
                return index;
            }
            set {
                if (value > 0 && value < data.Count()) {
                    index = value;
                }
            }
        }

        public T this[int i] {
            get {
                return data.ElementAt(i);
            }
        }

        public T Current {
            get {
                return data.ElementAt(index);
            }
        }

        public bool HasNext {
            get {
                return data != null && (wrap || index < data.Count() - 1);
            }
        }

        public bool HasPrev {
            get {
                return data != null && (wrap || index > 0);
            }
        }

        public T Next {
            get {
                if (index < data.Count() - 1 || wrap) {
                    index = (index + 1) % data.Count();
                }

                return data.ElementAt(index);
            }
        }

        public T Prev {
            get {
                if (index > 0 || wrap) {
                    index = (index + data.Count() - 1) % data.Count();
                }
                return data.ElementAt(index);
            }
        }

        internal void Goto(T element) {
            for (int i = 0 ; i < data.Count() ; i++) {
                if (data.ElementAt(i).Equals(element)) {
                    index = i;
                    return;
                }
            }
        }
    }
}