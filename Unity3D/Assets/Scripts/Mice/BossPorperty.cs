using UnityEngine;
using System.Collections;

public class BossPorperty : MonoBehaviour {
    BattleHUD battleHUD;
    public float hpMax { get; set; }
    public float hp { get; set; }

    private bool isDead;

	void Start () {
        isDead = false;
        battleHUD = GameObject.Find("GameManager").GetComponent<BattleHUD>();
	}
	

	void Update () {

        if (!isDead)
        {
            if (hp <= 0)
                isDead = true;
            battleHUD.ShowBossHPBar(hp / hpMax, isDead);
        }
        else
        {
            Debug.Log("BOSS DEAD:" + hp);
            transform.parent.parent.GetComponent<Animator>().Play("HoleScale_R");
            GetComponent<Animator>().Play("Die");
        }
	}

    void OnSpawn()
    {
        isDead = false;
        battleHUD = GameObject.Find("GameManager").GetComponent<BattleHUD>();
    }

}
