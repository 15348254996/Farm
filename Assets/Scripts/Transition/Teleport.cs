using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Transition
{
    public class Teleport : MonoBehaviour
    {
        [SceneName]
        public string sceneToGo;
        public Vector3 positionToGO;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log(sceneToGo);
                EventHandler.CallTransitionEvent(sceneToGo,positionToGO);
            }
        }
    }
}

