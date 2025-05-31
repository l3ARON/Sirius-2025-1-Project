using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breath : MonoBehaviour
{
    public int damage = 1;
    float angularPower = 2;
    bool isShoot;

    void Awake() {
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
    }
    //브레스 모으는 시간
    IEnumerator GainPowerTimer() {
        yield return new WaitForSeconds(2.2f);
        isShoot = true;
    }
    IEnumerator GainPower() {
        while(!isShoot){
            angularPower += 0.02f;
            yield return null;
        }
    }
}
