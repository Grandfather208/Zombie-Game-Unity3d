﻿using UnityEngine;

namespace Invector
{
    using vCharacterController;
    public class vHealthItem : MonoBehaviour
    {
        [Tooltip("How much health will be recovery")]
        public float value;
        public string tagFilter = "Player";

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag.Equals(tagFilter))
            {
                // access the basic character information
                var healthController = other.GetComponent<vHealthController>();
                if (healthController != null)
                {

                    // heal only if the character's health isn't full
                    if (healthController.currentHealth < healthController.maxHealth)
                    {
                        // limit healing to the max health
                        healthController.ChangeHealth((int)value);
                        Destroy(gameObject);
                    }
                    //else
                    //{
                    //    var tpInput = other.GetComponent<vThirdPersonInput>();
                    //    if (tpInput != null)
                    //    {
                    //        // display a message using the third person controller hud
                    //        tpInput.hud.ShowText("Health is Full");
                    //    }
                    //}
                }
            }
        }
    }
}