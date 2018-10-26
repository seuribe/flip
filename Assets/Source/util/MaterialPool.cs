using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.perroelectrico.flip.util {

    /// <summary>
    /// Generic material pool
    /// </summary>
    /// <typeparam name="T">A class or struct that contains the parameters for identifying and constructing a new material</typeparam>
    class MaterialPool<T> {

        // The definition of a function that given a T will return a new material.
        // This is used when the material is inserted in the pool for the first time.
        public delegate Material MaterialGenerator(T t);

        private Dictionary<T, Material> pool;
        private MaterialGenerator generator;

        public MaterialPool(MaterialGenerator generator) {
            this.pool = new Dictionary<T, Material>();
            this.generator = generator;
        }

        public Material GetMaterial(T t) {
            Material mat;
            if (!pool.TryGetValue(t, out mat)) {
                mat = generator(t);
                pool[t] = mat;
            }
            return mat;
        }
    }
}
