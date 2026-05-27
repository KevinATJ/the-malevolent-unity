using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleStats : MonoBehaviour
{
    [Header("General")]
    public List<GameObject> collectibleExist;
    public int collectedCollectible = 0;
    private bool isEndlessMode = false;

    [Header("Events")]
    public GameEvent onCollectibleBatchExhausted;
    public GameEvent onCollectibleStatsUpdated;

    public void OnCollectibleSpawn(Component sender, object data)
    {
        GameObject collectible = (GameObject)data;
        collectibleExist.Add(collectible);

        if (AIDirectorBlackboard.Instance != null)
        {
            AIDirectorBlackboard.Instance.remainingObjectives = collectibleExist.Count;
        }
    }

    public void OnEndlessStart(Component sender, object data)
    {
        isEndlessMode = true;
    }

    public void OnStoryStart(Component sender, object data)
    {
        isEndlessMode = false;
    }

    public void OnCollectiblePickup(Component sender, object data)
    {
        GameObject collectible = (GameObject)data;

        Vector3 stolenPosition = collectible != null ? collectible.transform.position : Vector3.zero;

        collectibleExist.Remove(collectible);
        collectedCollectible++;
        onCollectibleStatsUpdated.Raise(collectedCollectible);

        if (AIDirectorBlackboard.Instance != null)
        {
            AIDirectorBlackboard.Instance.remainingObjectives = collectibleExist.Count;

            int totalObjects = collectibleExist.Count + collectedCollectible;
            if (totalObjects > 0)
            {
                float progress = (float)collectedCollectible / totalObjects;
                AIDirectorBlackboard.Instance.aggressionLevel = progress * 100f;
            }

            if (stolenPosition != Vector3.zero)
            {
                AIDirectorBlackboard.Instance.lastSoundPosition = stolenPosition;
                AIDirectorBlackboard.Instance.soundAge = 0f;
            }

            if (collectibleExist.Count > 0)
            {
                int randomIndex = Random.Range(0, collectibleExist.Count);
                if (collectibleExist[randomIndex] != null)
                {
                    AIDirectorBlackboard.Instance.currentPriorityZone = collectibleExist[randomIndex].transform.position;
                }
            }
        }

        if (collectibleExist.Count == 0)
        {
            if (isEndlessMode)
            {
                onCollectibleBatchExhausted.Raise();
            }
            else
            {
                Debug.Log("Complete");
            }
        }
    }

    public void OnShowGameOver(Component sender, object data)
    {
        onCollectibleStatsUpdated.Raise(collectedCollectible);
    }
}