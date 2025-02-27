﻿using UnityEngine;

namespace Invector.vCharacterController.AI.FSMBehaviour
{
    public class AICheckHealth : vStateDecision
    {
       
        public override string defaultName
        {
            get
            {
                return "Check Health";
            }
        }
        public enum vCheckValue
        {
            Equals, Less, Greater, NoEqual
        }

        public vCheckValue checkValue = vCheckValue.NoEqual;

        public float value;

        public override bool Decide(vIFSMBehaviourController fsmBehaviour)
        {
            return CheckValue(fsmBehaviour);
        }

        protected virtual bool CheckValue(vIFSMBehaviourController fsmBehaviour)
        {
            if (fsmBehaviour == null) return false;

            float healthPercentage = (fsmBehaviour.aiController.currentHealth / fsmBehaviour.aiController.MaxHealth) * 100f;

            switch (checkValue)
            {
                case vCheckValue.Equals:
                    return healthPercentage == value;
                case vCheckValue.Less:
                    return healthPercentage < value;
                case vCheckValue.Greater:
                    return healthPercentage > value;
                case vCheckValue.NoEqual:
                    return healthPercentage != value;
            }            

            return false;
        }
    }
}