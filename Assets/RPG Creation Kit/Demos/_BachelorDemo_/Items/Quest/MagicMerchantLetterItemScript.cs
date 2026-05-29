using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class MagicMerchantLetterItemScript : ItemScript
    {
        public override void OnReadBook(BookItem book)
        {
            base.OnReadBook(book);
            RCKFunctions.CompleteQuestStage("SQ_DealingHealingCrystals", 50);
        }

    }
}