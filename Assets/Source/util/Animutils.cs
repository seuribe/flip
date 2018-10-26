using UnityEngine;

namespace com.perroelectrico.flip.util {

    class ClipBuilder {

        public AnimationClip Clip { get; private set; }

        public ClipBuilder(AnimationClip clip = null) {
            Clip = clip ?? new AnimationClip();
            Clip.legacy = true;
        }

        public ClipBuilder LocalPosition(GameObject gameObject, Vector3 from, Vector3 to, float duration) {
            Clip.SetCurve(gameObject.name + "/", typeof(Transform), "localPosition.x", AnimationCurve.EaseInOut(0, from.x, duration, to.x));
            Clip.SetCurve("/" + gameObject.name, typeof(Transform), "localPosition.y", AnimationCurve.EaseInOut(0, from.y, duration, to.y));
            Clip.SetCurve("/" + gameObject.name + "/", typeof(Transform), "localPosition.z", AnimationCurve.EaseInOut(0, from.z, duration, to.z));
            return this;
        }

        public ClipBuilder LocalPosition(Vector3 from, Vector3 to, float start, float end) {
            Clip.SetCurve("", typeof(Transform), "localPosition.x", AnimationCurve.EaseInOut(start, from.x, end, to.x));
            Clip.SetCurve("", typeof(Transform), "localPosition.y", AnimationCurve.EaseInOut(start, from.y, end, to.y));
            Clip.SetCurve("", typeof(Transform), "localPosition.z", AnimationCurve.EaseInOut(start, from.z, end, to.z));
            return this;
        }

        public ClipBuilder LocalScale(Vector3 from, Vector3 to, float duration) {
            return LocalScale(from, to, 0, duration);
        }

        public ClipBuilder LocalScale(Vector3 from, Vector3 to, float start, float end) {
            Clip.SetCurve("", typeof(Transform), "localScale.x", AnimationCurve.EaseInOut(start, from.x, end, to.x));
            Clip.SetCurve("", typeof(Transform), "localScale.y", AnimationCurve.EaseInOut(start, from.y, end, to.y));
            Clip.SetCurve("", typeof(Transform), "localScale.z", AnimationCurve.EaseInOut(start, from.z, end, to.z));
            return this;
        }

        public ClipBuilder LocalRotation(Vector3 from, Vector3 to, float start, float end) {
            Clip.SetCurve("", typeof(Transform), "Rotation.x", AnimationCurve.EaseInOut(start, from.x, end, to.x));
            Clip.SetCurve("", typeof(Transform), "Rotation.y", AnimationCurve.EaseInOut(start, from.y, end, to.y));
            Clip.SetCurve("", typeof(Transform), "Rotation.z", AnimationCurve.EaseInOut(start, from.z, end, to.z));
            return this;
        }

    }
}
