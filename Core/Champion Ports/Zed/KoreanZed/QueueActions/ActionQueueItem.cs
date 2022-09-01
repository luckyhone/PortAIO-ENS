using System;

namespace KoreanZed.QueueActions
{
    struct ActionQueueItem
    {
        public float Time;
        public Func<bool> PreConditionFunc;
        public Func<bool> ConditionToRemoveFunc;
        public Action ComboAction;
    }
}