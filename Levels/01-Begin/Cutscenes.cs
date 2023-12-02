using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScenes : MonoBehaviour
{

    private GameObject oldManGameObject;
    private GameObject playerObject;
    private Player playerScript;

    private bool startWalkingTowardsThePlayer = false;

    // Start is called before the first frame update
    void Start()
    {
        oldManGameObject = GameObject.Find("OldMan").gameObject;
        playerObject = GameObject.Find("Player");
        playerScript = playerObject.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startWalkingTowardsThePlayer)
        {
            float distance = Vector3.Distance(oldManGameObject.transform.position, playerObject.transform.position);

            oldManGameObject.transform.LookAt(playerObject.transform);
            playerObject.transform.LookAt(oldManGameObject.transform);
            if (distance > 4.5f)
            {
                oldManGameObject.transform.position += oldManGameObject.transform.forward * 2f * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        playerScript.SetCanWalk(false);
        playerScript.ResetMovementAnimations();
        playerScript.StartChat(true);

        startWalkingTowardsThePlayer = true;
    }
}
