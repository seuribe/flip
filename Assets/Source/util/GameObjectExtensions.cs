using UnityEngine;

namespace com.perroelectrico.flip.util {

    public static class GameObjectExtensions {
        public static GameObject Child(this GameObject obj, string name) {
            return obj.transform.Find(name).gameObject;
        }

        public static void RemoveChild(this GameObject obj, string name) {
            var child = obj.transform.Find(name);
            if (child != null)
                GameObject.DestroyImmediate(child.gameObject);
        }

        public static void ShowChild(this GameObject obj, string name) {
            var child = obj.transform.Find(name);
            if (child != null)
                child.GetComponent<Renderer>().enabled = true;
        }

        public static void SetChildText(this GameObject parent, string objId, string text) {
            var tr = parent.transform.Find(objId);
            if (tr == null)
                return;

            var tm = tr.gameObject.GetComponent<TextMesh>();
            if (tm != null)
                tm.text = text;
        }
    }
}