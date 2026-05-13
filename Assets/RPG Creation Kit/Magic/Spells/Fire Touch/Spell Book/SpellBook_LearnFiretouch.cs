using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class SpellBook_LearnFiretouch : ItemScript
    {
        public override void OnReadBook(BookItem book)
        {
            base.OnReadBook(book);

            if(!SpellsKnowledge.Player.HasSpell("S_Firetouch01"))
            {
                SpellsKnowledge.Player.LearnSpell("S_Firetouch01");
                AlertMessage.instance.InitAlertMessage("You learn: Fire Touch", 5.0f, false);
            }
        }
    }
}