using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameTowardsCamera : MonoBehaviour {

    public Text NameLabel;

	// Update is called once per frame
	void Update () {
        Vector3 NamePos = Camera.main.WorldToScreenPoint(this.transform.position);
        NameLabel.transform.position = NamePos;
	}
}
