using UnityEngine;
using RPGCreationKit;

namespace RPGCreationKit
{
    public class SpellBook_LearnFireball : ItemScript
    {
        public override void OnReadBook(BookItem book)
        {
            base.OnReadBook(book);

            if(!SpellsKnowledge.Player.HasSpell("S_Fireball01"))
            {
                SpellsKnowledge.Player.LearnSpell("S_Fireball01");
                AlertMessage.instance.InitAlertMessage("You learn: Fireball", 5.0f, false);
            }
        }
    }
}