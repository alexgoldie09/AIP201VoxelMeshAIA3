using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToCheckIn : GAction
{
    private GameObject receptionStaff;

    public override bool PostPerform()
    {
        Debug.Log("Finding reception staff...");
        agent.updateRotation = false;
        receptionStaff = GameObject.FindWithTag("ReceptionStaff");

        StartCoroutine(TurnAgent());

        return true;
    }

    public override bool PrePerform()
    {
        return true;
    }

    private IEnumerator TurnAgent()
    {
        if (receptionStaff != null)
        {
            Vector3 direction = receptionStaff.transform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                Quaternion initialRotation = transform.rotation;

                float rotateDuration = 1f;
                float elapsed = 0f;

                while (elapsed < rotateDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / rotateDuration);
                    transform.rotation = Quaternion.Slerp(initialRotation, lookRotation, t);
                    yield return null;
                }

                transform.rotation = lookRotation; // ✅ snap to exact rotation
            }
        }

        yield return new WaitForSeconds(duration); // Optional: delay after rotating
        agent.updateRotation = true;
    }

}