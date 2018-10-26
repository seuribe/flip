using System;
using UnityEngine;

using com.perroelectrico.flip.controller.ui;
using com.perroelectrico.flip.core;
using com.perroelectrico.flip.util;

public class AchievementController : MonoBehaviour {

    private BadgeController badgeController;
    public TextMesh text;

    public float showTime = 3f;

    private Action onEnd;

    void Awake() {
        badgeController = GetComponentInChildren<BadgeController>();
    }

    public void Show(Badge badge, Action onEnd) {
        Debug.LogFormat("Show {0}", badge);
        badgeController.Badge = badge;
        this.onEnd = onEnd;

        var tr = TextResource.Get("gui");
        text.text = tr["game.gui.BadgeUnlocked"];

        var anim = GetComponent<Animation>();
        if (anim != null) {
            anim.Play();
            showTime = anim.clip.length;
        }
        Invoke("Close", showTime);
    }

    void Close() {
        Destroy(gameObject);
        onEnd();
    }
}
